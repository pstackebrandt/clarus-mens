# ClarusMens

A .NET API Template Project - Learning by Building

## Table of Contents

- [Table of Contents](#table-of-contents)
- [About](#about)
- [Using as a Template](#using-as-a-template)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Testing the API](#testing-the-api)
  - [Automated Tests](#automated-tests)
  - [Docker-based Testing](#docker-based-testing)
  - [Using REST Client](#using-rest-client)
  - [Using Browser or Postman](#using-browser-or-postman)
- [Project Structure](#project-structure)
- [Development](#development)
  - [Using Hot Reload](#using-hot-reload)
- [Documentation](#documentation)
- [Versioning](#versioning)
- [License](#license)

## About

ClarusMens is primarily a training project that demonstrates how to build and deploy a production-ready .NET API.
While it includes a simple question-answering endpoint, its main value lies in showcasing:

- Modern .NET API architecture and best practices
- Complete CI/CD and deployment setup
- Comprehensive testing infrastructure
- Production-ready Docker configuration
- Structured documentation approach

The question-answering functionality serves as a simple example endpoint, allowing focus on the
infrastructure and deployment aspects of API development.

## Using as a Template

This project is designed as a learning resource and template for future .NET API projects. It includes:

- Production-ready architecture following modern .NET API best practices
- Comprehensive test infrastructure with unit, integration, and functional tests
- Documentation patterns that ensure maintainability
- Solutions for common .NET 9 issues (such as serialization workarounds)
- CI/CD pipeline configuration ready for enterprise development

To use this project as a template:

1. Clone or download this repository
2. Rename the solution and projects to match your new API
3. Update namespaces, API information, and configuration
4. Customize endpoints and models for your specific requirements
5. Retain the testing structure and documentation patterns

See [Using ClarusMens as a Template](docs/TEMPLATE_USAGE.md) for detailed instructions.

## Prerequisites

- .NET 9.0 SDK or later
- Visual Studio 2022, Visual Studio Code, or Cursor (with C# extensions)

## Getting Started

1. Clone the repository

   ``` powershell
   git clone https://github.com/pstackebrandt/clarus-mens.git
   ```

2. Navigate to the project directory

   ``` powershell
   cd clarus-mens
   ```

3. Build the solution

   ``` powershell
   dotnet build
   ```

4. Run the API

   ``` powershell
   cd ClarusMensAPI
   dotnet run
   ```

The API will be available at <http://localhost:5209>.

## Testing the API

### Automated Tests

Run the full test suite with the Testing configuration:
(Run the command from the root directory.)

```powershell
dotnet test -c Testing
```

The `Testing` configuration is a specialized build configuration optimized for test execution,
 with separate output directories and test-specific settings.

> **Note**: You may see a warning about no tests in the IntegrationTests project - this is expected and can be ignored.

If you need to configure test execution, you can set environment variables:

```powershell
$env:CLARUSMENS_TEST_TIMEOUT = 60000  # Set longer timeout (in ms)
dotnet test -c Testing
```

### Docker-based Testing

Tests can be included in the Docker image if needed:

```powershell
docker build -t clarusmens-api-test --target=test .
docker run -it clarusmens-api-test
```

For standard testing, use the non-containerized approach detailed above.

For detailed Docker build and testing documentation, see [Docker Build Documentation](docs/deployment/docker-build-documentation.md).

For more detailed testing options, CI/CD integration details, and environment variables, see the [tests README](./tests/README.md).

### Using REST Client

This project includes `.http` files for testing API endpoints with the REST Client VS Code extension.

1. Install the REST Client extension in VS Code/Cursor
2. Open `ClarusMensAPI/api-manual-endpoint-requests.http`
3. Click "Send Request" above any request definition
4. View the response in the split window

Example request:

```http
GET http://localhost:5209/api/question?query=hello
```

### Using Browser or Postman

You can also test the API using your browser or tools like Postman:

- Question-answer endpoint: <http://localhost:5209/api/question?query=hello>

## Project Structure

- **ClarusMensAPI/** - Main API project
  - Controllers for API endpoints
  - Service implementations
  - Data models

## Development

### Using Hot Reload

For faster development, use:

``` powershell
dotnet watch run
```

This enables hot reload so you can see changes without manually restarting the application.

## Documentation

- [API Testing Guide](docs/api-testing.md)
- [Configuration Management](docs/Configuration.md)
- [MVP Plan](docs/mvp/mvp-plan.md)

## Versioning

This project uses Semantic Versioning. For version update instructions and processes, see [VERSIONING.md](./VERSIONING.md).

## License

This project is licensed under the [Apache 2.0](LICENSE) file in the repository.
