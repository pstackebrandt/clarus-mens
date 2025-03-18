# Web Server Suppression in Test Containers

## Table of Contents

- [Table of Contents](#table-of-contents)
- [The Issue](#the-issue)
- [Why We Suppressed the Web Server](#why-we-suppressed-the-web-server)
- [How ASP.NET Core Testing Works](#how-aspnet-core-testing-works)
- [Implementation in Docker](#implementation-in-docker)
- [Benefits of This Approach](#benefits-of-this-approach)
- [Alternative Approaches](#alternative-approaches)

## The Issue

When running tests in Docker containers, we found that the web server was
automatically starting and causing an error because of a duplicate endpoint.

## Why We Suppressed the Web Server

We suppressed the web server in the test container for the following reasons:

1. **Avoiding Routing Conflicts**: The duplicate health check endpoints were causing routing conflicts
2. **Unnecessary for Tests**: Most tests don't need a real HTTP server
3. **Resource Efficiency**: Running a web server consumes unnecessary resources during testing
4. **Isolation**: Tests should run in isolation without depending on network services

## How ASP.NET Core Testing Works

ASP.NET Core provides specific tools for testing web applications without starting a real web server:

1. **TestServer**: Creates an in-memory server that processes HTTP requests without binding to network ports
2. **WebApplicationFactory**: Configures and sets up TestServer with your application's startup code
3. **HttpClient**: Used to make requests to the TestServer

```csharp
// Example of ASP.NET Core test using WebApplicationFactory
public class ApiTests
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ApiTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Get_EndpointsReturnSuccessAndCorrectContentType()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/data");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
    }
}
```

## Implementation in Docker

In our Docker test image, we suppressed the web server by:

1. Setting `ASPNETCORE_URLS=` (empty string)
2. Setting `ASPNETCORE_ENVIRONMENT=Development`
3. Creating a dedicated test stage with its own entrypoint

```dockerfile
# Test stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS test
WORKDIR /src
COPY --from=build /src .
ENV ASPNETCORE_URLS=
ENV ASPNETCORE_ENVIRONMENT=Development
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENTRYPOINT ["dotnet", "test"]
```

## Benefits of This Approach

1. **Faster Tests**: No overhead of starting a real web server
2. **No Port Conflicts**: Tests can run in parallel without port binding conflicts
3. **More Reliable**: Tests don't depend on network configuration
4. **Cleaner Output**: No web server logs cluttering test output
5. **Consistent Environment**: Tests run the same way locally and in CI

## Alternative Approaches

If tests need to interact with a real web server, consider:

1. **Dynamic Port Assignment**: Configure the server to use any available port
2. **Containerized Dependencies**: Use test containers for external dependencies
3. **Network Isolation**: Use Docker networks to isolate the test environment
4. **Controlled Startup**: Start the web server only when needed for specific tests
