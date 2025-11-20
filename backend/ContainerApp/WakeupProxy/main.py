import logging
from fastapi import FastAPI
from k8s_client import lifespan
from routes import router


logging.basicConfig(level=logging.INFO)

# Create an app with lifespan
app = FastAPI(lifespan=lifespan)
app.include_router(router)