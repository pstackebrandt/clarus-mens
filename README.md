# ClarusMens

A clear mind, a clear path forward.

## About

ClarusMens is a .NET-based API project designed to [brief description of what your project aims to achieve].

## Prerequisites

- .NET 9.0 SDK or later
- Visual Studio 2022, Visual Studio Code, or Cursor (with C# extensions)

## Getting Started

1. Clone the repository
   ```
   git clone [your-repository-url]
   ```

2. Navigate to the project directory
   ```
   cd clarus-mens
   ```

3. Build the solution
   ```
   dotnet build
   ```

4. Run the API
   ```
   cd ClarusMensAPI
   dotnet run
   ```

The API will be available at http://localhost:5209.

## Testing the API

### Using REST Client

This project includes `.http` files for testing API endpoints with the REST Client VS Code extension.

1. Install the REST Client extension in VS Code/Cursor
2. Open `ClarusMensAPI/ClarusMensAPI.http`
3. Click "Send Request" above any request definition
4. View the response in the split window

Example request:
```http
GET http://localhost:5209/weatherforecast/
```

### Using Browser or Postman

You can also test the API using your browser or tools like Postman:

- Weather forecast endpoint: http://localhost:5209/weatherforecast

## Project Structure

- **ClarusMensAPI/** - Main API project
  - Controllers for API endpoints
  - Service implementations
  - Data models

## Development

### Using Hot Reload

For faster development, use:
```
dotnet watch run
```

This enables hot reload so you can see changes without manually restarting the application.

## Documentation

- [API Testing Guide](docs/api-testing.md)
- [MVP Plan](docs/mvp/mvp-plan.md)

## License

This project is licensed under the [LICENSE](LICENSE) file in the repository. 