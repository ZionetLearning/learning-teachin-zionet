import asyncio
import time
import logging
import os
from typing import Dict, Optional

import httpx
from fastapi import FastAPI, Request, Response, HTTPException
from kubernetes_asyncio import client, config
from kubernetes_asyncio.client.exceptions import ApiException

app = FastAPI()
logger = logging.getLogger(__name__)
logging.basicConfig(level=logging.INFO)

# Configuration - READ FROM ENVIRONMENT
TARGET_SERVICE_NAME = os.getenv("TARGET_SERVICE_NAME", "manager")
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

logger.info(f"Config: TARGET_SERVICE={TARGET_SERVICE_NAME}, NAMESPACE={NAMESPACE}, PORT={TARGET_SERVICE_PORT}")


# -----------------------------------
# Kubernetes initialization
# -----------------------------------
@app.on_event("startup")
async def startup_event():
    global k8s_ready, http_client, k8s_apis

    logger.info("Loading Kubernetes config...")

    try:
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

    # Create and store API clients (reused to avoid unclosed session warnings)
    k8s_apis['apps'] = client.AppsV1Api()
    k8s_apis['core'] = client.CoreV1Api()
    k8s_apis['coordination'] = client.CoordinationV1Api()
    
    http_client = httpx.AsyncClient(timeout=FORWARD_TIMEOUT)
    k8s_ready = True
    
    logger.info("Kubernetes client initialized")
    asyncio.create_task(scale_down_loop())


@app.on_event("shutdown")
async def shutdown_event():
    global http_client, k8s_apis
    
    if http_client:
        await http_client.aclose()
        logger.info("HTTP client closed")
    
    # Close all k8s API clients
    for name, api_client in k8s_apis.items():
        try:
            await api_client.api_client.close()
            logger.info(f"Closed K8s API client: {name}")
        except:
            pass


# -----------------------------------
# Scale helpers
# -----------------------------------
async def get_deployment(namespace: str):
    try:
        return await k8s_apis['apps'].read_namespaced_deployment(TARGET_SERVICE_NAME, namespace)
    except ApiException as e:
        if e.status == 404:
            raise HTTPException(
                status_code=404, 
                detail=f"Deployment '{TARGET_SERVICE_NAME}' not found in {namespace}"
            )
        raise


async def get_replicas(namespace: str) -> int:
    dep = await get_deployment(namespace)
    return dep.spec.replicas or 0


async def scale(namespace: str, replicas: int):
    body = {"spec": {"replicas": replicas}}
    await k8s_apis['apps'].patch_namespaced_deployment_scale(
        name=TARGET_SERVICE_NAME,
        namespace=namespace,
        body=body
    )
    logger.info(f"[{namespace}] Scaled to {replicas}")


# -----------------------------------
# Lease lock (one scaling at a time)
# -----------------------------------
async def acquire_lock(namespace: str) -> bool:
    lease_name = f"{TARGET_SERVICE_NAME}-scaler-lock"
    body = client.V1Lease(
        metadata=client.V1ObjectMeta(name=lease_name, namespace=namespace),
        spec=client.V1LeaseSpec(holder_identity="proxy")
    )

    try:
        await k8s_apis['coordination'].create_namespaced_lease(namespace, body)
        return True
    except ApiException as e:
        if e.status != 409:
            raise

    # Lease exists → try to acquire it
    for _ in range(10):
        try:
            lease = await k8s_apis['coordination'].read_namespaced_lease(lease_name, namespace)
            lease.spec.holder_identity = "proxy"
            await k8s_apis['coordination'].replace_namespaced_lease(lease_name, namespace, lease)
            return True
        except:
            await asyncio.sleep(1)

    return False


# -----------------------------------
# Wait for pods to be Ready
# -----------------------------------
async def wait_for_ready(namespace: str, timeout: int = MAX_SCALEUP_WAIT) -> bool:
    """Wait for at least one pod to be Ready"""
    waited = 0
    logger.info(f"[{namespace}] Waiting for pods to be ready (timeout={timeout}s)...")
    
    while waited < timeout:
        try:
            pods = await k8s_apis['core'].list_namespaced_pod(
                namespace, 
                label_selector=f"app={TARGET_SERVICE_NAME}"
            )

            for p in pods.items:
                conds = p.status.conditions or []
                for c in conds:
                    if c.type == "Ready" and c.status == "True":
                        logger.info(f"[{namespace}] Pod {p.metadata.name} is Ready after {waited}s")
                        return True
        except Exception as e:
            logger.warning(f"[{namespace}] Error checking pod status: {e}")

        await asyncio.sleep(1)
        waited += 1
        
        # Log progress every 10 seconds
        if waited % 10 == 0:
            logger.info(f"[{namespace}] Still waiting for pods... ({waited}/{timeout}s)")

    logger.warning(f"[{namespace}] Pods did NOT become ready within {timeout}s")
    return False


