# Clarus Mens Tests

## Table of Contents

- [Table of Contents](#table-of-contents)
- [Test Structure](#test-structure)
- [Build Configurations](#build-configurations)
- [Running Tests](#running-tests)
  - [.NET Tests](#net-tests)
  - [Known Warnings](#known-warnings)
  - [Environment Variables for Testing](#environment-variables-for-testing)
  - [PowerShell Tests](#powershell-tests)
- [Test Categories](#test-categories)
- [Creating New Tests](#creating-new-tests)
- [Code Coverage](#code-coverage)
- [Test Data](#test-data)
- [Continuous Integration](#continuous-integration)
  - [CI/CD Pipeline Integration](#cicd-pipeline-integration)
- [Docker-Based Testing](#docker-based-testing)
  - [Running Tests in Docker](#running-tests-in-docker)
  - [Benefits of Docker-Based Testing](#benefits-of-docker-based-testing)
  - [When to Use Docker for Testing](#when-to-use-docker-for-testing)
  - [When to Use Direct Testing (Non-Docker)](#when-to-use-direct-testing-non-docker)
  - [Known Limitations and Future Improvements](#known-limitations-and-future-improvements)
    - [Potential Future Improvements](#potential-future-improvements)
- [Testing the Production Dockerfile](#testing-the-production-dockerfile)
  - [What the Script Tests](#what-the-script-tests)
  - [Test Execution Environment](#test-execution-environment)

## Test Structure

This project uses a multi-layered testing approach:

- **Unit Tests** (`ClarusMensAPI.UnitTests`): Tests individual components in isolation
  - `Services/`: Tests for application services
  - `Models/`: Tests for data models
  - `Helpers/`: Test utilities and base classes

- **Integration Tests** (`ClarusMensAPI.IntegrationTests`): Tests interactions between components
  - Test API endpoints with in-memory test server
  - Test database interactions
  
- **Functional Tests** (`ClarusMensAPI.FunctionalTests`): End-to-end tests
  - Test complete user scenarios
  - Uses test containers for external dependencies

- **PowerShell Tests** (`PowerShell.Tests`): Tests for build and versioning scripts
  - Uses Pester framework for PowerShell testing

## Build Configurations

The project uses several build configurations:

- **Debug**: Default configuration for development work
- **Release**: Optimized configuration for production deployment
- **Testing**: Specialized configuration for running tests with:
  - Optimized test-specific settings
  - Separate output directories to avoid conflicts with Debug/Release builds
  - Customized environment variables and configuration settings
  - Disabled optimizations that might interfere with test coverage tools

Always use the `Testing` configuration when running tests to ensure consistent and reliable results.

## Running Tests

### .NET Tests

Run tests with the recommended Testing configuration (from solution root):

```powershell
dotnet test -c Testing
```

Run a specific test project (from solution root):

```powershell
dotnet test ./tests/ClarusMensAPI.UnitTests -c Testing
```

Run tests with a specific category (from solution root):

```powershell
dotnet test --filter "Category=Versioning" -c Testing
```

Run tests with verbose output to see detailed results (from solution root):

```powershell
dotnet test -c Testing -v detailed
```

### Known Warnings

When running tests, you may see this warning:

```text
No test is available in [...]\ClarusMensAPI.IntegrationTests.dll
```

This is **expected behavior** because:

- The IntegrationTests project is set up as a placeholder for future integration tests
- It currently has no test classes or methods implemented
- The warning can be safely ignored

### Environment Variables for Testing

The following environment variables can be set to customize test execution:

| Variable                   | Purpose                             | Default |
| -------------------------- | ----------------------------------- | ------- |
| `CLARUSMENS_TEST_TIMEOUT`  | Timeout for tests in milliseconds   | 30000   |
| `CLARUSMENS_TEST_PARALLEL` | Maximum parallel test threads       | 4       |
| `CLARUSMENS_TEST_API_URL`  | Override API URL for external tests | Not set |

These can be set before running tests:

```powershell
$env:CLARUSMENS_TEST_TIMEOUT = 60000
dotnet test -c Testing
```

### PowerShell Tests

To run PowerShell tests (requires Pester module):

```powershell
# Install Pester if not already installed
Install-Module Pester -Force

# Run tests
Invoke-Pester ./tests/PowerShell.Tests -Output Detailed
```

## Test Categories

Tests are organized by categories to allow selective running:

- `Versioning`: Tests related to version management
- `API`: Tests for API functionality
- `Security`: Tests focusing on security aspects
- `Performance`: Tests measuring performance metrics

## Creating New Tests

When creating new tests:

1. Follow the naming convention: `{ClassName}_{MethodName}_{ExpectedBehavior}`
2. Add the appropriate `[TestCategory]` attributes
3. Keep tests focused on a single aspect or behavior
4. Use the `TestBase` class for common setup when appropriate

## Code Coverage

Code coverage reports can be generated using:

```powershell
dotnet test ./tests/ClarusMensAPI.UnitTests /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

## Test Data

Test data is managed in several ways:

- Small data sets are defined inline in tests
- Larger fixtures are stored in the `TestData` folder
- Database seeds for integration tests are in `IntegrationTests/Data`

## Continuous Integration

Tests are automatically run in the CI pipeline on:

- Pull requests to main branch
- Direct pushes to main branch
- Nightly builds

### CI/CD Pipeline Integration

Our tests are integrated into our CI/CD pipeline as follows:

1. **Test Discovery**: The pipeline automatically discovers all tests using MSTest's discovery mechanism
2. **Parallel Execution**: Tests are executed in parallel, with unit tests running first, followed by functional tests
3. **Test Reports**:
   - Results are published to Azure DevOps/GitHub Actions dashboard
   - JUnit-compatible test result files are generated
   - Code coverage reports in Cobertura format are available
4. **Quality Gates**:
   - PR builds require all tests to pass
   - Code coverage must remain above 80%
   - Test execution time is monitored for performance regression
5. **Deployment Triggers**:
   - Successful test runs on the main branch trigger deployment to staging
   - Additional smoke tests run after deployment before promoting to production

To reproduce CI test execution locally:

```powershell
# Same configuration used in CI
dotnet test -c Testing --blame-hang-timeout 5m --logger "trx;LogFileName=test-results.trx"
```

## Docker-Based Testing

This project uses a dedicated Dockerfile for testing (Dockerfile.tests) separate from the production Dockerfile.

### Running Tests in Docker

To run tests in a Docker container (from solution root):

```powershell
# Build the test image using implicitly the Testing configuration.
docker build -t clarusmens-tests -f Dockerfile.tests .

# Run all tests with default configuration implicitly using the Testing configuration.
docker run --rm clarusmens-tests

# Important: When providing any arguments to the test command, you must explicitly include -c Testing
# as custom arguments replace the default CMD entirely

# Run a specific test project (requires explicit -c Testing)
docker run --rm clarusmens-tests test ./tests/ClarusMensAPI.UnitTests -c Testing

# Run tests with a specific category (requires explicit -c Testing)
docker run --rm clarusmens-tests test -c Testing --filter "Category=Versioning"

# Run with detailed output (requires explicit -c Testing)
docker run --rm clarusmens-tests test -c Testing -v detailed
```

### Benefits of Docker-Based Testing

- Consistent test environment across all development machines
- Tests run in an environment similar to CI/CD pipelines
- Isolation from local system dependencies
- Easy verification of container-specific functionality
- No need to install .NET SDK locally

### When to Use Docker for Testing

- Container-specific functionality testing
- CI/CD pipelines in containerized environments
- Testing in environments identical to production
- Functional tests that require specific container configuration

### When to Use Direct Testing (Non-Docker)

- Local development for faster test cycles
- Normal CI/CD workflows (our GitHub Actions workflow)
- Code coverage analysis
- Running individual test categories or projects

### Known Limitations and Future Improvements

The current Docker testing approach has some limitations to be aware of:

1. **Environment Differences**: The test Docker environment (using SDK image) differs from the production \
Docker environment (using runtime image), which may lead to environment-specific issues not being \
caught during testing.

2. **Security Configuration**: The test container does not implement the same security settings as \
production (non-root user, etc.), potentially masking security-related issues.

3. **Configuration Differences**: Different environment variables and settings between test and production \
containers.

4. **Build Process**: The build process in the test container is simplified compared to the production \
multi-stage build.

#### Potential Future Improvements

- **Layer Caching Optimization**: Separate project file copying and dependency restoration to improve build speed:

  ```dockerfile
  WORKDIR /src
  COPY *.sln .
  COPY */*.csproj ./
  RUN for file in $(ls *.csproj); do mkdir -p ${file%.*}/ && mv $file ${file%.*}/; done
  RUN dotnet restore
  COPY . .
  ```

- **More Precise Base Image**: Specify patch version for the base image to ensure consistency
- **Container Health Monitoring**: Add HEALTHCHECK instruction to verify container is working properly
- **Test Results Volume**: Define a volume for test results: `VOLUME ["/src/TestResults"]`
- **Better Ignore Rules**: Create or update .dockerignore file to exclude unnecessary files
- **Parallel Test Execution**: Configure optimal parallelism settings for container environment

These limitations are currently accepted for practical reasons, but should be considered when evaluating test results.

## Testing the Production Dockerfile

The project includes a dedicated script for testing the production Dockerfile without using the test code:

```powershell
# Run full test suite (validation, build, health check, API endpoints)
.\scripts\Test-DockerImage.ps1

# Run specific test types
.\scripts\Test-DockerImage.ps1 -TestType Validate
.\scripts\Test-DockerImage.ps1 -TestType HealthCheck
.\scripts\Test-DockerImage.ps1 -TestType API

# Skip rebuilding the image if it's already built
.\scripts\Test-DockerImage.ps1 -SkipBuild

# Keep the container running after tests
.\scripts\Test-DockerImage.ps1 -KeepContainer

# Show detailed test output
.\scripts\Test-DockerImage.ps1 -Detailed

# Specify a custom image name and port
.\scripts\Test-DockerImage.ps1 -ImageName my-api-image -ApiPort 5000
```

> 📄 **Detailed Documentation**: For comprehensive information on Docker image testing, see [Docker Image Testing](../docs/testing/docker-image-testing.md).

### What the Script Tests

The `Test-DockerImage.ps1` script performs the following tests:

1. **Dockerfile Validation**: Checks that the Dockerfile syntax is valid and the build process completes
2. **Health Check**: Verifies the container starts successfully and reports healthy status
3. **API Endpoints**: Tests that key endpoints respond correctly and version information matches
4. **Version Verification**: Checks that the version from Directory.Build.props matches the API response and Docker labels

The script automatically extracts version information from Directory.Build.props, so tests always use \
the current project version.

### Test Execution Environment

These tests run against the actual production Dockerfile (not Dockerfile.tests) to ensure the \
production container works as expected. This complements the unit/functional tests run with \
Dockerfile.tests by verifying the final production image.
