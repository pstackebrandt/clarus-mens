using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClarusMensAPI.Services;

namespace ClarusMensAPI.UnitTests.Models
{
    [TestClass]
    public class SemVersionTests
    {
        [TestMethod]
        [TestCategory("Versioning")]
        public void Constructor_SetsProperties_Correctly()
        {
            // Arrange & Act
            var version = new SemVersion(1, 2, 3, "beta", "build001");
            
            // Assert
            Assert.AreEqual(1, version.Major);
            Assert.AreEqual(2, version.Minor);
            Assert.AreEqual(3, version.Patch);
            Assert.AreEqual("beta", version.PreRelease);
            Assert.AreEqual("build001", version.BuildMetadata);
        }
        
        [TestMethod]
        [TestCategory("Versioning")]
        public void ToString_FormatsVersion_Correctly()
        {
            // Test basic version
            var basicVersion = new SemVersion(1, 2, 3);
            Assert.AreEqual("1.2.3", basicVersion.ToString());
            
            // Test with pre-release
            var preReleaseVersion = new SemVersion(1, 2, 3, "beta");
            Assert.AreEqual("1.2.3-beta", preReleaseVersion.ToString());
            
            // Test with build metadata
            var buildMetadataVersion = new SemVersion(1, 2, 3, null, "build123");
            Assert.AreEqual("1.2.3+build123", buildMetadataVersion.ToString());
            
            // Test with both pre-release and build metadata
            var fullVersion = new SemVersion(1, 2, 3, "beta", "build123");
            Assert.AreEqual("1.2.3-beta+build123", fullVersion.ToString());
        }
        
        [TestMethod]
        [TestCategory("Versioning")]
        public void IsPreRelease_ReturnsCorrectValue()
        {
            // Regular version should not be pre-release
            Assert.IsFalse(new SemVersion(1, 2, 3).IsPreRelease);
            
            // With pre-release identifier should be pre-release
            Assert.IsTrue(new SemVersion(1, 2, 3, "beta").IsPreRelease);
            
            // With only build metadata should not be pre-release
            Assert.IsFalse(new SemVersion(1, 2, 3, null, "build123").IsPreRelease);
        }
    }
} 