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

# Note: External Secret will be created by Helm chart, not manually here

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
  --set langfuse.additionalEnv[4].name="AUTH_DISABLE_SIGNUP" \
  --set-string langfuse.additionalEnv[4].value="false" \
  --set langfuse.additionalEnv[5].name="NEXTAUTH_URL" \
  --set-string langfuse.additionalEnv[5].value="https://teachin.westeurope.cloudapp.azure.com/langfuse" \
  --set langfuse.additionalEnv[6].name="NEXT_PUBLIC_BASE_PATH" \
  --set-string langfuse.additionalEnv[6].value="/langfuse" \
  --set langfuse.additionalEnv[7].name="EMAIL_PROVIDER" \
  --set-string langfuse.additionalEnv[7].value="smtp" \
  --set langfuse.additionalEnv[8].name="SMTP_CONNECTION_URL" \
  --set-string langfuse.additionalEnv[8].value="smtps://$SMTP_USER:$SMTP_PASSWORD@smtp.gmail.com:465" \
  --set langfuse.additionalEnv[9].name="SMTP_HOST" \
  --set-string langfuse.additionalEnv[9].value="smtp.gmail.com" \
  --set langfuse.additionalEnv[10].name="SMTP_PORT" \
  --set-string langfuse.additionalEnv[10].value="465" \
  --set langfuse.additionalEnv[11].name="SMTP_USER" \
  --set-string langfuse.additionalEnv[11].value="$SMTP_USER" \
  --set langfuse.additionalEnv[12].name="SMTP_PASSWORD" \
  --set-string langfuse.additionalEnv[12].value="$SMTP_PASSWORD" \
  --set langfuse.additionalEnv[13].name="SMTP_SECURE" \
  --set-string langfuse.additionalEnv[13].value="true" \
  --set langfuse.additionalEnv[14].name="SMTP_FROM" \
  --set-string langfuse.additionalEnv[14].value="$SMTP_USER" \
  --set langfuse.additionalEnv[15].name="AUTH_SMTP_USER" \
  --set-string langfuse.additionalEnv[15].value="$SMTP_USER" \
  --set langfuse.additionalEnv[16].name="AUTH_SMTP_PASS" \
  --set-string langfuse.additionalEnv[16].value="$SMTP_PASSWORD" \
  --set langfuse.additionalEnv[17].name="SMTP_DEBUG" \
  --set-string langfuse.additionalEnv[17].value="false" \
  --set langfuse.additionalEnv[18].name="SMTP_LOG_LEVEL" \
  --set-string langfuse.additionalEnv[18].value="info" \
  --set langfuse.additionalEnv[19].name="SMTP_LOGGER" \
  --set-string langfuse.additionalEnv[19].value="true" \
  --set langfuse.additionalEnv[20].name="NEXT_PUBLIC_INVITE_URL" \
  --set-string langfuse.additionalEnv[20].value="https://teachin.westeurope.cloudapp.azure.com/langfuse" \
  --set langfuse.additionalEnv[21].name="NEXTAUTH_INVITATION_EMAIL_SUBJECT" \
  --set-string langfuse.additionalEnv[21].value="You have been invited to TeachIn's Langfuse" \
  --set langfuse.additionalEnv[22].name="NEXTAUTH_INVITATION_EMAIL_TEMPLATE" \
  --set-string langfuse.additionalEnv[22].value="<p>You have been invited to join TeachIn's Langfuse instance.</p><p>Click the button below to accept the invitation:</p>" \
  --set langfuse.additionalEnv[23].name="INVITE_FROM_NAME" \
  --set-string langfuse.additionalEnv[23].value="TeachIn Admin" \
  --set langfuse.additionalEnv[24].name="INVITE_FROM_EMAIL" \
  --set-string langfuse.additionalEnv[24].value="$SMTP_USER" \
  --set langfuse.additionalEnv[25].name="NEXT_PUBLIC_CONTACT_EMAIL" \
  --set-string langfuse.additionalEnv[25].value="$SMTP_USER" \
  --set langfuse.additionalEnv[26].name="EMAIL_FROM_NAME" \
  --set-string langfuse.additionalEnv[26].value="TeachIn" \
  --set langfuse.additionalEnv[27].name="EMAIL_FROM_ADDRESS" \
  --set-string langfuse.additionalEnv[27].value="$SMTP_USER" \
  --set langfuse.additionalEnv[28].name="EMAIL_FROM" \
  --set-string langfuse.additionalEnv[28].value="$SMTP_USER" \
  --set langfuse.additionalEnv[29].name="MEMBERSHIP_INVITATION_EMAIL_SUBJECT" \
  --set-string langfuse.additionalEnv[29].value="You're invited to join the Zionet organization on Langfuse" \
  --set langfuse.additionalEnv[30].name="MEMBERSHIP_INVITATION_EMAIL_TEMPLATE" \
  --set-string langfuse.additionalEnv[30].value="<p>Admin User ({{inviterName}}) has invited you to join the Zionet organization on Langfuse.</p><p><a href=\"{{url}}\" style=\"background: #667eea; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block; margin: 10px 0;\">Accept Invitation</a></p><p><em>(you need to create an account with this email address)</em></p><p>or copy and paste this URL into your browser: {{url}}</p>" \
  --set langfuse.additionalEnv[31].name="INVITE_URL_BASE" \
  --set-string langfuse.additionalEnv[31].value="https://teachin.westeurope.cloudapp.azure.com/langfuse" \
  --set langfuse.additionalEnv[32].name="NEXT_PUBLIC_SIGNUP_DISABLED" \
  --set-string langfuse.additionalEnv[32].value="true" \
  --set langfuse.additionalEnv[33].name="AUTH_DISABLE_SIGNUP_DEFAULT_DOMAIN" \
  --set-string langfuse.additionalEnv[33].value="true" \
  --set langfuse.additionalEnv[34].name="INVITATION_REQUIRED" \
  --set-string langfuse.additionalEnv[34].value="true" \
  --set langfuse.additionalEnv[35].name="VALIDATE_INVITATION_EMAIL" \
  --set-string langfuse.additionalEnv[35].value="true" \
  --set redis.auth.existingSecret="langfuse-secrets" \
  --set redis.auth.existingSecretPasswordKey="REDIS_PASSWORD" \
  --set redis.auth.username="default" \
  --set redis.auth.enabled=true \
  --set redis.architecture="standalone" \
  --set langfuse.redis.host="langfuse-redis-primary" \
  --set langfuse.redis.port="6379" \
  --set langfuse.redis.database="0" \
  --set langfuse.redis.tls="false" \
  --timeout=5m

