using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Threading.Tasks;

namespace ClarusMensAPI.FunctionalTests;

[TestClass]
public class BasicApiTests : FunctionalTestBase
{
    [TestMethod]
    public async Task Get_Root_ReturnsSuccessStatusCode()
    {
        // Arrange
        // Already set up in base class
        
        // Act
        var response = await Client.GetAsync("/");
        
        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
} 