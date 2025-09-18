#!/bin/bash
set -euo pipefail

NAMESPACE="devops-tools"
PG_HOST="dev-pg-zionet-learning.postgres.database.azure.com"
ENVIRONMENT_NAME="${1:-dev}"
ADMIN_EMAIL="${2:-admin@teachin.local}"
ADMIN_PASSWORD="${3:-ChangeMe123!}"

echo "🎯 Deploying Langfuse into $NAMESPACE (DB suffix: $ENVIRONMENT_NAME)"

helm repo add langfuse https://langfuse.github.io/langfuse-k8s || true
helm repo update

kubectl create namespace "$NAMESPACE" --dry-run=client -o yaml | kubectl apply -f -

ACTION="install"
if helm status langfuse -n "$NAMESPACE" >/dev/null 2>&1; then
  ACTION="upgrade"
else
  helm uninstall langfuse -n "$NAMESPACE" --keep-history=false || true
  kubectl delete pvc --all -n "$NAMESPACE" --ignore-not-found=true
  sleep 5
fi

# Phase 1: install with web replicas=0 (avoid race with migrations)
helm $ACTION langfuse langfuse/langfuse \
  --namespace "$NAMESPACE" \
  --set langfuse.replicas=0 \
  --set langfuse.nextauth.url="https://teachin.westeurope.cloudapp.azure.com/langfuse" \
  --set langfuse.salt.secretKeyRef.name="langfuse-secrets" \
  --set langfuse.salt.secretKeyRef.key="SALT" \
  --set langfuse.nextauth.secret.secretKeyRef.name="langfuse-secrets" \
  --set langfuse.nextauth.secret.secretKeyRef.key="NEXTAUTH_SECRET" \
  --set langfuse.resources.requests.cpu="250m" \
  --set langfuse.resources.requests.memory="512Mi" \
  --set langfuse.resources.limits.cpu="1000m" \
  --set langfuse.resources.limits.memory="1Gi" \
  --set langfuse.worker.replicas=1 \
  --set langfuse.worker.resources.requests.cpu="200m" \
  --set langfuse.worker.resources.requests.memory="256Mi" \
  --set langfuse.worker.resources.limits.cpu="500m" \
  --set langfuse.worker.resources.limits.memory="512Mi" \
  --set postgresql.deploy=false \
  --set postgresql.host="${PG_HOST}" \
  --set postgresql.port=5432 \
  --set postgresql.auth.database="langfuse-${ENVIRONMENT_NAME}" \
  --set postgresql.auth.existingSecret="langfuse-secrets" \
  --set postgresql.auth.secretKeys.usernameKey="DATABASE_USERNAME" \
  --set postgresql.auth.secretKeys.userPasswordKey="DATABASE_PASSWORD" \
  --set clickhouse.auth.existingSecret="langfuse-secrets" \
  --set clickhouse.auth.existingSecretKey="CLICKHOUSE_PASSWORD" \
  --set clickhouse.resourcesPreset="nano" \
  --set clickhouse.replicaCount=1 \
  --set clickhouse.resources.requests.cpu="100m" \
  --set clickhouse.resources.requests.memory="256Mi" \
  --set clickhouse.resources.limits.cpu="200m" \
  --set clickhouse.resources.limits.memory="512Mi" \
  --set clickhouse.zookeeper.enabled=false \
  --set redis.auth.existingSecret="langfuse-secrets" \
  --set redis.auth.existingSecretPasswordKey="REDIS_PASSWORD" \
  --set s3.auth.existingSecret="langfuse-secrets" \
  --set s3.auth.rootUserSecretKey="S3_USER" \
  --set s3.auth.rootPasswordSecretKey="S3_PASSWORD" \
  --set s3.bucket="langfuse-bucket" \
  --set langfuse.worker.replicas=1 \
  --set langfuse.worker.resources.requests.cpu="50m" \
  --set langfuse.worker.resources.requests.memory="128Mi" \
  --set langfuse.worker.resources.limits.cpu="200m" \
  --set langfuse.worker.resources.limits.memory="256Mi" \
  --set langfuse.additionalEnv[0].name="LANGFUSE_LOG_LEVEL" \
  --set-string langfuse.additionalEnv[0].value="debug" \
  --set langfuse.additionalEnv[1].name="LANGFUSE_AUTO_POSTGRES_MIGRATION_DISABLED" \
  --set-string langfuse.additionalEnv[1].value="false" \
  --set langfuse.additionalEnv[2].name="DISABLE_LIVENESS_PROBE" \
  --set-string langfuse.additionalEnv[2].value="true" \
  --set langfuse.additionalEnv[3].name="DISABLE_READINESS_PROBE" \
  --set-string langfuse.additionalEnv[3].value="true" \
  --set langfuse.additionalEnv[4].name="NEXT_PUBLIC_DISABLE_SIGNUP" \
  --set-string langfuse.additionalEnv[4].value="true" \
  --set langfuse.additionalEnv[5].name="DISABLE_SIGNUP" \
  --set-string langfuse.additionalEnv[5].value="true" \
  --set langfuse.additionalEnv[6].name="AUTH_DISABLE_SIGNUP" \
  --set-string langfuse.additionalEnv[6].value="true" \
  --timeout=5m

