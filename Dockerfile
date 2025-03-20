# This Dockerfile containerizes an ASP.NET Core API application for deployment
# to Azure App Service.
# It follows a multi-stage build pattern to create an optimized image:
# 1. Build stage: Restores dependencies and builds the application
# 2. Publish stage: Creates a production-ready version of the application
# 3. Test stage: Creates an image for running tests (only used with --target=test)
# 4. Final stage: Creates a minimal image with only what's needed to run the application

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY *.sln .
COPY ClarusMensAPI/*.csproj ./ClarusMensAPI/
RUN dotnet restore ClarusMensAPI/ClarusMensAPI.csproj

# Copy all files after restore to leverage build layer caching
COPY . .

# Build the app
WORKDIR /src/ClarusMensAPI
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Test stage (use with --target=test)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS test
WORKDIR /src
COPY . .
RUN dotnet restore
ENV ASPNETCORE_URLS=
ENV ASPNETCORE_ENVIRONMENT=Development
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENTRYPOINT ["dotnet", "test"]

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Create non-root user for security
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app

# Install curl and create health check script
USER root
RUN apt-get update && \
    apt-get install -y curl --no-install-recommends && \
    rm -rf /var/lib/apt/lists/* && \
    echo '#!/bin/sh\ncurl -f http://localhost:80/health || exit 1' > /usr/local/bin/healthcheck.sh && \
    chmod +x /usr/local/bin/healthcheck.sh

USER appuser

# Configure container health check (using exec form)
HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
    CMD ["/usr/local/bin/healthcheck.sh"]

EXPOSE 80
ENTRYPOINT ["dotnet", "ClarusMensAPI.dll"] 