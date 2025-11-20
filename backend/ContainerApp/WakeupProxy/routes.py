from time import time
from fastapi import APIRouter, Request, HTTPException
from scale import scale_up_services, last_access
from forward import forward_to_manager
from k8s_client import k8s_ready
from config import MAX_SCALEUP_WAIT, TARGET_SERVICE_NAMES, NAMESPACE, FORWARD_TO_SERVICE
import logging

logger = logging.getLogger("proxy")
router = APIRouter()

# ---------------------------
# Main route
# ---------------------------
@router.api_route("/{path:path}", methods=["GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS"])
async def handle(path: str, request: Request):
    if not k8s_ready:
        raise HTTPException(status_code=500, detail="Kubernetes client not ready")

    logger.info("[%s] Received %s request for /%s", NAMESPACE, request.method, path.lstrip("/"))
    last_access[NAMESPACE] = time.time()

    # Scale up all configured services (non-blocking per-service, but waits for manager readiness)
    ready = await scale_up_services(NAMESPACE, TARGET_SERVICE_NAMES)

    if not ready:
        logger.error("[%s] Forward target did not become ready in time", NAMESPACE)
        raise HTTPException(
            status_code=503,
            detail=f"Service '{FORWARD_TO_SERVICE}' failed to start within {MAX_SCALEUP_WAIT} seconds",
        )

    # Forward the request to the manager
    return await forward_to_manager(path, request)

# ---------------------------
# Health endpoint
# ---------------------------
@router.get("/health")
async def health():
    return {
        "status": "ok",
        "k8s_ready": k8s_ready,
        "forward_to": FORWARD_TO_SERVICE,
        "namespace": NAMESPACE,
        "target_services": TARGET_SERVICE_NAMES,
    }