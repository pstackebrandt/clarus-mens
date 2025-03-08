using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Hosting;

namespace ClarusMensAPI.Services;

/// <summary>
/// Service for retrieving application version information following SemVer 2.0.0
/// </summary>
public class VersionService
{
    private readonly Version _dotNetVersion;
    private readonly SemVersion _semVersion;
    private readonly IHostEnvironment _environment;
    
    public VersionService(IHostEnvironment environment)
    {
        _environment = environment;
        
        // Get version from executing assembly
        _dotNetVersion = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0);
        
        // Parse the Assembly version into SemVer format
        string versionString = $"{_dotNetVersion.Major}.{_dotNetVersion.Minor}.{_dotNetVersion.Build}";
        
        // Check if there's pre-release info in AssemblyInformationalVersion
        var assemblyInfo = Assembly.GetExecutingAssembly()
            .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
        
        string? preRelease = null;
        string? buildMetadata = null;
        
        if (assemblyInfo.Length > 0)
        {
            var fullVersion = ((AssemblyInformationalVersionAttribute)assemblyInfo[0]).InformationalVersion;
            var match = Regex.Match(fullVersion, @"^[\d\.]+(?:-([a-zA-Z0-9\.\-]+))?(?:\+([a-zA-Z0-9\.\-]+))?$");
            if (match.Success)
            {
                if (match.Groups.Count > 1 && match.Groups[1].Success)
                {
                    preRelease = match.Groups[1].Value;
                }
                
                if (match.Groups.Count > 2 && match.Groups[2].Success)
                {
                    buildMetadata = match.Groups[2].Value;
                }
            }
        }
        
        _semVersion = new SemVersion(_dotNetVersion.Major, _dotNetVersion.Minor, _dotNetVersion.Build, preRelease, buildMetadata);
    }
    
    /// <summary>
    /// Gets the version string in SemVer format (e.g. "1.2.3" or "1.2.3-beta")
    /// </summary>
    public virtual string GetVersionString() => _semVersion.ToString();
    
    /// <summary>
    /// Gets the SemVer representation of the version
    /// </summary>
    public virtual SemVersion GetSemVersion() => _semVersion;
    
    /// <summary>
    /// Gets the standard .NET Version object
    /// </summary>
    public virtual Version GetDotNetVersion() => _dotNetVersion;
    
    /// <summary>
    /// Gets a display-friendly version string with environment name in non-production
    /// </summary>
    public virtual string GetDisplayVersion()
    {
        string version = _semVersion.ToString();
        
        // Add environment name for non-production environments
        if (_environment.EnvironmentName != "Production")
        {
            version = $"{version} ({_environment.EnvironmentName})";
        }
        
        return version;
    }
}

/// <summary>
/// Represents a Semantic Version according to SemVer 2.0.0
/// </summary>
public class SemVersion
{
    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }
    public string? PreRelease { get; }
    public string? BuildMetadata { get; }
    
    public SemVersion(int major, int minor, int patch, string? preRelease = null, string? buildMetadata = null)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        PreRelease = preRelease;
        BuildMetadata = buildMetadata;
    }
    
    public override string ToString()
    {
        var version = $"{Major}.{Minor}.{Patch}";
        if (!string.IsNullOrEmpty(PreRelease))
        {
            version = $"{version}-{PreRelease}";
        }
        if (!string.IsNullOrEmpty(BuildMetadata))
        {
            version = $"{version}+{BuildMetadata}";
        }
        return version;
    }
    
    public bool IsPreRelease => !string.IsNullOrEmpty(PreRelease);
}
