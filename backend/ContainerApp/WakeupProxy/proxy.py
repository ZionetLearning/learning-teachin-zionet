import asyncio
from contextlib import asynccontextmanager

import time
import logging
import os
from typing import Dict, List, Optional
import httpx
from fastapi import FastAPI, Request, Response, HTTPException
from kubernetes_asyncio import client, config
from kubernetes_asyncio.client.exceptions import ApiException

logger = logging.getLogger("proxy")
logging.basicConfig(level=logging.INFO)

# ---------------------------
# Configuration from environment
# ---------------------------
TARGET_SERVICE_NAMES: List[str] = [
    s.strip() for s in os.getenv("TARGET_SERVICE_NAME", "manager,accessor,engine").split(",") if s.strip()
]
FORWARD_TO_SERVICE = os.getenv("FORWARD_TO_SERVICE", "manager")
NAMESPACE = os.getenv("NAMESPACE", "default")
TARGET_SERVICE_PORT = int(os.getenv("TARGET_SERVICE_PORT", "80"))
FORWARD_TIMEOUT = httpx.Timeout(float(os.getenv("FORWARD_TIMEOUT", "60.0")))
SCALE_UP_REPLICAS = int(os.getenv("SCALE_UP_REPLICAS", "1"))
SCALE_DOWN_REPLICAS = int(os.getenv("SCALE_DOWN_REPLICAS", "0"))
MAX_SCALEUP_WAIT = int(os.getenv("MAX_SCALEUP_WAIT", "150"))
INACTIVITY_TIMEOUT = int(os.getenv("INACTIVITY_TIMEOUT", "300"))
CHECK_INTERVAL = int(os.getenv("CHECK_INTERVAL", "30"))

last_access: Dict[str, float] = {}
k8s_ready = False
k8s_apis = {}
http_client: Optional[httpx.AsyncClient] = None

logger.info(
    "Config: TARGET_SERVICES=%s FORWARD_TO=%s NAMESPACE=%s PORT=%s",
    TARGET_SERVICE_NAMES,
    FORWARD_TO_SERVICE,
    NAMESPACE,
    TARGET_SERVICE_PORT,
)


# ---------------------------
# Kubernetes & HTTP client init/teardown
# ---------------------------
@asynccontextmanager
async def lifespan():
    global k8s_ready, http_client, k8s_apis

    # Startup - your existing startup code
    logger.info("Loading Kubernetes config...")
    try:
        kube_path = os.path.expanduser("~/.kube/config")
        if os.path.exists(kube_path):
            logger.info("Loading kubeconfig from file")
            await config.load_kube_config()
        else:
            logger.info("Loading in-cluster config")
            config.load_incluster_config()
    except Exception as e:
        logger.exception("Failed loading kube config: %s", e)
        raise

    k8s_apis["apps"] = client.AppsV1Api()
    k8s_apis["core"] = client.CoreV1Api()
    k8s_apis["coordination"] = client.CoordinationV1Api()

    http_client = httpx.AsyncClient(timeout=FORWARD_TIMEOUT)
    k8s_ready = True
    logger.info("Kubernetes client initialized")
    asyncio.create_task(scale_down_loop())

    yield  # App runs here

    # Shutdown - your existing shutdown code
    if http_client:
        await http_client.aclose()
        logger.info("HTTP client closed")
    for name, api_client in k8s_apis.items():
        try:
            await api_client.api_client.close()
            logger.info("Closed K8s API client: %s", name)
        except Exception:
            pass


# Create an app with lifespan
app = FastAPI(lifespan=lifespan)


# ---------------------------
# Helpers: deployments, scale, leases
# ---------------------------
async def read_deployment(namespace: str, name: str):
    try:
        return await k8s_apis["apps"].read_namespaced_deployment(name, namespace)
    except ApiException as e:
        if e.status == 404:
            return None
        raise


async def get_replicas(namespace: str, name: str) -> int:
    dep = await read_deployment(namespace, name)
    if dep is None:
        return 0
    return dep.spec.replicas or 0


async def patch_scale(namespace: str, name: str, replicas: int):
    body = {"spec": {"replicas": replicas}}
    await k8s_apis["apps"].patch_namespaced_deployment_scale(
        name=name, namespace=namespace, body=body
    )
    logger.info("[%s] patch_scale %s -> %d", namespace, name, replicas)


