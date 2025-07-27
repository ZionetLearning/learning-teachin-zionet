# Stage 1: restore & publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj and restore
COPY ["AzureFunctionsProject.csproj", "./"]
RUN dotnet restore "AzureFunctionsProject.csproj"

# copy the rest and publish
COPY . .
RUN dotnet publish "AzureFunctionsProject.csproj" \
    -c Release \
    -o /home/site/wwwroot

# Stage 2: runtime image
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

# copy published output
COPY --from=build /home/site/wwwroot /home/site/wwwroot
