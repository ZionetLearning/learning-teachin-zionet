import os
from typing import List
import httpx
import logging


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

logger = logging.getLogger("proxy")
logger.info(
    "Config: TARGET_SERVICES=%s FORWARD_TO=%s NAMESPACE=%s PORT=%s",
    TARGET_SERVICE_NAMES,
    FORWARD_TO_SERVICE,
    NAMESPACE,
    TARGET_SERVICE_PORT,
)