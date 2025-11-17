import asyncio
import time
import logging
from typing import Dict

import httpx
from fastapi import FastAPI, Request, Response, HTTPException
from kubernetes_asyncio import client, config
from kubernetes_asyncio.client.exceptions import ApiException

app = FastAPI()
logger = logging.getLogger(__name__)
logging.basicConfig(level=logging.INFO)

# configuration
TARGET_SERVICE_NAME = "manager"
TARGET_SERVICE_PORT = 80
FORWARD_TIMEOUT = httpx.Timeout(20.0)
SCALE_UP_REPLICAS = 1
SCALE_DOWN_REPLICAS = 0
MAX_SCALEUP_WAIT = 90
INACTIVITY_TIMEOUT = 300
CHECK_INTERVAL = 30

last_access: Dict[str, float] = {}
k8s_ready = False


# -----------------------------------
# Kubernetes initialization
# -----------------------------------
@app.on_event("startup")
async def startup_event():
    global k8s_ready

    logger.info("Loading Kubernetes config...")

    try:
        # kubeconfig local? (useful for local dev)
        import os
        kube_path = os.path.expanduser("~/.kube/config")

        if os.path.exists(kube_path):
            logger.info("Loading kubeconfig from file...")
            await config.load_kube_config()
        else:
            logger.info("Loading in-cluster config...")
            config.load_incluster_config()

    except Exception as e:
        logger.error(f"Kubernetes config load FAILED: {e}")
        raise

    k8s_ready = True
    logger.info("Kubernetes client initialized")

    asyncio.create_task(scale_down_loop())


# -----------------------------------
# Scale helpers
# -----------------------------------
async def get_deployment(namespace: str):
    api = client.AppsV1Api()
    try:
        return await api.read_namespaced_deployment(TARGET_SERVICE_NAME, namespace)
    except ApiException as e:
        if e.status == 404:
            raise HTTPException(status_code=404, detail=f"Deployment '{TARGET_SERVICE_NAME}' not found in {namespace}")
        raise


async def get_replicas(namespace: str) -> int:
    dep = await get_deployment(namespace)
    return dep.spec.replicas or 0


async def scale(namespace: str, replicas: int):
    api = client.AppsV1Api()
    body = {"spec": {"replicas": replicas}}

    await api.patch_namespaced_deployment_scale(
        name=TARGET_SERVICE_NAME,
        namespace=namespace,
        body=body
    )
    logger.info(f"[{namespace}] Scaled to {replicas}")


# -----------------------------------
# Lease lock (one scaling at a time)
# -----------------------------------
async def acquire_lock(namespace: str):
    api = client.CoordinationV1Api()
    lease_name = f"{TARGET_SERVICE_NAME}-scaler-lock"

    body = client.V1Lease(
        metadata=client.V1ObjectMeta(name=lease_name, namespace=namespace),
        spec=client.V1LeaseSpec(holder_identity="proxy")
    )

    try:
        # try create first
        await api.create_namespaced_lease(namespace, body)
        return True
    except ApiException as e:
        if e.status != 409:
            raise

    # lease exists → try update
    for _ in range(10):
        try:
            lease = await api.read_namespaced_lease(lease_name, namespace)
            lease.spec.holder_identity = "proxy"
            await api.replace_namespaced_lease(lease_name, namespace, lease)
            return True
        except:
            await asyncio.sleep(1)

    return False


# -----------------------------------
# Wait for pods to be Ready
# -----------------------------------
async def wait_for_ready(namespace: str) -> bool:
    core = client.CoreV1Api()
    waited = 0

    while waited < MAX_SCALEUP_WAIT:
        pods = await core.list_namespaced_pod(namespace, label_selector=f"app={TARGET_SERVICE_NAME}")

        for p in pods.items:
            conds = p.status.conditions or []
            for c in conds:
                if c.type == "Ready" and c.status == "True":
                    return True

        await asyncio.sleep(1)
        waited += 1

    return False


# -----------------------------------
# Scale-up logic with lock
# -----------------------------------
async def scale_up_if_needed(namespace: str):
    replicas = await get_replicas(namespace)
    if replicas > 0:
        return

    if not await acquire_lock(namespace):
        logger.warning(f"[{namespace}] lock busy")
        return

    logger.info(f"[{namespace}] scaling UP")
    await scale(namespace, SCALE_UP_REPLICAS)

    if not await wait_for_ready(namespace):
        logger.warning(f"[{namespace}] pod did NOT become ready in time")


# -----------------------------------
# Proxy request
# -----------------------------------
async def forward_request(namespace: str, path: str, request: Request) -> Response:
    target_url = f"http://{TARGET_SERVICE_NAME}.{namespace}.svc.cluster.local:{TARGET_SERVICE_PORT}/{path}"

    async with httpx.AsyncClient(timeout=FORWARD_TIMEOUT) as client_http:
        try:
            resp = await client_http.request(
                request.method,
                target_url,
                params=request.query_params,
                content=await request.body(),
                headers={k: v for k, v in request.headers.items() if k.lower() not in ["host", "connection"]},
            )
            return Response(content=resp.content, status_code=resp.status_code, headers=resp.headers)
        except Exception as e:
            logger.exception(f"[{namespace}] forwarding failed: {e}")
            raise HTTPException(status_code=502, detail="Bad Gateway")


# -----------------------------------
# Main route
# -----------------------------------
@app.api_route("/scaling}",
               methods=["GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS"])
async def handle(env: str, path: str, request: Request):
    if not k8s_ready:
        raise HTTPException(status_code=500, detail="Kubernetes client not ready")

    namespace = "scaling"  # EXACT match to namespace
    logger.info(f"[{namespace}] Received request for /{path}")
    last_access[namespace] = time.time()

    await scale_up_if_needed(namespace)

    return await forward_request(namespace, path, request)


# -----------------------------------
# Background scale-down loop
# -----------------------------------
async def scale_down_loop():
    await asyncio.sleep(5)
    while True:
        now = time.time()
        for ns, last_t in list(last_access.items()):
            if now - last_t > INACTIVITY_TIMEOUT:
                logger.info(f"[{ns}] inactivity timeout — scaling DOWN")
                try:
                    await scale(ns, SCALE_DOWN_REPLICAS)
                except Exception as e:
                    logger.exception(f"[{ns}] scale-down failed: {e}")
                last_access.pop(ns, None)
        await asyncio.sleep(CHECK_INTERVAL)