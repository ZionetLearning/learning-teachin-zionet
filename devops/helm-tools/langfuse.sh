#!/bin/bash
set -euo pipefail

NAMESPACE="devops-tools"
PG_HOST="dev-pg-zionet-learning.postgres.database.azure.com"
ENVIRONMENT_NAME="${1:-dev}"
ADMIN_EMAIL="${2:-admin@teachin.local}"
ADMIN_PASSWORD="${3:-ChangeMe123!}"
PG_USERNAME="${4:-postgres}"
PG_PASSWORD="${5:-postgres}"

echo "🎯 Deploying Langfuse into $NAMESPACE (DB suffix: $ENVIRONMENT_NAME)"
echo "📊 PostgreSQL Host: $PG_HOST"
echo "👤 Using PostgreSQL User: $PG_USERNAME"

helm repo add langfuse https://langfuse.github.io/langfuse-k8s || true
helm repo update

kubectl create namespace "$NAMESPACE" --dry-run=client -o yaml | kubectl apply -f -

ACTION="install"
if helm status langfuse -n "$NAMESPACE" >/dev/null 2>&1; then
  echo "🔄 Existing deployment found. Uninstalling for clean reinstall..."
  helm uninstall langfuse -n "$NAMESPACE" --keep-history=false || true
  kubectl delete pvc --all -n "$NAMESPACE" --ignore-not-found=true
  sleep 5
fi

# --- Phase 1: install with web=0 (avoid race with migrations) ---
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
  --set clickhouse.resources.requests.memory="512Mi" \
  --set clickhouse.resources.limits.cpu="200m" \
  --set clickhouse.resources.limits.memory="1Gi" \
  --set clickhouse.zookeeper.enabled=false \
  --set redis.auth.existingSecret="langfuse-secrets" \
  --set redis.auth.existingSecretPasswordKey="REDIS_PASSWORD" \
  --set s3.auth.existingSecret="langfuse-secrets" \
  --set s3.auth.rootUserSecretKey="S3_USER" \
  --set s3.auth.rootPasswordSecretKey="S3_PASSWORD" \
  --set s3.bucket="langfuse-bucket" \
  --set langfuse.additionalEnv[0].name="LANGFUSE_LOG_LEVEL" \
  --set-string langfuse.additionalEnv[0].value="info" \
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
  --set langfuse.additionalEnv[7].name="NEXTAUTH_URL" \
  --set-string langfuse.additionalEnv[7].value="https://teachin.westeurope.cloudapp.azure.com/langfuse" \
  --timeout=5m

echo "✅ Chart applied with web=0. Running Prisma migrations as a Job..."

# --- Phase 1.5: run Prisma migrations ---
kubectl delete job langfuse-migrate -n "$NAMESPACE" --ignore-not-found=true
sleep 2

cat <<EOF | kubectl apply -f -
apiVersion: batch/v1
kind: Job
metadata:
  name: langfuse-migrate
  namespace: $NAMESPACE
spec:
  backoffLimit: 3
  activeDeadlineSeconds: 600
  template:
    spec:
      restartPolicy: Never
      containers:
      - name: migrate
        image: langfuse/langfuse:3.112.0
        command: ["sh", "-c"]
        args:
          - |
            echo "Running Prisma migrations..."
            echo "Checking database connection..."
            npx prisma db pull --schema=packages/shared/prisma/schema.prisma --print || echo "DB connection check completed"
            
            echo "Starting migration deployment..."
            npx prisma migrate deploy --schema=packages/shared/prisma/schema.prisma
        envFrom:
        - secretRef:
            name: langfuse-secrets
EOF

kubectl wait --for=condition=complete job/langfuse-migrate -n "$NAMESPACE" --timeout=600s
kubectl delete job langfuse-migrate -n "$NAMESPACE" --ignore-not-found

echo "✅ Migrations applied."

# --- Phase 1.6: seed admin user ---
echo "🔐 Creating admin user: $ADMIN_EMAIL"

# Check if user already exists and get existing hash
echo "� Checking if user exists: $ADMIN_EMAIL"

EXISTING_HASH=$(kubectl run -n $NAMESPACE temp-check-user --image=postgres:16 --rm -i --restart=Never -- \
  psql "host=$PG_HOST port=5432 dbname=langfuse-${ENVIRONMENT_NAME} user=$PG_USERNAME password=$PG_PASSWORD sslmode=require" \
  -t -c "SELECT password FROM users WHERE email = '$ADMIN_EMAIL';" 2>/dev/null | tr -d ' ' || echo "")

# Always generate fresh hash for the provided password - ignore existing hash
echo "🔐 Generating bcrypt hash for password: $ADMIN_PASSWORD"

