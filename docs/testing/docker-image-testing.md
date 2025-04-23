# Docker Image Testing

## Overview

The `Test-DockerImage.ps1` script provides comprehensive validation and testing for the Clarus Mens
production Docker image. Unlike the `Dockerfile.tests` which focuses on testing application code,
this script targets the production container itself to ensure it functions as expected.

## Test Capabilities

The script performs four types of tests:

1. **Dockerfile Validation**: Checks that the Dockerfile syntax is valid and the build process completes
2. **Container Build**: Builds the Docker image with current version information
3. **Health Check**: Verifies the container starts successfully and reports healthy status
4. **API Endpoints**: Tests that key endpoints respond correctly and version information matches

## Usage

```powershell
# Run all tests
.\scripts\Test-DockerImage.ps1

# Test specific aspects
.\scripts\Test-DockerImage.ps1 -TestType Validate    # Only validate Dockerfile
.\scripts\Test-DockerImage.ps1 -TestType HealthCheck # Test container health
.\scripts\Test-DockerImage.ps1 -TestType API         # Test API endpoints

# Additional options
.\scripts\Test-DockerImage.ps1 -SkipBuild            # Skip rebuilding image
.\scripts\Test-DockerImage.ps1 -KeepContainer        # Keep container running after tests
.\scripts\Test-DockerImage.ps1 -Detailed             # Show detailed output
.\scripts\Test-DockerImage.ps1 -ImageName custom-name -ApiPort 5000 # Custom settings
```

## Parameters

| Parameter     | Description                                  | Default     |
| ------------- | -------------------------------------------- | ----------- |
| TestType      | Test scope (Validate\|HealthCheck\|API\|All) | All         |
| ImageName     | Docker image name                            | clarus-mens |
| SkipBuild     | Skip building the image                      | false       |
| ApiPort       | Port for API tests                           | 8080        |
| KeepContainer | Keep test container running                  | false       |
| Detailed      | Show detailed test output                    | false       |

## Version Verification

The script automatically extracts version information from `Directory.Build.props` and verifies that
this version is consistent across:

- API version endpoint response
- Docker image labels (org.clarus-mens.version)

This ensures version information is correctly propagated through the build process.

## Integration with CI/CD

This script is designed to be used both during local development and in CI/CD pipelines:

```yaml
# Example GitHub Action step
- name: Test Docker Image
  run: pwsh -File ./scripts/Test-DockerImage.ps1 -TestType All
```

## Best Practices

- Run the script locally before pushing changes to ensure the Docker image works correctly
- Include the script in CI/CD pipelines to catch issues early
- Use the `-Detailed` flag to troubleshoot issues
- Run with `-KeepContainer` to inspect container state after test completion

## Related Documentation

- See `tests/README.md` for information on other testing approaches
- Refer to the [Docker Best Practices](../deployment/docker-best-practices.md) document for general Docker guidelines
