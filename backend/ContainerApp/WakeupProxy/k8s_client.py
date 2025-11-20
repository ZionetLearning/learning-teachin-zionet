import asyncio
import logging
from typing import Dict, Optional
from scale import scale_down_loop
import httpx
from kubernetes_asyncio import client, config
from config import FORWARD_TIMEOUT
from contextlib import asynccontextmanager


logger = logging.getLogger("proxy")

# ---------------------------
# Kubernetes & HTTP client init/teardown
# ---------------------------
k8s_apis: Dict[str, object] = {}
http_client: Optional[httpx.AsyncClient] = None
k8s_ready = False

@asynccontextmanager
async def lifespan(app):
    global k8s_apis, http_client, k8s_ready

    logger.info("Loading in-cluster Kubernetes config")
    try:
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

    # Shutdown - close clients
    if http_client:
        await http_client.aclose()
        logger.info("HTTP client closed")
    for name, api_client in k8s_apis.items():
        try:
            await api_client.api_client.close()
            logger.info("Closed K8s API client: %s", name)
        except Exception:
            pass