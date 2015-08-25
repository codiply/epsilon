using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Wrappers.Interfaces;
using Ninject;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Epsilon.IntegrationTests.Logic.Wrappers
{
    public class GeoipRotatingClientTest : BaseIntegrationTestWithRollback
    {
        [Test]
        public async Task GeoipRotatingClientWorks()
        {
            var ipAddress = "8.8.8.8";
            var expectedLatitude = 37.386;
            var expectedLongitude = -122.084;
            var acceptableDelta = 0.001;

            var container = CreateContainer();
            var geoipRotatingClient = container.Get<IGeoipRotatingClient>();


            var response = await geoipRotatingClient.Geoip(ipAddress);

            Assert.AreEqual(WebClientResponseStatus.Success, response.Status, "Status is not the expected.");
            Assert.AreEqual(expectedLatitude, response.Latitude, acceptableDelta, "Latitude is not the expected.");
            Assert.AreEqual(expectedLongitude, response.Longitude, acceptableDelta, "Longitude is not the expected.");
            Assert.IsNullOrEmpty(response.ErrorMessage, "ErrorMessage is not the expected.");
        }
    }
}
