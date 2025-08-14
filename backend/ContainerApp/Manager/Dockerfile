# ----------------- Base Runtime -----------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5001

# ----------------- Build Stage -----------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Common/Common.csproj Common/
COPY Manager/Manager.csproj Manager/

RUN dotnet restore Manager/Manager.csproj

COPY Common/ Common/
COPY Manager/ Manager/

WORKDIR /src/Manager
RUN dotnet publish -c Release -o /app/publish

# ----------------- Final Image -----------------
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Manager.dll"]
