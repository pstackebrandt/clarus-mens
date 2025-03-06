using System;
using System.Reflection;
using Microsoft.Extensions.Hosting;

namespace ClarusMensAPI.Services;

/// <summary>
/// Service for retrieving application version information
/// </summary>
public class VersionService
{
    private readonly Version _version;
    private readonly IHostEnvironment _environment;
    
    public VersionService(IHostEnvironment environment)
    {
        _environment = environment;
        // Get version from executing assembly
        _version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0);
    }
    
    /// <summary>
    /// Gets the full semantic version string (v1.2.3)
    /// </summary>
    public string GetVersionString() => $"v{_version.Major}.{_version.Minor}.{_version.Build}";
    
    /// <summary>
    /// Gets the version object
    /// </summary>
    public Version GetVersion() => _version;
    
    /// <summary>
    /// Gets a display-friendly version string with optional suffix and
    /// includes environment name in non-production environments
    /// </summary>
    public string GetDisplayVersion(string? suffix = null)
    {
        string version = $"{_version.Major}.{_version.Minor}.{_version.Build}";
        
        if (!string.IsNullOrEmpty(suffix))
        {
            version = $"{version}-{suffix}";
        }
        
        // Add environment name for non-production environments
        if (!_environment.IsProduction())
        {
            version = $"{version} ({_environment.EnvironmentName})";
        }
        
        return version;
    }
} 