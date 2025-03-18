# Project Structure

This document describes the folder structure and organization of the Clarus Mens project.

## Folder Structure

```plaintext
ClarusMensAPI/
├── Controllers/              # Controller-based endpoints (if any)
├── Configuration/            # Configuration-related classes
│   ├── OpenApiConfiguration.cs
│   └── SwaggerVersionSetup.cs
├── Extensions/               # Extension methods
│   ├── ResultsExtensions.cs  # JSON serialization extensions
│   └── ServiceCollectionExtensions.cs
├── Middleware/               # Custom middleware components
├── Models/                   # Data models
│   ├── Requests/             # Input models
│   └── Responses/            # Response models 
│       ├── RootResponse.cs
│       └── QuestionAnswerResponse.cs
├── Services/                 # Service implementations
│   ├── Interfaces/           # Service interfaces
│   │   └── IQuestionService.cs
│   ├── VersionService.cs  
│   └── SimpleQuestionService.cs
├── Endpoints/                # Grouped endpoint definitions
│   ├── QuestionEndpoints.cs
│   ├── VersionEndpoints.cs
│   └── RootEndpoint.cs
├── Validators/               # Input validation
├── Program.cs                # Main entry point
├── appsettings.json          # Configuration
└── appsettings.Development.json
```

## Component Descriptions

### Controllers

Contains any controller-based endpoints. In a minimal API project, this folder
 might be empty as most endpoints are defined using the minimal API pattern.

### Configuration

Houses configuration-related classes that set up various aspects of the application:

- **OpenApiConfiguration.cs**: Configures OpenAPI documentation
- **SwaggerVersionSetup.cs**: Sets up Swagger with proper versioning information

### Extensions

Contains extension methods that enhance functionality:

- **ResultsExtensions.cs**: Provides safe JSON serialization helpers (like `JsonSafeOk`)
- **ServiceCollectionExtensions.cs**: Simplifies service registration

### Middleware

Holds custom middleware components for processing HTTP requests and responses.

### Models

Organizes the data structures used throughout the application:

- **Requests/**: Input models for API endpoints
- **Responses/**: Output models returned by API endpoints

### Services

Contains business logic implementations:

- **Interfaces/**: Service contract definitions
- **VersionService.cs**: Handles application versioning
- **SimpleQuestionService.cs**: Implements question answering functionality

### Endpoints

Groups related API endpoints into separate files for better organization:

- **QuestionEndpoints.cs**: Endpoints for question answering functionality
- **VersionEndpoints.cs**: Endpoints for version information
- **RootEndpoint.cs**: Root endpoint providing API information

### Validators

Contains validation logic for input models, potentially using FluentValidation.

## Main Application Files

- **Program.cs**: The entry point for the application, now much leaner after the restructuring
- **appsettings.json**: Application configuration
- **appsettings.Development.json**: Environment-specific configuration

## Relationship with Architecture

This project structure supports the API architecture described in ARCHITECTURE.md.
The separation of concerns and organization of code into distinct folders enables
the recommended patterns for .NET 9 minimal APIs.
