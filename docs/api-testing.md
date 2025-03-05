# API Testing with REST Client

## Setup

1. Install the REST Client extension for VS Code/Cursor
   - Search for "REST Client" by Huachao Mao in the extensions marketplace
   - Click "Install"

## Using REST Client with ClarusMens API

### Existing HTTP Files

The project includes `.http` files for testing API endpoints:

- `ClarusMensAPI/ClarusMensAPI.http` - Basic API endpoints

### Running Requests

1. Open any `.http` file
2. Make sure the API is running (`dotnet run`/ `dotnet watch run` from the ClarusMensAPI directory)
3. Click "Send Request" above any request or use Ctrl+Alt+R (Cmd+Alt+R on Mac)
4. View the response in the response pane

### Creating New Requests

When adding new API endpoints, add corresponding requests to the appropriate `.http` file:

```http
### Get endpoint description
GET {{baseUrl}}/endpoint-path
Accept: application/json

### Post to endpoint
POST {{baseUrl}}/endpoint-path
Content-Type: application/json

{
  "key": "value"
}
```

## Conventions

- Separate requests with `###`
- Use variables for reusable values: `@baseUrl = http://localhost:5209`
- Add descriptive comments before each request
- Group related requests in the same `.http` file.
  