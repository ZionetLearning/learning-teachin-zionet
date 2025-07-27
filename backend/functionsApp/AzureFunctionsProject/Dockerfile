# 1) Base image: the isolated Functions host
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0 AS base
WORKDIR /home/site/wwwroot

# Tell the Functions runtime to bind to port 7278 inside the container
ENV FUNCTIONS_HTTPWORKER_PORT=7278

# Document that port
EXPOSE 7278

# 2) Build stage: restore & compile your C# project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy only the csproj and restore (leverages Docker cache)
COPY ["AzureFunctionsProject/AzureFunctionsProject.csproj", "AzureFunctionsProject/"]
RUN dotnet restore "AzureFunctionsProject/AzureFunctionsProject.csproj"

# Copy the rest of the code and build
COPY . .
WORKDIR "/src/AzureFunctionsProject"
RUN dotnet build "AzureFunctionsProject.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AzureFunctionsProject.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# 4) Final image: copy published output into the Functions host
FROM base AS final
WORKDIR /home/site/wwwroot

COPY --from=publish /app/publish .

ENV AzureWebJobsScriptRoot=/home/site/wwwroot
ENV AzureFunctionsJobHost__Logging__Console__IsEnabled=true