async def try_acquire_lease(namespace: str, service: str) -> bool:
    lease_name = f"{service}-scaler-lock"
    body = client.V1Lease(
        metadata=client.V1ObjectMeta(name=lease_name, namespace=namespace),
        spec=client.V1LeaseSpec(holder_identity="proxy"),
    )
    try:
        await k8s_apis["coordination"].create_namespaced_lease(namespace, body)
        return True
    except ApiException as e:
        # 409 = already exists, 403 = forbidden (no permission)
        if e.status == 409:
            # try to replace the lease (optimistic)
            try:
                lease = await k8s_apis["coordination"].read_namespaced_lease(lease_name, namespace)
                lease.spec.holder_identity = "proxy"
                await k8s_apis["coordination"].replace_namespaced_lease(lease_name, namespace, lease)
                return True
            except Exception:
                # race or other failure
                return False
        if e.status == 403:
            # no permission to use leases — continue without a lease
            logger.warning("[%s][%s] Cannot create lease (forbidden). Continuing without lock.", namespace, service)
            return True
        raise


# ---------------------------
# Wait for pods to be ready
# tries several label selectors to be robust
# ---------------------------
async def wait_for_pod_ready(namespace: str, service: str, timeout: int = MAX_SCALEUP_WAIT) -> bool:
    waited = 0
    logger.info("[%s][%s] Waiting for pod ready (timeout=%ds)...", namespace, service, timeout)
    selectors = [f"io.kompose.service={service}", f"app={service}", f"app.kubernetes.io/name={service}"]

    while waited < timeout:
        try:
            for sel in selectors:
                pods = await k8s_apis["core"].list_namespaced_pod(namespace, label_selector=sel)
                for p in pods.items:
                    if p.status.phase != "Running":
                        continue
                    conds = p.status.conditions or []
                    for c in conds:
                        if c.type == "Ready" and c.status == "True":
                            logger.info("[%s][%s] Pod %s ready after %ds (selector=%s)", namespace, service,
                                        p.metadata.name, waited, sel)
                            return True
        except Exception as e:
            logger.warning("[%s][%s] Error listing pods: %s", namespace, service, e)

        await asyncio.sleep(1)
        waited += 1
        if waited % 10 == 0:
            logger.info("[%s][%s] Still waiting for pods... (%d/%d)", namespace, service, waited, timeout)

    logger.warning("[%s][%s] Pods did NOT become ready within %ds", namespace, service, timeout)
    return False


# ---------------------------
# Scale up multiple services (concurrently)
# ---------------------------
async def scale_up_services(namespace: str, services: List[str]) -> bool:
    """
    Ensure services are scaled up. Returns True if at least FORWARD_TO_SERVICE became ready.
    We'll attempt to scale each service to SCALE_UP_REPLICAS if it's currently 0.
    """
    # Acquire leases and issue scale patches concurrently
    tasks = []
    for svc in services:
        tasks.append(_scale_service_if_needed(namespace, svc))
    await asyncio.gather(*tasks, return_exceptions=True)
    # After scaling, make sure the forward target is ready (wait)
    ready = await wait_for_pod_ready(namespace, FORWARD_TO_SERVICE, timeout=MAX_SCALEUP_WAIT)
    return ready


async def _scale_service_if_needed(namespace: str, service: str):
    try:
        replicas = await get_replicas(namespace, service)
        if replicas and replicas > 0:
            logger.info("[%s][%s] already has %d replicas, skipping scale", namespace, service, replicas)
            return
        # Acquire a lease (if possible)
        ok = await try_acquire_lease(namespace, service)
        if not ok:
            logger.warning("[%s][%s] failed to acquire lease, skipping", namespace, service)
            return
        logger.info("[%s][%s] scaling up to %d", namespace, service, SCALE_UP_REPLICAS)
        await patch_scale(namespace, service, SCALE_UP_REPLICAS)
        # wait a short time for k8s to create pods before checking readiness
        await asyncio.sleep(1)
        # optionally, we can wait here for each service, but the main wait is for FORWARD_TO_SERVICE
        await wait_for_pod_ready(namespace, service, timeout=30)
    except Exception as e:
        logger.exception("[%s][%s] error during scale-up: %s", namespace, service, e)


# ---------------------------
# Scale down all configured services in a namespace
# ---------------------------
async def scale_down_services(namespace: str, services: List[str]):
    tasks = []
    for svc in services:
        tasks.append(_scale_service_down(namespace, svc))
    await asyncio.gather(*tasks, return_exceptions=True)


