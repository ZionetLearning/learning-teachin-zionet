# 📊 Comparison: Loki vs ELK Stack vs K9s

## Features Overview

| Feature / Tool                          | 🔷 **Loki + Grafana**                          | 🟦 **ELK Stack (Elasticsearch + Kibana)**              | ⚫ **K9s**                             |
|----------------------------------------|-----------------------------------------------|--------------------------------------------------------|----------------------------------------|
| **Historical Logs**                    | ✅ Yes (custom retention)                      | ✅ Yes (long-term indexing)                            | ❌ No (ephemeral logs only)             |
| **Live Log Tailing**                   | ✅ Yes                                         | ✅ Yes                                                  | ✅ Yes                                  |
| **Full Text Search**                   | 🟡 Partial (LogQL: structured filtering)       | ✅ Yes (very powerful, supports text/regex/wildcards)   | ❌ No                                   |
| **Cross-Pod/Namespace Search**         | ✅ Yes                                         | ✅ Yes                                                  | ❌ No                                   |
| **Search Speed**                       | 🟢 Fast (if logs are well-labeled)             | 🟡 Slower with scale (depending on shard config)       | ✅ Instant (but only current logs)      |
| **Resource Usage**                     | 🟢 Light (~0.5–2 GB RAM per component)         | 🔴 Heavy (≥8 GB RAM per node, Elasticsearch intensive) | 🟢 Minimal                              |
| **Cloud Deployable**                   | ✅ Yes (AKS, EKS, GKE, etc.)                   | ✅ Yes                                                  | ❌ Local CLI only                       |
| **UI (Web)**                           | ✅ Yes (Grafana)                               | ✅ Yes (Kibana)                                         | ❌ No GUI                               |
| **Custom Dashboards**                  | ✅ Yes (Grafana)                               | ✅ Yes (Kibana)                                         | ❌ No                                   |
| **Access Control**                     | ✅ (OAuth2, Auth proxy, LDAP)                  | ✅ (X-Pack/Shield, Auth proxy)                         | ❌ None                                 |
| **Multi-user Friendly**               | ✅ Yes                                         | ✅ Yes                                                  | ❌ Single-user CLI                      |
| **Multi-cluster Support**             | ✅ Yes (via labels or Promtail config)         | ✅ Yes (custom setup)                                   | 🟡 Context switch only                  |
| **Self-Hosting Complexity**            | 🟡 Medium (Helm chart, Promtail setup)         | 🔴 High (many components, storage config, tuning)       | ✅ None                                 |
| **Learning Curve**                     | 🟡 Moderate (LogQL syntax)                     | 🔴 Steep (Indexing, shards, mappings, etc.)             | 🟢 Simple CLI (for CLI users)           |
| **Kubernetes Integration**             | ✅ Excellent (labels, namespaces, etc.)        | ✅ Good                                                 | ✅ Native                               |

---

## 💰 Cost Estimate (Monthly)

| Tool            | Infra Size         | Est. Monthly Cost | Notes |
|-----------------|--------------------|-------------------|-------|
| **Loki + Grafana** | Small AKS/GKE cluster + 20GB log volume | ~$10–$30 | Storage cost + minimal CPU |
| **ELK Stack**     | 3-node Elasticsearch + Kibana | ~$100–$300+ | High RAM/storage demand |
| **K9s**           | None              | $0                | Local tool only |

> Costs assume self-hosted deployment in the cloud (AKS, EKS, GKE, etc.).  
> SaaS (Elastic Cloud, Grafana Cloud) = **extra monthly fees**.

---

## 🧠 TL;DR Recommendations

| Use Case                              | Best Tool         |
|---------------------------------------|-------------------|
| Lightweight + Free + Long-term logs   | **Loki + Grafana** |
| Advanced full-text search & analytics | **ELK Stack**      |
| Terminal-based instant debugging      | **K9s**            |
| Logs for teams (web UI, history)      | **Loki** or **ELK**|
| No cost, no setup, quick checks       | **K9s**            |




## 💰 Real Pricing Breakdown

### **Amazon EKS (Control Plane Fee)**
- You pay **$0.10 per hour per cluster** during standard support → about **$72/month** per cluster :contentReference[oaicite:1]{index=1}
- If your cluster runs on a Kubernetes version in extended support, the fee increases to **$0.60/hour** :contentReference[oaicite:2]{index=2}