# Create a job to generate the hash to avoid kubectl output issues
kubectl delete job hash-generator -n "$NAMESPACE" --ignore-not-found=true
sleep 1

cat <<EOF | kubectl apply -f -
apiVersion: batch/v1
kind: Job
metadata:
  name: hash-generator
  namespace: $NAMESPACE
spec:
  template:
    spec:
      restartPolicy: Never
      containers:
      - name: hash-gen
        image: python:3.11-alpine
        command: ["/bin/sh", "-c"]
        args:
        - |
          pip install bcrypt >/dev/null 2>&1
          python3 -c "
          import bcrypt
          password = '$ADMIN_PASSWORD'
          salt = bcrypt.gensalt(rounds=12, prefix=b'2a')
          hash_value = bcrypt.hashpw(password.encode('utf-8'), salt)
          print('HASH:' + hash_value.decode('utf-8'))
          "
EOF

kubectl wait --for=condition=complete job/hash-generator -n "$NAMESPACE" --timeout=60s
HASH=$(kubectl logs job/hash-generator -n "$NAMESPACE" | grep "HASH:" | cut -d: -f2)
kubectl delete job hash-generator -n "$NAMESPACE"

if [ -z "$HASH" ] || [ ${#HASH} -lt 20 ]; then
  echo "❌ Failed to generate password hash. Cannot proceed."
  exit 1
fi

echo "✅ Generated bcrypt hash for password: $ADMIN_PASSWORD"

echo "🔐 Setting password for admin user..."

kubectl run -n $NAMESPACE temp-ensure-user --image=postgres:16 --rm -i --restart=Never -- \
  psql "host=$PG_HOST port=5432 dbname=langfuse-${ENVIRONMENT_NAME} user=$PG_USERNAME password=$PG_PASSWORD sslmode=require" \
  -c "
    INSERT INTO users (id, name, email, password, admin, email_verified, created_at, updated_at)
    VALUES (gen_random_uuid()::text, 'Admin User', '$ADMIN_EMAIL', '$HASH', true, NOW(), NOW(), NOW())
    ON CONFLICT (email) DO UPDATE
      SET password = EXCLUDED.password,
          admin = true,
          email_verified = NOW(),
          updated_at = NOW();

    SELECT 'User status:' as message, email, admin, email_verified IS NOT NULL as email_verified
    FROM users WHERE email = '$ADMIN_EMAIL';
  "

echo "🏢 Adding admin user to Default Organization..."

kubectl run -n $NAMESPACE temp-add-org-membership --image=postgres:16 --rm -i --restart=Never -- \
  psql "host=$PG_HOST port=5432 dbname=langfuse-${ENVIRONMENT_NAME} user=$PG_USERNAME password=$PG_PASSWORD sslmode=require" \
  -c "
    -- Ensure Default Organization exists
    INSERT INTO organizations (id, name, created_at, updated_at)
    SELECT gen_random_uuid()::text, 'Default Organization', NOW(), NOW()
    WHERE NOT EXISTS (SELECT 1 FROM organizations WHERE name = 'Default Organization');

    -- Add admin user to Default Organization with ADMIN role
    INSERT INTO organization_memberships (id, org_id, user_id, role, created_at, updated_at)
    SELECT 
        gen_random_uuid()::text as id,
        o.id as org_id,
        u.id as user_id,
        'ADMIN' as role,
        NOW() as created_at,
        NOW() as updated_at
    FROM users u, organizations o
    WHERE u.email = '$ADMIN_EMAIL' 
      AND o.name = 'Default Organization'
      AND NOT EXISTS (
          SELECT 1 FROM organization_memberships om 
          WHERE om.user_id = u.id AND om.org_id = o.id
      );

    SELECT 'Membership created:' as message, u.email, om.role, o.name as organization
    FROM organization_memberships om
    JOIN users u ON om.user_id = u.id  
    JOIN organizations o ON om.org_id = o.id
    WHERE u.email = '$ADMIN_EMAIL';
  "

echo "✅ Admin user created with password: $ADMIN_PASSWORD"

# --- Phase 2: scale web back up ---
helm upgrade langfuse langfuse/langfuse \
  --namespace "$NAMESPACE" \
  --set langfuse.replicas=1 \
  --reuse-values

kubectl rollout status deploy/langfuse-web -n "$NAMESPACE" --timeout=300s
kubectl rollout status deploy/langfuse-worker -n "$NAMESPACE" --timeout=300s

echo "🎉 Langfuse deployed successfully."
echo "🔗 Access Langfuse at: https://teachin.westeurope.cloudapp.azure.com"
echo "👤 Admin login: $ADMIN_EMAIL / $ADMIN_PASSWORD"
echo "ℹ️ Please change the temporary password after first login."