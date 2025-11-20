from fastapi import Request, Response, HTTPException
from k8s_client import http_client
from config import FORWARD_TO_SERVICE, NAMESPACE, TARGET_SERVICE_PORT
import logging
import asyncio
import httpx

logger = logging.getLogger("proxy")

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
    target_url = f"http://{FORWARD_TO_SERVICE}.{namespace}.svc.cluster.local:{TARGET_SERVICE_PORT}/{path.lstrip('/')}"
    try:
        body = await request.body()
    except Exception:
        body = None

    headers = {k: v for k, v in request.headers.items() if k.lower() not in {"host", "connection"}}
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