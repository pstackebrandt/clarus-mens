# API Architecture

This document describes the recommended API setup pattern for .NET 9 minimal APIs in the Clarus Mens project.
For detailed information about the project's folder structure, please refer to [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md).

## Overview

When using this project as a template, preserve the overall structure while customizing specific
service registrations, middleware configuration, and endpoints for your domain.

## API Structure

The API is structured into clear sections:

### Service Registration

This section is responsible for registering all application services.
Customize services but maintain the organizational structure:

- Application services are added via extension methods
- OpenAPI documentation is configured
- HTTPS redirection is set up
- Health checks are added

### Application Lifecycle Events

This pattern should be preserved for logging and startup tasks:

- Version information is logged at application start
- Additional lifecycle events can be added here

### Middleware Configuration

Maintain this order while customizing for your needs:

- HTTPS redirection is only applied in non-development environments to prevent certificate issues during local development
- OpenAPI is always available regardless of environment
- Swagger UI is configured with appropriate versioning

### Endpoint Registration

All endpoints are registered using extension methods to keep the Program.cs file clean.

## API Contract Versioning

The API uses a contract version constant to maintain versioning across OpenAPI documentation.

## Testability

The Program class is made partial to allow for test accessibility.
