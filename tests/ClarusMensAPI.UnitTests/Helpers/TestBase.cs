using Microsoft.Extensions.Hosting;
using Moq;

namespace ClarusMensAPI.UnitTests.Helpers
{
    /// <summary>
    /// Base class for tests that need common mock setups
    /// </summary>
    public abstract class TestBase
    {
        protected Mock<IHostEnvironment> MockEnvironment { get; }
        
        protected TestBase()
        {
            MockEnvironment = new Mock<IHostEnvironment>();
            MockEnvironment.Setup(e => e.EnvironmentName).Returns("Testing");
        }
        
        /// <summary>
        /// Configure the environment as Production
        /// </summary>
        protected virtual void SetupProduction()
        {
            MockEnvironment.Setup(e => e.IsProduction()).Returns(true);
            MockEnvironment.Setup(e => e.EnvironmentName).Returns("Production");
        }
        
        /// <summary>
        /// Configure the environment as Development
        /// </summary>
        protected virtual void SetupDevelopment()
        {
            MockEnvironment.Setup(e => e.IsProduction()).Returns(false);
            MockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");
        }
        
        /// <summary>
        /// Configure the environment as Staging
        /// </summary>
        protected virtual void SetupStaging()
        {
            MockEnvironment.Setup(e => e.IsProduction()).Returns(false);
            MockEnvironment.Setup(e => e.EnvironmentName).Returns("Staging");
        }
    }
} 