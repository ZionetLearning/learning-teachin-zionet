FROM python:3.12-slim

# system deps
RUN apt-get update && apt-get install -y --no-install-recommends gcc libpq-dev && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# copy only requirements first for cache
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY proxy.py /app/proxy.py

# run as nobody (optional)
RUN useradd -m proxyuser
USER proxyuser

EXPOSE 80

CMD ["uvicorn", "proxy:app", "--host", "0.0.0.0", "--port", "80", "--loop", "asyncio"]