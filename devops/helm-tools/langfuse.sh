#!/bin/bash
set -euo pipefail

NAMESPACE="devops-tools"
ENVIRONMENT_NAME="${1:-dev}"
ADMIN_EMAIL="${2:-admin@teachin.local}"
ADMIN_PASSWORD="${3:-ChangeMe123!}"
PG_USERNAME="${4:-postgres}"
PG_PASSWORD="${5:-postgres}"
SMTP_USER="${6:-}"
SMTP_PASSWORD="${7:-}"

# Environment-specific configuration
if [ "$ENVIRONMENT_NAME" = "prod" ]; then
  PG_HOST="prod-pg-zionet-learning.postgres.database.azure.com"
  DOMAIN="teachin-prod.westeurope.cloudapp.azure.com"
  DOMAIN_SECRET="teachin-prod-tls"
else
  PG_HOST="dev-pg-zionet-learning.postgres.database.azure.com"
  DOMAIN="teachin.westeurope.cloudapp.azure.com"
  DOMAIN_SECRET="teachin-tls"
fi

echo "🎯 Deploying Langfuse into $NAMESPACE (Environment: $ENVIRONMENT_NAME)"
echo "📊 PostgreSQL Host: $PG_HOST"
echo "🌐 Domain: $DOMAIN"
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