echo "✅ Chart applied with web=0. Running Prisma migrations as a Job..."

# --- Phase 1.5: run Prisma migrations once, with the same secrets env as the app ---
kubectl delete job langfuse-migrate -n "$NAMESPACE" --ignore-not-found

cat <<EOF | kubectl apply -f -
apiVersion: batch/v1
kind: Job
metadata:
  name: langfuse-migrate
  namespace: $NAMESPACE
spec:
  backoffLimit: 1
  template:
    spec:
      restartPolicy: Never
      containers:
      - name: migrate
        image: langfuse/langfuse:3.108.0
        command: ["sh", "-c"]
        args:
          - |
            npx prisma migrate resolve --applied 20240104210052_add_model_indices_pricing --schema=packages/shared/prisma/schema.prisma || true
            npx prisma migrate deploy --schema=packages/shared/prisma/schema.prisma
        envFrom:
        - secretRef:
            name: langfuse-secrets
EOF

kubectl wait --for=condition=complete job/langfuse-migrate -n "$NAMESPACE" --timeout=600s
kubectl delete job langfuse-migrate -n "$NAMESPACE" --ignore-not-found

echo "✅ Migrations applied."

# --- Phase 1.6: seed admin user ---
kubectl delete job langfuse-seed-admin -n "$NAMESPACE" --ignore-not-found

# Precompute bcrypt hash outside Postgres (using a Node container)
ADMIN_PASSWORD_HASH=$(docker run --rm node:22-alpine \
  sh -c "npm install -g bcrypt-cli >/dev/null 2>&1 && echo '$ADMIN_PASSWORD' | npx bcrypt-cli -s 12")

cat <<EOF | kubectl apply -f -
apiVersion: batch/v1
kind: Job
metadata:
  name: langfuse-seed-admin
  namespace: $NAMESPACE
spec:
  backoffLimit: 1
  template:
    spec:
      restartPolicy: Never
      containers:
      - name: seed-admin
        image: postgres:16
        envFrom:
        - secretRef:
            name: langfuse-secrets
        env:
        - name: PGHOST
          value: "$PG_HOST"
        - name: PGDB
          value: "langfuse-${ENVIRONMENT_NAME}"
        - name: ADMIN_EMAIL
          value: "$ADMIN_EMAIL"
        - name: ADMIN_PASSWORD_HASH
          value: "$ADMIN_PASSWORD_HASH"
        command:
        - bash
        - -c
        - |
          psql -v ON_ERROR_STOP=1 \
               -v admin_email="$ADMIN_EMAIL" \
               -v ADMIN_PASSWORD_HASH="$ADMIN_PASSWORD_HASH" \
               "host=\$PGHOST port=5432 dbname=\$PGDB user=\$DATABASE_USERNAME password=\$DATABASE_PASSWORD sslmode=require" -c "
          DO \$\$
          DECLARE
            v_user_id text;
            v_org_id  text;
          BEGIN
            SELECT id INTO v_user_id FROM users WHERE email = :'admin_email' LIMIT 1;
            IF v_user_id IS NULL THEN
              v_user_id := gen_random_uuid()::text;
              INSERT INTO users (id, name, email, password, admin, created_at, updated_at)
              VALUES (v_user_id, 'Admin', :'admin_email', :'ADMIN_PASSWORD_HASH', true, NOW(), NOW());
            END IF;

            SELECT id INTO v_org_id FROM organizations LIMIT 1;
            IF v_org_id IS NULL THEN
              v_org_id := gen_random_uuid()::text;
              INSERT INTO organizations (id, name, created_at, updated_at)
              VALUES (v_org_id, 'Default Org', NOW(), NOW());
            END IF;

            IF NOT EXISTS (
              SELECT 1 FROM organization_memberships
              WHERE user_id = v_user_id AND organization_id = v_org_id
            ) THEN
              INSERT INTO organization_memberships (id, user_id, organization_id, role, created_at, updated_at)
              VALUES (gen_random_uuid()::text, v_user_id, v_org_id, 'OWNER', NOW(), NOW());
            END IF;
          END
          \$\$;

          TABLE (SELECT id, email, admin FROM users WHERE email = :'admin_email');
          "
EOF

kubectl -n $NAMESPACE wait --for=condition=complete job/langfuse-seed-admin --timeout=30s
kubectl -n $NAMESPACE logs job/langfuse-seed-admin
kubectl delete job langfuse-seed-admin -n "$NAMESPACE" --ignore-not-found

# --- Phase 2: scale web back up to actually serve traffic ---
helm upgrade langfuse langfuse/langfuse \
  --namespace "$NAMESPACE" \
  --set langfuse.replicas=1 \
  --reuse-values

kubectl rollout status deploy/langfuse-web -n "$NAMESPACE" --timeout=300s
kubectl rollout status deploy/langfuse-worker -n "$NAMESPACE" --timeout=300s

echo "🎉 Langfuse deployed successfully."