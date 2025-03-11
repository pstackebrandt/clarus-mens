# Clarus Mens Tests

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

Run all tests with default configuration:

```powershell
dotnet test clarus-mens.sln
```

Run tests with the Testing configuration (recommended):

```powershell
dotnet test -c Testing
```

Run a specific test project:

```powershell
dotnet test ./tests/ClarusMensAPI.UnitTests -c Testing
```

Run tests with a specific category:

```powershell
dotnet test --filter "Category=Versioning" -c Testing
```

Run tests with verbose output to see detailed results:

```powershell
dotnet test -c Testing -v detailed
```

### Known Warnings

When running tests, you may see this warning:

```
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
