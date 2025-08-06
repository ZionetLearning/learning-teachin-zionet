# CI/CD Workflows Documentation

This document provides an overview of all GitHub Actions workflows in this repository for Azure Kubernetes Service (AKS) deployment and management.

## 📋 Workflow Overview

| Workflow | Purpose | Trigger | Duration |
|----------|---------|---------|----------|
| [AKS - Full CICD](#aks---full-cicd) | Complete infrastructure + app deployment | Manual | ~15-20 min |
| [AKS - Update Images](#aks---update-images) | Updates backend images into the cloud aks | Manual | ~5-8 min |
| [AKS - Start Cluster (at 8:00)](#aks---start-cluster-morning) | Automated morning startup | Scheduled | ~5-15 min |
| [AKS - Stop Cluster (at 19:30)](#aks---stop-cluster-evening) | Automated evening shutdown | Scheduled | ~2-5 min |
| [AKS - Manual Start](#aks---manual-start) | Start AKS cluster | Manual | ~5-15 min |
| [AKS - Manual Stop](#aks---manual-stop) | Stop AKS cluster | Manual | ~2-5 min |

---

## 🚀 Main Deployment Workflows

### AKS - Full CICD
**File:** `aks-full-cicd.yml`

**Purpose:** Complete CI/CD pipeline that builds, deploys infrastructure, and deploys applications.

**What it does:**
1. **Build & Push Images** - Builds Docker images and pushes to registry
2. **Terraform Apply** - Creates/updates Azure infrastructure (AKS, databases, networking)
3. **Deploy to AKS** - Deploys applications, Dapr, monitoring stack (Grafana, Prometheus, Loki)

**Inputs:**
- `environment`: Development or Production

---

### AKS - Update Images
**File:** `update-images.yml`

**Purpose:** Fast deployment for application code changes only.

**When to use:**
- Application code updates
- Bug fixes
- Feature updates
- When infrastructure hasn't changed

**What it does:**
1. **Build & Push Images** - Builds new Docker images
2. **Restart Pods** - Restarts application pods to pull new images

**Inputs:**
- `environment`: Development or Production

**Duration:** ~5-8 minutes (much faster than full CICD)

---

## 🕒 Automated Scheduling Workflows

### AKS - Start Cluster (Morning)
**File:** `aks-start-schedule.yml`

**Purpose:** Automatically start AKS cluster each morning to save costs.

**Schedule:** 
- **Time:** 8:00 AM Israel Time (5:00 UTC)
- **Days:** Sunday-Thursday (skips Friday & Saturday)

**What it does:**
1. Checks current Israel time and day
2. Starts AKS cluster
3. Waits for cluster to be fully ready
4. Verifies cluster status

**Cost Savings:** Cluster runs only during work hours

---

### AKS - Stop Cluster (Evening)
**File:** `aks-stop-schedule.yml`

**Purpose:** Automatically stop AKS cluster each evening to save costs.

**Schedule:**
- **Time:** 7:30 PM Israel Time (16:30 UTC)  
- **Days:** Sunday-Thursday + Saturday (skips Friday evening)

**What it does:**
1. Checks current Israel time and day
2. Stops AKS cluster
3. Verifies cluster is stopped

**Cost Savings:** ~11 hours downtime per weekday + full weekends

---

## 🔧 Manual Control Workflows

### AKS - Manual Start
**File:** `aks-manual-start.yml`

**Purpose:** Manually start AKS cluster with safety confirmation.

**When to use:**
- Weekend work
- Outside scheduled hours
- Emergency access
- Testing

**Safety Features:**
- Requires typing "START" to confirm
- Shows timing information
- Waits for full cluster readiness

---

### AKS - Manual Stop
**File:** `aks-manual-stop.yml`

**Purpose:** Manually stop AKS cluster with safety confirmation.
- Requires typing "STOP" to confirm
