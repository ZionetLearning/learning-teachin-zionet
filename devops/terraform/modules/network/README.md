# Network Module

This module creates the core network infrastructure for the learning-teachin-zionet project, implementing a hub-and-spoke network topology with proper subnet segmentation.

## Overview

The network module provisions:
- **Virtual Network (VNet)** - The main network container
- **Four dedicated subnets** with specific purposes:
  - **AKS Subnet** - For Azure Kubernetes Service nodes and pods
  - **Database Subnet** - For database services with delegation to PostgreSQL
  - **Integration Subnet** - For private endpoints and integration services
  - **Management Subnet** - For management and monitoring tools

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Virtual Network                      │
│                   (10.10.0.0/16)                      │
├─────────────────────────────────────────────────────────┤
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐      │
│  │ AKS Subnet  │  │ DB Subnet   │  │ Integration │      │
│  │10.10.1.0/24 │  │10.10.2.0/24 │  │ Subnet      │      │
│  │             │  │             │  │10.10.3.0/24 │      │
│  └─────────────┘  └─────────────┘  └─────────────┘      │
│                                                         │
│  ┌─────────────┐                                        │
│  │ Management  │                                        │
│  │ Subnet      │                                        │
│  │10.10.4.0/24 │                                        │
│  └─────────────┘                                        │
└─────────────────────────────────────────────────────────┘
```

## Features

### Security & Compliance
- ✅ **Network Segmentation** - Separate subnets for different workload types
- ✅ **Database Delegation** - Dedicated subnet with proper delegation for PostgreSQL
- ✅ **Private Endpoint Support** - Integration subnet configured for private endpoints
- ⏳ **NSGs** - Network Security Groups (coming in next iteration)
- ⏳ **Private Endpoints** - For secure service access (coming in next iteration)

### Best Practices Implemented
- **Modular Design** - Reusable and maintainable Terraform module
- **Input Validation** - CIDR validation on subnet prefixes
- **Proper Tagging** - Consistent resource tagging strategy
- **Clear Outputs** - Well-defined outputs for use in other modules

## Usage

```hcl
module "network" {
  source              = "./modules/network"
  
  # VNet Configuration
  vnet_name           = "my-vnet"
  address_space       = ["10.10.0.0/16"]
  location            = "Israel Central"
  resource_group_name = "my-resource-group"

  # Subnet Configuration
  aks_subnet_name           = "aks-subnet"
  aks_subnet_prefix         = "10.10.1.0/24"
  db_subnet_name            = "database-subnet"
  db_subnet_prefix          = "10.10.2.0/24"
  integration_subnet_name   = "integration-subnet"
  integration_subnet_prefix = "10.10.3.0/24"
  management_subnet_name    = "management-subnet"
  management_subnet_prefix  = "10.10.4.0/24"

  # Optional
  tags = {
    Environment = "dev"
    Project     = "learning-teachin"
  }
}
```

## Outputs

The module provides comprehensive outputs for integration with other modules:

- **VNet Information** - ID, name, address space, location
- **Individual Subnet Details** - IDs, names, and address prefixes for each subnet
- **Convenience Maps** - `all_subnet_ids` and `all_subnet_names` for easy reference

## Next Steps

This is the foundation step of our network infrastructure. Future iterations will add:

1. **Network Security Groups (NSGs)** - Traffic filtering rules
2. **Private Endpoints** - Secure access to Azure services
3. **Route Tables** - Custom routing for advanced scenarios
4. **Application Gateway** - Load balancing and WAF capabilities
5. **Network Peering** - Multi-region connectivity if needed

## Validation

After applying this module, validate the network setup:

```bash
# Check VNet
az network vnet show --name <vnet-name> --resource-group <rg-name>

# List all subnets
az network vnet subnet list --vnet-name <vnet-name> --resource-group <rg-name>

# Verify database subnet delegation
az network vnet subnet show --name <db-subnet-name> --vnet-name <vnet-name> --resource-group <rg-name>
```

## Requirements

- Terraform >= 1.0
- Azure Provider >= 4.0
- Appropriate Azure permissions for network resource creation
