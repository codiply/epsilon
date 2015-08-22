using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Infrastructure;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.Services;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.Services.Interfaces.UserResidenceService;
using Epsilon.Logic.Wrappers;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.UnitTests.Logic.Services
{
    [TestFixture]
    public class UserInterfaceCustomisationServiceTest
    {

        #region GetForUser Scenarios

        [Test]
        public void GetForUser_Scenario1()
        {
            var userId = "user-id";
            var getResidenceResponse = new GetResidenceResponse()
            {
                HasNoSubmissions = true,
                Address = null,
                IsVerified = false
            };
            var canCreateTenancyDetailsSubmissionResponse = new AntiAbuseServiceResponse()
            {
                IsRejected = false
            };

            var appCache = CreateAppCache();
            appCache.Clear();
            var userResidenceService = CreateUserResidenceService(userId, getResidenceResponse);
            var antiAbuseService = CreateAntiAbuseService(userId, canCreateTenancyDetailsSubmissionResponse);
            var serviceUnderTest = new UserInterfaceCustomisationService(
                appCache, userResidenceService, antiAbuseService);

            var response1 = serviceUnderTest.GetForUser(userId);

            Assert.IsTrue(response1.CanCreateTenancyDetailsSubmission, 
                "CanCreateTenancyDetailsSubmission is not the expected in response1.");
            Assert.IsFalse(response1.CanPickOutgoingVerification, 
                "CanPickOutgoingVerification is not the expected in response1.");
            Assert.IsTrue(response1.HasNoTenancyDetailsSubmissions, 
                "HasNoTenancyDetailsSubmissions is not the expected in response1.");
            Assert.IsFalse(response1.IsUserResidenceVerified, 
                "IsUserResidenceVerified is not the expected in response1.");
            Assert.IsNull(response1.UserResidenceCountry, 
                "UserResidenceCountry is not the expected in response1.");

            // Check that the response is cached.
            var serviceWithOnlyCache = new UserInterfaceCustomisationService(appCache, null, null);

            var response2 = serviceUnderTest.GetForUser(userId);

            Assert.IsTrue(response2.CanCreateTenancyDetailsSubmission,
                "CanCreateTenancyDetailsSubmission is not the expected in response2.");
            Assert.IsFalse(response2.CanPickOutgoingVerification,
                "CanPickOutgoingVerification is not the expected in response2.");
            Assert.IsTrue(response2.HasNoTenancyDetailsSubmissions,
                "HasNoTenancyDetailsSubmissions is not the expected in response2.");
            Assert.IsFalse(response2.IsUserResidenceVerified,
                "IsUserResidenceVerified is not the expected in response2.");
            Assert.IsNull(response2.UserResidenceCountry,
                "UserResidenceCountry is not the expected in response2.");

            // Test that we can clear the cache.
            serviceWithOnlyCache.ClearCachedCustomisationForUser(userId);

            Assert.Throws<AggregateException>(() => serviceWithOnlyCache.GetForUser(userId),
                "Service with services missing did not throw exception after clearing the cache. This means it still uses cached value.");

        }

        [Test]
        public void GetForUser_Scenario2()
        {
            var userId = "user-id";
            var getResidenceResponse = new GetResidenceResponse()
            {
                HasNoSubmissions = false,
                Address = new Address() { Country = new Country { Id = EnumsHelper.CountryId.ToString(Epsilon.Logic.Constants.Enums.CountryId.GB) } },
                IsVerified = true
            };
            var canCreateTenancyDetailsSubmissionResponse = new AntiAbuseServiceResponse()
            {
                IsRejected = true
            };

            var appCache = CreateAppCache();
            appCache.Clear();
            var userResidenceService = CreateUserResidenceService(userId, getResidenceResponse);
            var antiAbuseService = CreateAntiAbuseService(userId, canCreateTenancyDetailsSubmissionResponse);
            var serviceUnderTest = new UserInterfaceCustomisationService(
                appCache, userResidenceService, antiAbuseService);

            var response1 = serviceUnderTest.GetForUser(userId);

            Assert.IsFalse(response1.CanCreateTenancyDetailsSubmission,
                "CanCreateTenancyDetailsSubmission is not the expected in response1.");
            Assert.IsTrue(response1.CanPickOutgoingVerification,
                "CanPickOutgoingVerification is not the expected in response1.");
            Assert.IsFalse(response1.HasNoTenancyDetailsSubmissions,
                "HasNoTenancyDetailsSubmissions is not the expected in response1.");
            Assert.IsNotNull(response1.UserResidenceCountry,
                "UserResidenceCountry should not be null in response1.");
            Assert.AreSame(getResidenceResponse.Address.Country, response1.UserResidenceCountry,
                "UserResidenceCountry is not the expected in response1.");

            // Check that the response is cached.
            var serviceWithOnlyCache = new UserInterfaceCustomisationService(appCache, null, null);

            var response2 = serviceUnderTest.GetForUser(userId);

            Assert.IsFalse(response2.CanCreateTenancyDetailsSubmission,
                "CanCreateTenancyDetailsSubmission is not the expected in response2.");
            Assert.IsTrue(response2.CanPickOutgoingVerification,
                "CanPickOutgoingVerification is not the expected in response2.");
            Assert.IsFalse(response2.HasNoTenancyDetailsSubmissions,
                "HasNoTenancyDetailsSubmissions is not the expected in response2.");
            Assert.IsTrue(response2.IsUserResidenceVerified,
                "IsUserResidenceVerified is not the expected in response2.");
            Assert.IsNotNull(response2.UserResidenceCountry,
                "UserResidenceCountry should not be null in response2.");
            Assert.AreSame(getResidenceResponse.Address.Country, response2.UserResidenceCountry,
                "UserResidenceCountry is not the expected in response2.");

            // Test that we can clear the cache.
            serviceWithOnlyCache.ClearCachedCustomisationForUser(userId);

            Assert.Throws<AggregateException>(() => serviceWithOnlyCache.GetForUser(userId),
                "Service with services missing did not throw exception after clearing the cache. This means it still uses cached value.");
        }

        #endregion

        #region GetForUser Scenarios

        [Test]
        public async Task GetForUserAsync_Scenario1()
        {
            var userId = "user-id";
            var getResidenceResponse = new GetResidenceResponse()
            {
                HasNoSubmissions = true,
                Address = null,
                IsVerified = false
            };
            var canCreateTenancyDetailsSubmissionResponse = new AntiAbuseServiceResponse()
            {
                IsRejected = false
            };

            var appCache = CreateAppCache();
            appCache.Clear();
            var userResidenceService = CreateUserResidenceService(userId, getResidenceResponse);
            var antiAbuseService = CreateAntiAbuseService(userId, canCreateTenancyDetailsSubmissionResponse);
            var serviceUnderTest = new UserInterfaceCustomisationService(
                appCache, userResidenceService, antiAbuseService);

            var response1 = await serviceUnderTest.GetForUserAsync(userId);

            Assert.IsTrue(response1.CanCreateTenancyDetailsSubmission,
                "CanCreateTenancyDetailsSubmission is not the expected in response1.");
            Assert.IsFalse(response1.CanPickOutgoingVerification,
                "CanPickOutgoingVerification is not the expected in response1.");
            Assert.IsTrue(response1.HasNoTenancyDetailsSubmissions,
                "HasNoTenancyDetailsSubmissions is not the expected in response1.");
            Assert.IsFalse(response1.IsUserResidenceVerified,
                "IsUserResidenceVerified is not the expected in response1.");
            Assert.IsNull(response1.UserResidenceCountry,
                "UserResidenceCountry is not the expected in response1.");

            // Check that the response is cached.
            var serviceWithOnlyCache = new UserInterfaceCustomisationService(appCache, null, null);

            var response2 = await serviceUnderTest.GetForUserAsync(userId);

            Assert.IsTrue(response2.CanCreateTenancyDetailsSubmission,
                "CanCreateTenancyDetailsSubmission is not the expected in response2.");
            Assert.IsFalse(response2.CanPickOutgoingVerification,
                "CanPickOutgoingVerification is not the expected in response2.");
            Assert.IsTrue(response2.HasNoTenancyDetailsSubmissions,
                "HasNoTenancyDetailsSubmissions is not the expected in response2.");
            Assert.IsFalse(response2.IsUserResidenceVerified,
                "IsUserResidenceVerified is not the expected in response2.");
            Assert.IsNull(response2.UserResidenceCountry,
                "UserResidenceCountry is not the expected in response2.");
        }

        [Test]
        public async Task GetForUserAsync_Scenario2()
        {
            var userId = "user-id";
            var getResidenceResponse = new GetResidenceResponse()
            {
                HasNoSubmissions = false,
                Address = new Address() { Country = new Country { Id = EnumsHelper.CountryId.ToString(Epsilon.Logic.Constants.Enums.CountryId.GB) } },
                IsVerified = true
            };
            var canCreateTenancyDetailsSubmissionResponse = new AntiAbuseServiceResponse()
            {
                IsRejected = true
            };

            var appCache = CreateAppCache();
            appCache.Clear();
            var userResidenceService = CreateUserResidenceService(userId, getResidenceResponse);
            var antiAbuseService = CreateAntiAbuseService(userId, canCreateTenancyDetailsSubmissionResponse);
            var serviceUnderTest = new UserInterfaceCustomisationService(
                appCache, userResidenceService, antiAbuseService);

            var response1 = await serviceUnderTest.GetForUserAsync(userId);

            Assert.IsFalse(response1.CanCreateTenancyDetailsSubmission,
                "CanCreateTenancyDetailsSubmission is not the expected in response1.");
            Assert.IsTrue(response1.CanPickOutgoingVerification,
                "CanPickOutgoingVerification is not the expected in response1.");
            Assert.IsFalse(response1.HasNoTenancyDetailsSubmissions,
                "HasNoTenancyDetailsSubmissions is not the expected in response1.");
            Assert.IsNotNull(response1.UserResidenceCountry,
                "UserResidenceCountry should not be null in response1.");
            Assert.AreSame(getResidenceResponse.Address.Country, response1.UserResidenceCountry,
                "UserResidenceCountry is not the expected in response1.");

            // Check that the response is cached.
            var serviceWithOnlyCache = new UserInterfaceCustomisationService(appCache, null, null);

            var response2 = await serviceUnderTest.GetForUserAsync(userId);

            Assert.IsFalse(response2.CanCreateTenancyDetailsSubmission,
                "CanCreateTenancyDetailsSubmission is not the expected in response2.");
            Assert.IsTrue(response2.CanPickOutgoingVerification,
                "CanPickOutgoingVerification is not the expected in response2.");
            Assert.IsFalse(response2.HasNoTenancyDetailsSubmissions,
                "HasNoTenancyDetailsSubmissions is not the expected in response2.");
            Assert.IsTrue(response2.IsUserResidenceVerified,
                "IsUserResidenceVerified is not the expected in response2.");
            Assert.IsNotNull(response2.UserResidenceCountry,
                "UserResidenceCountry should not be null in response2.");
            Assert.AreSame(getResidenceResponse.Address.Country, response2.UserResidenceCountry,
                "UserResidenceCountry is not the expected in response2.");
        }

        #endregion

        #region Private Helpers

        private IAntiAbuseService CreateAntiAbuseService(
            string expectedUserId,
            AntiAbuseServiceResponse canCreateTenancyDetailsSubmissionResponse)
        {
            var mockAntiAbuseService = new Mock<IAntiAbuseService>();

            mockAntiAbuseService.Setup(x => x.CanCreateTenancyDetailsSubmissionCheckUserFrequency(It.IsAny<string>()))
                .Returns<string>((userId) =>
                {
                    if (userId.Equals(expectedUserId))
                        return Task.FromResult(canCreateTenancyDetailsSubmissionResponse);
                    else
                        throw new Exception(string.Format("UserId used in CanCreateTenancyDetailsSubmissionCheckUserFrequency was '{0}' but I was expecting '{1}'.", userId, expectedUserId));
                });

            return mockAntiAbuseService.Object;
        }

        private IUserResidenceService CreateUserResidenceService(
            string expectedUserId, GetResidenceResponse getResidenceResponse)
        {
            var mockUserResidenceService = new Mock<IUserResidenceService>();


            mockUserResidenceService.Setup(x => x.GetResidence(It.IsAny<string>()))
                .Returns<string>((userId) =>
                {
                    if (userId.Equals(expectedUserId))
                        return Task.FromResult(getResidenceResponse);
                    else
                        throw new Exception(string.Format("UserId used in GetResidence was '{0}' but I was expecting '{1}'.", userId, expectedUserId));
                });

            return mockUserResidenceService.Object;
        }

        private IAppCache CreateAppCache()
        {
            var cache = new HttpRuntimeCache();

            var mockAppCacheConfig = new Mock<IAppCacheConfig>();
            mockAppCacheConfig.Setup(x => x.DisableAppCache).Returns(false);
            mockAppCacheConfig.Setup(x => x.DisableAsynchronousLocking).Returns(false);
            mockAppCacheConfig.Setup(x => x.DisableSynchronousLocking).Returns(false);

            return new AppCache(cache, mockAppCacheConfig.Object);
        }

        #endregion
    }
}
