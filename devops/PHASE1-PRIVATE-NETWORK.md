# Phase 1: Private Network for AKS - Implementation Summary

## What Was Built

**Objective**: Enable AKS to run on a private VNet (not internet-facing) with a delegated subnet reserved for PostgreSQL.

### New Module: `modules/network/`

A reusable Terraform module that creates:

1. **Main VNet** (`azurerm_virtual_network`)

   - Single VNet per environment in same region (West Europe)
   - Environment-specific CIDR (dev: `10.10.0.0/16`, prod: `10.20.0.0/16`)
   - Extensible for future phase 2 (separate DB VNet + peering)

2. **AKS Subnet** (`azurerm_subnet`)

   - Hosts AKS nodes and pods
   - Service endpoints enabled: ACR, Storage, Key Vault
   - Attached NSG for security

3. **Database Subnet** (optional, delegated)

   - Reserved exclusively for PostgreSQL Flexible Server
   - Only created when `include_db_subnet = true` (phase 1)
   - Will be moved to separate DB VNet in phase 2

4. **Network Security Group (NSG)**
   - Allows outbound internet (pods can reach external APIs)
   - Allows inbound from subnet CIDR (node-to-node communication)
   - Ready for additional rules in phase 2 (e.g., peering, ingress)

### Integration Points

#### Root Module (`terraform/main.tf`)

- Network module called when `use_shared_aks = false`
- Outputs (`vnet_id`, `aks_subnet_id`, `db_subnet_id`) passed to AKS module
- Depends on resource group; AKS depends on network

#### AKS Module (`modules/aks/`)

- Nodes placed on `var.aks_subnet_id` (private subnet)
- Network plugin: **Azure CNI** (production-grade)
- Network policy: **Azure** (built-in Kubernetes network policies)
- **Private cluster enabled**:
  - API server not exposed to public internet
  - Private DNS zone created automatically (unless custom zone provided)
  - Access via private endpoint only
  - Requires VPN, bastion, or same-VNet jumpbox to reach API

#### Variables & Tfvars

- **Root variables**: `vnet_name`, `vnet_address_space`, `aks_subnet_prefix`, `include_db_subnet`, `enable_private_cluster`, `private_dns_zone_id`
- **Dev tfvars**: VNet `10.10.0.0/16`, AKS subnet `10.10.1.0/24`, DB subnet `10.10.2.0/24`
- **Prod tfvars**: VNet `10.20.0.0/16`, AKS subnet `10.20.1.0/24`, DB subnet `10.20.2.0/24`
- **Template tfvars**: VNet `10.30.0.0/16` (for dynamic environments)

---

## Architecture

```
┌─────────────────────────────────────────────┐
│      Resource Group: {env}-zionet-learning  │
│                                               │
│  ┌───────────────────────────────────────┐  │
│  │ VNet: teachin-aks-vnet-{env}          │  │
│  │ CIDR: 10.{10|20|30}.0.0/16             │  │
│  │                                         │  │
│  │  ┌─────────────────────────────────┐  │  │
│  │  │ AKS Subnet: 10.{10|20|30}.1.0/24 │  │  │
│  │  │ (AKS nodes + pods)              │  │  │
│  │  │ NSG: Allow outbound, inbound    │  │  │
│  │  │ Service Endpoints: ACR, KV, SA  │  │  │
│  │  └─────────────────────────────────┘  │  │
│  │                                         │  │
│  │  ┌─────────────────────────────────┐  │  │
│  │  │ DB Subnet: 10.{10|20|30}.2.0/24  │  │  │
│  │  │ (PostgreSQL - phase 1)          │  │  │
│  │  │ Delegated to PostgreSQL         │  │  │
│  │  └─────────────────────────────────┘  │  │
│  └───────────────────────────────────────┘  │
│                                               │
│   AKS Cluster (Private)                      │
│   └─ API Server: Private endpoint            │
│   └─ Nodes: All in AKS subnet               │
│   └─ Network Plugin: Azure CNI              │
│   └─ Network Policy: Azure                  │
└─────────────────────────────────────────────┘

Phase 2 (Future):
┌──────────────────────────────────┐
│ Separate DB VNet (different region)
│ CIDR: 10.x.0.0/16
│ └─ DB Subnet: delegated
│
VNet Peering (bidirectional)
├─ Main VNet → DB VNet
└─ DB VNet → Main VNet
```

---

## Key Features

### Security

- ✅ AKS API server is private (no public exposure)
- ✅ Nodes only accessible from within VNet
- ✅ NSG restricts traffic (outbound allowed, inbound restricted)
- ✅ Service endpoints for Azure services (no internet hops)

