using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;

namespace ClarusMensAPI.FunctionalTests;

// TEMPLATE: This base class for functional tests provides essential test infrastructure.
// When using this project as a template, keep this pattern for test organization.
// The base class pattern ensures consistent test setup and cleanup across all test classes.

/// <summary>
/// Base class for all functional tests that provides common test infrastructure.
/// 
/// This abstract class:
/// 1. Creates and manages a TestWebApplicationFactory instance
/// 2. Provides a configured HttpClient for making requests to the test server
/// 3. Handles proper resource cleanup after tests complete
/// 
/// By inheriting from this base class, test classes automatically get:
/// - An isolated test environment with its own web server
/// - A pre-configured HTTP client that doesn't follow redirects
/// - Automatic cleanup of resources after test execution
/// </summary>
public abstract class FunctionalTestBase
{
    /// <summary>
    /// The test web application factory that hosts the application under test.
    /// Uses dynamic port allocation and custom test configuration.
    /// </summary>
    protected readonly TestWebApplicationFactory<Program> Factory;
    
    /// <summary>
    /// Pre-configured HTTP client for making requests to the test server.
    /// Configured to not automatically follow redirects to allow testing
    /// of redirection behavior.
    /// </summary>
    protected readonly HttpClient Client;
    
    // TEMPLATE: Constructor pattern - maintain this setup approach for consistent test initialization
    /// <summary>
    /// Initializes the test environment by creating a test server and HTTP client.
    /// </summary>
    protected FunctionalTestBase()
    {
        Factory = new TestWebApplicationFactory<Program>();
        Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false // Allows testing of redirect responses
        });
    }
    
    // TEMPLATE: Cleanup pattern - always include proper resource disposal in test classes
    /// <summary>
    /// Cleans up resources after test execution.
    /// This method is automatically called after each test method runs.
    /// </summary>
    [TestCleanup]
    public virtual void Cleanup()
    {
        Client.Dispose();
        Factory.Dispose();
    }
} 