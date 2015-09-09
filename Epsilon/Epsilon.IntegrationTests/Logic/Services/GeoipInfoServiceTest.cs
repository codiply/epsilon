using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.Models;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using Moq;
using Ninject;
using NUnit.Framework;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Epsilon.IntegrationTests.Logic.Services
{
    [TestFixture]
    public class GeoipInfoServiceTest : BaseIntegrationTestWithRollback
    {

        #region GetInfo

        [Test]
        public async Task GetInfo_CachesTheResult()
        {
            var expiryPeriod = TimeSpan.FromSeconds(600);
            var ipAddress = "1.2.3.4";
            var geoipRotatingClientResponse = new GeoipClientResponse
            {
                CountryCode = EnumsHelper.CountryId.ToString(CountryId.GB),
                GeoipProviderName = GeoipProviderName.Nekudo,
                Latitude = 1.0,
                Longitude = 2.0,
                RawResponse = "raw-response",
                Status = WebClientResponseStatus.Success
            };

            var container = CreateContainer();
            SetupConfig(container, expiryPeriod);
            SetupGeoipRotatingClient(container, ipAddress, geoipRotatingClientResponse);

            var clock = container.Get<IClock>();
            var service = container.Get<IGeoipInfoService>();

            var timeBefore = clock.OffsetNow;
            var geoipInfo = service.GetInfo(ipAddress);
            var timeAfter = clock.OffsetNow;

            Assert.AreEqual(geoipRotatingClientResponse.CountryCode, geoipInfo.CountryCode,
                "The CountryCode was not the expected on the GeoipInfo.");
            Assert.AreEqual(EnumsHelper.GeoipProviderName.ToString(geoipRotatingClientResponse.GeoipProviderName), 
                geoipInfo.GeoipProviderName,
                "The GeoipProviderName was not the expected on the GeoipInfo.");
            Assert.AreEqual(geoipRotatingClientResponse.Latitude, geoipInfo.Latitude,
                "The Latitude was not the expected on the GeoipInfo.");
            Assert.AreEqual(geoipRotatingClientResponse.Longitude, geoipInfo.Longitude,
                "The Longitude was not the expected on the GeoipInfo.");
            Assert.AreEqual(ipAddress, geoipInfo.IpAddress,
                "The IpAddress was not the expected on the GeoipInfo.");
            Assert.That(geoipInfo.RecordedOn, Is.GreaterThanOrEqualTo(timeBefore),
                "The RecordedOn on the GeoipInfo should be after the timeBefore.");
            Assert.That(geoipInfo.RecordedOn, Is.LessThanOrEqualTo(timeAfter),
                "The RecordedOn on the GeoipInfo should be before the timeAfter.");

            var retrievedGeoipInfo = await DbProbe.GeoipInfos.SingleOrDefaultAsync(x => x.IpAddress.Equals(ipAddress));

            Assert.IsNotNull(retrievedGeoipInfo, "The GeoipInfo was not found in the database.");
            Assert.AreEqual(geoipRotatingClientResponse.CountryCode, retrievedGeoipInfo.CountryCode,
                "The CountryCode was not the expected on the retrieved GeoipInfo.");
            Assert.AreEqual(EnumsHelper.GeoipProviderName.ToString(geoipRotatingClientResponse.GeoipProviderName),
                retrievedGeoipInfo.GeoipProviderName,
                "The GeoipProviderName was not the expected on the retrieved GeoipInfo.");
            Assert.AreEqual(geoipRotatingClientResponse.Latitude, retrievedGeoipInfo.Latitude,
                "The Latitude was not the expected on the retrieved GeoipInfo.");
            Assert.AreEqual(geoipRotatingClientResponse.Longitude, retrievedGeoipInfo.Longitude,
                "The Longitude was not the expected on the retrieved GeoipInfo.");
            Assert.AreEqual(ipAddress, retrievedGeoipInfo.IpAddress,
                "The IpAddress was not the expected on the retrieved GeoipInfo.");
            Assert.That(retrievedGeoipInfo.RecordedOn, Is.GreaterThanOrEqualTo(timeBefore),
                "The RecordedOn on the retrieved GeoipInfo should be after the timeBefore.");
            Assert.That(retrievedGeoipInfo.RecordedOn, Is.LessThanOrEqualTo(timeAfter),
                "The RecordedOn on the retrieved GeoipInfo should be before the timeAfter.");

            KillDatabase(container);
            DestroyGeoipRotatingClient(container);
            var serviceWithoutDatabaseOrGeoipRotatingClient = container.Get<IGeoipInfoService>();

            var cachedGeoipInfo = service.GetInfo(ipAddress);

            Assert.AreEqual(geoipRotatingClientResponse.CountryCode, cachedGeoipInfo.CountryCode,
                "The CountryCode was not the expected on the cached GeoipInfo.");
            Assert.AreEqual(EnumsHelper.GeoipProviderName.ToString(geoipRotatingClientResponse.GeoipProviderName),
                cachedGeoipInfo.GeoipProviderName,
                "The GeoipProviderName was not the expected on the cached GeoipInfo.");
            Assert.AreEqual(geoipRotatingClientResponse.Latitude, cachedGeoipInfo.Latitude,
                "The Latitude was not the expected on the cached GeoipInfo.");
            Assert.AreEqual(geoipRotatingClientResponse.Longitude, cachedGeoipInfo.Longitude,
                "The Longitude was not the expected on the cached GeoipInfo.");
            Assert.AreEqual(ipAddress, cachedGeoipInfo.IpAddress,
                "The IpAddress was not the expected on the cached GeoipInfo.");
            Assert.That(cachedGeoipInfo.RecordedOn, Is.GreaterThanOrEqualTo(timeBefore),
                "The RecordedOn on the cached GeoipInfo should be after the timeBefore.");
            Assert.That(cachedGeoipInfo.RecordedOn, Is.LessThanOrEqualTo(timeAfter),
                "The RecordedOn on the cached GeoipInfo should be before the timeAfter.");
        }

        [Test]
        public async Task GetInfo_CallsTheRotatingGeoipClientAgainAfterTheExpiryPeriod()
        {
            var expiryPeriod = TimeSpan.FromSeconds(0.4);
            var ipAddress = "1.2.3.4";
            var geoipRotatingClientResponse = new GeoipClientResponse
            {
                CountryCode = EnumsHelper.CountryId.ToString(CountryId.GB),
                GeoipProviderName = GeoipProviderName.Nekudo,
                Latitude = 1.0,
                Longitude = 2.0,
                RawResponse = "raw-response",
                Status = WebClientResponseStatus.Success
            };

            var container = CreateContainer();
            SetupConfig(container, expiryPeriod);
            SetupGeoipRotatingClient(container, ipAddress, geoipRotatingClientResponse);

            var clock = container.Get<IClock>();
            var service = container.Get<IGeoipInfoService>();

            var timeBefore1 = clock.OffsetNow;
            var geoipInfo1 = service.GetInfo(ipAddress);
            Assert.IsNotNull(geoipInfo1, "GeoipInfo1 is null.");
            var timeAfter1 = clock.OffsetNow;

            Assert.AreEqual(geoipRotatingClientResponse.CountryCode, geoipInfo1.CountryCode,
                "The CountryCode was not the expected on the GeoipInfo 1.");
            Assert.AreEqual(EnumsHelper.GeoipProviderName.ToString(geoipRotatingClientResponse.GeoipProviderName),
                geoipInfo1.GeoipProviderName,
                "The GeoipProviderName was not the expected on the GeoipInfo 1.");
            Assert.AreEqual(geoipRotatingClientResponse.Latitude, geoipInfo1.Latitude,
                "The Latitude was not the expected on the GeoipInfo 1.");
            Assert.AreEqual(geoipRotatingClientResponse.Longitude, geoipInfo1.Longitude,
                "The Longitude was not the expected on the GeoipInfo 1.");
            Assert.AreEqual(ipAddress, geoipInfo1.IpAddress,
                "The IpAddress was not the expected on the GeoipInfo 1.");
            Assert.That(geoipInfo1.RecordedOn, Is.GreaterThanOrEqualTo(timeBefore1),
                "The RecordedOn on the GeoipInfo 1 should be after the timeBefore.");
            Assert.That(geoipInfo1.RecordedOn, Is.LessThanOrEqualTo(timeAfter1),
                "The RecordedOn on the GeoipInfo 1 should be before the timeAfter.");

            var retrievedGeoipInfo1 = await CreateContainer().Get<IEpsilonContext>()
                .GeoipInfos.SingleOrDefaultAsync(x => x.IpAddress.Equals(ipAddress));
            Assert.IsNotNull(retrievedGeoipInfo1, "Retrieved GeoipInfo1 is null.");

            Assert.IsNotNull(retrievedGeoipInfo1, "The GeoipInfo 1 was not found in the database.");
            Assert.AreEqual(geoipRotatingClientResponse.CountryCode, retrievedGeoipInfo1.CountryCode,
                "The CountryCode was not the expected on the retrieved GeoipInfo 1.");
            Assert.AreEqual(EnumsHelper.GeoipProviderName.ToString(geoipRotatingClientResponse.GeoipProviderName),
                retrievedGeoipInfo1.GeoipProviderName,
                "The GeoipProviderName was not the expected on the retrieved GeoipInfo 1.");
            Assert.AreEqual(geoipRotatingClientResponse.Latitude, retrievedGeoipInfo1.Latitude,
                "The Latitude was not the expected on the retrieved GeoipInfo 1.");
            Assert.AreEqual(geoipRotatingClientResponse.Longitude, retrievedGeoipInfo1.Longitude,
                "The Longitude was not the expected on the retrieved GeoipInfo.");
            Assert.AreEqual(ipAddress, retrievedGeoipInfo1.IpAddress,
                "The IpAddress was not the expected on the retrieved GeoipInfo 1.");
            Assert.That(retrievedGeoipInfo1.RecordedOn, Is.GreaterThanOrEqualTo(timeBefore1),
                "The RecordedOn on the retrieved GeoipInfo 1 should be after the timeBefore.");
            Assert.That(retrievedGeoipInfo1.RecordedOn, Is.LessThanOrEqualTo(timeAfter1),
                "The RecordedOn on the retrieved GeoipInfo 1 should be before the timeAfter.");

            await Task.Delay(expiryPeriod);

            var newGeoipRotatingClientResponse = new GeoipClientResponse
            {
                CountryCode = EnumsHelper.CountryId.ToString(CountryId.GR),
                GeoipProviderName = GeoipProviderName.Telize,
                Latitude = -1.0,
                Longitude = -2.0,
                RawResponse = "new-raw-response",
                Status = WebClientResponseStatus.Success
            };
            SetupGeoipRotatingClient(container, ipAddress, newGeoipRotatingClientResponse);
            var newService = container.Get<IGeoipInfoService>();

            var timeBefore2 = clock.OffsetNow;
            var geoipInfo2 = service.GetInfo(ipAddress);
            Assert.IsNotNull(geoipInfo2, "GeoipInfo2 is null.");
            var timeAfter2 = clock.OffsetNow;

            Assert.AreEqual(geoipRotatingClientResponse.CountryCode, geoipInfo2.CountryCode,
                "The CountryCode was not the expected on the GeoipInfo 2.");
            Assert.AreEqual(EnumsHelper.GeoipProviderName.ToString(geoipRotatingClientResponse.GeoipProviderName),
                geoipInfo2.GeoipProviderName,
                "The GeoipProviderName was not the expected on the GeoipInfo 2.");
            Assert.AreEqual(geoipRotatingClientResponse.Latitude, geoipInfo2.Latitude,
                "The Latitude was not the expected on the GeoipInfo 2.");
            Assert.AreEqual(geoipRotatingClientResponse.Longitude, geoipInfo2.Longitude,
                "The Longitude was not the expected on the GeoipInfo 2.");
            Assert.AreEqual(ipAddress, geoipInfo2.IpAddress,
                "The IpAddress was not the expected on the GeoipInfo 2.");
            Assert.That(geoipInfo2.RecordedOn, Is.GreaterThanOrEqualTo(timeBefore2),
                "The RecordedOn on the GeoipInfo 2 should be after the timeBefore.");
            Assert.That(geoipInfo2.RecordedOn, Is.LessThanOrEqualTo(timeAfter2),
                "The RecordedOn on the GeoipInfo 2 should be before the timeAfter.");

            var retrievedGeoipInfo2 = await CreateContainer().Get<IEpsilonContext>()
                .GeoipInfos.SingleOrDefaultAsync(x => x.IpAddress.Equals(ipAddress));
            Assert.IsNotNull(retrievedGeoipInfo2, "Retrieved GeoipInfo2 is null.");

            Assert.IsNotNull(retrievedGeoipInfo2, "The GeoipInfo 2 was not found in the database.");
            Assert.AreEqual(geoipRotatingClientResponse.CountryCode, retrievedGeoipInfo2.CountryCode,
                "The CountryCode was not the expected on the retrieved GeoipInfo 2.");
            Assert.AreEqual(EnumsHelper.GeoipProviderName.ToString(geoipRotatingClientResponse.GeoipProviderName),
                retrievedGeoipInfo2.GeoipProviderName,
                "The GeoipProviderName was not the expected on the retrieved GeoipInfo 2.");
            Assert.AreEqual(geoipRotatingClientResponse.Latitude, retrievedGeoipInfo2.Latitude,
                "The Latitude was not the expected on the retrieved GeoipInfo 2.");
            Assert.AreEqual(geoipRotatingClientResponse.Longitude, retrievedGeoipInfo2.Longitude,
                "The Longitude was not the expected on the retrieved GeoipInfo 2.");
            Assert.AreEqual(ipAddress, retrievedGeoipInfo2.IpAddress,
                "The IpAddress was not the expected on the retrieved GeoipInfo 2.");
            Assert.That(retrievedGeoipInfo2.RecordedOn, Is.GreaterThanOrEqualTo(timeBefore2),
                "The RecordedOn on the retrieved GeoipInfo 2 should be after the timeBefore.");
            Assert.That(retrievedGeoipInfo2.RecordedOn, Is.LessThanOrEqualTo(timeAfter2),
                "The RecordedOn on the retrieved GeoipInfo 2 should be before the timeAfter.");
        }

        [Test]
        public async Task GetInfo_CallsTheRotatingGeoipClientAgainAfterTheExpiryPeriod_TestWhereCacheIsCleared()
        {
            var expiryPeriodInSeconds = 0.5;
            var expiryPeriod = TimeSpan.FromSeconds(expiryPeriodInSeconds);
            var alpha = 0.1;

            var ipAddress = "1.2.3.4";
            var geoipRotatingClientResponse = new GeoipClientResponse
            {
                CountryCode = EnumsHelper.CountryId.ToString(CountryId.GB),
                GeoipProviderName = GeoipProviderName.Nekudo,
                Latitude = 1.0,
                Longitude = 2.0,
                RawResponse = "raw-response",
                Status = WebClientResponseStatus.Success
            };

            var container = CreateContainer();
            SetupConfig(container, expiryPeriod);
            SetupGeoipRotatingClient(container, ipAddress, geoipRotatingClientResponse);

            var clock = container.Get<IClock>();
            var service = container.Get<IGeoipInfoService>();

            var timeBefore1 = clock.OffsetNow;
            var geoipInfo1 = service.GetInfo(ipAddress);
            Assert.IsNotNull(geoipInfo1, "GeoipInfo1 is null.");
            var timeAfter1 = clock.OffsetNow;

            Assert.AreEqual(geoipRotatingClientResponse.CountryCode, geoipInfo1.CountryCode,
                "The CountryCode was not the expected on the GeoipInfo 1.");
            Assert.AreEqual(EnumsHelper.GeoipProviderName.ToString(geoipRotatingClientResponse.GeoipProviderName),
                geoipInfo1.GeoipProviderName,
                "The GeoipProviderName was not the expected on the GeoipInfo 1.");
            Assert.AreEqual(geoipRotatingClientResponse.Latitude, geoipInfo1.Latitude,
                "The Latitude was not the expected on the GeoipInfo 1.");
            Assert.AreEqual(geoipRotatingClientResponse.Longitude, geoipInfo1.Longitude,
                "The Longitude was not the expected on the GeoipInfo 1.");
            Assert.AreEqual(ipAddress, geoipInfo1.IpAddress,
                "The IpAddress was not the expected on the GeoipInfo 1.");
            Assert.That(geoipInfo1.RecordedOn, Is.GreaterThanOrEqualTo(timeBefore1),
                "The RecordedOn on the GeoipInfo 1 should be after the timeBefore.");
            Assert.That(geoipInfo1.RecordedOn, Is.LessThanOrEqualTo(timeAfter1),
                "The RecordedOn on the GeoipInfo 1 should be before the timeAfter.");

            var retrievedGeoipInfo1 = await CreateContainer().Get<IEpsilonContext>()
                .GeoipInfos.SingleOrDefaultAsync(x => x.IpAddress.Equals(ipAddress));
            Assert.IsNotNull(retrievedGeoipInfo1, "Retrieved GeoipInfo1 is null.");

            Assert.IsNotNull(retrievedGeoipInfo1, "The GeoipInfo 1 was not found in the database.");
            Assert.AreEqual(geoipRotatingClientResponse.CountryCode, retrievedGeoipInfo1.CountryCode,
                "The CountryCode was not the expected on the retrieved GeoipInfo 1.");
            Assert.AreEqual(EnumsHelper.GeoipProviderName.ToString(geoipRotatingClientResponse.GeoipProviderName),
                retrievedGeoipInfo1.GeoipProviderName,
                "The GeoipProviderName was not the expected on the retrieved GeoipInfo 1.");
            Assert.AreEqual(geoipRotatingClientResponse.Latitude, retrievedGeoipInfo1.Latitude,
                "The Latitude was not the expected on the retrieved GeoipInfo 1.");
            Assert.AreEqual(geoipRotatingClientResponse.Longitude, retrievedGeoipInfo1.Longitude,
                "The Longitude was not the expected on the retrieved GeoipInfo.");
            Assert.AreEqual(ipAddress, retrievedGeoipInfo1.IpAddress,
                "The IpAddress was not the expected on the retrieved GeoipInfo 1.");
            Assert.That(retrievedGeoipInfo1.RecordedOn, Is.GreaterThanOrEqualTo(timeBefore1),
                "The RecordedOn on the retrieved GeoipInfo 1 should be after the timeBefore.");
            Assert.That(retrievedGeoipInfo1.RecordedOn, Is.LessThanOrEqualTo(timeAfter1),
                "The RecordedOn on the retrieved GeoipInfo 1 should be before the timeAfter.");

            await Task.Delay(TimeSpan.FromSeconds(expiryPeriodInSeconds * alpha));

            var newGeoipRotatingClientResponse = new GeoipClientResponse
            {
                CountryCode = EnumsHelper.CountryId.ToString(CountryId.GR),
                GeoipProviderName = GeoipProviderName.Telize,
                Latitude = -1.0,
                Longitude = -2.0,
                RawResponse = "new-raw-response",
                Status = WebClientResponseStatus.Success
            };

            var appCache = container.Get<IAppCache>();
            appCache.Clear();

            SetupGeoipRotatingClient(container, ipAddress, newGeoipRotatingClientResponse);
            var newService = container.Get<IGeoipInfoService>();

            // We haven't reaced the expiry yet.
            var savedGeoipInfo1 = newService.GetInfo(ipAddress);
            Assert.IsNotNull(savedGeoipInfo1, "Saved GeoipInfo1 was null.");

            Assert.AreEqual(geoipRotatingClientResponse.CountryCode, savedGeoipInfo1.CountryCode,
                    "The CountryCode was not the expected on the saved GeoipInfo 1.");
            Assert.AreEqual(EnumsHelper.GeoipProviderName.ToString(geoipRotatingClientResponse.GeoipProviderName),
                savedGeoipInfo1.GeoipProviderName,
                "The GeoipProviderName was not the expected on the saved GeoipInfo 1.");
            Assert.AreEqual(geoipRotatingClientResponse.Latitude, savedGeoipInfo1.Latitude,
                "The Latitude was not the expected on the saved GeoipInfo 1.");
            Assert.AreEqual(geoipRotatingClientResponse.Longitude, savedGeoipInfo1.Longitude,
                "The Longitude was not the expected on the saved GeoipInfo 1.");
            Assert.AreEqual(ipAddress, savedGeoipInfo1.IpAddress,
                "The IpAddress was not the expected on the saved GeoipInfo 1.");
            Assert.That(savedGeoipInfo1.RecordedOn, Is.GreaterThanOrEqualTo(timeBefore1),
                "The RecordedOn on the saved GeoipInfo 1 should be after the timeBefore.");
            Assert.That(savedGeoipInfo1.RecordedOn, Is.LessThanOrEqualTo(timeAfter1),
                "The RecordedOn on the saved GeoipInfo 1 should be before the timeAfter.");

            await Task.Delay(TimeSpan.FromSeconds(expiryPeriodInSeconds * (1 - alpha)));

            // The old GeoipInfo should be expired now.

            var timeBefore2 = clock.OffsetNow;
            var geoipInfo2 = service.GetInfo(ipAddress);
            Assert.IsNotNull(geoipInfo2, "GeoipInfo2 is null.");
            var timeAfter2 = clock.OffsetNow;

            Assert.AreEqual(geoipRotatingClientResponse.CountryCode, geoipInfo2.CountryCode,
                "The CountryCode was not the expected on the GeoipInfo 2.");
            Assert.AreEqual(EnumsHelper.GeoipProviderName.ToString(geoipRotatingClientResponse.GeoipProviderName),
                geoipInfo2.GeoipProviderName,
                "The GeoipProviderName was not the expected on the GeoipInfo 2.");
            Assert.AreEqual(geoipRotatingClientResponse.Latitude, geoipInfo2.Latitude,
                "The Latitude was not the expected on the GeoipInfo 2.");
            Assert.AreEqual(geoipRotatingClientResponse.Longitude, geoipInfo2.Longitude,
                "The Longitude was not the expected on the GeoipInfo 2.");
            Assert.AreEqual(ipAddress, geoipInfo2.IpAddress,
                "The IpAddress was not the expected on the GeoipInfo 2.");
            Assert.That(geoipInfo2.RecordedOn, Is.GreaterThanOrEqualTo(timeBefore2),
                "The RecordedOn on the GeoipInfo 2 should be after the timeBefore.");
            Assert.That(geoipInfo2.RecordedOn, Is.LessThanOrEqualTo(timeAfter2),
                "The RecordedOn on the GeoipInfo 2 should be before the timeAfter.");

            var retrievedGeoipInfo2 = await CreateContainer().Get<IEpsilonContext>()
                .GeoipInfos.SingleOrDefaultAsync(x => x.IpAddress.Equals(ipAddress));
            Assert.IsNotNull(retrievedGeoipInfo2, "Retrieved GeoipInfo2 is null.");

            Assert.IsNotNull(retrievedGeoipInfo2, "The GeoipInfo 2 was not found in the database.");
            Assert.AreEqual(geoipRotatingClientResponse.CountryCode, retrievedGeoipInfo2.CountryCode,
                "The CountryCode was not the expected on the retrieved GeoipInfo 2.");
            Assert.AreEqual(EnumsHelper.GeoipProviderName.ToString(geoipRotatingClientResponse.GeoipProviderName),
                retrievedGeoipInfo2.GeoipProviderName,
                "The GeoipProviderName was not the expected on the retrieved GeoipInfo 2.");
            Assert.AreEqual(geoipRotatingClientResponse.Latitude, retrievedGeoipInfo2.Latitude,
                "The Latitude was not the expected on the retrieved GeoipInfo 2.");
            Assert.AreEqual(geoipRotatingClientResponse.Longitude, retrievedGeoipInfo2.Longitude,
                "The Longitude was not the expected on the retrieved GeoipInfo 2.");
            Assert.AreEqual(ipAddress, retrievedGeoipInfo2.IpAddress,
                "The IpAddress was not the expected on the retrieved GeoipInfo 2.");
            Assert.That(retrievedGeoipInfo2.RecordedOn, Is.GreaterThanOrEqualTo(timeBefore2),
                "The RecordedOn on the retrieved GeoipInfo 2 should be after the timeBefore.");
            Assert.That(retrievedGeoipInfo2.RecordedOn, Is.LessThanOrEqualTo(timeAfter2),
                "The RecordedOn on the retrieved GeoipInfo 2 should be before the timeAfter.");
        }

        [Test]
        public void GetInfo_ReturnsNull_WhenGeoipRotatingClientDoesNotReturnSuccess()
        {
            var expiryPeriod = TimeSpan.FromSeconds(600);
            var ipAddress = "1.2.3.4";

            var nonSuccessStatuses = EnumsHelper.WebClientResponseStatus.GetValues()
                .Where(x => x != WebClientResponseStatus.Success)
                .ToList();

            var container = CreateContainer();
            SetupConfig(container, expiryPeriod);
            var cache = container.Get<IAppCache>();

            foreach (var status in nonSuccessStatuses)
            {
                cache.Clear();

                var geoipRotatingClientResponse = new GeoipClientResponse
                {
                    CountryCode = EnumsHelper.CountryId.ToString(CountryId.GB),
                    GeoipProviderName = GeoipProviderName.Nekudo,
                    Latitude = 1.0,
                    Longitude = 2.0,
                    RawResponse = "raw-response",
                    Status = status
                };

                SetupGeoipRotatingClient(container, ipAddress, geoipRotatingClientResponse);

                var service = container.Get<IGeoipInfoService>();
                var geoipInfo = service.GetInfo(ipAddress);

                Assert.IsNull(geoipInfo, string.Format("GeoipInfo was not null for status '{0}'.",
                    EnumsHelper.WebClientResponseStatus.ToString(status)));
            }
        }

        #endregion

        #region GetInfoAsync

        [Test]
        public async Task GetInfoAsync_CachesTheResult()
        {
            var expiryPeriod = TimeSpan.FromSeconds(600);
            var ipAddress = "1.2.3.4";
            var geoipRotatingClientResponse = new GeoipClientResponse
            {
                CountryCode = EnumsHelper.CountryId.ToString(CountryId.GB),
                GeoipProviderName = GeoipProviderName.Nekudo,
                Latitude = 1.0,
                Longitude = 2.0,
                RawResponse = "raw-response",
                Status = WebClientResponseStatus.Success
            };

            var container = CreateContainer();
            SetupConfig(container, expiryPeriod);
            SetupGeoipRotatingClient(container, ipAddress, geoipRotatingClientResponse);

            var clock = container.Get<IClock>();
            var service = container.Get<IGeoipInfoService>();

            var timeBefore = clock.OffsetNow;
            var geoipInfo = await service.GetInfoAsync(ipAddress);
            var timeAfter = clock.OffsetNow;

            Assert.AreEqual(geoipRotatingClientResponse.CountryCode, geoipInfo.CountryCode,
                "The CountryCode was not the expected on the GeoipInfo.");
            Assert.AreEqual(EnumsHelper.GeoipProviderName.ToString(geoipRotatingClientResponse.GeoipProviderName),
                geoipInfo.GeoipProviderName,
                "The GeoipProviderName was not the expected on the GeoipInfo.");
            Assert.AreEqual(geoipRotatingClientResponse.Latitude, geoipInfo.Latitude,
                "The Latitude was not the expected on the GeoipInfo.");
            Assert.AreEqual(geoipRotatingClientResponse.Longitude, geoipInfo.Longitude,
                "The Longitude was not the expected on the GeoipInfo.");
            Assert.AreEqual(ipAddress, geoipInfo.IpAddress,
                "The IpAddress was not the expected on the GeoipInfo.");
            Assert.That(geoipInfo.RecordedOn, Is.GreaterThanOrEqualTo(timeBefore),
                "The RecordedOn on the GeoipInfo should be after the timeBefore.");
            Assert.That(geoipInfo.RecordedOn, Is.LessThanOrEqualTo(timeAfter),
                "The RecordedOn on the GeoipInfo should be before the timeAfter.");

            var retrievedGeoipInfo = await DbProbe.GeoipInfos.SingleOrDefaultAsync(x => x.IpAddress.Equals(ipAddress));

            Assert.IsNotNull(retrievedGeoipInfo, "The GeoipInfo was not found in the database.");
            Assert.AreEqual(geoipRotatingClientResponse.CountryCode, retrievedGeoipInfo.CountryCode,
                "The CountryCode was not the expected on the retrieved GeoipInfo.");
            Assert.AreEqual(EnumsHelper.GeoipProviderName.ToString(geoipRotatingClientResponse.GeoipProviderName),
                retrievedGeoipInfo.GeoipProviderName,
                "The GeoipProviderName was not the expected on the retrieved GeoipInfo.");
            Assert.AreEqual(geoipRotatingClientResponse.Latitude, retrievedGeoipInfo.Latitude,
                "The Latitude was not the expected on the retrieved GeoipInfo.");
            Assert.AreEqual(geoipRotatingClientResponse.Longitude, retrievedGeoipInfo.Longitude,
                "The Longitude was not the expected on the retrieved GeoipInfo.");
            Assert.AreEqual(ipAddress, retrievedGeoipInfo.IpAddress,
                "The IpAddress was not the expected on the retrieved GeoipInfo.");
            Assert.That(retrievedGeoipInfo.RecordedOn, Is.GreaterThanOrEqualTo(timeBefore),
                "The RecordedOn on the retrieved GeoipInfo should be after the timeBefore.");
            Assert.That(retrievedGeoipInfo.RecordedOn, Is.LessThanOrEqualTo(timeAfter),
                "The RecordedOn on the retrieved GeoipInfo should be before the timeAfter.");

            KillDatabase(container);
            DestroyGeoipRotatingClient(container);
            var serviceWithoutDatabaseOrGeoipRotatingClient = container.Get<IGeoipInfoService>();

            var cachedGeoipInfo = await service.GetInfoAsync(ipAddress);

            Assert.AreEqual(geoipRotatingClientResponse.CountryCode, cachedGeoipInfo.CountryCode,
                "The CountryCode was not the expected on the cached GeoipInfo.");
            Assert.AreEqual(EnumsHelper.GeoipProviderName.ToString(geoipRotatingClientResponse.GeoipProviderName),
                cachedGeoipInfo.GeoipProviderName,
                "The GeoipProviderName was not the expected on the cached GeoipInfo.");
            Assert.AreEqual(geoipRotatingClientResponse.Latitude, cachedGeoipInfo.Latitude,
                "The Latitude was not the expected on the cached GeoipInfo.");
            Assert.AreEqual(geoipRotatingClientResponse.Longitude, cachedGeoipInfo.Longitude,
                "The Longitude was not the expected on the cached GeoipInfo.");
            Assert.AreEqual(ipAddress, cachedGeoipInfo.IpAddress,
                "The IpAddress was not the expected on the cached GeoipInfo.");
            Assert.That(cachedGeoipInfo.RecordedOn, Is.GreaterThanOrEqualTo(timeBefore),
                "The RecordedOn on the cached GeoipInfo should be after the timeBefore.");
            Assert.That(cachedGeoipInfo.RecordedOn, Is.LessThanOrEqualTo(timeAfter),
                "The RecordedOn on the cached GeoipInfo should be before the timeAfter.");
        }

        [Test]
        public async Task GetInfoAsync_CallsTheRotatingGeoipClientAgainAfterTheExpiryPeriod()
        {
            var expiryPeriod = TimeSpan.FromSeconds(0.4);
            var ipAddress = "1.2.3.4";
            var geoipRotatingClientResponse = new GeoipClientResponse
            {
                CountryCode = EnumsHelper.CountryId.ToString(CountryId.GB),
                GeoipProviderName = GeoipProviderName.Nekudo,
                Latitude = 1.0,
                Longitude = 2.0,
                RawResponse = "raw-response",
                Status = WebClientResponseStatus.Success
            };

            var container = CreateContainer();
            SetupConfig(container, expiryPeriod);
            SetupGeoipRotatingClient(container, ipAddress, geoipRotatingClientResponse);

            var clock = container.Get<IClock>();
            var service = container.Get<IGeoipInfoService>();

            var timeBefore1 = clock.OffsetNow;
            var geoipInfo1 = await service.GetInfoAsync(ipAddress);
            Assert.IsNotNull(geoipInfo1, "GeoipInfo1 is null.");
            var timeAfter1 = clock.OffsetNow;

            Assert.AreEqual(geoipRotatingClientResponse.CountryCode, geoipInfo1.CountryCode,
                "The CountryCode was not the expected on the GeoipInfo 1.");
            Assert.AreEqual(EnumsHelper.GeoipProviderName.ToString(geoipRotatingClientResponse.GeoipProviderName),
                geoipInfo1.GeoipProviderName,
                "The GeoipProviderName was not the expected on the GeoipInfo 1.");
            Assert.AreEqual(geoipRotatingClientResponse.Latitude, geoipInfo1.Latitude,
                "The Latitude was not the expected on the GeoipInfo 1.");
            Assert.AreEqual(geoipRotatingClientResponse.Longitude, geoipInfo1.Longitude,
                "The Longitude was not the expected on the GeoipInfo 1.");
            Assert.AreEqual(ipAddress, geoipInfo1.IpAddress,
                "The IpAddress was not the expected on the GeoipInfo 1.");
            Assert.That(geoipInfo1.RecordedOn, Is.GreaterThanOrEqualTo(timeBefore1),
                "The RecordedOn on the GeoipInfo 1 should be after the timeBefore.");
            Assert.That(geoipInfo1.RecordedOn, Is.LessThanOrEqualTo(timeAfter1),
                "The RecordedOn on the GeoipInfo 1 should be before the timeAfter.");

            var retrievedGeoipInfo1 = await CreateContainer().Get<IEpsilonContext>()
                .GeoipInfos.SingleOrDefaultAsync(x => x.IpAddress.Equals(ipAddress));
            Assert.IsNotNull(retrievedGeoipInfo1, "Retrieved GeoipInfo1 is null.");

            Assert.IsNotNull(retrievedGeoipInfo1, "The GeoipInfo 1 was not found in the database.");
            Assert.AreEqual(geoipRotatingClientResponse.CountryCode, retrievedGeoipInfo1.CountryCode,
                "The CountryCode was not the expected on the retrieved GeoipInfo 1.");
            Assert.AreEqual(EnumsHelper.GeoipProviderName.ToString(geoipRotatingClientResponse.GeoipProviderName),
                retrievedGeoipInfo1.GeoipProviderName,
                "The GeoipProviderName was not the expected on the retrieved GeoipInfo 1.");
            Assert.AreEqual(geoipRotatingClientResponse.Latitude, retrievedGeoipInfo1.Latitude,
                "The Latitude was not the expected on the retrieved GeoipInfo 1.");
            Assert.AreEqual(geoipRotatingClientResponse.Longitude, retrievedGeoipInfo1.Longitude,
                "The Longitude was not the expected on the retrieved GeoipInfo.");
            Assert.AreEqual(ipAddress, retrievedGeoipInfo1.IpAddress,
                "The IpAddress was not the expected on the retrieved GeoipInfo 1.");
            Assert.That(retrievedGeoipInfo1.RecordedOn, Is.GreaterThanOrEqualTo(timeBefore1),
                "The RecordedOn on the retrieved GeoipInfo 1 should be after the timeBefore.");
            Assert.That(retrievedGeoipInfo1.RecordedOn, Is.LessThanOrEqualTo(timeAfter1),
                "The RecordedOn on the retrieved GeoipInfo 1 should be before the timeAfter.");

            await Task.Delay(expiryPeriod);

            var newGeoipRotatingClientResponse = new GeoipClientResponse
            {
                CountryCode = EnumsHelper.CountryId.ToString(CountryId.GR),
                GeoipProviderName = GeoipProviderName.Telize,
                Latitude = -1.0,
                Longitude = -2.0,
                RawResponse = "new-raw-response",
                Status = WebClientResponseStatus.Success
            };
            SetupGeoipRotatingClient(container, ipAddress, newGeoipRotatingClientResponse);
            var newService = container.Get<IGeoipInfoService>();

            var timeBefore2 = clock.OffsetNow;
            var geoipInfo2 = await service.GetInfoAsync(ipAddress);
            Assert.IsNotNull(geoipInfo2, "GeoipInfo2 is null.");
            var timeAfter2 = clock.OffsetNow;

            Assert.AreEqual(geoipRotatingClientResponse.CountryCode, geoipInfo2.CountryCode,
                "The CountryCode was not the expected on the GeoipInfo 2.");
            Assert.AreEqual(EnumsHelper.GeoipProviderName.ToString(geoipRotatingClientResponse.GeoipProviderName),
                geoipInfo2.GeoipProviderName,
                "The GeoipProviderName was not the expected on the GeoipInfo 2.");
            Assert.AreEqual(geoipRotatingClientResponse.Latitude, geoipInfo2.Latitude,
                "The Latitude was not the expected on the GeoipInfo 2.");
            Assert.AreEqual(geoipRotatingClientResponse.Longitude, geoipInfo2.Longitude,
                "The Longitude was not the expected on the GeoipInfo 2.");
            Assert.AreEqual(ipAddress, geoipInfo2.IpAddress,
                "The IpAddress was not the expected on the GeoipInfo 2.");
            Assert.That(geoipInfo2.RecordedOn, Is.GreaterThanOrEqualTo(timeBefore2),
                "The RecordedOn on the GeoipInfo 2 should be after the timeBefore.");
            Assert.That(geoipInfo2.RecordedOn, Is.LessThanOrEqualTo(timeAfter2),
                "The RecordedOn on the GeoipInfo 2 should be before the timeAfter.");

            var retrievedGeoipInfo2 = await CreateContainer().Get<IEpsilonContext>()
                .GeoipInfos.SingleOrDefaultAsync(x => x.IpAddress.Equals(ipAddress));
            Assert.IsNotNull(retrievedGeoipInfo2, "Retrieved GeoipInfo2 is null.");

            Assert.IsNotNull(retrievedGeoipInfo2, "The GeoipInfo 2 was not found in the database.");
            Assert.AreEqual(geoipRotatingClientResponse.CountryCode, retrievedGeoipInfo2.CountryCode,
                "The CountryCode was not the expected on the retrieved GeoipInfo 2.");
            Assert.AreEqual(EnumsHelper.GeoipProviderName.ToString(geoipRotatingClientResponse.GeoipProviderName),
                retrievedGeoipInfo2.GeoipProviderName,
                "The GeoipProviderName was not the expected on the retrieved GeoipInfo 2.");
            Assert.AreEqual(geoipRotatingClientResponse.Latitude, retrievedGeoipInfo2.Latitude,
                "The Latitude was not the expected on the retrieved GeoipInfo 2.");
            Assert.AreEqual(geoipRotatingClientResponse.Longitude, retrievedGeoipInfo2.Longitude,
                "The Longitude was not the expected on the retrieved GeoipInfo 2.");
            Assert.AreEqual(ipAddress, retrievedGeoipInfo2.IpAddress,
                "The IpAddress was not the expected on the retrieved GeoipInfo 2.");
            Assert.That(retrievedGeoipInfo2.RecordedOn, Is.GreaterThanOrEqualTo(timeBefore2),
                "The RecordedOn on the retrieved GeoipInfo 2 should be after the timeBefore.");
            Assert.That(retrievedGeoipInfo2.RecordedOn, Is.LessThanOrEqualTo(timeAfter2),
                "The RecordedOn on the retrieved GeoipInfo 2 should be before the timeAfter.");
        }

        [Test]
        public async Task GetInfoAsync_CallsTheRotatingGeoipClientAgainAfterTheExpiryPeriod_TestWhereCacheIsCleared()
        {
            var expiryPeriodInSeconds = 0.5;
            var expiryPeriod = TimeSpan.FromSeconds(expiryPeriodInSeconds);
            var alpha = 0.1;

            var ipAddress = "1.2.3.4";
            var geoipRotatingClientResponse = new GeoipClientResponse
            {
                CountryCode = EnumsHelper.CountryId.ToString(CountryId.GB),
                GeoipProviderName = GeoipProviderName.Nekudo,
                Latitude = 1.0,
                Longitude = 2.0,
                RawResponse = "raw-response",
                Status = WebClientResponseStatus.Success
            };

            var container = CreateContainer();
            SetupConfig(container, expiryPeriod);
            SetupGeoipRotatingClient(container, ipAddress, geoipRotatingClientResponse);

            var clock = container.Get<IClock>();
            var service = container.Get<IGeoipInfoService>();

            var timeBefore1 = clock.OffsetNow;
            var geoipInfo1 = await service.GetInfoAsync(ipAddress);
            Assert.IsNotNull(geoipInfo1, "GeoipInfo1 is null.");
            var timeAfter1 = clock.OffsetNow;

            Assert.AreEqual(geoipRotatingClientResponse.CountryCode, geoipInfo1.CountryCode,
                "The CountryCode was not the expected on the GeoipInfo 1.");
            Assert.AreEqual(EnumsHelper.GeoipProviderName.ToString(geoipRotatingClientResponse.GeoipProviderName),
                geoipInfo1.GeoipProviderName,
                "The GeoipProviderName was not the expected on the GeoipInfo 1.");
            Assert.AreEqual(geoipRotatingClientResponse.Latitude, geoipInfo1.Latitude,
                "The Latitude was not the expected on the GeoipInfo 1.");
            Assert.AreEqual(geoipRotatingClientResponse.Longitude, geoipInfo1.Longitude,
                "The Longitude was not the expected on the GeoipInfo 1.");
            Assert.AreEqual(ipAddress, geoipInfo1.IpAddress,
                "The IpAddress was not the expected on the GeoipInfo 1.");
            Assert.That(geoipInfo1.RecordedOn, Is.GreaterThanOrEqualTo(timeBefore1),
                "The RecordedOn on the GeoipInfo 1 should be after the timeBefore.");
            Assert.That(geoipInfo1.RecordedOn, Is.LessThanOrEqualTo(timeAfter1),
                "The RecordedOn on the GeoipInfo 1 should be before the timeAfter.");

            var retrievedGeoipInfo1 = await CreateContainer().Get<IEpsilonContext>()
                .GeoipInfos.SingleOrDefaultAsync(x => x.IpAddress.Equals(ipAddress));
            Assert.IsNotNull(retrievedGeoipInfo1, "Retrieved GeoipInfo1 is null.");

            Assert.IsNotNull(retrievedGeoipInfo1, "The GeoipInfo 1 was not found in the database.");
            Assert.AreEqual(geoipRotatingClientResponse.CountryCode, retrievedGeoipInfo1.CountryCode,
                "The CountryCode was not the expected on the retrieved GeoipInfo 1.");
            Assert.AreEqual(EnumsHelper.GeoipProviderName.ToString(geoipRotatingClientResponse.GeoipProviderName),
                retrievedGeoipInfo1.GeoipProviderName,
                "The GeoipProviderName was not the expected on the retrieved GeoipInfo 1.");
            Assert.AreEqual(geoipRotatingClientResponse.Latitude, retrievedGeoipInfo1.Latitude,
                "The Latitude was not the expected on the retrieved GeoipInfo 1.");
            Assert.AreEqual(geoipRotatingClientResponse.Longitude, retrievedGeoipInfo1.Longitude,
                "The Longitude was not the expected on the retrieved GeoipInfo.");
            Assert.AreEqual(ipAddress, retrievedGeoipInfo1.IpAddress,
                "The IpAddress was not the expected on the retrieved GeoipInfo 1.");
            Assert.That(retrievedGeoipInfo1.RecordedOn, Is.GreaterThanOrEqualTo(timeBefore1),
                "The RecordedOn on the retrieved GeoipInfo 1 should be after the timeBefore.");
            Assert.That(retrievedGeoipInfo1.RecordedOn, Is.LessThanOrEqualTo(timeAfter1),
                "The RecordedOn on the retrieved GeoipInfo 1 should be before the timeAfter.");

            await Task.Delay(TimeSpan.FromSeconds(expiryPeriodInSeconds * alpha));

            var newGeoipRotatingClientResponse = new GeoipClientResponse
            {
                CountryCode = EnumsHelper.CountryId.ToString(CountryId.GR),
                GeoipProviderName = GeoipProviderName.Telize,
                Latitude = -1.0,
                Longitude = -2.0,
                RawResponse = "new-raw-response",
                Status = WebClientResponseStatus.Success
            };

            var appCache = container.Get<IAppCache>();
            appCache.Clear();

            SetupGeoipRotatingClient(container, ipAddress, newGeoipRotatingClientResponse);
            var newService = container.Get<IGeoipInfoService>();

            // We haven't reaced the expiry yet.
            var savedGeoipInfo1 = await newService.GetInfoAsync(ipAddress);
            Assert.IsNotNull(savedGeoipInfo1, "Saved GeoipInfo1 was null.");

            Assert.AreEqual(geoipRotatingClientResponse.CountryCode, savedGeoipInfo1.CountryCode,
                    "The CountryCode was not the expected on the saved GeoipInfo 1.");
            Assert.AreEqual(EnumsHelper.GeoipProviderName.ToString(geoipRotatingClientResponse.GeoipProviderName),
                savedGeoipInfo1.GeoipProviderName,
                "The GeoipProviderName was not the expected on the saved GeoipInfo 1.");
            Assert.AreEqual(geoipRotatingClientResponse.Latitude, savedGeoipInfo1.Latitude,
                "The Latitude was not the expected on the saved GeoipInfo 1.");
            Assert.AreEqual(geoipRotatingClientResponse.Longitude, savedGeoipInfo1.Longitude,
                "The Longitude was not the expected on the saved GeoipInfo 1.");
            Assert.AreEqual(ipAddress, savedGeoipInfo1.IpAddress,
                "The IpAddress was not the expected on the saved GeoipInfo 1.");
            Assert.That(savedGeoipInfo1.RecordedOn, Is.GreaterThanOrEqualTo(timeBefore1),
                "The RecordedOn on the saved GeoipInfo 1 should be after the timeBefore.");
            Assert.That(savedGeoipInfo1.RecordedOn, Is.LessThanOrEqualTo(timeAfter1),
                "The RecordedOn on the saved GeoipInfo 1 should be before the timeAfter.");

            await Task.Delay(TimeSpan.FromSeconds(expiryPeriodInSeconds * (1 - alpha)));

            // The old GeoipInfo should be expired now.

            var timeBefore2 = clock.OffsetNow;
            var geoipInfo2 = await service.GetInfoAsync(ipAddress);
            Assert.IsNotNull(geoipInfo2, "GeoipInfo2 is null.");
            var timeAfter2 = clock.OffsetNow;

            Assert.AreEqual(geoipRotatingClientResponse.CountryCode, geoipInfo2.CountryCode,
                "The CountryCode was not the expected on the GeoipInfo 2.");
            Assert.AreEqual(EnumsHelper.GeoipProviderName.ToString(geoipRotatingClientResponse.GeoipProviderName),
                geoipInfo2.GeoipProviderName,
                "The GeoipProviderName was not the expected on the GeoipInfo 2.");
            Assert.AreEqual(geoipRotatingClientResponse.Latitude, geoipInfo2.Latitude,
                "The Latitude was not the expected on the GeoipInfo 2.");
            Assert.AreEqual(geoipRotatingClientResponse.Longitude, geoipInfo2.Longitude,
                "The Longitude was not the expected on the GeoipInfo 2.");
            Assert.AreEqual(ipAddress, geoipInfo2.IpAddress,
                "The IpAddress was not the expected on the GeoipInfo 2.");
            Assert.That(geoipInfo2.RecordedOn, Is.GreaterThanOrEqualTo(timeBefore2),
                "The RecordedOn on the GeoipInfo 2 should be after the timeBefore.");
            Assert.That(geoipInfo2.RecordedOn, Is.LessThanOrEqualTo(timeAfter2),
                "The RecordedOn on the GeoipInfo 2 should be before the timeAfter.");

            var retrievedGeoipInfo2 = await CreateContainer().Get<IEpsilonContext>()
                .GeoipInfos.SingleOrDefaultAsync(x => x.IpAddress.Equals(ipAddress));
            Assert.IsNotNull(retrievedGeoipInfo2, "Retrieved GeoipInfo2 is null.");

            Assert.IsNotNull(retrievedGeoipInfo2, "The GeoipInfo 2 was not found in the database.");
            Assert.AreEqual(geoipRotatingClientResponse.CountryCode, retrievedGeoipInfo2.CountryCode,
                "The CountryCode was not the expected on the retrieved GeoipInfo 2.");
            Assert.AreEqual(EnumsHelper.GeoipProviderName.ToString(geoipRotatingClientResponse.GeoipProviderName),
                retrievedGeoipInfo2.GeoipProviderName,
                "The GeoipProviderName was not the expected on the retrieved GeoipInfo 2.");
            Assert.AreEqual(geoipRotatingClientResponse.Latitude, retrievedGeoipInfo2.Latitude,
                "The Latitude was not the expected on the retrieved GeoipInfo 2.");
            Assert.AreEqual(geoipRotatingClientResponse.Longitude, retrievedGeoipInfo2.Longitude,
                "The Longitude was not the expected on the retrieved GeoipInfo 2.");
            Assert.AreEqual(ipAddress, retrievedGeoipInfo2.IpAddress,
                "The IpAddress was not the expected on the retrieved GeoipInfo 2.");
            Assert.That(retrievedGeoipInfo2.RecordedOn, Is.GreaterThanOrEqualTo(timeBefore2),
                "The RecordedOn on the retrieved GeoipInfo 2 should be after the timeBefore.");
            Assert.That(retrievedGeoipInfo2.RecordedOn, Is.LessThanOrEqualTo(timeAfter2),
                "The RecordedOn on the retrieved GeoipInfo 2 should be before the timeAfter.");
        }

        [Test]
        public async Task GetInfoAsync_ReturnsNull_WhenGeoipRotatingClientDoesNotReturnSuccess()
        {
            var expiryPeriod = TimeSpan.FromSeconds(600);
            var ipAddress = "1.2.3.4";

            var nonSuccessStatuses = EnumsHelper.WebClientResponseStatus.GetValues()
                .Where(x => x != WebClientResponseStatus.Success)
                .ToList();

            var container = CreateContainer();
            SetupConfig(container, expiryPeriod);
            var cache = container.Get<IAppCache>();

            foreach (var status in nonSuccessStatuses)
            {
                cache.Clear();

                var geoipRotatingClientResponse = new GeoipClientResponse
                {
                    CountryCode = EnumsHelper.CountryId.ToString(CountryId.GB),
                    GeoipProviderName = GeoipProviderName.Nekudo,
                    Latitude = 1.0,
                    Longitude = 2.0,
                    RawResponse = "raw-response",
                    Status = status
                };

                SetupGeoipRotatingClient(container, ipAddress, geoipRotatingClientResponse);

                var service = container.Get<IGeoipInfoService>();
                var geoipInfo = await service.GetInfoAsync(ipAddress);

                Assert.IsNull(geoipInfo, string.Format("GeoipInfo was not null for status '{0}'.",
                    EnumsHelper.WebClientResponseStatus.ToString(status)));
            }
        }

        #endregion


        #region Private Helpers

        private void SetupConfig(IKernel container, TimeSpan expiryPeriod)
        {
            var mockConfig = new Mock<IGeoipInfoServiceConfig>();

            mockConfig.Setup(x => x.ExpiryPeriod).Returns(expiryPeriod);

            container.Rebind<IGeoipInfoServiceConfig>().ToConstant(mockConfig.Object);
        }

        private void SetupGeoipRotatingClient(
            IKernel container,
            string ipAddress,
            GeoipClientResponse response)
        {
            var mockGeoipRotatingClient = new Mock<IGeoipRotatingClient>();

            mockGeoipRotatingClient.Setup(x => x.Geoip(It.IsAny<string>(), It.IsAny<int>()))
                .Returns<string, int>((ip, rotation) =>
                {
                    if (ip.Equals(ipAddress))
                        return Task.FromResult(response);
                    else
                        throw new Exception(
                            string.Format("I was expecting IP Address '{0}' to be used in GeoiopRotatingClient but it was '{1}' instead.",
                            ip, ipAddress));
                });

            container.Rebind<IGeoipRotatingClient>().ToConstant(mockGeoipRotatingClient.Object);
        }

        private void DestroyGeoipRotatingClient(IKernel container)
        {
            var mockGeoipRotatingClient = new Mock<IGeoipRotatingClient>();

            container.Rebind<IGeoipRotatingClient>().ToConstant(mockGeoipRotatingClient.Object);
        }

        #endregion
    }
}
