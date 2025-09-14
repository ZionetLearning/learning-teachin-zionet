# PostgreSQL Database Access Guide for Backend Developers

## Prerequisites
- kubectl configured with access to the production cluster
- VS Code with PostgreSQL extension installed

## Setup Steps

### 1. Create Database Tunnel Pod
```bash
kubectl run postgres-tunnel --image=postgres:16 --restart=Never -n prod -- sleep 3600
```

### 2. Install socat in the tunnel pod
```bash
kubectl exec postgres-tunnel -n prod -- sh -c "apt-get update && apt-get install -y socat"
```

### 3. Start socat tunnel (forwards connections to private PostgreSQL)
```bash
kubectl exec postgres-tunnel -n prod -- sh -c "nohup socat TCP-LISTEN:5432,fork,reuseaddr TCP:e96178425ffb.privatelink.postgres.database.azure.com:5432 > /dev/null 2>&1 &"
```

### 4. Start port forwarding (in background terminal)
```bash
kubectl port-forward postgres-tunnel 5433:5432 -n prod
```

### 5. Connect from VS Code
- **Server:** localhost
- **Port:** 5433 (in Advanced settings)
- **Database:** appdb-prod
- **Username:** postgres
- **Password:** postgres

### 6. Test Connection
```bash
kubectl exec postgres-tunnel -n prod -- env PGPASSWORD=postgres psql -h localhost -p 5432 -U postgres -d appdb-prod -c "\dt"
```

## Cleanup (when done)
```bash
kubectl delete pod postgres-tunnel -n prod
```

## Connection Details
- **Host:** e96178425ffb.privatelink.postgres.database.azure.com
- **Database:** appdb-prod
- **Username:** postgres
- **Password:** postgres
- **SSL:** Required

## Troubleshooting
- If connection fails, check kubectl access to prod namespace
- Restart port forwarding if it stops
- Recreate tunnel pod if needed