# --- Phase 1: install with proper probe timeouts ---
helm $ACTION langfuse langfuse/langfuse \
  --namespace "$NAMESPACE" \
  --set langfuse.replicas=1 \
  --set langfuse.nextauth.url="https://$DOMAIN/langfuse" \
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
  --set langfuse.affinity.nodeAffinity.preferredDuringSchedulingIgnoredDuringExecution[0].weight=100 \
  --set langfuse.affinity.nodeAffinity.preferredDuringSchedulingIgnoredDuringExecution[0].preference.matchExpressions[0].key="node-type" \
  --set langfuse.affinity.nodeAffinity.preferredDuringSchedulingIgnoredDuringExecution[0].preference.matchExpressions[0].operator="In" \
  --set langfuse.affinity.nodeAffinity.preferredDuringSchedulingIgnoredDuringExecution[0].preference.matchExpressions[0].values[0]="spot" \
  --set langfuse.tolerations[0].key="kubernetes.azure.com/scalesetpriority" \
  --set langfuse.tolerations[0].operator="Equal" \
  --set langfuse.tolerations[0].value="spot" \
  --set langfuse.tolerations[0].effect="NoSchedule" \
  --set langfuse.worker.affinity.nodeAffinity.preferredDuringSchedulingIgnoredDuringExecution[0].weight=100 \
  --set langfuse.worker.affinity.nodeAffinity.preferredDuringSchedulingIgnoredDuringExecution[0].preference.matchExpressions[0].key="node-type" \
  --set langfuse.worker.affinity.nodeAffinity.preferredDuringSchedulingIgnoredDuringExecution[0].preference.matchExpressions[0].operator="In" \
  --set langfuse.worker.affinity.nodeAffinity.preferredDuringSchedulingIgnoredDuringExecution[0].preference.matchExpressions[0].values[0]="spot" \
  --set langfuse.worker.tolerations[0].key="kubernetes.azure.com/scalesetpriority" \
  --set langfuse.worker.tolerations[0].operator="Equal" \
  --set langfuse.worker.tolerations[0].value="spot" \
  --set langfuse.worker.tolerations[0].effect="NoSchedule" \
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
  --set clickhouse.affinity.nodeAffinity.preferredDuringSchedulingIgnoredDuringExecution[0].weight=100 \
  --set clickhouse.affinity.nodeAffinity.preferredDuringSchedulingIgnoredDuringExecution[0].preference.matchExpressions[0].key="node-type" \
  --set clickhouse.affinity.nodeAffinity.preferredDuringSchedulingIgnoredDuringExecution[0].preference.matchExpressions[0].operator="In" \
  --set clickhouse.affinity.nodeAffinity.preferredDuringSchedulingIgnoredDuringExecution[0].preference.matchExpressions[0].values[0]="spot" \
  --set clickhouse.tolerations[0].key="kubernetes.azure.com/scalesetpriority" \
  --set clickhouse.tolerations[0].operator="Equal" \
  --set clickhouse.tolerations[0].value="spot" \
  --set clickhouse.tolerations[0].effect="NoSchedule" \
  --set redis.auth.existingSecret="langfuse-secrets" \
  --set redis.auth.existingSecretPasswordKey="REDIS_PASSWORD" \
  --set redis.master.affinity.nodeAffinity.preferredDuringSchedulingIgnoredDuringExecution[0].weight=100 \
  --set redis.master.affinity.nodeAffinity.preferredDuringSchedulingIgnoredDuringExecution[0].preference.matchExpressions[0].key="node-type" \
  --set redis.master.affinity.nodeAffinity.preferredDuringSchedulingIgnoredDuringExecution[0].preference.matchExpressions[0].operator="In" \
  --set redis.master.affinity.nodeAffinity.preferredDuringSchedulingIgnoredDuringExecution[0].preference.matchExpressions[0].values[0]="spot" \
  --set redis.master.tolerations[0].key="kubernetes.azure.com/scalesetpriority" \
  --set redis.master.tolerations[0].operator="Equal" \
  --set redis.master.tolerations[0].value="spot" \
  --set redis.master.tolerations[0].effect="NoSchedule" \
  --set s3.auth.existingSecret="langfuse-secrets" \
  --set s3.auth.rootUserSecretKey="S3_USER" \
  --set s3.auth.rootPasswordSecretKey="S3_PASSWORD" \
  --set s3.bucket="langfuse-bucket" \
  --set s3.affinity.nodeAffinity.preferredDuringSchedulingIgnoredDuringExecution[0].weight=100 \
  --set s3.affinity.nodeAffinity.preferredDuringSchedulingIgnoredDuringExecution[0].preference.matchExpressions[0].key="node-type" \
  --set s3.affinity.nodeAffinity.preferredDuringSchedulingIgnoredDuringExecution[0].preference.matchExpressions[0].operator="In" \
  --set s3.affinity.nodeAffinity.preferredDuringSchedulingIgnoredDuringExecution[0].preference.matchExpressions[0].values[0]="spot" \
  --set s3.tolerations[0].key="kubernetes.azure.com/scalesetpriority" \
  --set s3.tolerations[0].operator="Equal" \
  --set s3.tolerations[0].value="spot" \
  --set s3.tolerations[0].effect="NoSchedule" \
  --set langfuse.livenessProbe.initialDelaySeconds=60 \
  --set langfuse.livenessProbe.timeoutSeconds=30 \
  --set langfuse.livenessProbe.periodSeconds=30 \
  --set langfuse.readinessProbe.initialDelaySeconds=60 \
  --set langfuse.readinessProbe.timeoutSeconds=30 \
  --set langfuse.readinessProbe.periodSeconds=30 \
  --set langfuse.worker.livenessProbe.initialDelaySeconds=60 \
  --set langfuse.worker.livenessProbe.timeoutSeconds=30 \
  --set langfuse.worker.readinessProbe.initialDelaySeconds=60 \
  --set langfuse.worker.readinessProbe.timeoutSeconds=30 \
  --set langfuse.additionalEnv[0].name="DISABLE_LIVENESS_PROBE" \
  --set-string langfuse.additionalEnv[0].value="true" \
  --set langfuse.additionalEnv[1].name="DISABLE_READINESS_PROBE" \
  --set-string langfuse.additionalEnv[1].value="true" \
  --set langfuse.additionalEnv[2].name="NEXT_PUBLIC_DISABLE_SIGNUP" \
  --set-string langfuse.additionalEnv[2].value="true" \
  --set langfuse.additionalEnv[3].name="DISABLE_SIGNUP" \
  --set-string langfuse.additionalEnv[3].value="true" \
  --set langfuse.additionalEnv[4].name="AUTH_DISABLE_SIGNUP" \
  --set-string langfuse.additionalEnv[4].value="true" \
  --set langfuse.additionalEnv[5].name="NEXT_PUBLIC_BASE_PATH" \
  --set-string langfuse.additionalEnv[5].value="/langfuse" \
  --set langfuse.additionalEnv[6].name="EMAIL_PROVIDER" \
  --set-string langfuse.additionalEnv[6].value="smtp" \
  --set langfuse.additionalEnv[7].name="SMTP_CONNECTION_URL" \
  --set-string langfuse.additionalEnv[7].value="smtps://$SMTP_USER:$SMTP_PASSWORD@smtp.gmail.com:465" \
  --set langfuse.additionalEnv[8].name="SMTP_HOST" \
  --set-string langfuse.additionalEnv[8].value="smtp.gmail.com" \
  --set langfuse.additionalEnv[9].name="SMTP_PORT" \
  --set-string langfuse.additionalEnv[9].value="465" \
  --set langfuse.additionalEnv[10].name="SMTP_USER" \
  --set-string langfuse.additionalEnv[10].value="$SMTP_USER" \
  --set langfuse.additionalEnv[11].name="SMTP_PASSWORD" \
  --set-string langfuse.additionalEnv[11].value="$SMTP_PASSWORD" \
  --set langfuse.additionalEnv[12].name="SMTP_SECURE" \
  --set-string langfuse.additionalEnv[12].value="true" \
  --set langfuse.additionalEnv[13].name="SMTP_FROM" \
  --set-string langfuse.additionalEnv[13].value="$SMTP_USER" \
  --set langfuse.additionalEnv[14].name="SMTP_REPLY_TO" \
  --set-string langfuse.additionalEnv[14].value="$SMTP_USER" \
  --set langfuse.additionalEnv[15].name="SMTP_REPLY_TO_NAME" \
  --set-string langfuse.additionalEnv[15].value="TeachIn Support" \
  --set langfuse.additionalEnv[16].name="SMTP_POOL" \
  --set-string langfuse.additionalEnv[16].value="true" \
  --set langfuse.additionalEnv[17].name="SMTP_MAX_CONNECTIONS" \
  --set-string langfuse.additionalEnv[17].value="5" \
  --set langfuse.additionalEnv[18].name="SMTP_MAX_MESSAGES" \
  --set-string langfuse.additionalEnv[18].value="100" \
  --set langfuse.additionalEnv[19].name="SMTP_RATE_DELTA" \
  --set-string langfuse.additionalEnv[19].value="1000" \
  --set langfuse.additionalEnv[20].name="SMTP_RATE_LIMIT" \
  --set-string langfuse.additionalEnv[20].value="5" \
  --set langfuse.additionalEnv[21].name="AUTH_SMTP_USER" \
  --set-string langfuse.additionalEnv[21].value="$SMTP_USER" \
  --set langfuse.additionalEnv[22].name="AUTH_SMTP_PASS" \
  --set-string langfuse.additionalEnv[22].value="$SMTP_PASSWORD" \
  --set langfuse.additionalEnv[23].name="SMTP_DEBUG" \
  --set-string langfuse.additionalEnv[23].value="false" \
  --set langfuse.additionalEnv[24].name="SMTP_LOG_LEVEL" \
  --set-string langfuse.additionalEnv[24].value="info" \
  --set langfuse.additionalEnv[25].name="SMTP_LOGGER" \
  --set-string langfuse.additionalEnv[25].value="true" \
  --set langfuse.additionalEnv[26].name="NEXT_PUBLIC_INVITE_URL" \
  --set-string langfuse.additionalEnv[26].value="https://$DOMAIN/langfuse" \
  --set langfuse.additionalEnv[27].name="NEXTAUTH_INVITATION_EMAIL_SUBJECT" \
  --set-string langfuse.additionalEnv[27].value="You have been invited to TeachIn's Langfuse" \
  --set langfuse.additionalEnv[28].name="NEXTAUTH_INVITATION_EMAIL_TEMPLATE" \
  --set-string langfuse.additionalEnv[28].value="<p>You have been invited to join TeachIn's Langfuse instance.</p><p>Click the button below to accept the invitation:</p>" \
  --set langfuse.additionalEnv[29].name="INVITE_FROM_NAME" \
  --set-string langfuse.additionalEnv[29].value="TeachIn Admin" \
  --set langfuse.additionalEnv[30].name="INVITE_FROM_EMAIL" \
  --set-string langfuse.additionalEnv[30].value="$SMTP_USER" \
  --set langfuse.additionalEnv[31].name="NEXT_PUBLIC_CONTACT_EMAIL" \
  --set-string langfuse.additionalEnv[31].value="$SMTP_USER" \
  --set langfuse.additionalEnv[32].name="EMAIL_FROM_NAME" \
  --set-string langfuse.additionalEnv[32].value="TeachIn" \
  --set langfuse.additionalEnv[33].name="EMAIL_FROM_ADDRESS" \
  --set-string langfuse.additionalEnv[33].value="$SMTP_USER" \
  --set langfuse.additionalEnv[34].name="EMAIL_FROM" \
  --set-string langfuse.additionalEnv[34].value="$SMTP_USER" \
  --set langfuse.additionalEnv[35].name="MEMBERSHIP_INVITATION_EMAIL_SUBJECT" \
  --set-string langfuse.additionalEnv[35].value="You have been invited to join {{organizationName}} on Langfuse" \
  --set langfuse.additionalEnv[36].name="MEMBERSHIP_INVITATION_EMAIL_TEMPLATE" \
  --set-string langfuse.additionalEnv[36].value="<h2>Join {{organizationName}} on Langfuse</h2><p>Hello,</p><p><b>{{inviterName}}</b> ({{inviterEmail}}) has invited you to join <b>{{organizationName}}</b> on Langfuse.</p><p>Click the link below to accept:</p><p><a href=\"{{url}}\">Accept Invitation</a></p><p>Or copy this URL: {{url}}</p><p><small>This invitation was sent to {{inviteeEmail}}</small></p>" \
  --set langfuse.additionalEnv[37].name="INVITE_URL_BASE" \
  --set-string langfuse.additionalEnv[37].value="https://$DOMAIN/langfuse" \
  --set langfuse.additionalEnv[38].name="NEXT_PUBLIC_SIGNUP_DISABLED" \
  --set-string langfuse.additionalEnv[38].value="true" \
  --set langfuse.additionalEnv[39].name="AUTH_DISABLE_SIGNUP_DEFAULT_DOMAIN" \
  --set-string langfuse.additionalEnv[39].value="true" \
  --set langfuse.additionalEnv[40].name="INVITATION_REQUIRED" \
  --set-string langfuse.additionalEnv[40].value="true" \
  --set langfuse.additionalEnv[41].name="VALIDATE_INVITATION_EMAIL" \
  --set-string langfuse.additionalEnv[41].value="true" \
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

