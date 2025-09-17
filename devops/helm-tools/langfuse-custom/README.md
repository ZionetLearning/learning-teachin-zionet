# the custom image is already built and saved in the acr, but if for some reason it will be deleted, heres how to build and push it again:

1. Make sure Docker Desktop is running.

2. run these from the `devops/helm-tools` folder

az acr login --name teachindevacr
docker build -t teachindevacr.azurecr.io/langfuse-web:3.108.0-langfusepath ./langfuse-custom
docker push teachindevacr.azurecr.io/langfuse-web:3.108.0-langfusepath
