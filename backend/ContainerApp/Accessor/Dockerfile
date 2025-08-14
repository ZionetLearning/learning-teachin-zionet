FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5003

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files individually so restore can resolve references
COPY Common/Common.csproj Common/
COPY Accessor/Accessor.csproj Accessor/

# Restore
RUN dotnet restore Accessor/Accessor.csproj

# Copy the rest of the source code
COPY Common/ Common/
COPY Accessor/ Accessor/

# Publish
WORKDIR /src/Accessor
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Accessor.dll"]
