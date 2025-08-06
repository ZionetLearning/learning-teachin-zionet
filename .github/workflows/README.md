# CI/CD Workflows Documentation

This document provides an overview of all GitHub Actions workflows in this repository for Azure Kubernetes Service (AKS) deployment and management.

## 📋 Workflow Overview

| Workflow | Purpose | Trigger | Duration |
|----------|---------|---------|----------|
| [AKS - Full CICD](#aks---full-cicd) | Complete infrastructure + app deployment | Manual | ~15-20 min |
| [AKS - Update Images](#aks---update-images) | Quick app updates only | Manual | ~5-8 min |
| [AKS - Toggle Cluster](#aks---toggle-cluster) | Manual start/stop cluster | Manual | ~2-15 min |
| [AKS - Scheduled Start/Stop](#aks---scheduled-startstop) | Automated daily start/stop | Scheduled | ~2-15 min |

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

### AKS - Scheduled Start/Stop
**File:** `aks-schedule.yml`

**Purpose:** Automatically start/stop AKS cluster daily to save costs.

**Schedules:** 
- **START:** 8:00 AM Israel Time (5:00 UTC) - Sunday-Thursday
- **STOP:** 7:30 PM Israel Time (16:30 UTC) - Sunday-Thursday + Saturday (skips Friday evening)

**What it does:**
1. Determines if it's morning (START) or evening (STOP) schedule
2. Checks current Israel time and day
3. Starts or stops AKS cluster accordingly
4. Waits for operation to complete
5. Verifies final status

---

## 🔧 Manual Control Workflows

### AKS - Toggle Cluster
**File:** `toggle-aks.yml`

**Purpose:** Manually start or stop AKS cluster with safety confirmation.

**When to use:**
- Weekend work
- Outside scheduled hours
- Emergency access/shutdown
- Testing

**Features:**
- Dropdown selection: START or STOP
- Double confirmation: Must type action name to confirm
- Shows current cluster status
- Times the operation
- Provides next steps guidance

**Safety Features:**
- Requires typing "START" or "STOP" to confirm
- Shows timing information
- Detects if cluster already in desired state
- Clear operation summary