# -----------------------------------
# Scale-up logic with lock and wait
# -----------------------------------
async def scale_up_and_wait(namespace: str) -> bool:
    """Scale up the deployment and wait for it to be ready. Returns True if successful."""
    logger.info(f"[{namespace}] Checking if scale-up is needed...")
    
    replicas = await get_replicas(namespace)
    if replicas > 0:
        # Already scaled up, check if pods are ready
        logger.info(f"[{namespace}] Already scaled to {replicas}, checking pod readiness...")
        return await wait_for_ready(namespace, timeout=30)

    # Need to scale up
    if not await acquire_lock(namespace):
        logger.warning(f"[{namespace}] Lock is busy, another process is scaling")
        # Wait for the other process to finish scaling
        return await wait_for_ready(namespace, timeout=MAX_SCALEUP_WAIT)

    try:
        logger.info(f"[{namespace}] Scaling UP to {SCALE_UP_REPLICAS} replicas")
        await scale(namespace, SCALE_UP_REPLICAS)
        return await wait_for_ready(namespace, timeout=MAX_SCALEUP_WAIT)
    except Exception as e:
        logger.error(f"[{namespace}] Scale-up failed: {e}")
        return False


# -----------------------------------
# Proxy request with retries
# -----------------------------------
async def forward_request(namespace: str, path: str, request: Request) -> Response:
    """Forward the request to the target service with retries"""
    global http_client
    
    target_url = f"http://{TARGET_SERVICE_NAME}.{namespace}.svc.cluster.local:{TARGET_SERVICE_PORT}/{path}"
    
    MAX_RETRIES = 5
    RETRY_DELAY = 2.0
    
    HOP_BY_HOP_HEADERS = {
        'connection', 'keep-alive', 'proxy-authenticate', 'proxy-authorization', 
        'te', 'trailers', 'transfer-encoding', 'upgrade'
    }
    
    request_headers = {
        k: v for k, v in request.headers.items() 
        if k.lower() not in ["host", "connection"]
    }
    
    # Read request body once (can't be re-read)
    try:
        body_content = await request.body()
    except Exception as e:
        logger.error(f"[{namespace}] Failed to read request body: {e}")
        raise HTTPException(status_code=400, detail="Failed to read request body")

    for attempt in range(MAX_RETRIES):
        try:
            logger.info(f"[{namespace}] Forwarding request to {target_url} (attempt {attempt + 1}/{MAX_RETRIES})")
            
            resp = await http_client.request(
                request.method,
                target_url,
                params=request.query_params,
                content=body_content,
                headers=request_headers,
            )
            
            # Success! Filter hop-by-hop headers and return
            response_headers = {
                k: v for k, v in resp.headers.items() 
                if k.lower() not in HOP_BY_HOP_HEADERS
            }
            
            logger.info(f"[{namespace}] Successfully forwarded request (status={resp.status_code})")
            return Response(
                content=resp.content,
                status_code=resp.status_code,
                headers=response_headers
            )
        
        except (httpx.ConnectError, httpx.TimeoutException) as e:
            if attempt < MAX_RETRIES - 1:
                logger.warning(
                    f"[{namespace}] Connection failed on attempt {attempt + 1}: {e}. "
                    f"Retrying in {RETRY_DELAY}s..."
                )
                await asyncio.sleep(RETRY_DELAY)
                continue
            
            logger.error(f"[{namespace}] All {MAX_RETRIES} forwarding attempts failed")
            raise HTTPException(
                status_code=502,
                detail=f"Service unavailable after {MAX_RETRIES} attempts"
            )
        
        except Exception as e:
            logger.exception(f"[{namespace}] Unexpected forwarding error: {e}")
            raise HTTPException(status_code=502, detail="Internal proxy error")


# -----------------------------------
# Main route - HANDLES SCALING AUTOMATICALLY
# -----------------------------------
@app.api_route("/{path:path}", methods=["GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS"])
async def handle(path: str, request: Request):
    if not k8s_ready:
        raise HTTPException(status_code=500, detail="Kubernetes client not ready")
    
    namespace = NAMESPACE
    logger.info(f"[{namespace}] Received {request.method} request for path='/{path}'")
    
    # Update last access time
    last_access[namespace] = time.time()
    
    # Scale up if needed and wait for it to be ready
    is_ready = await scale_up_and_wait(namespace)
    
    if not is_ready:
        logger.error(f"[{namespace}] Service failed to become ready within timeout")
        raise HTTPException(
            status_code=503,
            detail=f"Service failed to start within {MAX_SCALEUP_WAIT} seconds. Please try again."
        )
    
    # Service is ready, forward the request
    return await forward_request(namespace, path, request)


# -----------------------------------
# Background scale-down loop
# -----------------------------------
async def scale_down_loop():
    """Background task that scales down inactive services"""
    await asyncio.sleep(5)  # Initial delay
    
    logger.info("Scale-down loop started")
    
    while True:
        try:
            now = time.time()
            for ns, last_t in list(last_access.items()):
                inactive_seconds = now - last_t
                
                if inactive_seconds > INACTIVITY_TIMEOUT:
                    logger.info(
                        f"[{ns}] Inactivity timeout reached ({inactive_seconds:.0f}s) - scaling DOWN"
                    )
                    try:
                        await scale(ns, SCALE_DOWN_REPLICAS)
                        last_access.pop(ns, None)
                    except Exception as e:
                        logger.exception(f"[{ns}] Scale-down failed: {e}")
        except Exception as e:
            logger.exception(f"Error in scale-down loop: {e}")
        
        await asyncio.sleep(CHECK_INTERVAL)


# -----------------------------------
# Health check endpoint
# -----------------------------------
@app.get("/health")
async def health():
    """Health check endpoint"""
    return {
        "status": "healthy",
        "k8s_ready": k8s_ready,
        "target_service": TARGET_SERVICE_NAME,
        "namespace": NAMESPACE
    }