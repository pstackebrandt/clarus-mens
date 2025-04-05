# Architecture Overview

> **Training Project**: This architecture demonstrates best practices for .NET API development.
> While the API functionality is basic, the infrastructure and patterns shown here are
> production-ready and can be used as a template for more complex APIs.

## Table of Contents

- [Table of Contents](#table-of-contents)
- [API Architecture](#api-architecture)
- [Overview](#overview)
- [API Structure](#api-structure)
  - [Service Registration](#service-registration)
  - [Application Lifecycle Events](#application-lifecycle-events)
  - [Middleware Configuration](#middleware-configuration)
  - [Endpoint Registration](#endpoint-registration)
- [API Contract Versioning](#api-contract-versioning)
- [Testability](#testability)

## API Architecture

This document describes the recommended API setup pattern for .NET 9 minimal APIs in the Clarus Mens project.
For detailed information about the project's folder structure, please refer to \
[PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md).

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

- HTTPS redirection is only applied in non-development environments to prevent certificate issues during \
local development
- OpenAPI is always available regardless of environment
- Swagger UI is configured with appropriate versioning

### Endpoint Registration

All endpoints are registered using extension methods to keep the Program.cs file clean.

## API Contract Versioning

The API uses a contract version constant to maintain versioning across OpenAPI documentation.

## Testability

The Program class is made partial to allow for test accessibility.