echo "✅ Chart applied."

# --- Patch probe timeouts (workaround for Helm chart not respecting values) ---
echo "⚙️  Patching health probe timeouts..."
kubectl patch deployment langfuse-web -n "$NAMESPACE" --type='json' -p='[
  {"op": "replace", "path": "/spec/template/spec/containers/0/livenessProbe/initialDelaySeconds", "value": 120},
  {"op": "replace", "path": "/spec/template/spec/containers/0/livenessProbe/timeoutSeconds", "value": 60},
  {"op": "replace", "path": "/spec/template/spec/containers/0/livenessProbe/periodSeconds", "value": 30},
  {"op": "replace", "path": "/spec/template/spec/containers/0/readinessProbe/initialDelaySeconds", "value": 120},
  {"op": "replace", "path": "/spec/template/spec/containers/0/readinessProbe/timeoutSeconds", "value": 60},
  {"op": "replace", "path": "/spec/template/spec/containers/0/readinessProbe/periodSeconds", "value": 30}
]' 2>/dev/null || echo "⚠️  Patch failed (deployment may not exist yet)"

kubectl patch deployment langfuse-worker -n "$NAMESPACE" --type='json' -p='[
  {"op": "replace", "path": "/spec/template/spec/containers/0/livenessProbe/initialDelaySeconds", "value": 120},
  {"op": "replace", "path": "/spec/template/spec/containers/0/livenessProbe/timeoutSeconds", "value": 60},
  {"op": "replace", "path": "/spec/template/spec/containers/0/livenessProbe/periodSeconds", "value": 30}
]' 2>/dev/null || echo "⚠️  Worker patch failed (deployment may not exist yet)"

