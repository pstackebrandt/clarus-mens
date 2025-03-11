# ClarusMens

A clear mind, a clear path forward.

## About

ClarusMens is a .NET-based API project  that generates structured answers (MVP), learning questions with answers, and quizzes based on user input.

## Using as a Template

This project is designed to serve as a template for future .NET API projects. It includes:

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

```powershell
dotnet test -c Testing
```

The `Testing` configuration is a specialized build configuration optimized for test execution, with separate output directories and test-specific settings.

> **Note**: You may see a warning about no tests in the IntegrationTests project - this is expected and can be ignored.

If you need to configure test execution, you can set environment variables:

```powershell
$env:CLARUSMENS_TEST_TIMEOUT = 60000  # Set longer timeout (in ms)
dotnet test -c Testing
```

For more detailed testing options, CI/CD integration details, and environment variables, see the [tests README](./tests/README.md).

### Using REST Client

This project includes `.http` files for testing API endpoints with the REST Client VS Code extension.

1. Install the REST Client extension in VS Code/Cursor
2. Open `ClarusMensAPI/ClarusMensAPI.http`
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
