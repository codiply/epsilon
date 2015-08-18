using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Models;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.Wrappers;
using Epsilon.Logic.Wrappers.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.UnitTests.Logic.Wrappers
{
    [TestFixture]
    public class GeoipRotatingClientTest
    {
        [Test]
        public async Task AllProvidersFail_RotationOfProvidersTest()
        {
            var ipAddress = "8.8.8.8";
            var geoipClientResponseStatus = WebClientResponseStatus.Error;
            var maxRotations = 3;
            var providers = EnumsHelper.GeoipProviderName.GetValues();
            var providerCount = providers.Count;
            var providerRotation = string.Join(",", providers.Select(x => EnumsHelper.GeoipProviderName.ToString(x)));

            var expectedProviders = Enumerable.Range(1, maxRotations).SelectMany(x => providers).ToList();

            var providersUsed = new List<GeoipProviderName>();
            var ipAddressesUsed = new List<string>();
            var adminAlerts = new List<Tuple<string, bool>>();
            var adminEvents = new List<Tuple<AdminEventLogKey, Dictionary<string, object>>>();

            var geoipClient = CreateGeoipClient((prov, ip) =>
            {
                providersUsed.Add(prov);
                ipAddressesUsed.Add(ip);
                return new GeoipClientResponse { Status = geoipClientResponseStatus };
            });
            var geoipRotatingClient = CreateGeoipRotatingClient(
                maxRotations,
                providerRotation,
                (adminAlertKey, doNotUseDatabase) => adminAlerts.Add(Tuple.Create(adminAlertKey, doNotUseDatabase)),
                (adminEventLogKey, extraInfo) => adminEvents.Add(Tuple.Create(adminEventLogKey, extraInfo)),
                geoipClient);


            var geoipRotatingClientResponse = await geoipRotatingClient.Geoip(ipAddress);

            Assert.AreEqual(WebClientResponseStatus.Error, geoipRotatingClientResponse.Status,
                "WebClientResponseStatus was not the expected.");

            Assert.AreEqual(expectedProviders.Count, providersUsed.Count,
                "The total number of geoip calls made is not the expected.");
            var expectedNumberOfGeoipClientFailures = providers.Count * maxRotations;
            Assert.AreEqual(expectedNumberOfGeoipClientFailures, 
                adminEvents.Count(x => x.Item1 == AdminEventLogKey.GeoipClientFailure),
                "The number of Admin Events for GeoipClientFailure logged is not the expected.");
            Assert.AreEqual(expectedNumberOfGeoipClientFailures,
                adminAlerts.Count(x => x.Item1.StartsWith(AdminAlertKey.GeoipRotatingClientProviderFailed_PREFIX)),
                "The number of Admin Alerts for GeoipClientFailure send is not the expected.");

            for (var i = 0; i < expectedProviders.Count; i++)
            {
                var expectedProvider = expectedProviders[i];
                Assert.AreEqual(expectedProvider, providersUsed[i],
                    string.Format("Provider used in call '{0}' is not the expected.", i + 1));
                Assert.AreEqual(ipAddress, ipAddressesUsed[i],
                    string.Format("IpAddress used in call '{0}' is not the expected.", i + 1));
                Assert.AreEqual(AdminEventLogKey.GeoipClientFailure,
                    adminEvents[i].Item1,
                    string.Format("The type of admin event raised in call '{0}' is not the expected.", i + 1));
                Assert.AreEqual(EnumsHelper.GeoipProviderName.ToString(expectedProvider), 
                    adminEvents[i].Item2[AdminEventLogExtraInfoKey.GeoipProviderName],
                    string.Format("The provider on the admin event raised in call '{0}' is not the expected.", i + 1));
                Assert.AreEqual(
                    AdminAlertKey.GeoipRotatingClientProviderFailed(expectedProvider, geoipClientResponseStatus),
                    adminAlerts[i].Item1,
                    string.Format("The key on the admin alert sent in call '{0}' is not the expected.", i + 1));
            }

            var totalNumberOfAdminEvents = expectedNumberOfGeoipClientFailures + 1;
            Assert.AreEqual(totalNumberOfAdminEvents, adminEvents.Count,
                "The total number of Admin Events logged is not the expected.");
            var lastAdminEven = adminEvents.Last();
            Assert.AreEqual(AdminEventLogKey.GeoipRotatingClientMaxRotationsReached,
                lastAdminEven.Item1,
                "The type of the final Admin Event logged is not the expected.");

            var totalNumberOfAdminAlerts = expectedNumberOfGeoipClientFailures + 1;
            Assert.AreEqual(totalNumberOfAdminAlerts, adminAlerts.Count,
                "The total number of Admin Alerts sent is not the expected.");
            var lastAdminAlert = adminAlerts.Last();
            Assert.AreEqual(AdminAlertKey.GeoipRotatingClientMaxRotationsReached,
                lastAdminAlert.Item1,
                "The key of the final Admin Alert sent is not the expected.");
        }

        [Test]
        public async Task SingleProviderFailTest()
        {
            var ipAddress = "8.8.8.8";
            var maxRotations = 3;
            var latitude = 1.0;
            var longitude = 1.0;
            var countryCode = "GB";

            var failingProvider = GeoipProviderName.Freegeoip;
            var successfulProvider = GeoipProviderName.Telize;
            var providerRotation =
                string.Format("{0},{1}",
                    EnumsHelper.GeoipProviderName.ToString(failingProvider),
                    EnumsHelper.GeoipProviderName.ToString(successfulProvider));

            var providersUsed = new List<GeoipProviderName>();
            var ipAddressesUsed = new List<string>();
            var adminAlerts = new List<Tuple<string, bool>>();
            var adminEvents = new List<Tuple<AdminEventLogKey, Dictionary<string, object>>>();

            var geoipClient = CreateGeoipClient((prov, ip) =>
            {
                providersUsed.Add(prov);
                ipAddressesUsed.Add(ip);
                if (prov == failingProvider)
                {
                    return new GeoipClientResponse { Status = WebClientResponseStatus.Error };
                }
                return new GeoipClientResponse {
                    Status = WebClientResponseStatus.Success,
                    GeoipProviderName = prov,
                    CountryCode = countryCode,
                    Latitude = latitude,
                    Longitude = longitude
                };
            });
            var geoipRotatingClient = CreateGeoipRotatingClient(
                maxRotations,
                providerRotation,
                (adminAlertKey, doNotUseDatabase) => adminAlerts.Add(Tuple.Create(adminAlertKey, doNotUseDatabase)),
                (adminEventLogKey, extraInfo) => adminEvents.Add(Tuple.Create(adminEventLogKey, extraInfo)),
                geoipClient);

            var geoipRotatingClientResponse = await geoipRotatingClient.Geoip(ipAddress);

            Assert.AreEqual(WebClientResponseStatus.Success, geoipRotatingClientResponse.Status,
                "WebClientResponseStatus was not the expected.");
            Assert.AreEqual(countryCode, geoipRotatingClientResponse.CountryCode,
                "CountryCode was not the expected.");
            Assert.AreEqual(latitude, geoipRotatingClientResponse.Latitude,
                "CountryCode was not the expected.");
            Assert.AreEqual(longitude, geoipRotatingClientResponse.Longitude,
                "CountryCode was not the expected.");
            Assert.AreEqual(successfulProvider, geoipRotatingClientResponse.GeoipProviderName,
                "GeoipProviderName on the response was not the expected.");

            Assert.AreEqual(2, providersUsed.Count,
                "The total number of geoip calls made is not the expected.");

            Assert.AreEqual(1, adminAlerts.Count,
                "The number of AdminAlerts sent is not the expected.");
            Assert.AreEqual(
                AdminAlertKey.GeoipRotatingClientProviderFailed(failingProvider, WebClientResponseStatus.Error),
                adminAlerts.Single().Item1);

            Assert.AreEqual(2, adminEvents.Count,
                "The number of AdminEvents sent is not the expected.");
            Assert.AreEqual(AdminEventLogKey.GeoipClientFailure, adminEvents[0].Item1,
                "Type of AdminEvent logged first is not the expected.");
            Assert.AreEqual(AdminEventLogKey.GeoipRotatingClientSuccessAfterFailures, adminEvents[1].Item1,
                "Type of AdminEvent logged second is not the expected.");
        }

        [Test]
        public void ConstructorThrowsIfProviderRotationCannotBeParsed()
        {
            var maxRotations = 3;
            var providers = EnumsHelper.GeoipProviderName.GetValues();
            var providerCount = providers.Count;
            var providerRotation = 
                string.Join(",", providers.Select(x => EnumsHelper.GeoipProviderName.ToString(x)))
                + ",NonExistentProvider";

            var geoipClient = CreateGeoipClient((prov, ip) => { return null; });
            Assert.Throws<Exception>(() => CreateGeoipRotatingClient(
                maxRotations,
                providerRotation,
                (adminAlertKey, doNotUseDatabase) => { },
                (adminEventLogKey, extraInfo) => { },
                geoipClient),
                string.Format("Constructor didn't throw exception for providerRotation '{0}'.", providerRotation));
        }

        private IGeoipRotatingClient CreateGeoipRotatingClient(
            int maxRotations,
            string providerRotation,
            Action<string, bool> adminAlertServiceSendCallback,
            Action<AdminEventLogKey, Dictionary<string, object>> adminEventLogServiceCallback,
            IGeoipClient geoipClient)
        {
            var config = new Mock<IGeoipRotatingClientConfig>();
            config.Setup(x => x.MaxRotations).Returns(maxRotations);
            config.Setup(x => x.ProviderRotation).Returns(providerRotation);

            var mockAdminAlertService = new Mock<IAdminAlertService>();
            mockAdminAlertService.Setup(x => x.SendAlert(It.IsAny<string>(), It.IsAny<bool>()))
                .Callback(adminAlertServiceSendCallback);

            var mockAdminEventLogService = new Mock<IAdminEventLogService>();
            mockAdminEventLogService.Setup(x => x.Log(It.IsAny<AdminEventLogKey>(), It.IsAny<Dictionary<string, object>>()))
                .Returns(Task.FromResult(1))
                .Callback(adminEventLogServiceCallback);

            return new GeoipRotatingClient(
                config.Object, mockAdminAlertService.Object, mockAdminEventLogService.Object, geoipClient);
        }

        private IGeoipClient CreateGeoipClient(
            Func<GeoipProviderName, string, GeoipClientResponse> callback)
        {
            var mockGeoipClient = new Mock<IGeoipClient>();
            mockGeoipClient.Setup(x => x.Geoip(It.IsAny<GeoipProviderName>(), It.IsAny<string>()))
                .Returns<GeoipProviderName, string>((provider, ipAddress) =>
                    Task.FromResult(callback(provider, ipAddress)));

            return mockGeoipClient.Object;
        }
    }
}