echo "✅ Chart applied with web=0."

# Wait for the External Secret to be created by Helm and then create the Kubernetes secret
echo "⏳ Waiting for langfuse-secrets to be created by External Secrets..."
for i in {1..30}; do
  if kubectl get secret langfuse-secrets -n "$NAMESPACE" >/dev/null 2>&1; then
    echo "✅ langfuse-secrets created successfully"
    break
  fi
  echo "Waiting for External Secrets to create the secret... ($i/30)"
  sleep 10
done

if ! kubectl get secret langfuse-secrets -n "$NAMESPACE" >/dev/null 2>&1; then
  echo "❌ Failed to create langfuse-secrets via External Secrets"
  echo "Please check Azure Key Vault for the required secrets"
  exit 1
fi

echo "✅ Secrets ready. Running Prisma migrations as a Job..."

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
            # If migration fails, try to resolve common failed migrations and retry
            if ! npx prisma migrate deploy --schema=packages/shared/prisma/schema.prisma; then
              echo "Migration failed, attempting to resolve and retry..."
              npx prisma migrate resolve --applied 20240104210051_add_model_indices --schema=packages/shared/prisma/schema.prisma || true
              npx prisma migrate resolve --applied 20240111152124_add_gpt_35_pricing --schema=packages/shared/prisma/schema.prisma || true
              npx prisma migrate resolve --applied 20240226165118_add_observations_index --schema=packages/shared/prisma/schema.prisma || true
              npx prisma migrate resolve --applied 20250519073249_add_trace_media_media_id_index --schema=packages/shared/prisma/schema.prisma || true
              npx prisma migrate deploy --schema=packages/shared/prisma/schema.prisma
            fi
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

# --- Phase 3: Deploy ingress ---
echo "🌐 Deploying Langfuse ingress..."
INGRESS_FILE="../kubernetes/ingress/langfuse-ingress.yaml"

if [ -f "$INGRESS_FILE" ]; then
  echo "📁 Applying ingress from: $INGRESS_FILE"
  kubectl apply -f "$INGRESS_FILE"
else
  echo "⚠️  Ingress file not found at $INGRESS_FILE"
  echo "📝 You may need to create an ingress manually to expose Langfuse externally."
fi

echo "✅ Langfuse ingress configured."

# --- Cleanup temporary files ---
echo "🧹 Cleaning up temporary resources..."
kubectl delete job --selector=app.kubernetes.io/name=langfuse -n "$NAMESPACE" --ignore-not-found=true

echo ""
echo "🎉 Langfuse deployed successfully!"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "🔗 Access URL: https://teachin.westeurope.cloudapp.azure.com/langfuse"
echo "👤 Admin Email: $ADMIN_EMAIL"
echo "🔑 Admin Password: $ADMIN_PASSWORD"
echo "📊 Environment: $ENVIRONMENT_NAME"
echo "🗄️  Database: langfuse-$ENVIRONMENT_NAME"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "⚠️  Please change the admin password after first login!"
echo "📖 For more information, visit: https://langfuse.com/docs"
echo ""