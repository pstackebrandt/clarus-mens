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

## Running Tests

### .NET Tests

Run all tests:

```powershell
dotnet test clarus-mens.sln
```

Run a specific test project:

```powershell
dotnet test ./tests/ClarusMensAPI.UnitTests
```

Run tests with a specific category:

```powershell
dotnet test --filter "Category=Versioning"
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