### Cost

- ✅ Single VNet with two subnets (cheaper than multi-VNet setup)
- ✅ No DDoS protection (can be added later if needed)
- ✅ NSG doesn't add extra cost

### Flexibility

- ✅ Database subnet is optional (`include_db_subnet = true/false`)
- ✅ Private DNS zone can be custom or system-managed
- ✅ Ready for phase 2: just add second VNet and peering

### Operations

- ✅ To reach private AKS API, use:
  - Azure CLI with managed identity
  - Kubectl from jumpbox/bastion on same VNet
  - Port-forward via private connection
- ✅ Monitoring/logging agents still reach Log Analytics (via service endpoints + private link)

---

## Phase 1 Validation

Run a plan to see what will be created:

```bash
cd devops/terraform

# Dev environment
terraform plan -var-file="terraform.tfvars.dev"

# Check for errors, then apply
terraform apply -var-file="terraform.tfvars.dev"
```

**Expected resources created**:

- 1 Virtual Network (VNet)
- 2 Subnets (AKS + DB)
- 1 Network Security Group (NSG)
- 2 NSG Rules
- 1 NSG-Subnet Association
- 1 AKS Cluster (private, on the subnet)
- 1 User-Assigned Identity (for AKS)

---

## Phase 2 Plan (Future)

When ready to add a separate database VNet:

1. **Create `modules/network_db/`** (similar to `modules/network/`, but for DB VNet in different region)
2. **Set `include_db_subnet = false`** in tfvars (to skip DB subnet in main VNet)
3. **Add peering** between main VNet and DB VNet in root module
4. **Update PostgreSQL module** to use delegated DB subnet from DB VNet
5. **Update routing** if using custom UDRs for outbound traffic

Example phase 2 tfvars addition:

```hcl
# Phase 2: Separate Database VNet
include_db_subnet      = false  # Remove DB subnet from main VNet

# Create separate DB VNet (new module)
db_vnet_name           = "teachin-db-vnet-prod"
db_vnet_address_space  = ["10.100.0.0/16"]
db_vnet_location       = "Israel Central"  # Different region
db_subnet_name         = "db-subnet-prod"
db_subnet_prefix       = "10.100.1.0/24"

# Wire PostgreSQL module to use DB subnet
delegated_subnet_id = module.network_db[0].db_subnet_id
```

---

## Files Modified/Created

### Created

- `modules/network/main.tf` — VNet, subnets, NSG resources
- `modules/network/variables.tf` — Input variables
- `modules/network/outputs.tf` — Output values

### Updated

- `variables.tf` — Added networking vars (vnet_name, aks_subnet_prefix, etc.)
- `main.tf` — Added network module call and wire to AKS
- `modules/aks/variables.tf` — Added aks_subnet_id, enable_private_cluster, private_dns_zone_id
- `modules/aks/main.tf` — Wire subnet ID to nodes, add network_profile, enable private cluster
- `terraform.tfvars.dev` — Added networking values for dev
- `terraform.tfvars.prod` — Added networking values for prod
- `terraform.tfvars.template` — Added networking values for dynamic environments

### Not Modified (ready for phase 2)

- PostgreSQL module — Will consume delegated_subnet_id in phase 2
- AKS module networking — Already supports both single-VNet (phase 1) and multi-VNet (phase 2)

---

## Next Steps

1. **Validate**: Run `terraform plan -var-file="terraform.tfvars.dev"` to see planned changes
2. **Review private endpoint DNS**: Ensure private DNS zone is resolvable from your network (may need VPN/bastion)
3. **Test API access**: After apply, test kubectl access from within the VNet
4. **Apply when ready**: `terraform apply -var-file="terraform.tfvars.dev"`
5. **Plan phase 2**: Design separate DB VNet (region, CIDR, peering rules)

---

## Troubleshooting

### "Private API server not reachable"

- Ensure your client (kubectl, terraform) is on same VNet or has VPN access
- Check private DNS zone resolution
- Verify NSG rules allow outbound from your client IP range

### "Pods can't reach internet"

- Check NSG allows outbound (`Allow *:* from *:* to *:*`)
- Verify outbound NAT is configured (standard load balancer provides default NAT)

### "Database subnet already delegated"

- If you get delegation conflicts in phase 2, ensure phase 1 DB subnet is removed before adding separate DB VNet
- Set `include_db_subnet = false` first, apply, then add DB VNet module

---

_Phase 1 Complete. Ready for Phase 2 planning._
