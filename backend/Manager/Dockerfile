# ----------------- Build Stage -----------------
FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build
WORKDIR /src

# Copy csproj and restore
COPY *.csproj ./
RUN dotnet restore

# Copy everything and build
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# ----------------- Runtime Stage -----------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview
WORKDIR /app

COPY --from=build /app/publish ./

# Expose the app port
EXPOSE 5001

# Start the app
ENTRYPOINT ["dotnet", "Manager.dll"]
