using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.IntegrationTests.TestHelpers;
using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.Services.Interfaces.UserResidenceService;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Wrappers;
using Epsilon.Logic.Wrappers.Interfaces;
using Moq;
using Ninject;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Epsilon.Logic.Helpers.RandomStringHelper;

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class PropertyInfoAccessServiceTest : BaseIntegrationTestWithRollback
    {
        private TimeSpan _smallDelay = TimeSpan.FromMilliseconds(20);

        #region GetUserExploredPropertiesSummary

        [Test]
        public async Task GetUserExploredPropertiesSummary_ForUserWithoutPropertyInfoAccesses()
        {
            var helperContainer = CreateContainer();

            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);

            var random = new RandomWrapper(2015);

            // I create a property info access for the other user and assign the submission to the user under test.
            // This is to test that the summary contains only verifications from the specific user.
            var otherUserIpAddress = "1.2.3.5";
            var otherUser = await CreateUser(helperContainer, "test2@test.com", otherUserIpAddress);
            var otherUserPropertyInfoAccess = await CreatePropertyInfoAccessAndSave(
                    random, helperContainer, otherUser.Id, otherUserIpAddress, user.Id, userIpAddress);
            Assert.IsNotNull(otherUserPropertyInfoAccess, "The property info access created for the other user is null.");

            var containerUnderTest = CreateContainer();
            var serviceUnderTest = containerUnderTest.Get<IPropertyInfoAccessService>();

            // Full summary
            var response1 = await serviceUnderTest.GetUserExploredPropertiesSummary(user.Id, false);

            Assert.IsNotNull(response1, "Response1 is null.");
            Assert.IsFalse(response1.moreItemsExist, "Field moreItemsExist on response1 is not the expected.");
            Assert.IsFalse(response1.exploredProperties.Any(), "Field exploredProperties on response1 should be empty.");

            // Summary with limit
            var response2 = await serviceUnderTest.GetUserExploredPropertiesSummary(user.Id, true);

            Assert.IsNotNull(response2, "Response2 is null.");
            Assert.IsFalse(response2.moreItemsExist, "Field moreItemsExist on response2 is not the expected.");
            Assert.IsFalse(response2.exploredProperties.Any(), "Field exploredProperties on response2 should be empty.");
        }

        [Test]
        public async Task GetUserExploredPropertiesSummary_WithPropertyInfoAccessesEqualToTheLimit_ItemsLimitIsNotRelevant()
        {
            var itemsLimit = 3;
            var propertyInfoAccessesToCreate = itemsLimit;

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", otherUserIpAddress);

            var random = new RandomWrapper(2015);
            var propertyInfoAccesses = new List<PropertyInfoAccess>();

            for (var i = 0; i <propertyInfoAccessesToCreate; i++)
            {
                var propertyInfoAccess = await CreatePropertyInfoAccessAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress);
                propertyInfoAccesses.Add(propertyInfoAccess);
                await Task.Delay(_smallDelay);
            }
            var propertyInfoAccessesByCreationDescending = propertyInfoAccesses.OrderByDescending(x => x.CreatedOn).ToList();

            // I create a property info access for the other user and assign the submission to the user under test.
            // This is to test that the summary contains only verifications from the specific user.
            var otherUserPropertyInfoAccess = await CreatePropertyInfoAccessAndSave(
                    random, helperContainer, otherUser.Id, otherUserIpAddress, user.Id, userIpAddress);
            Assert.IsNotNull(otherUserPropertyInfoAccess, "The property info access created for the other user is null.");

            var containerUnderTest = CreateContainer();
            SetupConfigForGetUserExploredPropertiesSummary(containerUnderTest, itemsLimit);
            var serviceUnderTest = containerUnderTest.Get<IPropertyInfoAccessService>();

            // Full summary
            var response1 = await serviceUnderTest.GetUserExploredPropertiesSummary(user.Id, false);

            Assert.IsNotNull(response1, "Response1 is null.");
            Assert.IsFalse(response1.moreItemsExist, "Field moreItemsExist on response1 is not the expected.");
            Assert.AreEqual(propertyInfoAccessesToCreate, response1.exploredProperties.Count,
                "Response1 should contain all property info accesses.");
            for (var i = 0; i < propertyInfoAccessesToCreate; i++)
            {
                Assert.AreEqual(propertyInfoAccessesByCreationDescending[i].UniqueId, response1.exploredProperties[i].accessUniqueId,
                    string.Format("Response1: explored property at position {0} does not have the expected uniqueId.", i));
            }

            Assert.IsFalse(response1.exploredProperties.Any(x => x.accessUniqueId.Equals(otherUserPropertyInfoAccess.UniqueId)),
                "Response1 should not contain the property info acccess of the other user.");

            // Summary with limit
            var response2 = await serviceUnderTest.GetUserExploredPropertiesSummary(user.Id, true);

            Assert.IsNotNull(response2, "Response2 is null.");
            Assert.IsFalse(response2.moreItemsExist, "Field moreItemsExist on response2 is not the expected.");
            Assert.IsTrue(response2.exploredProperties.Any(), "Field exploredProperties on response2 should not be empty.");

            Assert.AreEqual(itemsLimit, response2.exploredProperties.Count,
                "Response2 should contain a number of explored properties equal to the limit.");
            for (var i = 0; i < itemsLimit; i++)
            {
                Assert.AreEqual(propertyInfoAccessesByCreationDescending[i].UniqueId, response2.exploredProperties[i].accessUniqueId,
                    string.Format("Response2: explored property at position {0} does not have the expected uniqueId.", i));
            }

            Assert.IsFalse(response2.exploredProperties.Any(x => x.accessUniqueId.Equals(otherUserPropertyInfoAccess.UniqueId)),
                "Response1 should not contain the property info access of the other user.");
        }

        //[Test]
        //public async Task GetUserExploredPropertiesSummary_WithMoreOutgoingVerificationsThanTheLimit_ItemsLimitIsRespected()
        //{
        //    var itemsLimit = 2;
        //    var outgoingVerificationsToCreate = 3;

        //    var helperContainer = CreateContainer();
        //    var userIpAddress = "1.2.3.4";
        //    var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
        //    var otherUserIpAddress = "11.12.13.14";
        //    var otherUser = await CreateUser(helperContainer, "other-user@test.com", otherUserIpAddress);

        //    var random = new RandomWrapper(2015);
        //    var tenantVerifications = new List<TenantVerification>();

        //    for (var i = 0; i < outgoingVerificationsToCreate; i++)
        //    {
        //        var tenantVerification = await CreateTenantVerificationAndSave(
        //            random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress, false, false);
        //        tenantVerifications.Add(tenantVerification);
        //        await Task.Delay(_smallDelay);
        //    }
        //    var tenantVerificationsByCreationDescending = tenantVerifications.OrderByDescending(x => x.CreatedOn).ToList();

        //    // I create a property info access for the other user and assign the submission to the user under test.
        //    // This is to test that the summary contains only verifications from the specific user.
        //    var otherUserOutgoingVerification = await CreateTenantVerificationAndSave(
        //            random, helperContainer, otherUser.Id, otherUserIpAddress, user.Id, userIpAddress, false, false);
        //    Assert.IsNotNull(otherUserOutgoingVerification, "The outgoing verification created for the other user is null.");

        //    var containerUnderTest = CreateContainer();
        //    SetupConfigForGetUserOutgoingVerificationsSummary(containerUnderTest, itemsLimit);
        //    var serviceUnderTest = containerUnderTest.Get<IOutgoingVerificationService>();

        //    // Full summary
        //    var response1 = await serviceUnderTest.GetUserOutgoingVerificationsSummary(user.Id, false);

        //    Assert.IsNotNull(response1, "Response1 is null.");
        //    Assert.IsFalse(response1.moreItemsExist, "Field moreItemsExist on response1 is not the expected.");
        //    Assert.AreEqual(outgoingVerificationsToCreate, response1.tenantVerifications.Count,
        //        "Response1 should contain all tenant verifications.");
        //    for (var i = 0; i < outgoingVerificationsToCreate; i++)
        //    {
        //        Assert.AreEqual(tenantVerificationsByCreationDescending[i].UniqueId, response1.tenantVerifications[i].uniqueId,
        //            string.Format("Response1: tenant verification at position {0} does not have the expected uniqueId.", i));
        //    }

        //    Assert.IsFalse(response1.tenantVerifications.Any(x => x.uniqueId.Equals(otherUserOutgoingVerification.UniqueId)),
        //        "Response1 should not contain the outgoing verification of the other user.");

        //    // Summary with limit
        //    var response2 = await serviceUnderTest.GetUserOutgoingVerificationsSummary(user.Id, true);

        //    Assert.IsNotNull(response2, "Response2 is null.");
        //    Assert.IsTrue(response2.moreItemsExist, "Field moreItemsExist on response2 is not the expected.");
        //    Assert.IsTrue(response2.tenantVerifications.Any(), "Field tenantVerifications on response2 should not be empty.");

        //    Assert.AreEqual(itemsLimit, response2.tenantVerifications.Count,
        //        "Response2 should contain a number of outgoing verifications equal to the limit.");
        //    for (var i = 0; i < itemsLimit; i++)
        //    {
        //        Assert.AreEqual(tenantVerificationsByCreationDescending[i].UniqueId, response2.tenantVerifications[i].uniqueId,
        //            string.Format("Response2: tenant verification at position {0} does not have the expected uniqueId.", i));
        //    }

        //    Assert.IsFalse(response2.tenantVerifications.Any(x => x.uniqueId.Equals(otherUserOutgoingVerification.UniqueId)),
        //        "Response1 should not contain the outgoing verification of the other user.");
        //}

        //[Test]
        //public async Task GetUserExploredPropertiesSummary_SingleNewOutgoingVerificationTest()
        //{
        //    var itemsLimit = 10;
        //    var isSent = false;
        //    var isComplete = false;

        //    var helperContainer = CreateContainer();
        //    var userIpAddress = "1.2.3.4";
        //    var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
        //    var otherUserIpAddress = "11.12.13.14";
        //    var otherUser = await CreateUser(helperContainer, "other-user@test.com", otherUserIpAddress);

        //    var random = new RandomWrapper(2015);
        //    var tenantVerification = await CreateTenantVerificationAndSave(
        //            random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress, isSent, isComplete);

        //    var containerUnderTest = CreateContainer();
        //    SetupConfigForGetUserOutgoingVerificationsSummary(containerUnderTest, itemsLimit);
        //    var serviceUnderTest = containerUnderTest.Get<IOutgoingVerificationService>();

        //    var response = await serviceUnderTest.GetUserOutgoingVerificationsSummary(user.Id, false);

        //    Assert.AreEqual(1, response.tenantVerifications.Count,
        //        "The response should contain a single tenant verification.");

        //    var tenantVerificationInfo = response.tenantVerifications.Single();

        //    Assert.AreEqual(tenantVerification.UniqueId, tenantVerificationInfo.uniqueId,
        //        "Field uniqueId is not the expected.");
        //    Assert.IsTrue(tenantVerificationInfo.canMarkAsSent, "Field canMarkAsSent doesn't have the expected value.");

        //    Assert.IsFalse(tenantVerificationInfo.stepVerificationSentOutDone, "Field stepVerificationCodeSentOutDone doesn't have the expected value.");
        //    Assert.IsFalse(tenantVerificationInfo.stepVerificationReceivedDone, "Field stepVerificationReceivedDone doesn't have the expected value.");

        //    var retrievedTenantVerification = await DbProbe.TenantVerifications
        //        .Include(x => x.TenancyDetailsSubmission.Address)
        //        .SingleOrDefaultAsync(x => x.UniqueId.Equals(tenantVerificationInfo.uniqueId));
        //    Assert.AreEqual(retrievedTenantVerification.TenancyDetailsSubmission.Address.LocalityRegionPostcode(), tenantVerificationInfo.addressArea,
        //        "Field addressArea is not the expected.");
        //}

        //[Test]
        //public async Task GetUserExploredPropertiesSummary_SingleOutgoingVerificationJustSentOutTest()
        //{
        //    var itemsLimit = 10;
        //    var isSent = true;
        //    var isComplete = false;

        //    var helperContainer = CreateContainer();
        //    var userIpAddress = "1.2.3.4";
        //    var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
        //    var otherUserIpAddress = "11.12.13.14";
        //    var otherUser = await CreateUser(helperContainer, "other-user@test.com", otherUserIpAddress);

        //    var random = new RandomWrapper(2015);
        //    var tenantVerification = await CreateTenantVerificationAndSave(
        //            random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress, isSent, isComplete);

        //    var containerUnderTest = CreateContainer();
        //    SetupConfigForGetUserOutgoingVerificationsSummary(containerUnderTest, itemsLimit);
        //    var serviceUnderTest = containerUnderTest.Get<IOutgoingVerificationService>();

        //    var response = await serviceUnderTest.GetUserOutgoingVerificationsSummary(user.Id, false);

        //    Assert.AreEqual(1, response.tenantVerifications.Count,
        //        "The response should contain a single tenant verification.");

        //    var tenantVerificationInfo = response.tenantVerifications.Single();

        //    Assert.AreEqual(tenantVerification.UniqueId, tenantVerificationInfo.uniqueId,
        //        "Field uniqueId is not the expected.");
        //    Assert.IsFalse(tenantVerificationInfo.canMarkAsSent, "Field canMarkAsSent doesn't have the expected value.");

        //    Assert.IsTrue(tenantVerificationInfo.stepVerificationSentOutDone, "Field stepVerificationCodeSentOutDone doesn't have the expected value.");
        //    Assert.IsFalse(tenantVerificationInfo.stepVerificationReceivedDone, "Field stepVerificationReceivedDone doesn't have the expected value.");

        //    var retrievedTenantVerification = await DbProbe.TenantVerifications
        //        .Include(x => x.TenancyDetailsSubmission.Address)
        //        .SingleOrDefaultAsync(x => x.UniqueId.Equals(tenantVerificationInfo.uniqueId));
        //    Assert.AreEqual(retrievedTenantVerification.TenancyDetailsSubmission.Address.LocalityRegionPostcode(), tenantVerificationInfo.addressArea,
        //        "Field addressArea is not the expected.");
        //}

        //[Test]
        //public async Task GetUserExploredPropertiesSummary_SingleOutgoingVerificationSentOutAndCompleteTest()
        //{
        //    var itemsLimit = 10;
        //    var isSent = true;
        //    var isComplete = true;

        //    var helperContainer = CreateContainer();
        //    var userIpAddress = "1.2.3.4";
        //    var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
        //    var otherUserIpAddress = "11.12.13.14";
        //    var otherUser = await CreateUser(helperContainer, "other-user@test.com", otherUserIpAddress);

        //    var random = new RandomWrapper(2015);
        //    var tenantVerification = await CreateTenantVerificationAndSave(
        //            random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress, isSent, isComplete);

        //    var containerUnderTest = CreateContainer();
        //    SetupConfigForGetUserOutgoingVerificationsSummary(containerUnderTest, itemsLimit);
        //    var serviceUnderTest = containerUnderTest.Get<IOutgoingVerificationService>();

        //    var response = await serviceUnderTest.GetUserOutgoingVerificationsSummary(user.Id, false);

        //    Assert.AreEqual(1, response.tenantVerifications.Count,
        //        "The response should contain a single tenant verification.");

        //    var tenantVerificationInfo = response.tenantVerifications.Single();

        //    Assert.AreEqual(tenantVerification.UniqueId, tenantVerificationInfo.uniqueId,
        //        "Field uniqueId is not the expected.");
        //    Assert.IsFalse(tenantVerificationInfo.canMarkAsSent, "Field canMarkAsSent doesn't have the expected value.");

        //    Assert.IsTrue(tenantVerificationInfo.stepVerificationSentOutDone, "Field stepVerificationCodeSentOutDone doesn't have the expected value.");
        //    Assert.IsTrue(tenantVerificationInfo.stepVerificationReceivedDone, "Field stepVerificationReceivedDone doesn't have the expected value.");

        //    var retrievedTenantVerification = await DbProbe.TenantVerifications
        //        .Include(x => x.TenancyDetailsSubmission.Address)
        //        .SingleOrDefaultAsync(x => x.UniqueId.Equals(tenantVerificationInfo.uniqueId));
        //    Assert.AreEqual(retrievedTenantVerification.TenancyDetailsSubmission.Address.LocalityRegionPostcode(), tenantVerificationInfo.addressArea,
        //        "Field addressArea is not the expected.");
        //}

        //[Test]
        //public async Task GetUserExploredPropertiesSummary_SingleOutgoingVerificationCompleteButNotMarkedAsSentOutTest()
        //{
        //    var itemsLimit = 10;
        //    var isSent = false;
        //    var isComplete = true;

        //    var helperContainer = CreateContainer();
        //    var userIpAddress = "1.2.3.4";
        //    var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
        //    var otherUserIpAddress = "11.12.13.14";
        //    var otherUser = await CreateUser(helperContainer, "other-user@test.com", otherUserIpAddress);

        //    var random = new RandomWrapper(2015);
        //    var tenantVerification = await CreateTenantVerificationAndSave(
        //            random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress, isSent, isComplete);

        //    var containerUnderTest = CreateContainer();
        //    SetupConfigForGetUserOutgoingVerificationsSummary(containerUnderTest, itemsLimit);
        //    var serviceUnderTest = containerUnderTest.Get<IOutgoingVerificationService>();

        //    var response = await serviceUnderTest.GetUserOutgoingVerificationsSummary(user.Id, false);

        //    Assert.AreEqual(1, response.tenantVerifications.Count,
        //        "The response should contain a single tenant verification.");

        //    var tenantVerificationInfo = response.tenantVerifications.Single();

        //    Assert.AreEqual(tenantVerification.UniqueId, tenantVerificationInfo.uniqueId,
        //        "Field uniqueId is not the expected.");
        //    Assert.IsTrue(tenantVerificationInfo.canMarkAsSent, "Field canMarkAsSent doesn't have the expected value.");

        //    Assert.IsFalse(tenantVerificationInfo.stepVerificationSentOutDone, "Field stepVerificationCodeSentOutDone doesn't have the expected value.");
        //    Assert.IsTrue(tenantVerificationInfo.stepVerificationReceivedDone, "Field stepVerificationReceivedDone doesn't have the expected value.");

        //    var retrievedTenantVerification = await DbProbe.TenantVerifications
        //        .Include(x => x.TenancyDetailsSubmission.Address)
        //        .SingleOrDefaultAsync(x => x.UniqueId.Equals(tenantVerificationInfo.uniqueId));
        //    Assert.AreEqual(retrievedTenantVerification.TenancyDetailsSubmission.Address.LocalityRegionPostcode(), tenantVerificationInfo.addressArea,
        //        "Field addressArea is not the expected.");
        //}

        #endregion

        #region Private helper functions

        private static void SetupConfigForGetUserExploredPropertiesSummary(IKernel container, int itemsLimit)
        {
            var mockConfig = new Mock<IPropertyInfoAccessServiceConfig>();

            mockConfig.Setup(x => x.MyExploredPropertiesSummary_ItemsLimit).Returns(itemsLimit);

            container.Rebind<IPropertyInfoAccessServiceConfig>().ToConstant(mockConfig.Object);
        }

        private static void SetupConfigForGetUserExploredPropertiesSummaryWithCaching(IKernel container, int itemsLimit, TimeSpan cachingPeriod)
        {
            var mockConfig = new Mock<IPropertyInfoAccessServiceConfig>();

            mockConfig.Setup(x => x.MyExploredPropertiesSummary_ItemsLimit).Returns(itemsLimit);
            mockConfig.Setup(x => x.MyExploredPropertiesSummary_CachingPeriod).Returns(cachingPeriod);

            container.Rebind<IPropertyInfoAccessServiceConfig>().ToConstant(mockConfig.Object);
        }

        private static async Task<PropertyInfoAccess> CreatePropertyInfoAccessAndSave(
            IRandomWrapper random, IKernel container,
            string userId, string userIpAddress,
            string userIdForSubmission, string userForSubmissionIpAddress, int numberOfSubmission = 1, 
            CountryId countryId = CountryId.GB, CurrencyId currencyId = CurrencyId.GBP)
        {
            var clock = container.Get<IClock>();

            var address = await AddressHelper.CreateRandomAddressAndSave(
                random, container, userIdForSubmission, userForSubmissionIpAddress, countryId);

            var dbContext = container.Get<IEpsilonContext>();

            for (var i = 0; i < numberOfSubmission; i++)
            {
                var tenancyDetailsSubmission = new TenancyDetailsSubmission
                {
                    UniqueId = Guid.NewGuid(),
                    AddressId = address.Id,
                    UserId = userIdForSubmission,
                    CreatedByIpAddress = userForSubmissionIpAddress,
                    RentPerMonth = random.Next(100, 1000),
                    CurrencyId = EnumsHelper.CurrencyId.ToString(currencyId),
                    SubmittedOn = clock.OffsetNow
                };
                dbContext.TenancyDetailsSubmissions.Add(tenancyDetailsSubmission);
            }

            var propertyInfoAccess = new PropertyInfoAccess()
            {
                UniqueId = Guid.NewGuid(),
                AddressId = address.Id,
                CreatedOn = clock.OffsetNow,
                UserId = userId,
                CreatedByIpAddress = userIpAddress
            };

            dbContext.PropertyInfoAccesses.Add(propertyInfoAccess);
            await dbContext.SaveChangesAsync();

            return propertyInfoAccess;
        }

        #endregion
    }
}
