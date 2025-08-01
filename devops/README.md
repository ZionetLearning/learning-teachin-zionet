# DevOps


# App deploy to azure cloud with terraform infrastructure

## How to start  
 ```
cd terraform
terraform init
terraform plan -var-file="terraform.tfvars.dev"
terraform apply -var-file="terraform.tfvars.dev"
```

### after apply is done, run the script to set up the yaml files
 ```
 ./start-cloud.sh
 ```
- and we will get the external ip of the todomanager exported to us after the script is done. `http://<External_IP>:5073/swagger`


## How to destroy
```
terraform destroy -var-file="terraform.tfvars.dev"
```

## General usefull commands
 - set to cloud kubcetl context `az aks get-credentials   --resource-group dev-zionet-learning-2025   --name aks-cluster-dev   --overwrite-existing`

 - to be able to `kubectl get pods -n devops-model`

 - to get external ip `kubectl -n devops-model get svc todomanager`

 - see logs of pods `kubectl -n devops-model logs deployment/todomanager -f`

 - apply new/updated yaml `kubectl apply -f ./todoaccessor-deployment.yaml -n devops-model`

 - restart pod  `kubectl rollout restart deployment/todoaccessor -n devops-model`