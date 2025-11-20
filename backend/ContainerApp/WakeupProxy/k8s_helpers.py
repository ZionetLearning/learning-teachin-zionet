# k8s_helpers.py
import asyncio
import logging
from config import MAX_SCALEUP_WAIT
from k8s_client import k8s_apis
from kubernetes_asyncio import client
from kubernetes_asyncio.client.exceptions import ApiException

logger = logging.getLogger("proxy")

# ---------------------------
# Deployment helpers
# ---------------------------
async def read_deployment(namespace: str, name: str):
    try:
        return await k8s_apis["apps"].read_namespaced_deployment(name, namespace)
    except ApiException as e:
        if e.status == 404:
            return None
        raise


# ---------------------------
# Lease helpers
# ---------------------------
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
        if e.status == 409:
            try:
                lease = await k8s_apis["coordination"].read_namespaced_lease(lease_name, namespace)
                lease.spec.holder_identity = "proxy"
                await k8s_apis["coordination"].replace_namespaced_lease(lease_name, namespace, lease)
                return True
            except Exception:
                return False
        if e.status == 403:
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