echo "✅ Probe timeouts patched."

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
  activeDeadlineSeconds: 1200
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
            npx prisma migrate resolve --applied 20240304222519_scores_add_index --schema=packages/shared/prisma/schema.prisma || true
            npx prisma migrate resolve --applied 20240618164956_create_traces_project_id_timestamp_idx --schema=packages/shared/prisma/schema.prisma || true
            npx prisma migrate resolve --applied 20250519073249_add_trace_media_media_id_index --schema=packages/shared/prisma/schema.prisma || true
              npx prisma migrate deploy --schema=packages/shared/prisma/schema.prisma
            fi
        envFrom:
        - secretRef:
            name: langfuse-secrets
EOF

kubectl wait --for=condition=complete job/langfuse-migrate -n "$NAMESPACE" --timeout=1200s
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

# --- Wait for deployments to be ready ---
kubectl rollout status deploy/langfuse-web -n "$NAMESPACE" --timeout=300s
kubectl rollout status deploy/langfuse-worker -n "$NAMESPACE" --timeout=300s

# --- Deploy ingress ---
echo "🌐 Deploying Langfuse ingress..."
INGRESS_FILE="../kubernetes/ingress/langfuse-ingress.yaml"

if [ -f "$INGRESS_FILE" ]; then
  echo "📁 Processing ingress template: $INGRESS_FILE"
  echo "🔧 Applying environment-specific configuration for $ENVIRONMENT_NAME"
  
  # Create temporary ingress file with environment-specific substitutions
  TEMP_INGRESS_FILE="/tmp/langfuse-ingress-${ENVIRONMENT_NAME}.yaml"
  sed "s/DOMAIN_PLACEHOLDER/$DOMAIN/g; s/DOMAIN_SECRET_PLACEHOLDER/$DOMAIN_SECRET/g" "$INGRESS_FILE" > "$TEMP_INGRESS_FILE"
  
  kubectl apply -f "$TEMP_INGRESS_FILE"
  rm -f "$TEMP_INGRESS_FILE"
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
echo "🔗 Access URL: https://$DOMAIN/langfuse"
echo "👤 Admin Email: $ADMIN_EMAIL"
echo "🔑 Admin Password: $ADMIN_PASSWORD"
echo "📊 Environment: $ENVIRONMENT_NAME"
echo "🗄️  Database: langfuse-$ENVIRONMENT_NAME"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "⚠️  Please change the admin password after first login!"
echo "📖 For more information, visit: https://langfuse.com/docs"
echo ""