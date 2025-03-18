# Configuration Management

This document explains how configuration is managed in the Clarus Mens API project.

## Shared Settings Approach

The application uses a configuration inheritance model where common settings are defined in a base file and environment-specific settings override only what needs to be different.

### Files Structure

- **appsettings.json**: Contains all common settings used across all environments
- **appsettings.Development.json**: Contains only settings that are specific to the development environment
- **appsettings.{Environment}.json**: For other environments (Production, Staging, etc.)

### How It Works

1. ASP.NET Core's configuration system loads `appsettings.json` first
2. Then it loads environment-specific files (like `appsettings.Development.json`)
3. Values from the environment file override matching values from the base file
4. Non-overridden values are inherited from the base file

### Example

**appsettings.json** (base settings):

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ApiInfo": {
    "Name": "Clarus Mens API",
    "Description": "API for Clarus Mens question answering service",
    "Contact": {
      "Name": "Peter Stackebrandt",
      "Email": "info.stackebrandt@gmail.com"
    },
    "License": {
      "Name": "Apache License 2.0",
      "Url": "https://github.com/pstackebrandt/clarus-mens/blob/main/LICENSE"
    }
  }
}
```

**appsettings.Development.json** (only overrides what's different):

```json
{
  "ApiInfo": {
    "Name": "Clarus Mens API (Development)",
    "Description": "Development instance of the Clarus Mens question answering service"
  }
}
```

In the Development environment, the API name and description come from the Development file, while contact and license information are inherited from the base file.

### Benefits

1. **Reduced Duplication**: Common values are defined only once
2. **Clear Intent**: Environment files clearly show what's different
3. **Easier Maintenance**: Common values only need to be updated in one place
4. **Simpler Configuration Files**: Environment-specific files are shorter and easier to read

## Adding New Environments

To add configuration for a new environment:

1. Create a new file named `appsettings.{EnvironmentName}.json`
2. Include only the settings that differ from the base configuration
3. Set the environment variable `ASPNETCORE_ENVIRONMENT` to your environment name

## Accessing Configuration

Configuration is accessed through dependency injection:

```csharp
public class MyService
{
    private readonly IConfiguration _config;
    
    public MyService(IConfiguration config)
    {
        _config = config;****
    }
    
    public void DoSomething()
    {
        var apiName = _config["ApiInfo:Name"];
        // Use the configuration...
    }
}
```

## Configuration Loading

Configuration is loaded in `Program.cs` using the standard ASP.NET Core pattern:

```csharp
var builder = WebApplication.CreateBuilder(args);
// builder.Configuration now contains the merged configuration
```
