from ast import Dict
import asyncio
import logging
from typing import List
from k8s_helpers import try_acquire_lease, wait_for_pod_ready
from k8s_state import k8s_apis
from config import SCALE_UP_REPLICAS, SCALE_DOWN_REPLICAS, MAX_SCALEUP_WAIT, CHECK_INTERVAL, INACTIVITY_TIMEOUT
import time

logger = logging.getLogger("proxy")
last_access: Dict[str, float] = {}

async def get_replicas(namespace: str, name: str) -> int:
    dep = await k8s_apis["apps"].read_namespaced_deployment(name, namespace)
    return dep.spec.replicas or 0

async def patch_scale(namespace: str, name: str, replicas: int):
    body = {"spec": {"replicas": replicas}}
    await k8s_apis["apps"].patch_namespaced_deployment_scale(name=name, namespace=namespace, body=body)
    logger.info("[%s] patch_scale %s -> %d", namespace, name, replicas)

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
    except Exception as e:
        logger.exception("[%s][%s] error during scale-up: %s", namespace, service, e)

async def scale_up_services(namespace: str, services: List[str]) -> bool:
    # Acquire leases and issue scale patches concurrently
    tasks = []
    for svc in services:
        tasks.append(_scale_service_if_needed(namespace, svc))
    await asyncio.gather(*tasks, return_exceptions=True)

     # Wait for all services to be ready
    ready_list = await asyncio.gather(
        *(wait_for_pod_ready(namespace, svc, timeout=MAX_SCALEUP_WAIT) for svc in services)
    )
    all_ready = all(ready_list)
    return all_ready

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
                        await scale_down_services(ns, last_access.keys())
                        last_access.pop(ns, None)
                    except Exception as e:
                        logger.exception("[%s] error during scale-down loop: %s", ns, e)
        except Exception as e:
            logger.exception("Error in scale-down loop: %s", e)
        await asyncio.sleep(CHECK_INTERVAL)