async def _scale_service_down(namespace: str, service: str):
    try:
        replicas = await get_replicas(namespace, service)
        if replicas == 0:
            logger.info("[%s][%s] already at 0, skipping scale-down", namespace, service)
            return
        logger.info("[%s][%s] scaling down to %d", namespace, service, SCALE_DOWN_REPLICAS)
        await patch_scale(namespace, service, SCALE_DOWN_REPLICAS)
    except Exception as e:
        logger.exception("[%s][%s] error during scale-down: %s", namespace, service, e)


# ---------------------------
# Forwarding logic
# ---------------------------
HOP_BY_HOP_HEADERS = {
    "connection",
    "keep-alive",
    "proxy-authenticate",
    "proxy-authorization",
    "te",
    "trailers",
    "transfer-encoding",
    "upgrade",
}


async def forward_to_manager(namespace: str, path: str, request: Request) -> Response:
    global http_client
    target_url = f"http://{FORWARD_TO_SERVICE}.{namespace}.svc.cluster.local:{TARGET_SERVICE_PORT}/{path.lstrip('/')}"
    try:
        body = await request.body()
    except Exception:
        body = None

    headers = {k: v for k, v in request.headers.items() if k.lower() not in {"host", "connection"}}
    # Use the existing http_client
    attempt = 0
    max_attempts = 3
    while attempt < max_attempts:
        try:
            logger.info("[%s] forwarding to %s (attempt %d)", namespace, target_url, attempt + 1)
            resp = await http_client.request(
                request.method,
                target_url,
                params=request.query_params,
                content=body,
                headers=headers,
            )
            response_headers = {k: v for k, v in resp.headers.items() if k.lower() not in HOP_BY_HOP_HEADERS}
            return Response(content=resp.content, status_code=resp.status_code, headers=response_headers)
        except (httpx.ConnectError, httpx.TimeoutException) as e:
            logger.warning("[%s] connection/timeout when forwarding: %s", namespace, e)
            attempt += 1
            await asyncio.sleep(1)
            continue
        except Exception as e:
            logger.exception("[%s] unexpected error when forwarding: %s", namespace, e)
            raise HTTPException(status_code=502, detail="Bad Gateway")
    raise HTTPException(status_code=502, detail="Service unavailable")


# ---------------------------
# Main route
# ---------------------------
@app.api_route("/{path:path}", methods=["GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS"])
async def handle(path: str, request: Request):
    if not k8s_ready:
        raise HTTPException(status_code=500, detail="Kubernetes client not ready")

    namespace = NAMESPACE
    logger.info("[%s] Received %s request for /%s", namespace, request.method, path.lstrip("/"))
    last_access[namespace] = time.time()

    # Scale up all configured services (non-blocking per-service, but waits for manager readiness)
    ready = await scale_up_services(namespace, TARGET_SERVICE_NAMES)

    if not ready:
        logger.error("[%s] Forward target did not become ready in time", namespace)
        raise HTTPException(
            status_code=503,
            detail=f"Service '{FORWARD_TO_SERVICE}' failed to start within {MAX_SCALEUP_WAIT} seconds",
        )

    # Forward the request to the manager
    return await forward_to_manager(namespace, path, request)


# ---------------------------
# Background scale-down loop
# ---------------------------
async def scale_down_loop():
    await asyncio.sleep(5)
    logger.info("Scale-down loop started")
    while True:
        try:
            now = time.time()
            for ns, last_t in list(last_access.items()):
                if now - last_t > INACTIVITY_TIMEOUT:
                    logger.info("[%s] inactivity timeout reached, scaling down services", ns)
                    try:
                        await scale_down_services(ns, TARGET_SERVICE_NAMES)
                        last_access.pop(ns, None)
                    except Exception as e:
                        logger.exception("[%s] error during scale-down loop: %s", ns, e)
        except Exception as e:
            logger.exception("Error in scale-down loop: %s", e)
        await asyncio.sleep(CHECK_INTERVAL)


# ---------------------------
# Health endpoint
# ---------------------------
@app.get("/health")
async def health():
    return {
        "status": "ok",
        "k8s_ready": k8s_ready,
        "forward_to": FORWARD_TO_SERVICE,
        "namespace": NAMESPACE,
        "target_services": TARGET_SERVICE_NAMES,
    }