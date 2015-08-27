using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using GeocodeSharp.Google;
using Moq;
using Ninject;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class GeocodeServiceTest : BaseIntegrationTestWithRollback
    {
        private const string STATUS_TEXT_OK = "OK";
        private const string STATUS_TEXT_ZERO_RESULTS = "ZERO_RESULTS";
        private const string STATUS_TEXT_OVER_QUERY_LIMIT = "OVER_QUERY_LIMIT";
        private const string STATUS_TEXT_REQUEST_DENIED = "REQUEST_DENIED";
        private const string STATUS_TEXT_INVALID_REQUEST = "INVALID_REQUEST";
        private const string STATUS_TEXT_UNKNOWN_ERROR = "UNKNOWN_ERROR";
        private const string STATUS_TEXT_UNEXPECTED = "SOME_UNEXPECTED_TEXT";

        private const double DELAY_MIN_FRACTION = 0.6;
        private const double DELAY_MAX_FRACTION = 1.4;

        private readonly double LatitudeLongitudePrecision = 0.00001;
        private readonly TimeSpan DelayBetweenCallsToTheApi = TimeSpan.FromSeconds(0.2);

        #region GeocodeAddress

        [Test]
        public async Task GeocodeAddresss_ReturnsGeometryReturnedByGeocodeClient()
        {
            var container = CreateContainer();
            var service = container.Get<IGeocodeService>();

            var address = "229 Great Portland Street, London W1W5PN";
            var countryId = EnumsHelper.CountryId.ToString(CountryId.GB);

            await Task.Delay(DelayBetweenCallsToTheApi);
            var response = await service.GeocodeAddress(address, countryId);

            Assert.AreEqual(GeocodeAddressStatus.Success, response.Status, "The status returned was not the expected.");
            Assert.IsNotNull(response.Geometry, "The Geometry on the GeocodeService response is null.");

            // I geocode the address independently using a client.
            await Task.Delay(DelayBetweenCallsToTheApi);
            var geocodeClient = container.Get<IGeocodeClientFactory>().Create("");
            var geocodeClientResponse = await geocodeClient.GeocodeAddress(address, countryId);

            Assert.IsNotNull(geocodeClientResponse, "The geocode client response is null.");
            Assert.AreEqual(1, geocodeClientResponse.Results.Count(), "The geocode client response should contain a single result");
            var geocodeClientResponseResult = geocodeClientResponse.Results.Single();
            Assert.IsNotNull(geocodeClientResponseResult.Geometry, "The Geometry on the geocode client response result is null.");
            Assert.IsNotNull(geocodeClientResponseResult.Geometry.Location, "The Geometry.Location on the geocode client response result is null.");
            Assert.IsNotNull(geocodeClientResponseResult.Geometry.Viewport, "The Geometry.Viewport on the geocode client response result is null.");
            Assert.That(response.Geometry.Latitude, Is.EqualTo(geocodeClientResponseResult.Geometry.Location.Latitude).Within(LatitudeLongitudePrecision),
                "Found discrepancy in Latitude.");
            Assert.That(response.Geometry.Longitude, Is.EqualTo(geocodeClientResponseResult.Geometry.Location.Longitude).Within(LatitudeLongitudePrecision),
                "Found discrepancy in Longitude.");
            Assert.IsNotNull(geocodeClientResponseResult.Geometry.Viewport.Northeast,
                "The Geometry.Viewport.Northeast on the geocode client response result is null.");
            Assert.That(response.Geometry.ViewportNortheastLatitude, Is.EqualTo(geocodeClientResponseResult.Geometry.Viewport.Northeast.Latitude).Within(LatitudeLongitudePrecision),
                "Found discrepancy in ViewportNortheastLatitude.");
            Assert.That(response.Geometry.ViewportNortheastLongitude, Is.EqualTo(geocodeClientResponseResult.Geometry.Viewport.Northeast.Longitude).Within(LatitudeLongitudePrecision),
                "Found discrepancy in ViewportNortheastLongitude.");
            Assert.IsNotNull(geocodeClientResponseResult.Geometry.Viewport.Southwest,
                "The Geometry.Viewport.Southwest on the geocode client response result is null.");
            Assert.That(response.Geometry.ViewportSouthwestLatitude, Is.EqualTo(geocodeClientResponseResult.Geometry.Viewport.Southwest.Latitude).Within(LatitudeLongitudePrecision),
                "Found discrepancy in ViewportSouthwestLatitude.");
            Assert.That(response.Geometry.ViewportSouthwestLongitude, Is.EqualTo(geocodeClientResponseResult.Geometry.Viewport.Southwest.Longitude).Within(LatitudeLongitudePrecision),
                "Found discrepancy in ViewportSouthwestLongitude.");
        }

        [Test]
        public async Task GeocodeAddress_GeocodeClientReturnsNull()
        {
            var apiKey = "api-key";
            var delayBetweenRetries = TimeSpan.FromSeconds(0.2);
            var maxRetries = 1;

            var address = "some-address";
            var region = EnumsHelper.CountryId.ToString(CountryId.GB);

            var container = CreateContainer();
            SetupConfig(container, apiKey, delayBetweenRetries, maxRetries);
            SetupGeocodeClient(container, apiKey, address, region, (add, reg) => Task.FromResult<GeocodeResponse>(null));

            string adminAlertKeyUsed = null;
            bool? adminAlertDoNotUseDatabase = null;
            AdminEventLogKey? adminEventLogKeyUsed = null;
            Dictionary<string, object> extraInfoUsed = null;

            SetupAdminAlertService(container, (key, doNotUseDatabase) => { adminAlertKeyUsed = key; adminAlertDoNotUseDatabase = doNotUseDatabase; });
            SetupAdminEventLogService(container, (key, extraInfo) => { adminEventLogKeyUsed = key; extraInfoUsed = extraInfo; });
            SetupElmahHelper(container, (ex) => { throw new Exception("ElmahHelper should not be called."); });

            var service = container.Get<IGeocodeService>();

            var response = await service.GeocodeAddress(address, region);

            Assert.AreEqual(GeocodeAddressStatus.ServiceUnavailable, response.Status, "The Status on the response is not the expected.");

            Assert.IsNotNull(adminAlertKeyUsed, "An admin alert was not sent.");
            Assert.AreEqual(AdminAlertKey.GoogleGeocodeApiClientReturnedNull, adminAlertKeyUsed,
                "The key on the admin alert is not the expected.");
            Assert.AreEqual(false, adminAlertDoNotUseDatabase, "The default value for doNotUseDatabase was not used.");

            Assert.IsNotNull(adminEventLogKeyUsed, "An admin event was not logged.");
            Assert.AreEqual(AdminEventLogKey.GoogleGeocodeApiClientReturnedNull, adminEventLogKeyUsed,
                "The key used in the admin event is not the expected.");
            Assert.IsNull(extraInfoUsed, "The extra info on the admin event is not the expected.");
        }

        [Test]
        public async Task GeocodeAddress_GeocodeClientReturns_InvalidRequest()
        {
            var apiKey = "api-key";
            var delayBetweenRetries = TimeSpan.FromSeconds(0.2);
            var maxRetries = 1;

            var address = "some-address";
            var region = EnumsHelper.CountryId.ToString(CountryId.GB);

            var container = CreateContainer();
            SetupConfig(container, apiKey, delayBetweenRetries, maxRetries);
            SetupGeocodeClient(container, apiKey, address, region, (add, reg) => 
                Task.FromResult(new GeocodeResponse() { StatusText = STATUS_TEXT_INVALID_REQUEST }));

            string adminAlertKeyUsed = null;
            bool? adminAlertDoNotUseDatabase = null;
            AdminEventLogKey? adminEventLogKeyUsed = null;
            Dictionary<string, object> extraInfoUsed = null;

            SetupAdminAlertService(container, (key, doNotUseDatabase) => { adminAlertKeyUsed = key; adminAlertDoNotUseDatabase = doNotUseDatabase; });
            SetupAdminEventLogService(container, (key, extraInfo) => { adminEventLogKeyUsed = key; extraInfoUsed = extraInfo; });
            SetupElmahHelper(container, (ex) => { throw new Exception("ElmahHelper should not be called."); });

            var service = container.Get<IGeocodeService>();
            var response = await service.GeocodeAddress(address, region);
            
            Assert.AreEqual(GeocodeAddressStatus.ServiceUnavailable, response.Status, "The Status on the response is not the expected.");
            Assert.IsNotNull(adminAlertKeyUsed, "An admin alert was not sent.");
            Assert.AreEqual(AdminAlertKey.GoogleGeocodeApiStatusInvalidRequest, adminAlertKeyUsed,
                "The key on the admin alert is not the expected.");
            Assert.AreEqual(false, adminAlertDoNotUseDatabase, "The default value for doNotUseDatabase was not used.");

            Assert.IsNotNull(adminEventLogKeyUsed, "An admin event was not logged.");
            Assert.AreEqual(AdminEventLogKey.GoogleGeocodeApiStatusInvalidRequest, adminEventLogKeyUsed,
                "The key used in the admin event is not the expected.");
            Assert.IsNotNull(extraInfoUsed, "The extra info on the admin event is not the expected.");

            Assert.AreEqual(address, extraInfoUsed[AdminEventLogExtraInfoKey.Address],
                "Address on the extra info is not the expected.");
            Assert.AreEqual(region, extraInfoUsed[AdminEventLogExtraInfoKey.Region],
                "Region on the extra info is not the expected.");
        }

        [Test]
        public async Task GeocodeAddress_GeocodeClientReturns_RequestDenied()
        {
            var apiKey = "api-key";
            var delayBetweenRetries = TimeSpan.FromSeconds(0.2);
            var maxRetries = 1;

            var address = "some-address";
            var region = EnumsHelper.CountryId.ToString(CountryId.GB);

            var container = CreateContainer();
            SetupConfig(container, apiKey, delayBetweenRetries, maxRetries);
            SetupGeocodeClient(container, apiKey, address, region, (add, reg) =>
                Task.FromResult(new GeocodeResponse() { StatusText = STATUS_TEXT_REQUEST_DENIED }));

            string adminAlertKeyUsed = null;
            bool? adminAlertDoNotUseDatabase = null;
            AdminEventLogKey? adminEventLogKeyUsed = null;
            Dictionary<string, object> extraInfoUsed = null;

            SetupAdminAlertService(container, (key, doNotUseDatabase) => { adminAlertKeyUsed = key; adminAlertDoNotUseDatabase = doNotUseDatabase; });
            SetupAdminEventLogService(container, (key, extraInfo) => { adminEventLogKeyUsed = key; extraInfoUsed = extraInfo; });

            var service = container.Get<IGeocodeService>();
            var response = await service.GeocodeAddress(address, region);

            Assert.AreEqual(GeocodeAddressStatus.ServiceUnavailable, response.Status, "The Status on the response is not the expected.");
            Assert.IsNotNull(adminAlertKeyUsed, "An admin alert was not sent.");
            Assert.AreEqual(AdminAlertKey.GoogleGeocodeApiStatusRequestDenied, adminAlertKeyUsed,
                "The key on the admin alert is not the expected.");
            Assert.AreEqual(false, adminAlertDoNotUseDatabase, "The default value for doNotUseDatabase was not used.");

            Assert.IsNotNull(adminEventLogKeyUsed, "An admin event was not logged.");
            Assert.AreEqual(AdminEventLogKey.GoogleGeocodeApiStatusRequestDenied, adminEventLogKeyUsed,
                "The key used in the admin event is not the expected.");
            Assert.IsNull(extraInfoUsed, "The extra info on the admin event is not the expected.");
        }

        [Test]
        public async Task GeocodeAddress_GeocodeClientReturns_UnknownError()
        {
            var apiKey = "api-key";
            var delayBetweenRetries = TimeSpan.FromSeconds(0.2);
            var maxRetries = 1;

            var address = "some-address";
            var region = EnumsHelper.CountryId.ToString(CountryId.GB);

            var container = CreateContainer();
            SetupConfig(container, apiKey, delayBetweenRetries, maxRetries);
            SetupGeocodeClient(container, apiKey, address, region, (add, reg) =>
                Task.FromResult(new GeocodeResponse() { StatusText = STATUS_TEXT_UNKNOWN_ERROR }));

            string adminAlertKeyUsed = null;
            bool? adminAlertDoNotUseDatabase = null;
            AdminEventLogKey? adminEventLogKeyUsed = null;
            Dictionary<string, object> extraInfoUsed = null;

            SetupAdminAlertService(container, (key, doNotUseDatabase) => { adminAlertKeyUsed = key; adminAlertDoNotUseDatabase = doNotUseDatabase; });
            SetupAdminEventLogService(container, (key, extraInfo) => { adminEventLogKeyUsed = key; extraInfoUsed = extraInfo; });
            SetupElmahHelper(container, (ex) => { throw new Exception("ElmahHelper should not be called."); });

            var service = container.Get<IGeocodeService>();
            var response = await service.GeocodeAddress(address, region);

            Assert.AreEqual(GeocodeAddressStatus.ServiceUnavailable, response.Status, "The Status on the response is not the expected.");
            Assert.IsNotNull(adminAlertKeyUsed, "An admin alert was not sent.");
            Assert.AreEqual(AdminAlertKey.GoogleGeocodeApiStatusUknownError, adminAlertKeyUsed,
                "The key on the admin alert is not the expected.");
            Assert.AreEqual(false, adminAlertDoNotUseDatabase, "The default value for doNotUseDatabase was not used.");

            Assert.IsNotNull(adminEventLogKeyUsed, "An admin event was not logged.");
            Assert.AreEqual(AdminEventLogKey.GoogleGeocodeApiStatusUknownError, adminEventLogKeyUsed,
                "The key used in the admin event is not the expected.");
            Assert.IsNull(extraInfoUsed, "The extra info on the admin event is not the expected.");
        }

        [Test]
        public async Task GeocodeAddress_GeocodeClientReturns_Unexpected()
        {
            var apiKey = "api-key";
            var delayBetweenRetries = TimeSpan.FromSeconds(0.2);
            var maxRetries = 1;

            var address = "some-address";
            var region = EnumsHelper.CountryId.ToString(CountryId.GB);

            var container = CreateContainer();
            SetupConfig(container, apiKey, delayBetweenRetries, maxRetries);
            SetupGeocodeClient(container, apiKey, address, region, (add, reg) =>
                Task.FromResult(new GeocodeResponse() { StatusText = STATUS_TEXT_UNEXPECTED }));

            string adminAlertKeyUsed = null;
            bool? adminAlertDoNotUseDatabase = null;
            AdminEventLogKey? adminEventLogKeyUsed = null;
            Dictionary<string, object> extraInfoUsed = null;

            SetupAdminAlertService(container, (key, doNotUseDatabase) => { adminAlertKeyUsed = key; adminAlertDoNotUseDatabase = doNotUseDatabase; });
            SetupAdminEventLogService(container, (key, extraInfo) => { adminEventLogKeyUsed = key; extraInfoUsed = extraInfo; });
            SetupElmahHelper(container, (ex) => { throw new Exception("ElmahHelper should not be called."); });

            var service = container.Get<IGeocodeService>();
            var response = await service.GeocodeAddress(address, region);

            Assert.AreEqual(GeocodeAddressStatus.ServiceUnavailable, response.Status, "The Status on the response is not the expected.");
            Assert.IsNotNull(adminAlertKeyUsed, "An admin alert was not sent.");
            Assert.AreEqual(AdminAlertKey.GoogleGeocodeApiStatusUnexpected, adminAlertKeyUsed,
                "The key on the admin alert is not the expected.");
            Assert.AreEqual(false, adminAlertDoNotUseDatabase, "The default value for doNotUseDatabase was not used.");

            Assert.IsNotNull(adminEventLogKeyUsed, "An admin event was not logged.");
            Assert.AreEqual(AdminEventLogKey.GoogleGeocodeApiStatusUnexpected, adminEventLogKeyUsed,
                "The key used in the admin event is not the expected.");
            Assert.IsNotNull(extraInfoUsed, "The extra info on the admin event is not the expected.");
            Assert.AreEqual(STATUS_TEXT_UNEXPECTED, extraInfoUsed[AdminEventLogExtraInfoKey.ResponseStatus],
                "ResponseStatus on the extra info is not the expected.");
        }

        [Test]
        public async Task GeocodeAddress_GeocodeClientThrowsException()
        {
            var apiKey = "api-key";
            var delayBetweenRetries = TimeSpan.FromSeconds(0.2);
            var maxRetries = 1;

            var address = "some-address";
            var exceptionMessage = "exception-message";
            var region = EnumsHelper.CountryId.ToString(CountryId.GB);

            var container = CreateContainer();
            SetupConfig(container, apiKey, delayBetweenRetries, maxRetries);
            var exceptionToThrow = new Exception(exceptionMessage);
            SetupGeocodeClient(container, apiKey, address, region, (add, reg) => { throw exceptionToThrow; });

            string adminAlertKeyUsed = null;
            bool? adminAlertDoNotUseDatabase = null;
            AdminEventLogKey? adminEventLogKeyUsed = null;
            Dictionary<string, object> extraInfoUsed = null;
            Exception exceptionLogged = null;

            SetupAdminAlertService(container, (key, doNotUseDatabase) => { adminAlertKeyUsed = key; adminAlertDoNotUseDatabase = doNotUseDatabase; });
            SetupAdminEventLogService(container, (key, extraInfo) => { adminEventLogKeyUsed = key; extraInfoUsed = extraInfo; });
            SetupElmahHelper(container, (ex) => { exceptionLogged = ex; });

            var service = container.Get<IGeocodeService>();
            var response = await service.GeocodeAddress(address, region);

            Assert.AreEqual(GeocodeAddressStatus.ServiceUnavailable, response.Status, "The Status on the response is not the expected.");
            Assert.IsNotNull(adminAlertKeyUsed, "An admin alert was not sent.");
            Assert.AreEqual(AdminAlertKey.GoogleGeocodeApiClientException, adminAlertKeyUsed,
                "The key on the admin alert is not the expected.");
            Assert.AreEqual(false, adminAlertDoNotUseDatabase, "The default value for doNotUseDatabase was not used.");

            Assert.IsNotNull(adminEventLogKeyUsed, "An admin event was not logged.");
            Assert.AreEqual(AdminEventLogKey.GoogleGeocodeApiClientException, adminEventLogKeyUsed,
                "The key used in the admin event is not the expected.");
            Assert.IsNotNull(extraInfoUsed, "The extra info on the admin event is not the expected.");
            Assert.AreEqual(exceptionMessage, extraInfoUsed[AdminEventLogExtraInfoKey.ErrorMessage],
                "ErrorMessage on the extra info is not the expected.");

            Assert.IsNotNull(exceptionLogged, "The exception was not logged using the ElmahHelper.");
            Assert.AreSame(exceptionToThrow, exceptionLogged, "The exception logged is not the expected.");
        }

        [Test]
        public async Task GeocodeAddress_GeocodeClientReturnsOverQueryLimitOnce()
        {
            var apiKey = "api-key";
            var delayBetweenRetriesInSeconds = 0.2;
            var delayBetweenRetries = TimeSpan.FromSeconds(delayBetweenRetriesInSeconds);
            var maxRetries = 1;

            var address = "229 Great Portland Street, London W1W5PN";
            var region = EnumsHelper.CountryId.ToString(CountryId.GB);

            var container = CreateContainer();
            var clock = container.Get<IClock>();
            var realGeocodeClientFactory = container.Get<IGeocodeClientFactory>();
            var realApiKey = container.Get<IGeocodeServiceConfig>().GoogleApiServerKey;
            SetupConfig(container, apiKey, delayBetweenRetries, maxRetries);
            List<DateTimeOffset> geocodeClientCalledOn = new List<DateTimeOffset>();

            SetupGeocodeClient(container, apiKey, address, region, (add, reg) => 
            {
                var isFirstTime = !geocodeClientCalledOn.Any();
                geocodeClientCalledOn.Add(clock.OffsetNow);
                if (isFirstTime)
                    return Task.FromResult(new GeocodeResponse { StatusText = STATUS_TEXT_OVER_QUERY_LIMIT });
                else
                    return realGeocodeClientFactory.Create(realApiKey).GeocodeAddress(add, reg);
            });

            string adminAlertKeyUsed = null;
            bool? adminAlertDoNotUseDatabase = null;
            AdminEventLogKey? adminEventLogKeyUsed = null;
            Dictionary<string, object> extraInfoUsed = null;
            Exception exceptionLogged = null;

            SetupAdminAlertService(container, (key, doNotUseDatabase) => { adminAlertKeyUsed = key; adminAlertDoNotUseDatabase = doNotUseDatabase; });
            SetupAdminEventLogService(container, (key, extraInfo) => { adminEventLogKeyUsed = key; extraInfoUsed = extraInfo; });
            SetupElmahHelper(container, (ex) => { exceptionLogged = ex; });

            var service = container.Get<IGeocodeService>();
            var response = await service.GeocodeAddress(address, region);

            Assert.AreEqual(GeocodeAddressStatus.Success, response.Status, "The Status on the response is not the expected.");
            Assert.IsNull(adminAlertKeyUsed, "An admin alert should not be sent.");
            
            Assert.IsNotNull(adminEventLogKeyUsed, "An admin event was not logged.");
            Assert.AreEqual(AdminEventLogKey.GoogleGeocodeApiStatusOverQueryLimitSuccessAfterRetrying, adminEventLogKeyUsed,
                "The key used in the admin event is not the expected.");
            Assert.IsNotNull(extraInfoUsed, "The extra info on the admin event is not the expected.");
            Assert.AreEqual(AppConstant.GEOCODE_QUERY_TYPE_ADDRESS, extraInfoUsed[AdminEventLogExtraInfoKey.QueryType],
                "QueryType on the extraInfo is not the expected.");
            Assert.AreEqual(1, extraInfoUsed[AdminEventLogExtraInfoKey.RetriesUntilSuccess],
                "RetriesUntilSuccess on the extraInfo is not the expected.");

            Assert.IsNull(exceptionLogged, "No exceptions should be logged.");

            Assert.AreEqual(2, geocodeClientCalledOn.Count(), "GeocodeClient was not called the expected number of times.");
            var firstTimeGeocodeClientCalledOn = geocodeClientCalledOn[0];
            var secondTimeGeocodeClientCalledOn = geocodeClientCalledOn[1];
            var secondsBetweenCalls = (secondTimeGeocodeClientCalledOn - firstTimeGeocodeClientCalledOn).TotalSeconds;

            Assert.That(secondsBetweenCalls, Is.InRange(DELAY_MIN_FRACTION * delayBetweenRetriesInSeconds, DELAY_MAX_FRACTION * delayBetweenRetriesInSeconds),
                "The delay between the two calls is not in the expected range.");
        }

        [Test]
        public async Task GeocodeAddress_GeocodeClientKeepsReturningOverQueryLimit()
        {
            var apiKey = "api-key";
            var delayBetweenRetriesInSeconds = 0.2;
            var delayBetweenRetries = TimeSpan.FromSeconds(delayBetweenRetriesInSeconds);
            var maxRetries = 2;

            var address = "229 Great Portland Street, London W1W5PN";
            var region = EnumsHelper.CountryId.ToString(CountryId.GB);

            var container = CreateContainer();
            var clock = container.Get<IClock>();
            SetupConfig(container, apiKey, delayBetweenRetries, maxRetries);
            List<DateTimeOffset> geocodeClientCalledOn = new List<DateTimeOffset>();

            SetupGeocodeClient(container, apiKey, address, region, (add, reg) =>
            {
                geocodeClientCalledOn.Add(clock.OffsetNow);
                return Task.FromResult(new GeocodeResponse { StatusText = STATUS_TEXT_OVER_QUERY_LIMIT });
            });

            string adminAlertKeyUsed = null;
            bool? adminAlertDoNotUseDatabase = null;
            AdminEventLogKey? adminEventLogKeyUsed = null;
            Dictionary<string, object> extraInfoUsed = null;
            Exception exceptionLogged = null;

            SetupAdminAlertService(container, (key, doNotUseDatabase) => { adminAlertKeyUsed = key; adminAlertDoNotUseDatabase = doNotUseDatabase; });
            SetupAdminEventLogService(container, (key, extraInfo) => { adminEventLogKeyUsed = key; extraInfoUsed = extraInfo; });
            SetupElmahHelper(container, (ex) => { exceptionLogged = ex; });

            var service = container.Get<IGeocodeService>();
            var response = await service.GeocodeAddress(address, region);

            Assert.AreEqual(GeocodeAddressStatus.OverQueryLimitTriedMaxTimes, response.Status, "The Status on the response is not the expected.");

            Assert.IsNotNull(adminAlertKeyUsed, "An admin alert was not sent.");
            Assert.AreEqual(AdminAlertKey.GoogleGeocodeApiStatusOverQueryLimitMaxRetriesReached, adminAlertKeyUsed,
                "The key on the admin alert is not the expected.");
            Assert.AreEqual(false, adminAlertDoNotUseDatabase, "The default value for doNotUseDatabase was not used.");

            Assert.IsNotNull(adminEventLogKeyUsed, "An admin event was not logged.");
            Assert.AreEqual(AdminEventLogKey.GoogleGeocodeApiStatusOverQueryLimitMaxRetriesReached, adminEventLogKeyUsed,
                "The key used in the admin event is not the expected.");
            Assert.IsNotNull(extraInfoUsed, "The extra info on the admin event is not the expected.");
            Assert.AreEqual(AppConstant.GEOCODE_QUERY_TYPE_ADDRESS, extraInfoUsed[AdminEventLogExtraInfoKey.QueryType],
                "QueryType on the extraInfo is not the expected.");
            Assert.AreEqual(maxRetries, extraInfoUsed[AdminEventLogExtraInfoKey.MaximumRetries],
                "MaxRetries on the extraInfo is not the expected.");

            Assert.IsNull(exceptionLogged, "No exceptions should be logged.");

            Assert.AreEqual(maxRetries + 1, geocodeClientCalledOn.Count(), "GeocodeClient was not called the expected number of times.");
            var firstTimeGeocodeClientCalledOn = geocodeClientCalledOn[0];
            var secondTimeGeocodeClientCalledOn = geocodeClientCalledOn[1];
            var thirdTimeGeocodeClientCalledOn = geocodeClientCalledOn[2];
            var secondsBetweenCalls1 = (secondTimeGeocodeClientCalledOn - firstTimeGeocodeClientCalledOn).TotalSeconds;
            var secondsBetweenCalls2 = (thirdTimeGeocodeClientCalledOn - secondTimeGeocodeClientCalledOn).TotalSeconds;

            Assert.That(secondsBetweenCalls1, Is.InRange(DELAY_MIN_FRACTION * delayBetweenRetriesInSeconds, DELAY_MAX_FRACTION * delayBetweenRetriesInSeconds),
                "The delay between the first two calls is not in the expected range.");
            Assert.That(secondsBetweenCalls2, Is.InRange(DELAY_MIN_FRACTION * delayBetweenRetriesInSeconds, DELAY_MAX_FRACTION * delayBetweenRetriesInSeconds),
                "The delay between the last two calls is not in the expected range.");
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

            await Task.Delay(DelayBetweenCallsToTheApi);
            var timeBefore = clock.OffsetNow;
            var status = await service.GeocodePostcode(postcode, countryId);

            Assert.AreEqual(GeocodePostcodeStatus.Success, status, "The status returned was not the expected.");
            var timeAfter = clock.OffsetNow;

            var retrievedPostcodeGeometry = await DbProbe.PostcodeGeometries.FindAsync(countryId, postcode);
            Assert.IsNotNull(retrievedPostcodeGeometry, "A PostcodeGeometry should be created in the database.");

            // I geocode the postcode independently using a client.
            await Task.Delay(DelayBetweenCallsToTheApi);
            var geocodeClient = container.Get<IGeocodeClientFactory>().Create("");
            var geocodeClientResponse = await geocodeClient.GeocodeAddress(postcode, countryId);

            Assert.IsNotNull(geocodeClientResponse, "The geocode client response is null.");
            Assert.AreEqual(1, geocodeClientResponse.Results.Count(), "The geocode client response should contain a single result");
            var geocodeClientResponseResult = geocodeClientResponse.Results.Single();
            Assert.IsNotNull(geocodeClientResponseResult.Geometry, "The Geometry on the geocode client response result is null.");
            Assert.IsNotNull(geocodeClientResponseResult.Geometry.Location, "The Geometry.Location on the geocode client response result is null.");
            Assert.IsNotNull(geocodeClientResponseResult.Geometry.Viewport, "The Geometry.Viewport on the geocode client response result is null.");
            Assert.That(retrievedPostcodeGeometry.Latitude, Is.EqualTo(geocodeClientResponseResult.Geometry.Location.Latitude).Within(LatitudeLongitudePrecision),
                "Found discrepancy in Latitude.");
            Assert.That(retrievedPostcodeGeometry.Longitude, Is.EqualTo(geocodeClientResponseResult.Geometry.Location.Longitude).Within(LatitudeLongitudePrecision),
                "Found discrepancy in Longitude.");
            Assert.IsNotNull(geocodeClientResponseResult.Geometry.Viewport.Northeast,
                "The Geometry.Viewport.Northeast on the geocode client response result is null.");
            Assert.That(retrievedPostcodeGeometry.ViewportNortheastLatitude, Is.EqualTo(geocodeClientResponseResult.Geometry.Viewport.Northeast.Latitude).Within(LatitudeLongitudePrecision),
                "Found discrepancy in ViewportNortheastLatitude.");
            Assert.That(retrievedPostcodeGeometry.ViewportNortheastLongitude, Is.EqualTo(geocodeClientResponseResult.Geometry.Viewport.Northeast.Longitude).Within(LatitudeLongitudePrecision),
                "Found discrepancy in ViewportNortheastLongitude.");
            Assert.IsNotNull(geocodeClientResponseResult.Geometry.Viewport.Southwest,
                "The Geometry.Viewport.Southwest on the geocode client response result is null.");
            Assert.That(retrievedPostcodeGeometry.ViewportSouthwestLatitude, Is.EqualTo(geocodeClientResponseResult.Geometry.Viewport.Southwest.Latitude).Within(LatitudeLongitudePrecision),
                "Found discrepancy in ViewportSouthwestLatitude.");
            Assert.That(retrievedPostcodeGeometry.ViewportSouthwestLongitude, Is.EqualTo(geocodeClientResponseResult.Geometry.Viewport.Southwest.Longitude).Within(LatitudeLongitudePrecision),
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

            await Task.Delay(DelayBetweenCallsToTheApi);
            var status1 = await service1.GeocodePostcode(postcode, countryId);
            Assert.AreEqual(GeocodePostcodeStatus.Success, status1, "The status of the first geocoding was not the expected.");

            var retrievedPostcodeGeometryBefore2 = await DbProbe.PostcodeGeometries.FindAsync(countryId, postcode);
            Assert.IsNotNull(retrievedPostcodeGeometryBefore2, "A PostcodeGeometry should be present in the database after the first geocoding.");

            var container2 = CreateContainer();
            var apiKey = container2.Get<IGeocodeServiceConfig>().GoogleApiServerKey;
            var geocodeClientUsed = false;

            SetupGeocodeClient(container2, apiKey, postcode, countryId, (add, reg) => { geocodeClientUsed = true;
                return Task.FromResult(new GeocodeResponse()); });
            var service2 = container2.Get<IGeocodeService>();

            await Task.Delay(DelayBetweenCallsToTheApi);
            var status2 = await service2.GeocodePostcode(postcode, countryId);
            Assert.AreEqual(GeocodePostcodeStatus.Success, status2, "The status of the second geocoding was not the expected.");
            Assert.IsFalse(geocodeClientUsed, "The geocoding API should not be called the second time.");
        }

        [Test]
        public async Task GeocodePostcode_GeocodeClientReturnsNull()
        {
            var apiKey = "api-key";
            var delayBetweenRetries = TimeSpan.FromSeconds(0.2);
            var maxRetries = 1;

            var postcode = "some-postcode";
            var region = EnumsHelper.CountryId.ToString(CountryId.GB);

            var container = CreateContainer();
            SetupConfig(container, apiKey, delayBetweenRetries, maxRetries);
            SetupGeocodeClient(container, apiKey, postcode, region, (add, reg) => Task.FromResult<GeocodeResponse>(null));

            string adminAlertKeyUsed = null;
            bool? adminAlertDoNotUseDatabase = null;
            AdminEventLogKey? adminEventLogKeyUsed = null;
            Dictionary<string, object> extraInfoUsed = null;

            SetupAdminAlertService(container, (key, doNotUseDatabase) => { adminAlertKeyUsed = key; adminAlertDoNotUseDatabase = doNotUseDatabase; });
            SetupAdminEventLogService(container, (key, extraInfo) => { adminEventLogKeyUsed = key; extraInfoUsed = extraInfo; });
            SetupElmahHelper(container, (ex) => { throw new Exception("ElmahHelper should not be called."); });

            var service = container.Get<IGeocodeService>();
            var status = await service.GeocodePostcode(postcode, region);

            Assert.AreEqual(GeocodePostcodeStatus.ServiceUnavailable, status, "The status is not the expected.");

            Assert.IsNotNull(adminAlertKeyUsed, "An admin alert was not sent.");
            Assert.AreEqual(AdminAlertKey.GoogleGeocodeApiClientReturnedNull, adminAlertKeyUsed,
                "The key on the admin alert is not the expected.");
            Assert.AreEqual(false, adminAlertDoNotUseDatabase, "The default value for doNotUseDatabase was not used.");

            Assert.IsNotNull(adminEventLogKeyUsed, "An admin event was not logged.");
            Assert.AreEqual(AdminEventLogKey.GoogleGeocodeApiClientReturnedNull, adminEventLogKeyUsed,
                "The key used in the admin event is not the expected.");
            Assert.IsNull(extraInfoUsed, "The extra info on the admin event is not the expected.");

            var retrievedPostcodeGeometry = await DbProbe.PostcodeGeometries.FindAsync(region, postcode);
            Assert.IsNull(retrievedPostcodeGeometry, "A PostcodeGeometry should not be stored in the database.");

        }

        [Test]
        public async Task GeocodePostcode_GeocodeClientReturns_InvalidRequest()
        {
            var apiKey = "api-key";
            var delayBetweenRetries = TimeSpan.FromSeconds(0.2);
            var maxRetries = 1;

            var postcode = "some-postcode";
            var region = EnumsHelper.CountryId.ToString(CountryId.GB);

            var container = CreateContainer();
            SetupConfig(container, apiKey, delayBetweenRetries, maxRetries);
            SetupGeocodeClient(container, apiKey, postcode, region, (add, reg) => 
                Task.FromResult(new GeocodeResponse() { StatusText = STATUS_TEXT_INVALID_REQUEST }));

            string adminAlertKeyUsed = null;
            bool? adminAlertDoNotUseDatabase = null;
            AdminEventLogKey? adminEventLogKeyUsed = null;
            Dictionary<string, object> extraInfoUsed = null;

            SetupAdminAlertService(container, (key, doNotUseDatabase) => { adminAlertKeyUsed = key; adminAlertDoNotUseDatabase = doNotUseDatabase; });
            SetupAdminEventLogService(container, (key, extraInfo) => { adminEventLogKeyUsed = key; extraInfoUsed = extraInfo; });
            SetupElmahHelper(container, (ex) => { throw new Exception("ElmahHelper should not be called."); });

            var service = container.Get<IGeocodeService>();
            var status = await service.GeocodePostcode(postcode, region);

            Assert.AreEqual(GeocodePostcodeStatus.ServiceUnavailable, status, "The status is not the expected.");
            Assert.IsNotNull(adminAlertKeyUsed, "An admin alert was not sent.");
            Assert.AreEqual(AdminAlertKey.GoogleGeocodeApiStatusInvalidRequest, adminAlertKeyUsed,
                "The key on the admin alert is not the expected.");
            Assert.AreEqual(false, adminAlertDoNotUseDatabase, "The default value for doNotUseDatabase was not used.");

            Assert.IsNotNull(adminEventLogKeyUsed, "An admin event was not logged.");
            Assert.AreEqual(AdminEventLogKey.GoogleGeocodeApiStatusInvalidRequest, adminEventLogKeyUsed,
                "The key used in the admin event is not the expected.");
            Assert.IsNotNull(extraInfoUsed, "The extra info on the admin event is not the expected.");

            Assert.AreEqual(postcode, extraInfoUsed[AdminEventLogExtraInfoKey.Address],
                "Address on the extra info is not the expected.");
            Assert.AreEqual(region, extraInfoUsed[AdminEventLogExtraInfoKey.Region],
                "Region on the extra info is not the expected.");

            var retrievedPostcodeGeometry = await DbProbe.PostcodeGeometries.FindAsync(region, postcode);
            Assert.IsNull(retrievedPostcodeGeometry, "A PostcodeGeometry should not be stored in the database.");
        }

        [Test]
        public async Task GeocodePostcode_GeocodeClientReturns_RequestDenied()
        {
            var apiKey = "api-key";
            var delayBetweenRetries = TimeSpan.FromSeconds(0.2);
            var maxRetries = 1;

            var postcode = "some-postcode";
            var region = EnumsHelper.CountryId.ToString(CountryId.GB);

            var container = CreateContainer();
            SetupConfig(container, apiKey, delayBetweenRetries, maxRetries);
            SetupGeocodeClient(container, apiKey, postcode, region, (add, reg) => 
                Task.FromResult(new GeocodeResponse() { StatusText = STATUS_TEXT_REQUEST_DENIED }));

            string adminAlertKeyUsed = null;
            bool? adminAlertDoNotUseDatabase = null;
            AdminEventLogKey? adminEventLogKeyUsed = null;
            Dictionary<string, object> extraInfoUsed = null;

            SetupAdminAlertService(container, (key, doNotUseDatabase) => { adminAlertKeyUsed = key; adminAlertDoNotUseDatabase = doNotUseDatabase; });
            SetupAdminEventLogService(container, (key, extraInfo) => { adminEventLogKeyUsed = key; extraInfoUsed = extraInfo; });
            SetupElmahHelper(container, (ex) => { throw new Exception("ElmahHelper should not be called."); });

            var service = container.Get<IGeocodeService>();
            var status = await service.GeocodePostcode(postcode, region);

            Assert.AreEqual(GeocodePostcodeStatus.ServiceUnavailable, status, "The status is not the expected.");
            Assert.IsNotNull(adminAlertKeyUsed, "An admin alert was not sent.");
            Assert.AreEqual(AdminAlertKey.GoogleGeocodeApiStatusRequestDenied, adminAlertKeyUsed,
                "The key on the admin alert is not the expected.");
            Assert.AreEqual(false, adminAlertDoNotUseDatabase, "The default value for doNotUseDatabase was not used.");

            Assert.IsNotNull(adminEventLogKeyUsed, "An admin event was not logged.");
            Assert.AreEqual(AdminEventLogKey.GoogleGeocodeApiStatusRequestDenied, adminEventLogKeyUsed,
                "The key used in the admin event is not the expected.");
            Assert.IsNull(extraInfoUsed, "The extra info on the admin event is not the expected.");

            var retrievedPostcodeGeometry = await DbProbe.PostcodeGeometries.FindAsync(region, postcode);
            Assert.IsNull(retrievedPostcodeGeometry, "A PostcodeGeometry should not be stored in the database.");
        }

        [Test]
        public async Task GeocodePostcode_GeocodeClientReturns_UnknownError()
        {
            var apiKey = "api-key";
            var delayBetweenRetries = TimeSpan.FromSeconds(0.2);
            var maxRetries = 1;

            var postcode = "some-postcode";
            var region = EnumsHelper.CountryId.ToString(CountryId.GB);

            var container = CreateContainer();
            SetupConfig(container, apiKey, delayBetweenRetries, maxRetries);
            SetupGeocodeClient(container, apiKey, postcode, region, (add, reg) => 
                Task.FromResult(new GeocodeResponse() { StatusText = STATUS_TEXT_UNKNOWN_ERROR }));

            string adminAlertKeyUsed = null;
            bool? adminAlertDoNotUseDatabase = null;
            AdminEventLogKey? adminEventLogKeyUsed = null;
            Dictionary<string, object> extraInfoUsed = null;

            SetupAdminAlertService(container, (key, doNotUseDatabase) => { adminAlertKeyUsed = key; adminAlertDoNotUseDatabase = doNotUseDatabase; });
            SetupAdminEventLogService(container, (key, extraInfo) => { adminEventLogKeyUsed = key; extraInfoUsed = extraInfo; });
            SetupElmahHelper(container, (ex) => { throw new Exception("ElmahHelper should not be called."); });

            var service = container.Get<IGeocodeService>();
            var status = await service.GeocodePostcode(postcode, region);

            Assert.AreEqual(GeocodePostcodeStatus.ServiceUnavailable, status, "The status is not the expected.");

            Assert.IsNotNull(adminAlertKeyUsed, "An admin alert was not sent.");
            Assert.AreEqual(AdminAlertKey.GoogleGeocodeApiStatusUknownError, adminAlertKeyUsed,
                "The key on the admin alert is not the expected.");
            Assert.AreEqual(false, adminAlertDoNotUseDatabase, "The default value for doNotUseDatabase was not used.");

            Assert.IsNotNull(adminEventLogKeyUsed, "An admin event was not logged.");
            Assert.AreEqual(AdminEventLogKey.GoogleGeocodeApiStatusUknownError, adminEventLogKeyUsed,
                "The key used in the admin event is not the expected.");
            Assert.IsNull(extraInfoUsed, "The extra info on the admin event is not the expected.");

            var retrievedPostcodeGeometry = await DbProbe.PostcodeGeometries.FindAsync(region, postcode);
            Assert.IsNull(retrievedPostcodeGeometry, "A PostcodeGeometry should not be stored in the database.");
        }

        [Test]
        public async Task GeocodePostcode_GeocodeClientReturns_Unexpected()
        {
            var apiKey = "api-key";
            var delayBetweenRetries = TimeSpan.FromSeconds(0.2);
            var maxRetries = 1;

            var postcode = "some-postcode";
            var region = EnumsHelper.CountryId.ToString(CountryId.GB);

            var container = CreateContainer();
            SetupConfig(container, apiKey, delayBetweenRetries, maxRetries);
            SetupGeocodeClient(container, apiKey, postcode, region, (add, reg) => 
                Task.FromResult(new GeocodeResponse() { StatusText = STATUS_TEXT_UNEXPECTED }));
            SetupElmahHelper(container, (ex) => { throw new Exception("ElmahHelper should not be called."); });

            string adminAlertKeyUsed = null;
            bool? adminAlertDoNotUseDatabase = null;
            AdminEventLogKey? adminEventLogKeyUsed = null;
            Dictionary<string, object> extraInfoUsed = null;

            SetupAdminAlertService(container, (key, doNotUseDatabase) => { adminAlertKeyUsed = key; adminAlertDoNotUseDatabase = doNotUseDatabase; });
            SetupAdminEventLogService(container, (key, extraInfo) => { adminEventLogKeyUsed = key; extraInfoUsed = extraInfo; });

            var service = container.Get<IGeocodeService>();
            var status = await service.GeocodePostcode(postcode, region);

            Assert.AreEqual(GeocodePostcodeStatus.ServiceUnavailable, status, "The status is not the expected.");
            Assert.IsNotNull(adminAlertKeyUsed, "An admin alert was not sent.");
            Assert.AreEqual(AdminAlertKey.GoogleGeocodeApiStatusUnexpected, adminAlertKeyUsed,
                "The key on the admin alert is not the expected.");
            Assert.AreEqual(false, adminAlertDoNotUseDatabase, "The default value for doNotUseDatabase was not used.");

            Assert.IsNotNull(adminEventLogKeyUsed, "An admin event was not logged.");
            Assert.AreEqual(AdminEventLogKey.GoogleGeocodeApiStatusUnexpected, adminEventLogKeyUsed,
                "The key used in the admin event is not the expected.");
            Assert.IsNotNull(extraInfoUsed, "The extra info on the admin event is not the expected.");
            Assert.AreEqual(STATUS_TEXT_UNEXPECTED, extraInfoUsed[AdminEventLogExtraInfoKey.ResponseStatus],
                "ResponseStatus on the extra info is not the expected.");

            var retrievedPostcodeGeometry = await DbProbe.PostcodeGeometries.FindAsync(region, postcode);
            Assert.IsNull(retrievedPostcodeGeometry, "A PostcodeGeometry should not be stored in the database.");
        }

        [Test]
        public async Task GeocodePostcode_GeocodeClientThrowsException()
        {
            var apiKey = "api-key";
            var delayBetweenRetries = TimeSpan.FromSeconds(0.2);
            var maxRetries = 1;

            var postcode = "some-postcode";
            var exceptionMessage = "exception-message";
            var region = EnumsHelper.CountryId.ToString(CountryId.GB);

            var container = CreateContainer();
            SetupConfig(container, apiKey, delayBetweenRetries, maxRetries);
            var exceptionToThrow = new Exception(exceptionMessage);
            SetupGeocodeClient(container, apiKey, postcode, region, (add, reg) => { throw exceptionToThrow; });

            string adminAlertKeyUsed = null;
            bool? adminAlertDoNotUseDatabase = null;
            AdminEventLogKey? adminEventLogKeyUsed = null;
            Dictionary<string, object> extraInfoUsed = null;
            Exception exceptionLogged = null;

            SetupAdminAlertService(container, (key, doNotUseDatabase) => { adminAlertKeyUsed = key; adminAlertDoNotUseDatabase = doNotUseDatabase; });
            SetupAdminEventLogService(container, (key, extraInfo) => { adminEventLogKeyUsed = key; extraInfoUsed = extraInfo; });
            SetupElmahHelper(container, (ex) => { exceptionLogged = ex; });

            var service = container.Get<IGeocodeService>();
            var status = await service.GeocodePostcode(postcode, region);

            Assert.AreEqual(GeocodePostcodeStatus.ServiceUnavailable, status, "The status is not the expected.");
            Assert.IsNotNull(adminAlertKeyUsed, "An admin alert was not sent.");
            Assert.AreEqual(AdminAlertKey.GoogleGeocodeApiClientException, adminAlertKeyUsed,
                "The key on the admin alert is not the expected.");
            Assert.AreEqual(false, adminAlertDoNotUseDatabase, "The default value for doNotUseDatabase was not used.");

            Assert.IsNotNull(adminEventLogKeyUsed, "An admin event was not logged.");
            Assert.AreEqual(AdminEventLogKey.GoogleGeocodeApiClientException, adminEventLogKeyUsed,
                "The key used in the admin event is not the expected.");
            Assert.IsNotNull(extraInfoUsed, "The extra info on the admin event is not the expected.");
            Assert.AreEqual(exceptionMessage, extraInfoUsed[AdminEventLogExtraInfoKey.ErrorMessage],
                "ErrorMessage on the extra info is not the expected.");

            Assert.IsNotNull(exceptionLogged, "The exception was not logged using the ElmahHelper.");
            Assert.AreSame(exceptionToThrow, exceptionLogged, "The exception logged is not the expected.");
        }

        [Test]
        public async Task GeocodePostcode_GeocodeClientReturnsOverQueryLimitOnce()
        {
            var apiKey = "api-key";
            var delayBetweenRetriesInSeconds = 0.2;
            var delayBetweenRetries = TimeSpan.FromSeconds(delayBetweenRetriesInSeconds);
            var maxRetries = 1;

            var postcode = "W1W5PN";
            var region = EnumsHelper.CountryId.ToString(CountryId.GB);

            var container = CreateContainer();
            var clock = container.Get<IClock>();
            var realGeocodeClientFactory = container.Get<IGeocodeClientFactory>();
            var realApiKey = container.Get<IGeocodeServiceConfig>().GoogleApiServerKey;
            SetupConfig(container, apiKey, delayBetweenRetries, maxRetries);
            List<DateTimeOffset> geocodeClientCalledOn = new List<DateTimeOffset>();

            SetupGeocodeClient(container, apiKey, postcode, region, (add, reg) =>
            {
                var isFirstTime = !geocodeClientCalledOn.Any();
                geocodeClientCalledOn.Add(clock.OffsetNow);
                if (isFirstTime)
                    return Task.FromResult(new GeocodeResponse { StatusText = STATUS_TEXT_OVER_QUERY_LIMIT });
                else
                    return realGeocodeClientFactory.Create(realApiKey).GeocodeAddress(add, reg);
            });

            string adminAlertKeyUsed = null;
            bool? adminAlertDoNotUseDatabase = null;
            AdminEventLogKey? adminEventLogKeyUsed = null;
            Dictionary<string, object> extraInfoUsed = null;
            Exception exceptionLogged = null;

            SetupAdminAlertService(container, (key, doNotUseDatabase) => { adminAlertKeyUsed = key; adminAlertDoNotUseDatabase = doNotUseDatabase; });
            SetupAdminEventLogService(container, (key, extraInfo) => { adminEventLogKeyUsed = key; extraInfoUsed = extraInfo; });
            SetupElmahHelper(container, (ex) => { exceptionLogged = ex; });

            var service = container.Get<IGeocodeService>();
            var status = await service.GeocodePostcode(postcode, region);

            Assert.AreEqual(GeocodePostcodeStatus.Success, status, "The status is not the expected.");
            Assert.IsNull(adminAlertKeyUsed, "An admin alert should not be sent.");

            Assert.IsNotNull(adminEventLogKeyUsed, "An admin event was not logged.");
            Assert.AreEqual(AdminEventLogKey.GoogleGeocodeApiStatusOverQueryLimitSuccessAfterRetrying, adminEventLogKeyUsed,
                "The key used in the admin event is not the expected.");
            Assert.IsNotNull(extraInfoUsed, "The extra info on the admin event is not the expected.");
            Assert.AreEqual(AppConstant.GEOCODE_QUERY_TYPE_POSTCODE, extraInfoUsed[AdminEventLogExtraInfoKey.QueryType],
                "QueryType on the extraInfo is not the expected.");
            Assert.AreEqual(1, extraInfoUsed[AdminEventLogExtraInfoKey.RetriesUntilSuccess],
                "RetriesUntilSuccess on the extraInfo is not the expected.");

            Assert.IsNull(exceptionLogged, "No exceptions should be logged.");

            Assert.AreEqual(2, geocodeClientCalledOn.Count(), "GeocodeClient was not called the expected number of times.");
            var firstTimeGeocodeClientCalledOn = geocodeClientCalledOn[0];
            var secondTimeGeocodeClientCalledOn = geocodeClientCalledOn[1];
            var secondsBetweenCalls = (secondTimeGeocodeClientCalledOn - firstTimeGeocodeClientCalledOn).TotalSeconds;

            Assert.That(secondsBetweenCalls, Is.InRange(DELAY_MIN_FRACTION * delayBetweenRetriesInSeconds, DELAY_MAX_FRACTION * delayBetweenRetriesInSeconds),
                "The delay between the two calls is not in the expected range.");
        }

        [Test]
        public async Task GeocodePostcode_GeocodeClientKeepsReturningOverQueryLimit()
        {
            var apiKey = "api-key";
            var delayBetweenRetriesInSeconds = 0.2;
            var delayBetweenRetries = TimeSpan.FromSeconds(delayBetweenRetriesInSeconds);
            var maxRetries = 2;

            var postcode = "W1W5PN";
            var region = EnumsHelper.CountryId.ToString(CountryId.GB);

            var container = CreateContainer();
            var clock = container.Get<IClock>();
            SetupConfig(container, apiKey, delayBetweenRetries, maxRetries);
            List<DateTimeOffset> geocodeClientCalledOn = new List<DateTimeOffset>();

            SetupGeocodeClient(container, apiKey, postcode, region, (add, reg) =>
            {
                geocodeClientCalledOn.Add(clock.OffsetNow);
                return Task.FromResult(new GeocodeResponse { StatusText = STATUS_TEXT_OVER_QUERY_LIMIT });
            });

            string adminAlertKeyUsed = null;
            bool? adminAlertDoNotUseDatabase = null;
            AdminEventLogKey? adminEventLogKeyUsed = null;
            Dictionary<string, object> extraInfoUsed = null;
            Exception exceptionLogged = null;

            SetupAdminAlertService(container, (key, doNotUseDatabase) => { adminAlertKeyUsed = key; adminAlertDoNotUseDatabase = doNotUseDatabase; });
            SetupAdminEventLogService(container, (key, extraInfo) => { adminEventLogKeyUsed = key; extraInfoUsed = extraInfo; });
            SetupElmahHelper(container, (ex) => { exceptionLogged = ex; });

            var service = container.Get<IGeocodeService>();
            var status = await service.GeocodePostcode(postcode, region);

            Assert.AreEqual(GeocodePostcodeStatus.OverQueryLimitTriedMaxTimes, status, "The status is not the expected.");

            Assert.IsNotNull(adminAlertKeyUsed, "An admin alert was not sent.");
            Assert.AreEqual(AdminAlertKey.GoogleGeocodeApiStatusOverQueryLimitMaxRetriesReached, adminAlertKeyUsed,
                "The key on the admin alert is not the expected.");
            Assert.AreEqual(false, adminAlertDoNotUseDatabase, "The default value for doNotUseDatabase was not used.");

            Assert.IsNotNull(adminEventLogKeyUsed, "An admin event was not logged.");
            Assert.AreEqual(AdminEventLogKey.GoogleGeocodeApiStatusOverQueryLimitMaxRetriesReached, adminEventLogKeyUsed,
                "The key used in the admin event is not the expected.");
            Assert.IsNotNull(extraInfoUsed, "The extra info on the admin event is not the expected.");
            Assert.AreEqual(AppConstant.GEOCODE_QUERY_TYPE_POSTCODE, extraInfoUsed[AdminEventLogExtraInfoKey.QueryType],
                "QueryType on the extraInfo is not the expected.");
            Assert.AreEqual(maxRetries, extraInfoUsed[AdminEventLogExtraInfoKey.MaximumRetries],
                "MaxRetries on the extraInfo is not the expected.");

            Assert.IsNull(exceptionLogged, "No exceptions should be logged.");

            Assert.AreEqual(maxRetries + 1, geocodeClientCalledOn.Count(), "GeocodeClient was not called the expected number of times.");
            var firstTimeGeocodeClientCalledOn = geocodeClientCalledOn[0];
            var secondTimeGeocodeClientCalledOn = geocodeClientCalledOn[1];
            var thirdTimeGeocodeClientCalledOn = geocodeClientCalledOn[2];
            var secondsBetweenCalls1 = (secondTimeGeocodeClientCalledOn - firstTimeGeocodeClientCalledOn).TotalSeconds;
            var secondsBetweenCalls2 = (thirdTimeGeocodeClientCalledOn - secondTimeGeocodeClientCalledOn).TotalSeconds;

            Assert.That(secondsBetweenCalls1, Is.InRange(DELAY_MIN_FRACTION * delayBetweenRetriesInSeconds, DELAY_MAX_FRACTION * delayBetweenRetriesInSeconds),
                "The delay between the first two calls is not in the expected range.");
            Assert.That(secondsBetweenCalls2, Is.InRange(DELAY_MIN_FRACTION * delayBetweenRetriesInSeconds, DELAY_MAX_FRACTION * delayBetweenRetriesInSeconds),
                "The delay between the last two calls is not in the expected range.");
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

            await Task.Delay(DelayBetweenCallsToTheApi);
            var response = await service.GeocodeAddress(address, countryId);

            Assert.AreEqual(GeocodeAddressStatus.Success, response.Status, "The status returned was not the expected.");

            Assert.IsNotNull(response.Geometry, "Geometry on the response was null.");
            Assert.That(response.Geometry.Latitude, Is.EqualTo(51.5236113).Within(LatitudeLongitudePrecision),
                "Geometry.Latitude on the response was not the expected.");
            Assert.That(response.Geometry.Longitude, Is.EqualTo(-0.1444806).Within(LatitudeLongitudePrecision),
                "Geometry.Latitude on the response was not the expected.");
        }

        [Test]
        public async Task GeocodeAddress_GB_Failure_StatusResultInWrongCountry()
        {
            var container = CreateContainer();
            var service = container.Get<IGeocodeService>();
            
            var address = "Λεωφόρος Βασιλίσσης Σοφίας 46, Αθήνα, Αττική, 11528";
            var countryId = EnumsHelper.CountryId.ToString(CountryId.GB);

            await Task.Delay(DelayBetweenCallsToTheApi);
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

            await Task.Delay(DelayBetweenCallsToTheApi);
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

            await Task.Delay(DelayBetweenCallsToTheApi);
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

            await Task.Delay(DelayBetweenCallsToTheApi);
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

            await Task.Delay(DelayBetweenCallsToTheApi);
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

            await Task.Delay(DelayBetweenCallsToTheApi);
            var response = await service.GeocodeAddress(hiltonAthensAddress, countryId);

            Assert.AreEqual(GeocodeAddressStatus.Success, response.Status, "The status returned was not the expected.");

            Assert.IsNotNull(response.Geometry, "Geometry on the response was null.");
            Assert.That(response.Geometry.Latitude, Is.EqualTo(37.9761732).Within(LatitudeLongitudePrecision),
                "Geometry.Latitude on the response was not the expected.");
            Assert.That(response.Geometry.Longitude, Is.EqualTo(23.7505799).Within(LatitudeLongitudePrecision),
                "Geometry.Latitude on the response was not the expected.");
        }
       
        [Test]
        public async Task GeocodeAddress_GR_Failure_StatusResultInWrongCountry()
        {
            var container = CreateContainer();
            var service = container.Get<IGeocodeService>();

            var internationalStudenHouseLondonAddress = "229 Great Portland Street, London W1W5PN";
            var countryId = EnumsHelper.CountryId.ToString(CountryId.GR);

            await Task.Delay(DelayBetweenCallsToTheApi);
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

            await Task.Delay(DelayBetweenCallsToTheApi);
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

            await Task.Delay(DelayBetweenCallsToTheApi);
            var status = await service.GeocodePostcode(postcode, countryId);

            Assert.AreNotEqual(GeocodePostcodeStatus.Success, status, "The status should not be Success.");
            Assert.AreNotEqual(GeocodePostcodeStatus.ServiceUnavailable, status, "The status should not be ServiceUnavailable.");
            Assert.AreNotEqual(GeocodePostcodeStatus.OverQueryLimitTriedMaxTimes, status, "The status should not be OverQueryLimitTriedMaxTimes.");

            var retrievedPostcodeGeometry = await DbProbe.PostcodeGeometries.FindAsync(countryId, postcode);
            Assert.IsNull(retrievedPostcodeGeometry, "A PostcodeGeometry should not be created in the database.");
        }

        #endregion

        #region Private Helper Methods
        
        private void SetupGeocodeClient(
            IKernel container,
            string expectedApiKey, 
            string expectedAddress,
            string expectedRegion,
            Func<string, string, Task<GeocodeResponse>> geocodeClientResponseFunction)
        {
            var mockGeocodeClientWrapper = new Mock<IGeocodeClientWrapper>();
            mockGeocodeClientWrapper.Setup(x => x.GeocodeAddress(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((address, region) =>
                {
                    if (!address.Equals(expectedAddress))
                        throw new Exception(string.Format("I was expecting address '{0}' to be used in the GeocodeClient but instead I got '{0}'",
                            expectedAddress, address));
                    if (!region.Equals(expectedRegion))
                        throw new Exception(string.Format("I was expecting region '{0}' to be used in the GeocodeClient but instead I got '{0}'",
                            expectedRegion, region));
                    return geocodeClientResponseFunction(address, region);
                });

            var mockGeocodeClientFactory = new Mock<IGeocodeClientFactory>();
            mockGeocodeClientFactory.Setup(x => x.Create(It.IsAny<string>()))
                .Returns<string>((apiKey) =>
                {
                    if (!apiKey.Equals(expectedApiKey))
                        throw new Exception(string.Format("Expected API key '{0}' but '{1}' was used instead.",
                            expectedApiKey, apiKey));
                    return mockGeocodeClientWrapper.Object;
                });

            container.Rebind<IGeocodeClientFactory>().ToConstant(mockGeocodeClientFactory.Object);
        }

        private void SetupConfig(
            IKernel container,
            string googleApiServerKey,
            TimeSpan overQueryLimitDelayBetweenRetries,
            int overQueryLimitMaxRetries)
        {
            var mockConfig = new Mock<IGeocodeServiceConfig>();

            mockConfig.Setup(x => x.GoogleApiServerKey).Returns(googleApiServerKey);
            mockConfig.Setup(x => x.OverQueryLimitDelayBetweenRetries).Returns(overQueryLimitDelayBetweenRetries);
            mockConfig.Setup(x => x.OverQueryLimitMaxRetries).Returns(overQueryLimitMaxRetries);

            container.Rebind<IGeocodeServiceConfig>().ToConstant(mockConfig.Object);
        }

        #endregion
    }
}