### **Worker Node Costs (Example)**
- **t3.medium EC2 instance** costs approximately **$0.0416/hour**, around **$30/month**
- Running **2 worker nodes + control plane** totals approx. **$102/month** :contentReference[oaicite:3]{index=3}

### **Grafana Loki (Cloud / SaaS Reference)**
- Grafana Cloud billing for Loki logs starts at around **$0.50 per GB ingested per month** :contentReference[oaicite:4]{index=4}

### **Self-Hosted Storage (AWS S3 Standard)**
- AWS S3 Standard tier is priced at approximately **$0.023/GB/month** for the first 50 TB :contentReference[oaicite:5]{index=5}

---

## 🧾 Example Monthly Cost Scenarios

## 💰 Azure Hosting Cost Comparison: Loki vs ELK on AKS

### 🟢 **Loki + Grafana (Self-hosted on AKS)**

#### AKS Cluster Cost
- AKS control plane is **free for one cluster per region/subscription** in free tier.  
  **No control plane charge** unless SLA is enabled.  
  :contentReference[oaicite:1]{index=1}

#### Compute (Worker Nodes)
- Example: 2 × `Standard_D4s_v3` nodes ~2–4 vCPU + RAM.
- Rough estimate: **~$50–80/month** depending on instance type.

#### Storage (Logs in Azure Blob Storage – Hot Tier)
- Azure Blob Hot tier is roughly **$0.018/GB/month** (first 50 TB) :contentReference[oaicite:2]{index=2}
- Example: ingesting ~30 GB/day → ~900 GB/month → ~$16/month

#### Total Estimated Monthly Cost
- **Compute**: ~$60  
- **Storage**: ~$16  
- **Control plane**: $0  
- **Estimated total**: **~$75/month** (depending on log volume and node sizing)

---

### 🔵 **ELK Stack (Elasticsearch + Kibana on AKS)**

#### AKS Cluster Cost
- Same as Loki: control plane free unless SLA enabled :contentReference[oaicite:3]{index=3}

#### Compute (Worker Nodes + Elasticsearch)
- Need at least **3 Elasticsearch nodes** (e.g. 8 GB+ RAM each) plus a Kibana node.
- Example: 4 nodes × `Standard_D4s_v3` → approx. **$240–320/month**

#### Storage (Persistent Disks)
- Use Azure Managed Disks (e.g. Premium SSD or Blob storage).
- Example: 1 TB storage at ~$0.10/GB/month → **~$100/month**

#### Total Estimated Monthly Cost
- **Compute**: ~$300  
- **Storage**: ~$100  
- **Control plane**: $0  
- **Estimated total**: **~$400/month** (minimum for small production use)

---

### ⚫ **K9s**

- CLI-based only. No hosting required.
- **Cost**: $0/month (local use only)

---

## 📊 Summary Table

| Tool                 | AKS Control Plane | Compute Nodes       | Storage (Blob / Disk)         | Est. Monthly Cost  |
|----------------------|-------------------|----------------------|-------------------------------|---------------------|
| **Loki + Grafana**  | **Free**          | 2 small AKS nodes (~$60) | Blob Hot (30 GB/day ≈ $16)   | **~$75**            |
| **ELK on AKS**       | **Free**          | 3–4 nodes (~$300)         | Managed Disk ~1 TB ($100)    | **~$400**           |
| **K9s**              | —                 | —                    | —                             | **$0**              |

---

### 🧠 Notes & Tips
- You only pay for nodes, storage, and networking; AKS management plane is free unless SLA is enabled. :contentReference[oaicite:4]{index=4}
- Blob storage price starts at ~$0.018/GB/month for Hot tier; costs decrease at higher volumes. :contentReference[oaicite:5]{index=5}
- These estimates assume no heavy egress traffic; data transfer may add extra cost if logs are accessed externally. :contentReference[oaicite:6]{index=6}
- Larger log volumes or longer retention will increase storage costs accordingly.

---

Choose **Loki** if you need a lightweight, low-cost, cloud-agnostic logging solution with historical search.  
Choose **ELK** if you require powerful full-text search and dashboards and can invest in higher infrastructure costs.
