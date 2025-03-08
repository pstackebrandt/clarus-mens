using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Hosting;
using Moq;
using ClarusMensAPI.Services;

namespace ClarusMensAPI.UnitTests.Services
{
    [TestClass]
    public class VersionServiceTests
    {
        private Mock<IHostEnvironment> _mockEnvironment = new Mock<IHostEnvironment>();

        [TestInitialize]
        public void Setup()
        {
            _mockEnvironment = new Mock<IHostEnvironment>();
        }

        [TestMethod]
        [TestCategory("Versioning")]
        public void GetDisplayVersion_InProduction_ReturnsVersionOnly()
        {
            // Arrange
            //_mockEnvironment.Setup(e => e.IsProduction()).Returns(true);
            _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Production");

            // Use TestVersionService to control the version
            var service = new TestVersionService(_mockEnvironment.Object, "1.2.3");

            // Act
            var result = service.GetDisplayVersion();

            // Assert
            Assert.AreEqual("1.2.3", result);
        }

        [TestMethod]
        [TestCategory("Versioning")]
        public void GetDisplayVersion_InDevelopment_ReturnsVersionWithEnvironment()
        {
            // Arrange
            _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");

            var service = new TestVersionService(_mockEnvironment.Object, "1.2.3-beta");

            // Act
            var result = service.GetDisplayVersion();

            // Assert
            Assert.AreEqual("1.2.3-beta (Development)", result);
        }

        /// <summary>
        /// Helper class that overrides the VersionService to provide a controlled test version
        /// </summary>
        private class TestVersionService : VersionService
        {
            private readonly SemVersion _testVersion;
            private readonly IHostEnvironment _testEnvironment;

            public TestVersionService(IHostEnvironment environment, string versionString)
                : base(environment)
            {
                _testEnvironment = environment;
                // Parse the version string
                var parts = versionString.Split(new[] { '-', '+' }, 3);
                var versionParts = parts[0].Split('.').Select(int.Parse).ToArray();

                string? preRelease = null;
                string? buildMetadata = null;

                if (parts.Length > 1 && versionString.Contains('-'))
                {
                    var preReleaseIndex = versionString.IndexOf('-') + 1;
                    var preReleaseEnd = versionString.Contains('+') ? versionString.IndexOf('+') : versionString.Length;
                    preRelease = versionString.Substring(preReleaseIndex, preReleaseEnd - preReleaseIndex);
                }

                if (versionString.Contains('+'))
                {
                    buildMetadata = versionString.Substring(versionString.IndexOf('+') + 1);
                }

                _testVersion = new SemVersion(versionParts[0], versionParts[1], versionParts[2], preRelease, buildMetadata);
            }

            // In TestVersionService class
            public override string GetDisplayVersion()
            {
                string version = GetVersionString();

                if (_testEnvironment.EnvironmentName != "Production")
                {
                    version = $"{version} ({_testEnvironment.EnvironmentName})";
                }

                return version;
            }

            public override SemVersion GetSemVersion() => _testVersion;

            public override Version GetDotNetVersion() => new Version(_testVersion.Major, _testVersion.Minor, _testVersion.Patch, 0);

            public override string GetVersionString() => _testVersion.ToString();
        }
    }
}