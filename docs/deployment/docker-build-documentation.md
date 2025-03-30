# Docker Build Configuration

## Table of Contents

- [Table of Contents](#table-of-contents)
- [Multi-stage Dockerfile Structure](#multi-stage-dockerfile-structure)
- [Building for Production](#building-for-production)
- [Building for Testing](#building-for-testing)
- [Running Tests in Container](#running-tests-in-container)
- [Key Improvements](#key-improvements)
- [Lessons Learned](#lessons-learned)
  - [Docker Configuration](#docker-configuration)
  - [Testing in Containers](#testing-in-containers)
  - [Command Execution](#command-execution)
- [Future Improvements](#future-improvements)

This document outlines the Docker build configuration for the Clarus Mens API,
including how to build for both production and testing environments.

## Multi-stage Dockerfile Structure

Our Dockerfile uses a multi-stage build pattern with separate targets:

1. **Build stage**: Restores dependencies and builds the application
2. **Publish stage**: Creates a production-ready version
3. **Test stage**: Creates an image for running tests
4. **Final stage**: Creates a minimal runtime image

This approach allows us to generate different images for different purposes from a single Dockerfile.

## Building for Production

To build the production image:

```powershell
docker build -t clarusmens-api .
```

This creates a minimal image with:

- Only the published application (no source code)
- ASP.NET Core runtime (not SDK)
- Health check configured
- Non-root user for security

## Building for Testing

To build the testing image:

```powershell
docker build -t clarusmens-api-test --target=test .
```

This creates a testing-focused image with:

- Full source code
- .NET SDK for building and testing
- Environment configured for testing
- Test entrypoint

## Running Tests in Container

```powershell
# Run all tests with default settings
docker run --rm clarusmens-api-test

# Run with verbose output
docker run --rm clarusmens-api-test -- --verbosity normal

# Run specific test project
docker run --rm clarusmens-api-test -- tests/ClarusMensAPI.UnitTests --verbosity detailed

# Save test results to file
docker run --rm clarusmens-api-test -- --verbosity detailed > test-results.txt
```

## Key Improvements

1. **Separate Test Target**: Created a dedicated test image configuration
2. **Environment Variables**: Configured environment to avoid web startup in tests
3. **Apt-Get Error Handling**: Added fallbacks for package installation
4. **Build Simplification**: Removed conditional logic from build stages

## Lessons Learned

### Docker Configuration

1. **Multi-stage Builds**: Using the `--target` flag is cleaner than build args for different configurations
2. **Dockerfile Clarity**: Explicit stages with clear responsibilities improve maintainability
3. **Error Resilience**: Adding error handling to shell commands prevents build failures

### Testing in Containers

1. **Environment Variables**: Setting `ASPNETCORE_URLS=` and other
   environment variables prevents the web server from starting during tests
2. **Test Entry Point**: Using `ENTRYPOINT ["dotnet", "test"]` makes the container test-focused
3. **Test Output**: Redirecting test output to files helps analyze complex test results

### Command Execution

1. **Terminal Output Visibility**: Avoid using "Pop out" when analyzing test output
2. **Redirecting Output**: Use `> file.txt` for capturing detailed output
3. **Container Logs**: Always check Docker logs for container startup errors

## Future Improvements

1. Test reporting and artifact collection
2. Integration with CI/CD pipelines
3. Volume mounting for test results and coverage reports
4. Optimization of container size and build times
