# Cloud Architecture Documentation

## Overview

This document describes the complete cloud infrastructure for the Teachin Learning Platform deployed on Microsoft Azure. The platform uses a microservices architecture deployed on Azure Kubernetes Service (AKS) with Infrastructure as Code (IaC) managed by Terraform.

---

## Table of Contents

1. [Environment Strategy](#environment-strategy)
2. [Infrastructure Components](#infrastructure-components)
3. [Resource Sharing Model](#resource-sharing-model)
4. [Application Architecture](#application-architecture)
5. [Monitoring & Observability](#monitoring--observability)
6. [Deployment Architecture](#deployment-architecture)
7. [CI/CD Workflows](#cicd-workflows)
8. [Helm Charts & Kubernetes](#helm-charts--kubernetes)
9. [Cost Optimization](#cost-optimization)
10. [Quick Reference](#quick-reference)

---

## Environment Strategy

### Environments

The platform operates with **two primary environments**:

| Environment | Namespace | Region      | Purpose                                  | AKS Cluster | PostgreSQL | Redis     |
| ----------- | --------- | ----------- | ---------------------------------------- | ----------- | ---------- | --------- |
| **dev**     | `dev`     | West Europe | Development, testing, feature validation | Dedicated   | Dedicated  | Dedicated |
| **prod**    | `prod`    | West Europe | Production workloads                     | Dedicated   | Dedicated  | Dedicated |

### Dynamic Environments

The infrastructure supports **dynamic ephemeral environments** using the `terraform.tfvars.template` file. These temporary environments can be created for:

- Feature branch testing
- PR validation
- Integration testing
- Performance testing

Dynamic environments **share resources** from the `dev` environment to reduce costs:

- Shared AKS cluster (set `use_shared_aks = true`)
- Shared PostgreSQL server (set `use_shared_postgres = true`)
- Shared Redis cache (set `use_shared_redis = true`)

Each dynamic environment gets its own:

- Kubernetes namespace
- Service Bus namespace with queues
- SignalR service
- Database within shared PostgreSQL server
- Storage account for avatars

---

## Infrastructure Components

### Core Azure Resources

#### 1. **Azure Kubernetes Service (AKS)**

- **Purpose**: Container orchestration platform hosting all backend microservices
- **Configuration**:
  - Node pools with autoscaling
  - Spot instances enabled for non-prod environments (cost optimization)
  - Integration with Azure Key Vault for secrets
  - Dapr integration for microservices communication
  - KEDA for event-driven autoscaling

**Resource Naming Convention**:

- Dev: `aks-cluster-dev`
- Prod: `aks-cluster-prod`

#### 2. **Azure PostgreSQL Flexible Server**

- **Purpose**: Primary relational database for application data
- **Location**: Israel Central (optimized for data residency)
- **Features**:
  - High availability with zone redundancy (prod)
  - Automated backups
  - Point-in-time restore
  - Firewall rules for AKS integration

**Resource Naming Convention**: `{environment}-pg-zionet-learning`

#### 3. **Azure Service Bus**

- **Purpose**: Asynchronous message queue for inter-service communication
- **Queues**:
  - `manager-callback-queue` - Manager service callbacks
  - `engine-queue` - AI/ML processing tasks
  - `accessor-queue` - Data access operations
  - `manager-callback-session-queue` - Session-enabled callbacks

**Resource Naming Convention**: `{environment}-servicebus-zionet-learning`

#### 4. **Azure SignalR Service**

- **Purpose**: Real-time WebSocket communication for live updates
- **Use Cases**:
  - Real-time notifications
  - Live session updates
  - Student-teacher interactions

**Resource Naming Convention**: `signalr-teachin`

#### 5. **Redis (Self-Hosted on AKS)**

- **Purpose**: Distributed caching and state management
- **Deployment**: Self-hosted Redis StatefulSet on AKS (cost optimization)
- **Namespace**: `redis` (shared across all environments on the same cluster)
- **Features**:
  - In-memory data store
  - Session state management
  - Dapr state store component
  - StatefulSet with persistent volume
  - Single shared instance per AKS cluster
- **Architecture Decision**: Self-hosted instead of Azure Redis Cache to reduce costs

**Note**: Previously used Azure Redis Cache (`{environment}-redis-zionet-learning`), migrated to self-hosted for cost optimization. Azure Redis Cache code is commented out in Terraform and can be re-enabled if needed.

#### 6. **Azure Storage Account**

- **Purpose**: Blob storage for user-generated content
- **Usage**:
  - Avatar images
  - File uploads
  - Static assets

#### 7. **Azure Key Vault**

- **Purpose**: Secure secrets and certificate management
- **Secrets Stored**:
  - Database connection strings
  - Service Bus connection strings
  - Azure OpenAI API keys
  - JWT signing keys
  - Azure Speech Service keys
  - Third-party API keys (Tavily, Brevo)

**Resource Naming Convention**:

- Dev: `teachin-seo-kv`
- Prod: `prod-teachin-seo-kv`

#### 8. **Azure Static Web Apps** (Frontend)

- **Purpose**: Host React/Next.js frontend applications
- **Apps**:
  - Student portal
  - Teacher portal
  - Admin portal
- **Features**:
  - Global CDN distribution
  - Automatic HTTPS
  - Custom domains
  - CI/CD integration

#### 9. **Azure Log Analytics Workspace**

- **Purpose**: Centralized logging and diagnostics
- **Integration**:
  - Container Insights for AKS monitoring
  - Application Insights for APM
  - Diagnostic settings for all Azure resources

#### 10. **Azure Application Insights**

- **Purpose**: Application Performance Monitoring (APM)
- **Deployed For**:
  - Each Static Web App (student, teacher, admin)
  - Backend services telemetry

---

## Resource Sharing Model

### Why Resource Sharing?

Resource sharing allows multiple ephemeral environments to coexist without duplicating expensive infrastructure, significantly reducing cloud costs for testing and development.

### Shared Resources Architecture

**Master Environment**: `dev` (hosts shared resources)

```
┌─────────────────────────────────────────────────────────────┐
│                    DEV Environment (Master)                  │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  AKS Cluster: aks-cluster-dev                          │ │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │ │
│  │  │  Namespace:  │  │  Namespace:  │  │  Namespace:  │ │ │
│  │  │     dev      │  │   feature-x  │  │    pr-123    │ │ │
│  │  └──────────────┘  └──────────────┘  └──────────────┘ │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                               │
│  PostgreSQL Server: dev-pg-zionet-learning                   │
│  ┌────────────┐  ┌────────────┐  ┌────────────┐           │
│  │ Database:  │  │ Database:  │  │ Database:  │           │
│  │    dev     │  │ feature-x  │  │   pr-123   │           │
│  └────────────┘  └────────────┘  └────────────┘           │
│                                                               │
│  Redis StatefulSet (Self-Hosted on AKS)                      │
│  Namespace: redis | Host: redis.redis.svc.cluster.local     │
│  ┌────────────────────────────────────────────────────────┐ │
│  │ Shared Redis Instance (Database Index Isolation)        │ │
│  │ • dev: DB 0  • feature-x: DB 1  • pr-123: DB 2          │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                   PROD Environment (Isolated)                │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  AKS Cluster: aks-cluster-prod                         │ │
│  │  PostgreSQL Server: prod-pg-zionet-learning            │ │
│  │  Redis Cache: prod-redis-zionet-learning               │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### Configuration Variables

Control resource sharing via Terraform variables:

```hcl
# Use shared AKS cluster from dev environment
use_shared_aks = true/false

# Use shared PostgreSQL server from dev environment
use_shared_postgres = true/false

# Note: Redis is now self-hosted on AKS (redis namespace)
# Previously controlled by: use_shared_redis = true/false
# Now always deployed as shared StatefulSet on the AKS cluster
```

**Typical Configuration**:

- `dev`: All `false` (owns the resources)
- `prod`: All `false` (fully isolated)
- Dynamic environments: All `true` (shares from dev)

---

## Application Architecture

### Microservices

The backend consists of three main microservices deployed on AKS:

#### 1. **Manager Service**

- **Purpose**: API Gateway and business logic orchestration
- **Responsibilities**:
  - RESTful API endpoints
  - Authentication & authorization (JWT)
  - Session management
  - User management
  - Coordinating workflows between services
- **Port**: 5001
- **Dapr App ID**: `manager`
- **Scaling**: HTTP-based autoscaling (KEDA)

#### 2. **Engine Service**

- **Purpose**: AI/ML processing and content generation
- **Responsibilities**:
  - Azure OpenAI integration
  - AI-powered lesson generation
  - Speech synthesis (Azure Speech Service)
  - Natural language processing
- **Port**: 5003
- **Dapr App ID**: `engine`
- **Scaling**: Queue-based autoscaling (KEDA on `engine-queue`)

#### 3. **Accessor Service**

- **Purpose**: Data access layer and external integrations
- **Responsibilities**:
  - Database operations (PostgreSQL)
  - Azure Communication Service integration
  - Email sending (Brevo SMTP)
  - Avatar storage management
  - Real-time communication via SignalR
- **Port**: 5002
- **Dapr App ID**: `accessor`
- **Scaling**: Queue-based autoscaling (KEDA on `accessor-queue`)

### Communication Patterns

#### Synchronous Communication

- HTTP/REST via Manager service (API Gateway pattern)
- Dapr service-to-service invocation

#### Asynchronous Communication

- **Service Bus Queues** via Dapr pub/sub components
- **Message Flow**:
  ```
  Manager → accessor-queue → Accessor
  Manager → engine-queue → Engine
  Engine/Accessor → manager-callback-queue → Manager
  Manager → manager-callback-session-queue → Manager (session-based)
  ```

#### Real-time Communication

- Azure SignalR Service for WebSocket connections
- Bidirectional client-server communication

---

## Monitoring & Observability

### Monitoring Stack

The platform uses a comprehensive **cloud-native observability stack** deployed via Helm:

#### 1. **Grafana** (Visualization & Dashboards)

- **Namespace**: `devops-logs`
- **Version**: 10.1.2
- **Access**: `https://teachin.westeurope.cloudapp.azure.com/grafana/` (dev)
- **Access**: `https://teachin-prod.westeurope.cloudapp.azure.com/grafana/` (prod)
- **Credentials**: `admin` / `admin123` (should be rotated)
- **Features**:
  - Centralized dashboard visualization
  - Alerting with Microsoft Teams integration
  - Multi-tenant support with provisioned datasources
  - Dashboard-as-code (ConfigMaps)

#### 2. **Prometheus** (Metrics Collection)

- **Namespace**: `monitoring`
- **Chart**: `kube-prometheus-stack` v78.5.0
- **Purpose**:
  - Kubernetes cluster metrics
  - Dapr sidecar metrics
  - Application metrics (if exposed)
  - Node and pod resource usage
- **Dashboards**:
  - `dapr-metrics.json` - Dapr sidecar performance
  - `k8s-views-*.json` - Kubernetes cluster views
  - `k8s.json` - Overall cluster health

#### 3. **Loki** (Log Aggregation)

- **Namespace**: `devops-logs`
- **Chart**: `loki-stack` with Promtail
- **Purpose**:
  - Centralized log collection from all pods
  - Log querying with LogQL
  - Log retention and compression
- **Dashboards**:
  - `pod-logs.json` - Pod-level log viewer
  - `ai-dashboard.json` - AI service-specific logs
  - `auth-monitoring-dashboard.json` - Authentication logs

#### 4. **Azure Monitor** (Cloud-native metrics)

- **Purpose**: Azure resource-level monitoring
- **Monitored Resources**:
  - Application Insights (Static Web Apps)
  - PostgreSQL performance
  - Redis cache metrics
  - Service Bus queue depth
  - Key Vault access logs
- **Dashboards**:
  - `appinsights-webapp.json`
  - `postgre.json`
  - `redis.json`
  - `service-bus.json`
  - `key-vaults.json`

#### 5. **Dapr Dashboard** (Optional)

- **Purpose**: Dapr runtime debugging and visualization
- **Deployed**: Via `dapr-control-plane.sh` script
- **Version**: 1.16.3

### Alerting

**Microsoft Teams Integration**:

- Grafana alerts configured via `grafana/provisioning/alerting/`
- Notification policies route alerts to Teams channels
- Alert rules for:
  - Pod crash loops
  - High memory/CPU usage
  - Queue depth thresholds
  - Service unavailability

---

## Deployment Architecture

### Infrastructure as Code (Terraform)

#### Project Structure

```
devops/terraform/
├── main.tf                 # Root module: orchestrates all resources
├── variables.tf            # Input variables
├── outputs.tf              # Output values (connection strings, endpoints)
├── providers.tf            # Azure, Kubernetes, Helm providers
├── secrets.tf              # Kubernetes secrets creation
├── terraform.tfvars.dev    # Dev environment config
├── terraform.tfvars.prod   # Prod environment config
├── terraform.tfvars.template  # Template for dynamic environments
└── modules/
    ├── aks/                # AKS cluster module
    ├── postgresql/         # PostgreSQL server module
    ├── redis/              # Redis cache module
    ├── servicebus/         # Service Bus module
    ├── signalr/            # SignalR service module
    ├── storage/            # Storage account module
    ├── frontend/           # Static Web Apps module
    ├── log_analytics/      # Log Analytics workspace module
    ├── monitoring/         # Application Insights module
    ├── clustersecretstore/ # External Secrets Operator module
    └── k8s_manifests/      # Kubernetes manifests module
```

#### Terraform Workflow

1. **Initialize**: `terraform init`
2. **Plan**: `terraform plan -var-file="terraform.tfvars.dev"`
3. **Apply**: `terraform apply -var-file="terraform.tfvars.dev"`
4. **Post-Apply**: Run `./start-cloud.sh` to deploy Helm charts

#### Key Terraform Features

- **Conditional Resource Creation**: Uses `count` to create/skip resources based on sharing flags
- **Data Sources**: References existing resources (shared AKS, PostgreSQL)
- **Dynamic Namespace Management**: Creates Kubernetes namespaces per environment
- **Secret Management**: Automatically creates Kubernetes secrets from Key Vault
- **External Secrets Operator**: Syncs secrets from Azure Key Vault to Kubernetes

---

## Helm Charts & Kubernetes

### Helm Chart Architecture

The application is deployed using a **custom Helm chart** located in `devops/kubernetes/charts/`.

#### Chart Structure

```
charts/
├── Chart.yaml              # Chart metadata
├── values.yaml             # Default values
├── values.dev.yaml         # Dev environment overrides
├── values.prod.yaml        # Prod environment overrides
├── values.template.yaml    # Template for dynamic environments
└── templates/
    ├── _helpers.tpl        # Template helpers
    ├── accessor-deployment.yaml
    ├── engine-deployment.yaml
    ├── manager-deployment.yaml
    ├── cronjob-refresh-sessions.yaml
    ├── cronjob-stats.yaml
    ├── dapr/               # Dapr components
    │   ├── dapr-config.yaml
    │   ├── statestore.yaml  (Redis state store)
    │   ├── secretstore.yaml (Azure Key Vault)
    │   ├── *-queue-in.yaml  (Pub/sub subscribers)
    │   └── *-queue-out.yaml (Pub/sub publishers)
    ├── keda/               # KEDA autoscaling
    │   ├── accessor-scaledobject.yaml
    │   ├── engine-scaledobject.yaml
    │   └── manager-scaleobject-http.yaml
    ├── ingress/            # Ingress rules
    │   └── manager-ingress.yaml
    ├── kv/                 # External Secrets
    │   ├── externalsecret-postgres.yaml
    │   ├── externalsecret-servicebus.yaml
    │   ├── externalsecret-redis.yaml
    │   ├── externalsecret-signalr.yaml
    │   ├── externalsecret-avatars.yaml
    │   ├── externalsecret-langfuse.yaml
    │   └── externalsecrets-appsettings.yaml
    └── redis/              # Redis StatefulSet (optional)
        ├── redis-statefulset.yaml
        ├── redis-service.yaml
        ├── redis-pvc.yaml
        └── redis-secret.yaml
```

#### Key Helm Values

**Global Configuration**:

```yaml
global:
  environment: "dev" # Environment tag
  dockerRegistry: "snir1551" # Docker Hub org
  imagePullPolicy: IfNotPresent
  spot:
    enabled: true # Use spot instances (cost optimization)
  dapr:
    enabled: true
    configName: dapr-config
```

**Service Configuration**:

- Resource requests/limits
- Environment variables
- Secret references
- Dapr annotations
- Autoscaling parameters

### Dapr Integration

**Dapr (Distributed Application Runtime)** is deployed as sidecars to each microservice pod.

#### Dapr Components

1. **State Store** (`statestore.yaml`): Redis-backed distributed state
2. **Secret Store** (`secretstore.yaml`): Azure Key Vault integration
3. **Pub/Sub** (`*-queue-*.yaml`): Service Bus message queues
4. **Configuration** (`dapr-config.yaml`): Tracing and observability settings

#### Dapr Resource Optimization

```yaml
# Reduced Dapr sidecar resources based on actual usage
requests:
  cpu: "25m" # Reduced from 100m (0.1% actual usage)
  memory: "64Mi" # Reduced from 128Mi (45MiB actual usage)
limits:
  cpu: "200m"
  memory: "256Mi"
```

### KEDA Autoscaling

**KEDA (Kubernetes Event-Driven Autoscaling)** enables automatic scaling based on external metrics.

#### Autoscaling Strategies

1. **Accessor Service**: Scales based on `accessor-queue` depth (0-1 replicas)
2. **Engine Service**: Scales based on `engine-queue` depth (0-1 replicas)
3. **Manager Service**: HTTP-based scaling using KEDA HTTP Add-on (1-10 replicas)

**Scale-to-Zero**: Accessor and Engine can scale to 0 replicas when idle, saving costs.

### CronJobs

#### 1. **Stats Ping CronJob**

- **Schedule**: Every 5 minutes
- **Purpose**: Health check / statistics collection endpoint
- **Enabled**: `false` by default (safe)

#### 2. **Session Cleanup CronJob**

- **Schedule**: Daily at 02:30 AM (Israel Time)
- **Purpose**: Clean up stale user sessions
- **Enabled**: `false` by default

---

## CI/CD Workflows

All CI/CD workflows are defined in `.github/workflows/`.

### Workflow Categories

#### 1. **Infrastructure Workflows**

##### `terraform-apply.yaml`

- **Purpose**: Apply infrastructure changes via Terraform
- **Trigger**: Manual (workflow_dispatch)
- **Inputs**:
  - Environment (dev/prod)
  - Configuration file
- **Steps**:
  1. Checkout code
  2. Azure login
  3. Terraform init
  4. Terraform plan
  5. Terraform apply
  6. Run `start-cloud.sh` to deploy Helm charts

##### `terraform-destroy.yaml`

- **Purpose**: Destroy environment infrastructure
- **Trigger**: Manual with confirmation
- **Safety**: Requires typing environment name to confirm

#### 2. **Application Deployment Workflows**

##### `full-cicd.yaml` (Complete Pipeline)

- **Purpose**: Full CI/CD from build to deployment
- **Trigger**: Manual (workflow_dispatch)
- **Inputs**: Environment selection
- **Steps**:
  1. **Build & Push Images** (`build-and-push-images.yaml`)
     - Build Docker images for Manager, Accessor, Engine
     - Push to Docker Hub (snir1551 org)
  2. **Terraform Apply** (`terraform-apply.yaml`)
     - Provision/update infrastructure
  3. **Deploy to AKS** (`aks-helmcharts.yaml`)
     - Install/upgrade Helm charts
     - Apply Kubernetes manifests
  4. **Deploy Monitoring** (`aks-helmcharts.yaml`)
     - Dapr control plane
     - Grafana
     - Prometheus
     - Loki
     - KEDA

**Duration**: ~15-20 minutes

##### `update-images.yaml` (Fast Deployment)

- **Purpose**: Quick application updates without infrastructure changes
- **Trigger**: Manual (workflow_dispatch)
- **Steps**:
  1. Build & push Docker images
  2. Update image tags in Helm values
  3. Restart deployments to pull new images
- **Duration**: ~5-8 minutes

**When to Use**:

- Application code changes
- Bug fixes
- Feature updates
- No infrastructure changes required

#### 3. **AKS Management Workflows**

##### `aks-schedule.yaml` (Automated Start/Stop)

- **Purpose**: Cost optimization via scheduled cluster start/stop
- **Trigger**: Cron schedule
- **Schedules**:
  - **START**: 8:00 AM Israel Time (5:00 UTC), Sunday-Thursday
  - **STOP**: 7:30 PM Israel Time (16:30 UTC), Sunday-Thursday + Saturday
- **Features**:
  - Timezone-aware scheduling
  - Status verification
  - Operation timing
  - Slack/Teams notifications (optional)

##### `aks-kubectl-apply.yaml` (Toggle Cluster)

- **Purpose**: Manual cluster start/stop
- **Trigger**: Manual with confirmation
- **Inputs**: Action (START/STOP)
- **Safety**: Requires typing action name to confirm
- **Duration**: ~2-15 minutes depending on action

#### 4. **Frontend Workflows**

##### `frontend-pipeline.yaml`

- **Purpose**: Build and deploy Static Web Apps
- **Trigger**: Push to main/PR
- **Steps**:
  1. Build React/Next.js apps (Nx monorepo)
  2. Deploy to Azure Static Web Apps
  3. Update Application Insights
- **Apps**: Student, Teacher, Admin portals

##### `frontend-tests.yaml`

- **Purpose**: Run frontend tests
- **Trigger**: PR, manual
- **Tests**: Unit tests, E2E tests

#### 5. **Testing Workflows**

##### `integration-tests.yml`

- **Purpose**: Run backend integration tests
- **Trigger**: Manual, PR
- **Requirements**: Running AKS environment

##### `run-component-tests.yaml`

- **Purpose**: Run component-level tests
- **Trigger**: PR, manual

##### `unit-tests-selective.yml`

- **Purpose**: Selective unit tests based on changed files
- **Trigger**: PR

#### 6. **Maintenance Workflows**

##### `acr-purge.yml`

- **Purpose**: Clean up old container images
- **Trigger**: Scheduled (weekly)

##### `clear-postgres.yml`

- **Purpose**: Database cleanup for test environments
- **Trigger**: Manual

##### `langfuse-user-management.yaml`

- **Purpose**: Manage Langfuse observability users
- **Trigger**: Manual

##### `lock-environment.yaml`

- **Purpose**: Prevent concurrent deployments
- **Trigger**: Called by other workflows

### Workflow Best Practices

1. **Environment Protection**: Use GitHub environment secrets
2. **Approval Gates**: Production deployments require manual approval
3. **Rollback Strategy**: Keep previous Helm release for quick rollback
4. **Notifications**: Integrate with Teams/Slack for deployment status
5. **Lock Mechanism**: Prevent concurrent deployments to same environment

---

## Redis Architecture (Self-Hosted on AKS)

### Why Self-Hosted Redis?

**Architectural Decision**: The platform uses a **self-hosted Redis StatefulSet** deployed on AKS instead of Azure Redis Cache to significantly reduce costs.

#### Cost Comparison

- **Azure Redis Cache (Basic C0)**: ~$16/month per instance
- **Self-Hosted Redis on AKS**: Shared infrastructure, negligible additional cost
- **Savings**: ~$192/year for dev environment, more for multiple ephemeral environments

### Deployment Architecture

#### Redis StatefulSet Configuration

```yaml
Namespace: redis (shared)
Service: redis.redis.svc.cluster.local:6379
Image: redis:7-alpine
Replicas: 1 (StatefulSet)
Persistence: 8Gi PVC (ReadWriteOnce)
Resources:
  requests:
    memory: "256Mi"
    cpu: "100m"
  limits:
    memory: "512Mi"
    cpu: "500m"
```

#### Multi-Tenancy via Database Indexes

Since Redis is shared across all environments in the same AKS cluster, **database index isolation** is used:

```yaml
Environments on aks-cluster-dev:
  dev: redis://redis.redis.svc.cluster.local:6379/0
  feature-x: redis://redis.redis.svc.cluster.local:6379/1
  pr-123: redis://redis.redis.svc.cluster.local:6379/2

Environments on aks-cluster-prod:
  prod: redis://redis.redis.svc.cluster.local:6379/0
```

**Key Points**:

- Each environment gets a unique Redis database index (0-15 supported)
- Complete logical isolation between environments
- Single Redis instance per AKS cluster reduces memory overhead
- Environments on different AKS clusters have separate Redis instances

#### Kubernetes Resources

The Redis deployment consists of:

1. **redis-statefulset.yaml**: Main Redis deployment with persistent storage
2. **redis-service.yaml**: ClusterIP service exposing Redis internally
3. **redis-pvc.yaml**: PersistentVolumeClaim for data persistence
4. **redis-secret.yaml**: Secret containing Redis password

#### Secret Management

**Password Generation**:

- Generated once during first deployment: `openssl rand -base64 32 | tr -d "=+/" | cut -c1-25`
- Stored in `redis-secret` in `redis` namespace
- Copied to each application namespace for Dapr component access
- Reused across deployments (idempotent)

**Secret Propagation**:

```bash
# Master secret in redis namespace
kubectl get secret redis-secret -n redis

# Copied to each app namespace
kubectl get secret redis-secret -n dev
kubectl get secret redis-secret -n feature-x
```

### Helm Deployment Process

#### CI/CD Workflow Integration

From `aks-helmcharts.yaml` workflow:

```yaml
Steps: 1. Create redis namespace
  2. Generate/retrieve Redis password
  3. Deploy Redis StatefulSet (via helm template with --show-only)
  4. Wait for Redis pod to be ready
  5. Copy redis-secret to target namespace
  6. Deploy application with Dapr components
```

**Important**: Redis resources are rendered separately using `helm template --show-only` to avoid Helm release conflicts.

#### Dapr State Store Configuration

Each environment's Dapr state store component (`statestore.yaml`) points to the shared Redis:

```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: statestore
  namespace: { { .Values.global.environment } }
spec:
  type: state.redis
  version: v1
  metadata:
    - name: redisHost
      value: "redis.redis.svc.cluster.local:6379"
    - name: redisPassword
      secretKeyRef:
        name: redis-secret
        key: redis-password
    - name: redisDB
      value: "{{ .Values.redis.databaseIndex }}" # 0, 1, 2, etc.
```

### Migration Path

#### Reverting to Azure Redis Cache (if needed)

If you need to switch back to Azure Redis Cache:

1. **Uncomment Terraform Redis Module** (`devops/terraform/main.tf`):

   ```hcl
   module "redis" {
     count  = var.use_shared_redis ? 0 : 1
     source = "./modules/redis"
     # ...
   }
   ```

2. **Update Helm Values**:

   ```yaml
   redis:
     useAzureRedis: true
     # Remove: password, databaseIndex
   ```

3. **Remove Self-Hosted Deployment Steps** from `aks-helmcharts.yaml`:

   - Comment out "Create Redis namespace"
   - Comment out "Generate Redis password"
   - Comment out "Deploy shared Redis instance"

4. **Update External Secrets**: Redis connection string will come from Azure Key Vault

### Monitoring & Operations

#### Health Checks

```bash
# Check Redis pod status
kubectl get pods -n redis -l app=redis

# View Redis logs
kubectl logs -n redis -l app=redis -f

# Test Redis connectivity from a pod
kubectl run redis-test --rm -it --image=redis:alpine -- redis-cli -h redis.redis.svc.cluster.local -a <password> ping
```

#### Persistence

- **PVC Size**: 8Gi (adjust based on usage)
- **Storage Class**: Default (cluster-dependent)
- **Data Retention**: Survives pod restarts
- **Backup**: Not automated (ephemeral data acceptable)

#### Performance Considerations

**Pros**:

- Low latency (in-cluster communication)
- No additional network hops
- Reduced Azure egress costs

**Cons**:

- Limited to single instance (no clustering)
- Manual scaling required
- No built-in Azure monitoring

**Recommendation**: Self-hosted Redis is suitable for:

- Development/testing environments
- Low-to-medium traffic workloads
- Ephemeral state storage (Dapr state)

For production high-availability scenarios, consider Azure Redis Cache with clustering.

---

## Cost Optimization

### Strategies

#### 1. **Spot Instances (AKS Node Pools)**

- **Savings**: Up to 90% compared to standard VMs
- **Configuration**: `enable_spot_nodes = true` (non-prod)
- **Impact**: Some interruptions acceptable in dev/test

#### 2. **Resource Sharing**

- **Savings**: 60-70% for dynamic environments
- **Strategy**: Share AKS, PostgreSQL, Redis across test environments
- **Isolation**: Each environment gets separate namespace and database

#### 3. **Scale-to-Zero (KEDA)**

- **Savings**: Pay only when processing
- **Services**: Accessor and Engine scale to 0 when idle
- **Trigger**: Queue depth = 0 for cooldown period

#### 4. **Scheduled Cluster Start/Stop**

- **Savings**: ~65% of AKS costs
- **Schedule**: Stop non-prod clusters after business hours
- **Automation**: GitHub Actions cron workflows

#### 5. **Right-Sizing Resources**

- **CPU/Memory Requests**: Based on actual usage metrics
- **Dapr Sidecars**: Reduced from 100m/128Mi to 25m/64Mi
- **Monitoring**: Prometheus metrics guide optimization

#### 6. **Self-Hosted Redis on AKS**

- **Savings**: ~$192/year per environment (vs Azure Redis Cache)
- **Strategy**: Deploy Redis StatefulSet in shared `redis` namespace
- **Isolation**: Database index separation per environment
- **Trade-off**: No managed service features, suitable for dev/test

#### 7. **Grafana Subpath Deployment**

- **Savings**: No separate LoadBalancer needed
- **Strategy**: Deploy under existing ingress at `/grafana/`
- **Configuration**: `GF_SERVER_SERVE_FROM_SUB_PATH=true`

---

## Quick Reference

### Common Commands

#### Terraform

```bash
# Initialize
terraform init

# Plan (dev)
terraform plan -var-file="terraform.tfvars.dev"

# Apply (dev)
terraform apply -var-file="terraform.tfvars.dev"

# Create dynamic environment
terraform plan -var-file="terraform.tfvars.template" -var="environment_name=feature-x"
terraform apply -var-file="terraform.tfvars.template" -var="environment_name=feature-x"

# Destroy
terraform destroy -var-file="terraform.tfvars.dev"

# Import existing resource
terraform import -var-file="terraform.tfvars.prod" '<resource_address>' '<azure_resource_id>'
```

#### Kubernetes

```bash
# Login to Azure
az login --use-device-code

# Get AKS credentials
az aks get-credentials --resource-group dev-zionet-learning-2025 --name aks-cluster-dev --overwrite-existing

# List pods
kubectl get pods -n dev

# View logs
kubectl logs deployment/manager -n dev -f

# Restart deployment
kubectl rollout restart deployment/accessor -n dev

# Apply manifest
kubectl apply -f ./todoaccessor-deployment.yaml -n dev

# Get service external IP
kubectl get svc manager -n dev

# Port-forward for local testing
kubectl port-forward svc/manager 8080:80 -n dev
```

#### Helm

```bash
# Install/upgrade application chart
helm upgrade --install teachin-app ./charts \
  --namespace dev \
  --values ./charts/values.dev.yaml \
  --set global.environment=dev

# List releases
helm list -n dev

# Rollback to previous version
helm rollback teachin-app 1 -n dev

# Uninstall
helm uninstall teachin-app -n dev
```

### Endpoints

#### Dev Environment

- **Manager API**: `https://teachin.westeurope.cloudapp.azure.com/swagger`
- **Grafana**: `https://teachin.westeurope.cloudapp.azure.com/grafana/`
- **Langfuse**: `https://teachin.westeurope.cloudapp.azure.com/langfuse/`
- **Student Portal**: `https://{app-name}.azurestaticapps.net`
- **Teacher Portal**: `https://{app-name}.azurestaticapps.net`
- **Admin Portal**: `https://{app-name}.azurestaticapps.net`

#### Prod Environment

- **Manager API**: `https://teachin-prod.westeurope.cloudapp.azure.com/swagger`
- **Grafana**: `https://teachin-prod.westeurope.cloudapp.azure.com/grafana/`
- **Langfuse**: `https://teachin-prod.westeurope.cloudapp.azure.com/langfuse/`
- **Frontend**: Production URLs configured via Azure Static Web Apps

### Environment Variables

Key environment variables managed via Kubernetes secrets (sourced from Key Vault):

- `ServiceBus__ConnectionString`
- `ConnectionStrings__Postgres`
- `ConnectionStrings__Redis`
- `Speech__Key`
- `AZURE_OPENAI_API_KEY`
- `JWT_SECRET`
- `JWT_REFRESH_TOKEN_HASH_KEY`
- `CommunicationService__ConnectionString`
- `TAVILY_API_KEY`
- `BREVO_*` (Email service)
- `Langfuse__*` (AI observability)

### Resource Groups

| Resource Group               | Purpose              | Resources                                                       |
| ---------------------------- | -------------------- | --------------------------------------------------------------- |
| `dev-zionet-learning-2025`   | Dev environment      | AKS, PostgreSQL, Service Bus, SignalR, Storage, Key Vault       |
| `prod-zionet-learning-2025`  | Prod environment     | AKS, PostgreSQL, Service Bus, SignalR, Storage, Key Vault       |
| `{env}-zionet-learning-2025` | Dynamic environments | Service Bus, SignalR, Storage (shares AKS, PostgreSQL from dev) |

**Note**: Redis is deployed as a self-hosted StatefulSet on AKS in the `redis` namespace, not as a separate Azure resource.

### Kubernetes Namespaces

| Namespace       | Purpose                                                    |
| --------------- | ---------------------------------------------------------- |
| `redis`         | Self-hosted Redis StatefulSet (shared across environments) |
| `dev`           | Dev environment application workloads                      |
| `prod`          | Prod environment application workloads                     |
| `{env}`         | Dynamic environment application workloads                  |
| `dapr-system`   | Dapr control plane                                         |
| `monitoring`    | Prometheus stack                                           |
| `devops-logs`   | Grafana, Loki                                              |
| `devops-tools`  | Langfuse, utility services                                 |
| `keda`          | KEDA autoscaling                                           |
| `ingress-nginx` | NGINX Ingress Controller                                   |

---

## Architecture Diagrams

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        Azure Cloud                              │
│                                                                   │
│  ┌────────────────────────────────────────────────────────┐    │
│  │              Azure Kubernetes Service (AKS)             │    │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │    │
│  │  │   Manager    │  │   Accessor   │  │    Engine    │ │    │
│  │  │   Service    │  │   Service    │  │   Service    │ │    │
│  │  │  (API GW)    │  │ (Data Layer) │  │  (AI/ML)     │ │    │
│  │  │              │  │              │  │              │ │    │
│  │  │ ┌──────────┐ │  │ ┌──────────┐ │  │ ┌──────────┐ │ │    │
│  │  │ │   Dapr   │ │  │ │   Dapr   │ │  │ │   Dapr   │ │ │    │
│  │  │ │ Sidecar  │ │  │ │ Sidecar  │ │  │ │ Sidecar  │ │ │    │
│  │  │ └──────────┘ │  │ └──────────┘ │  │ └──────────┘ │ │    │
│  │  └──────────────┘  └──────────────┘  └──────────────┘ │    │
│  │                                                          │    │
│  │  ┌────────────────────────────────────────────────────┐│    │
│  │  │        Monitoring Stack (Prometheus/Loki)          ││    │
│  │  └────────────────────────────────────────────────────┘│    │
│  └────────────────────────────────────────────────────────┘    │
│                                                                   │
│  ┌──────────────┐  ┌──────────────┐                              │
│  │  PostgreSQL  │  │  Service Bus │                              │
│  │   Flexible   │  │    Queues    │                              │
│  │    Server    │  └──────────────┘                              │
│  └──────────────┘                                                │
│                                                                   │
│  ┌────────────────────────────────────────────────────────┐    │
│  │  Redis StatefulSet (Self-Hosted on AKS)                │    │
│  │  Namespace: redis | Shared across all environments     │    │
│  └────────────────────────────────────────────────────────┘    │
│                                                                   │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │   SignalR    │  │  Key Vault   │  │   Storage    │          │
│  │   Service    │  │   (Secrets)  │  │   Account    │          │
│  └──────────────┘  └──────────────┘  └──────────────┘          │
│                                                                   │
│  ┌────────────────────────────────────────────────────────┐    │
│  │           Static Web Apps (Frontend)                    │    │
│  │  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐   │    │
│  │  │   Student    │ │   Teacher    │ │    Admin     │   │    │
│  │  │    Portal    │ │    Portal    │ │    Portal    │   │    │
│  │  └──────────────┘ └──────────────┘ └──────────────┘   │    │
│  └────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
```

### Microservices Communication Flow

```
┌─────────────┐
│   Client    │
│ (Browser)   │
└──────┬──────┘
       │ HTTPS
       ↓
┌──────────────────────┐
│   NGINX Ingress      │
│   Controller         │
└──────┬───────────────┘
       │
       ↓
┌──────────────────────┐      ┌────────────────┐
│  Manager Service     │      │  Service Bus   │
│  (API Gateway)       │◄────►│  Queues        │
└──┬───────────────┬───┘      └────────────────┘
   │               │
   │ Dapr          │ Dapr
   │ Service       │ Pub/Sub
   │ Invocation    │
   ↓               ↓
┌──────────────┐ ┌──────────────┐
│  Accessor    │ │   Engine     │
│  Service     │ │   Service    │
└──┬───────────┘ └──┬───────────┘
   │                 │
   │                 │ Azure OpenAI API
   ↓                 ↓
┌──────────────┐ ┌──────────────┐
│ PostgreSQL   │ │ Azure Speech │
│  Database    │ │   Service    │
└──────────────┘ └──────────────┘
```

---

## Summary

This cloud architecture provides:

✅ **Scalability**: Auto-scaling with KEDA, spot instances  
✅ **Resilience**: High availability, health checks, retries  
✅ **Observability**: Full-stack monitoring with Grafana/Prometheus/Loki  
✅ **Security**: Key Vault integration, managed identities, network policies  
✅ **Cost Efficiency**: Resource sharing, scale-to-zero, scheduled shutdown  
✅ **Developer Experience**: IaC with Terraform, GitOps with Helm, CI/CD automation  
✅ **Flexibility**: Dynamic environment creation, multi-tenancy support

---

## Document Usage

**This document should be referenced when**:

- Planning infrastructure changes
- Creating new environments
- Troubleshooting deployment issues
- Onboarding new team members
- Explaining architecture to stakeholders
- Discussing with AI tools for automation/modifications

**Keep this document updated** as the infrastructure evolves.

---

_Last Updated: December 7, 2025_  
_Maintained by: DevOps Team_
