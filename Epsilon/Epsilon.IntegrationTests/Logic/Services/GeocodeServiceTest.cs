using Epsilon.IntegrationTests.BaseFixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Constants.Enums;
using NUnit.Framework;
using System.Data.Entity;
using Epsilon.Logic.Wrappers.Interfaces;
using Moq;
using GeocodeSharp.Google;

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class GeocodeServiceTest : BaseIntegrationTestWithRollback
    {
        private readonly TimeSpan DelayBetweenCallsToTheAPI = TimeSpan.FromSeconds(0.2);

        #region GeocodeAddress

        [Test]
        public async Task GeocodeAddresss_ReturnsGeometryReturnedByGeocodeClient()
        {
            var container = CreateContainer();
            var service = container.Get<IGeocodeService>();

            var address = "229 Great Portland Street, London W1W5PN";
            var countryId = EnumsHelper.CountryId.ToString(CountryId.GB);

            await Task.Delay(DelayBetweenCallsToTheAPI);
            var response = await service.GeocodeAddress(address, countryId);

            Assert.AreEqual(GeocodeAddressStatus.Success, response.Status, "The status returned was not the expected.");
            Assert.IsNotNull(response.Geometry, "The Geometry on the GeocodeService response is null.");

            // I geocode the address independently using a client.
            await Task.Delay(DelayBetweenCallsToTheAPI);
            var geocodeClient = container.Get<IGeocodeClientFactory>().Create("");
            var geocodeClientResponse = await geocodeClient.GeocodeAddress(address, countryId);

            Assert.IsNotNull(geocodeClientResponse, "The geocode client response is null.");
            Assert.AreEqual(1, geocodeClientResponse.Results.Count(), "The geocode client response should contain a single result");
            var geocodeClientResponseResult = geocodeClientResponse.Results.Single();
            Assert.IsNotNull(geocodeClientResponseResult.Geometry, "The Geometry on the geocode client response result is null.");
            Assert.IsNotNull(geocodeClientResponseResult.Geometry.Location, "The Geometry.Location on the geocode client response result is null.");
            Assert.IsNotNull(geocodeClientResponseResult.Geometry.Viewport, "The Geometry.Viewport on the geocode client response result is null.");
            Assert.AreEqual(response.Geometry.Latitude, geocodeClientResponseResult.Geometry.Location.Latitude,
                "Found discrepancy in Latitude.");
            Assert.AreEqual(response.Geometry.Longitude, geocodeClientResponseResult.Geometry.Location.Longitude,
                "Found discrepancy in Longitude.");
            Assert.IsNotNull(geocodeClientResponseResult.Geometry.Viewport.Northeast,
                "The Geometry.Viewport.Northeast on the geocode client response result is null.");
            Assert.AreEqual(response.Geometry.ViewportNortheastLatitude, geocodeClientResponseResult.Geometry.Viewport.Northeast.Latitude,
                "Found discrepancy in ViewportNortheastLatitude.");
            Assert.AreEqual(response.Geometry.ViewportNortheastLongitude, geocodeClientResponseResult.Geometry.Viewport.Northeast.Longitude,
                "Found discrepancy in ViewportNortheastLongitude.");
            Assert.IsNotNull(geocodeClientResponseResult.Geometry.Viewport.Southwest,
                "The Geometry.Viewport.Southwest on the geocode client response result is null.");
            Assert.AreEqual(response.Geometry.ViewportSouthwestLatitude, geocodeClientResponseResult.Geometry.Viewport.Southwest.Latitude,
                "Found discrepancy in ViewportSouthwestLatitude.");
            Assert.AreEqual(response.Geometry.ViewportSouthwestLongitude, geocodeClientResponseResult.Geometry.Viewport.Southwest.Longitude,
                "Found discrepancy in ViewportSouthwestLongitude.");
        }

        #endregion

        #region GeocodePostcode

        [Test]
        public async Task GeocodePostcode_StoresGeometryReturnedByGeocodeClient()
        {
            var container = CreateContainer();
            var service = container.Get<IGeocodeService>();
            var clock = container.Get<IClock>();

            var postcode = "EC3N4AB";
            var countryId = EnumsHelper.CountryId.ToString(CountryId.GB);

            await Task.Delay(DelayBetweenCallsToTheAPI);
            var timeBefore = clock.OffsetNow;
            var status = await service.GeocodePostcode(postcode, countryId);

            Assert.AreEqual(GeocodePostcodeStatus.Success, status, "The status returned was not the expected.");
            var timeAfter = clock.OffsetNow;

            var retrievedPostcodeGeometry = await DbProbe.PostcodeGeometries.FindAsync(countryId, postcode);
            Assert.IsNotNull(retrievedPostcodeGeometry, "A PostcodeGeometry should be created in the database.");

            // I geocode the postcode independently using a client.
            await Task.Delay(DelayBetweenCallsToTheAPI);
            var geocodeClient = container.Get<IGeocodeClientFactory>().Create("");
            var geocodeClientResponse = await geocodeClient.GeocodeAddress(postcode, countryId);

            Assert.IsNotNull(geocodeClientResponse, "The geocode client response is null.");
            Assert.AreEqual(1, geocodeClientResponse.Results.Count(), "The geocode client response should contain a single result");
            var geocodeClientResponseResult = geocodeClientResponse.Results.Single();
            Assert.IsNotNull(geocodeClientResponseResult.Geometry, "The Geometry on the geocode client response result is null.");
            Assert.IsNotNull(geocodeClientResponseResult.Geometry.Location, "The Geometry.Location on the geocode client response result is null.");
            Assert.IsNotNull(geocodeClientResponseResult.Geometry.Viewport, "The Geometry.Viewport on the geocode client response result is null.");
            Assert.AreEqual(retrievedPostcodeGeometry.Latitude, geocodeClientResponseResult.Geometry.Location.Latitude,
                "Found discrepancy in Latitude.");
            Assert.AreEqual(retrievedPostcodeGeometry.Longitude, geocodeClientResponseResult.Geometry.Location.Longitude,
                "Found discrepancy in Longitude.");
            Assert.IsNotNull(geocodeClientResponseResult.Geometry.Viewport.Northeast,
                "The Geometry.Viewport.Northeast on the geocode client response result is null.");
            Assert.AreEqual(retrievedPostcodeGeometry.ViewportNortheastLatitude, geocodeClientResponseResult.Geometry.Viewport.Northeast.Latitude,
                "Found discrepancy in ViewportNortheastLatitude.");
            Assert.AreEqual(retrievedPostcodeGeometry.ViewportNortheastLongitude, geocodeClientResponseResult.Geometry.Viewport.Northeast.Longitude,
                "Found discrepancy in ViewportNortheastLongitude.");
            Assert.IsNotNull(geocodeClientResponseResult.Geometry.Viewport.Southwest,
                "The Geometry.Viewport.Southwest on the geocode client response result is null.");
            Assert.AreEqual(retrievedPostcodeGeometry.ViewportSouthwestLatitude, geocodeClientResponseResult.Geometry.Viewport.Southwest.Latitude,
                "Found discrepancy in ViewportSouthwestLatitude.");
            Assert.AreEqual(retrievedPostcodeGeometry.ViewportSouthwestLongitude, geocodeClientResponseResult.Geometry.Viewport.Southwest.Longitude,
                "Found discrepancy in ViewportSouthwestLongitude.");

            Assert.IsTrue(timeBefore <= retrievedPostcodeGeometry.GeocodedOn && retrievedPostcodeGeometry.GeocodedOn <= timeAfter,
                    "The field GeocodeOn is not in the expected range.");
        }



        [Test]
        public async Task GeocodePostcode_UsesExistingGeometryIfPresentInDatabase()
        {
            var container1 = CreateContainer();
            var service1 = container1.Get<IGeocodeService>();

            var postcode = "EC3N4AB";
            var countryId = EnumsHelper.CountryId.ToString(CountryId.GB);

            var retrievedPostcodeGeometryBefore1 = await DbProbe.PostcodeGeometries.FindAsync(countryId, postcode);
            Assert.IsNull(retrievedPostcodeGeometryBefore1, "A PostcodeGeometry should not be initially present in the database.");

            await Task.Delay(DelayBetweenCallsToTheAPI);
            var status1 = await service1.GeocodePostcode(postcode, countryId);
            Assert.AreEqual(GeocodePostcodeStatus.Success, status1, "The status of the first geocoding was not the expected.");

            var retrievedPostcodeGeometryBefore2 = await DbProbe.PostcodeGeometries.FindAsync(countryId, postcode);
            Assert.IsNotNull(retrievedPostcodeGeometryBefore2, "A PostcodeGeometry should be present in the database after the first geocoding.");

            var container2 = CreateContainer();
            var addressUsed = string.Empty;
            var regionUsed = string.Empty;
            SetupMockGeocodeClient((add, reg) => { addressUsed = add; regionUsed = reg; }, new GeocodeResponse());
            var service2 = container2.Get<IGeocodeService>();

            await Task.Delay(DelayBetweenCallsToTheAPI);
            var status2 = await service2.GeocodePostcode(postcode, countryId);
            Assert.AreEqual(GeocodePostcodeStatus.Success, status2, "The status of the second geocoding was not the expected.");
            Assert.IsNullOrEmpty(addressUsed, "The addressUsed should be null which means that the geocoding API was not called.");
            Assert.IsNullOrEmpty(regionUsed, "The addressUsed should be null which means that the geocoding API was not called.");
        }

        #endregion

        #region Country Examples: GB GeocodeAddress

        [Test]
        public async Task GeocodeAddress_GB_Success()
        {
            var container = CreateContainer();
            var service = container.Get<IGeocodeService>();
            
            var address = "229 Great Portland Street, London W1W5PN";
            var countryId = EnumsHelper.CountryId.ToString(CountryId.GB);

            await Task.Delay(DelayBetweenCallsToTheAPI);
            var response = await service.GeocodeAddress(address, countryId);

            Assert.AreEqual(GeocodeAddressStatus.Success, response.Status, "The status returned was not the expected.");
            // TODO_PANOS inspect geometry
        }

        [Test]
        public async Task GeocodeAddress_GB_Failure_StatusResultInWrongCountry()
        {
            var container = CreateContainer();
            var service = container.Get<IGeocodeService>();
            
            var address = "Λεωφόρος Βασιλίσσης Σοφίας 46, Αθήνα, Αττική, 11528";
            var countryId = EnumsHelper.CountryId.ToString(CountryId.GB);

            await Task.Delay(DelayBetweenCallsToTheAPI);
            var response = await service.GeocodeAddress(address, countryId);

            Assert.AreEqual(GeocodeAddressStatus.ResultInWrongCountry, response.Status, "The status returned was not the expected.");
            Assert.IsNull(response.Geometry, "The Geometry on the response should be null.");
        }

        [Test]
        public async Task GeocodeAddress_GB_Failure_StatusMultipleMatches()
        {
            var container = CreateContainer();
            var service = container.Get<IGeocodeService>();
            
            var address = "Royal Park, London";
            var countryId = EnumsHelper.CountryId.ToString(CountryId.GB);

            await Task.Delay(DelayBetweenCallsToTheAPI);
            var response = await service.GeocodeAddress(address, countryId);

            Assert.AreEqual(GeocodeAddressStatus.MultipleMatches, response.Status, "The status returned was not the expected.");
            Assert.IsNull(response.Geometry, "The Geometry on the response should be null.");
        }

        #endregion

        #region Country Examples: GB GeocodePostcode

        [Test]
        public async Task GeocodePostcode_GB_Success()
        {
            var container = CreateContainer();
            var service = container.Get<IGeocodeService>();

            var postcode = "EC3N4AB";
            var countryId = EnumsHelper.CountryId.ToString(CountryId.GB);

            await Task.Delay(DelayBetweenCallsToTheAPI);
            var status = await service.GeocodePostcode(postcode, countryId);

            Assert.AreEqual(GeocodePostcodeStatus.Success, status, "The status returned was not the expected.");

            var retrievedPostcodeGeometry = await DbProbe.PostcodeGeometries.FindAsync(countryId, postcode);
            Assert.IsNotNull(retrievedPostcodeGeometry, "A PostcodeGeometry should be created in the database.");
        }

        [Test]
        public async Task GeocodePostcode_GB_NumericPostcode_Failure()
        {
            var container = CreateContainer();
            var service = container.Get<IGeocodeService>();

            var postcode = "11528";
            var countryId = EnumsHelper.CountryId.ToString(CountryId.GB);

            await Task.Delay(DelayBetweenCallsToTheAPI);
            var status = await service.GeocodePostcode(postcode, countryId);

            Assert.AreNotEqual(GeocodePostcodeStatus.Success, status, "The status should not be Success.");
            Assert.AreNotEqual(GeocodePostcodeStatus.ServiceUnavailable, status, "The status should not be ServiceUnavailable.");
            Assert.AreNotEqual(GeocodePostcodeStatus.OverQueryLimitTriedMaxTimes, status, "The status should not be OverQueryLimitTriedMaxTimes.");

            var retrievedPostcodeGeometry = await DbProbe.PostcodeGeometries.FindAsync(countryId, postcode);
            Assert.IsNull(retrievedPostcodeGeometry, "A PostcodeGeometry should not be created in the database.");
        }

        [Test]
        public async Task GeocodePostcode_GB_PostcodePrefix_Failure_StatusResultWithWrongType()
        {
            var container = CreateContainer();
            var service = container.Get<IGeocodeService>();

            var postcode = "SE1";
            var countryId = EnumsHelper.CountryId.ToString(CountryId.GB);

            await Task.Delay(DelayBetweenCallsToTheAPI);
            var status = await service.GeocodePostcode(postcode, countryId);

            Assert.AreEqual(GeocodePostcodeStatus.ResultWithWrongType, status, "The status returned was not the expected.");

            var retrievedPostcodeGeometry = await DbProbe.PostcodeGeometries.FindAsync(countryId, postcode);
            Assert.IsNull(retrievedPostcodeGeometry, "A PostcodeGeometry should not be created in the database.");
        }

        #endregion

        #region Country Examples: GR GeocodeAddress

        [Test]
        public async Task GeocodeAddress_GR_Success()
        {
            var container = CreateContainer();
            var service = container.Get<IGeocodeService>();

            var hiltonAthensAddress = "Λεωφόρος Βασιλίσσης Σοφίας 46, Αθήνα, Αττική, 11528";
            var countryId = EnumsHelper.CountryId.ToString(CountryId.GR);

            await Task.Delay(DelayBetweenCallsToTheAPI);
            var response = await service.GeocodeAddress(hiltonAthensAddress, countryId);

            Assert.AreEqual(GeocodeAddressStatus.Success, response.Status, "The status returned was not the expected.");
            // TODO_PANOS inspect_geometry
        }

        [Test]
        public async Task GeocodeAddress_GR_Failure_StatusResultInWrongCountry()
        {
            var container = CreateContainer();
            var service = container.Get<IGeocodeService>();

            var internationalStudenHouseLondonAddress = "229 Great Portland Street, London W1W5PN";
            var countryId = EnumsHelper.CountryId.ToString(CountryId.GR);

            await Task.Delay(DelayBetweenCallsToTheAPI);
            var response = await service.GeocodeAddress(internationalStudenHouseLondonAddress, countryId);

            Assert.AreEqual(GeocodeAddressStatus.ResultInWrongCountry, response.Status, "The status returned was not the expected.");
            Assert.IsNull(response.Geometry, "The Geometry on the response should be null.");
        }

        #endregion

        #region Country Examples: GR GeocodePostcode

        [Test]
        public async Task GeocodePostcode_GR_Success()
        {
            var container = CreateContainer();
            var service = container.Get<IGeocodeService>();

            var postcode = "11528";
            var countryId = EnumsHelper.CountryId.ToString(CountryId.GR);

            await Task.Delay(DelayBetweenCallsToTheAPI);
            var status = await service.GeocodePostcode(postcode, countryId);

            Assert.AreEqual(GeocodePostcodeStatus.Success, status, "The status returned was not the expected.");

            var retrievedPostcodeGeometry = await DbProbe.PostcodeGeometries.FindAsync(countryId, postcode);
            Assert.IsNotNull(retrievedPostcodeGeometry, "A PostcodeGeometry should be created in the database.");
        }

        [Test]
        public async Task GeocodePostcode_GR_Failure()
        {
            var container = CreateContainer();
            var service = container.Get<IGeocodeService>();

            var postcode = "EC3N4AB";
            var countryId = EnumsHelper.CountryId.ToString(CountryId.GR);

            await Task.Delay(DelayBetweenCallsToTheAPI);
            var status = await service.GeocodePostcode(postcode, countryId);

            Assert.AreNotEqual(GeocodePostcodeStatus.Success, status, "The status should not be Success.");
            Assert.AreNotEqual(GeocodePostcodeStatus.ServiceUnavailable, status, "The status should not be ServiceUnavailable.");
            Assert.AreNotEqual(GeocodePostcodeStatus.OverQueryLimitTriedMaxTimes, status, "The status should not be OverQueryLimitTriedMaxTimes.");

            var retrievedPostcodeGeometry = await DbProbe.PostcodeGeometries.FindAsync(countryId, postcode);
            Assert.IsNull(retrievedPostcodeGeometry, "A PostcodeGeometry should not be created in the database.");
        }

        #endregion

        #region Private Helper Methods
        
        private void SetupMockGeocodeClient(Action<string, string> callback, GeocodeResponse geocodeClientResponse)
        {
            var mockGeocodeClientWrapper = new Mock<IGeocodeClientWrapper>();
            mockGeocodeClientWrapper.Setup(x => x.GeocodeAddress(It.IsAny<string>(), It.IsAny<string>()))
                .Callback(callback)
                .Returns(Task.FromResult(geocodeClientResponse));

            var mockGeocodeClientFactory = new Mock<IGeocodeClientFactory>();
            mockGeocodeClientFactory.Setup(x => x.Create(It.IsAny<string>()))
                .Returns(mockGeocodeClientWrapper.Object);
        }

        #endregion
    }
}
