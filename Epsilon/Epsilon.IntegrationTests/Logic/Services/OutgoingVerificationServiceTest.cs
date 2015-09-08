using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.IntegrationTests.TestHelpers;
using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Entities.Interfaces;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.Services.Interfaces.UserResidenceService;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Wrappers;
using Epsilon.Logic.Wrappers.Interfaces;
using Epsilon.Resources.Logic.OutgoingVerification;
using Moq;
using Ninject;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using static Epsilon.Logic.Helpers.RandomStringHelper;

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class OutgoingVerificationServiceTest : BaseIntegrationTestWithRollback
    {
        private TimeSpan _smallDelay = TimeSpan.FromMilliseconds(30);

        #region GetUserOutgoingVerificationsSummary

        [Test]
        public async Task GetUserOutgoingVerificationsSummary_ForUserWithoutOutgoingVerifications()
        {
            var helperContainer = CreateContainer();

            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);

            var random = new RandomWrapper(2015);

            // I create an outgoing verification for the other user and assign the submission to the user under test.
            // This is to test that the summary contains only verifications from the specific user.
            var otherUserIpAddress = "1.2.3.5";
            var otherUser = await CreateUser(helperContainer, "test2@test.com", otherUserIpAddress);
            var otherUserOutgoingVerification = await CreateTenantVerificationAndSave(
                    random, helperContainer, otherUser.Id, otherUserIpAddress, user.Id, userIpAddress, false, false);
            Assert.IsNotNull(otherUserOutgoingVerification, "The outgoing verification created for the other user is null.");

            var containerUnderTest = CreateContainer();
            var serviceUnderTest = containerUnderTest.Get<IOutgoingVerificationService>();

            // Full summary
            var response1 = await serviceUnderTest.GetUserOutgoingVerificationsSummary(user.Id, false);

            Assert.IsNotNull(response1, "Response1 is null.");
            Assert.IsFalse(response1.moreItemsExist, "Field moreItemsExist on response1 is not the expected.");
            Assert.IsFalse(response1.tenantVerifications.Any(), "Field tenantVerifications on response1 should be empty.");

            // Summary with limit
            var response2 = await serviceUnderTest.GetUserOutgoingVerificationsSummary(user.Id, true);

            Assert.IsNotNull(response2, "Response2 is null.");
            Assert.IsFalse(response2.moreItemsExist, "Field moreItemsExist on response2 is not the expected.");
            Assert.IsFalse(response2.tenantVerifications.Any(), "Field tenantVerifications on response2 should be empty.");
        }

        [Test]
        public async Task GetUserOutgoingVerificationsSummary_WithOutgoingVerificationsEqualToTheLimit_ItemsLimitIsNotRelevant()
        {
            var itemsLimit = 3;
            var outgoingVerificationsToCreate = itemsLimit;

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", otherUserIpAddress);

            var random = new RandomWrapper(2015);
            var tenantVerifications = new List<TenantVerification>();

            for (var i = 0; i < outgoingVerificationsToCreate; i++)
            {
                var tenantVerification = await CreateTenantVerificationAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress, false, false);
                tenantVerifications.Add(tenantVerification);
                await Task.Delay(_smallDelay);
            }
            var tenantVerificationsByCreationDescending = tenantVerifications.OrderByDescending(x => x.CreatedOn).ToList();


            // I create an outgoing verification for the other user and assign the submission to the user under test.
            // This is to test that the summary contains only verifications from the specific user.
            var otherUserOutgoingVerification = await CreateTenantVerificationAndSave(
                    random, helperContainer, otherUser.Id, otherUserIpAddress, user.Id, userIpAddress, false, false);
            Assert.IsNotNull(otherUserOutgoingVerification, "The outgoing verification created for the other user is null.");

            var containerUnderTest = CreateContainer();
            SetupConfigForGetUserOutgoingVerificationsSummary(containerUnderTest, itemsLimit);
            var serviceUnderTest = containerUnderTest.Get<IOutgoingVerificationService>();

            // Full summary
            var response1 = await serviceUnderTest.GetUserOutgoingVerificationsSummary(user.Id, false);

            Assert.IsNotNull(response1, "Response1 is null.");
            Assert.IsFalse(response1.moreItemsExist, "Field moreItemsExist on response1 is not the expected.");
            Assert.AreEqual(outgoingVerificationsToCreate, response1.tenantVerifications.Count,
                "Response1 should contain all tenant verifications.");
            for (var i = 0; i < outgoingVerificationsToCreate; i++)
            {
                Assert.AreEqual(tenantVerificationsByCreationDescending[i].UniqueId, response1.tenantVerifications[i].uniqueId,
                    string.Format("Response1: tenant verification at position {0} does not have the expected uniqueId.", i));
            }

            Assert.IsFalse(response1.tenantVerifications.Any(x => x.uniqueId.Equals(otherUserOutgoingVerification.UniqueId)),
                "Response1 should not contain the outgoing verification of the other user.");

            // Summary with limit
            var response2 = await serviceUnderTest.GetUserOutgoingVerificationsSummary(user.Id, true);

            Assert.IsNotNull(response2, "Response2 is null.");
            Assert.IsFalse(response2.moreItemsExist, "Field moreItemsExist on response2 is not the expected.");
            Assert.IsTrue(response2.tenantVerifications.Any(), "Field tenantVerifications on response2 should not be empty.");

            Assert.AreEqual(itemsLimit, response2.tenantVerifications.Count,
                "Response2 should contain a number of outgoing verifications equal to the limit.");
            for (var i = 0; i < itemsLimit; i++)
            {
                Assert.AreEqual(tenantVerificationsByCreationDescending[i].UniqueId, response2.tenantVerifications[i].uniqueId,
                    string.Format("Response2: tenant verification at position {0} does not have the expected uniqueId.", i));
            }

            Assert.IsFalse(response2.tenantVerifications.Any(x => x.uniqueId.Equals(otherUserOutgoingVerification.UniqueId)),
                "Response1 should not contain the outgoing verification of the other user.");
        }

        [Test]
        public async Task GetUserOutgoingVerificationsSummary_WithMoreOutgoingVerificationsThanTheLimit_ItemsLimitIsRespected()
        {
            var itemsLimit = 2;
            var outgoingVerificationsToCreate = 3;

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", otherUserIpAddress);

            var random = new RandomWrapper(2015);
            var tenantVerifications = new List<TenantVerification>();

            for (var i = 0; i < outgoingVerificationsToCreate; i++)
            {
                var tenantVerification = await CreateTenantVerificationAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress, false, false);
                tenantVerifications.Add(tenantVerification);
                await Task.Delay(_smallDelay);
            }
            var tenantVerificationsByCreationDescending = tenantVerifications.OrderByDescending(x => x.CreatedOn).ToList();

            // I create an outgoing verification for the other user and assign the submission to the user under test.
            // This is to test that the summary contains only verifications from the specific user.
            var otherUserOutgoingVerification = await CreateTenantVerificationAndSave(
                    random, helperContainer, otherUser.Id, otherUserIpAddress, user.Id, userIpAddress, false, false);
            Assert.IsNotNull(otherUserOutgoingVerification, "The outgoing verification created for the other user is null.");

            var containerUnderTest = CreateContainer();
            SetupConfigForGetUserOutgoingVerificationsSummary(containerUnderTest, itemsLimit);
            var serviceUnderTest = containerUnderTest.Get<IOutgoingVerificationService>();

            // Full summary
            var response1 = await serviceUnderTest.GetUserOutgoingVerificationsSummary(user.Id, false);

            Assert.IsNotNull(response1, "Response1 is null.");
            Assert.IsFalse(response1.moreItemsExist, "Field moreItemsExist on response1 is not the expected.");
            Assert.AreEqual(outgoingVerificationsToCreate, response1.tenantVerifications.Count,
                "Response1 should contain all tenant verifications.");
            for (var i = 0; i < outgoingVerificationsToCreate; i++)
            {
                Assert.AreEqual(tenantVerificationsByCreationDescending[i].UniqueId, response1.tenantVerifications[i].uniqueId,
                    string.Format("Response1: tenant verification at position {0} does not have the expected uniqueId.", i));
            }

            Assert.IsFalse(response1.tenantVerifications.Any(x => x.uniqueId.Equals(otherUserOutgoingVerification.UniqueId)),
                "Response1 should not contain the outgoing verification of the other user.");

            // Summary with limit
            var response2 = await serviceUnderTest.GetUserOutgoingVerificationsSummary(user.Id, true);

            Assert.IsNotNull(response2, "Response2 is null.");
            Assert.IsTrue(response2.moreItemsExist, "Field moreItemsExist on response2 is not the expected.");
            Assert.IsTrue(response2.tenantVerifications.Any(), "Field tenantVerifications on response2 should not be empty.");

            Assert.AreEqual(itemsLimit, response2.tenantVerifications.Count,
                "Response2 should contain a number of outgoing verifications equal to the limit.");
            for (var i = 0; i < itemsLimit; i++)
            {
                Assert.AreEqual(tenantVerificationsByCreationDescending[i].UniqueId, response2.tenantVerifications[i].uniqueId,
                    string.Format("Response2: tenant verification at position {0} does not have the expected uniqueId.", i));
            }

            Assert.IsFalse(response2.tenantVerifications.Any(x => x.uniqueId.Equals(otherUserOutgoingVerification.UniqueId)),
                "Response1 should not contain the outgoing verification of the other user.");
        }

        [Test]
        public async Task GetUserOutgoingVerificationsSummary_SingleNewOutgoingVerificationTest()
        {
            var itemsLimit = 10;
            var isSent = false;
            var isComplete = false;

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", otherUserIpAddress);

            var random = new RandomWrapper(2015);
            var tenantVerification = await CreateTenantVerificationAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress, isSent, isComplete);

            var containerUnderTest = CreateContainer();
            SetupConfigForGetUserOutgoingVerificationsSummary(containerUnderTest, itemsLimit);
            var serviceUnderTest = containerUnderTest.Get<IOutgoingVerificationService>();

            var response = await serviceUnderTest.GetUserOutgoingVerificationsSummary(user.Id, false);

            Assert.AreEqual(1, response.tenantVerifications.Count,
                "The response should contain a single tenant verification.");

            var tenantVerificationInfo = response.tenantVerifications.Single();

            Assert.AreEqual(tenantVerification.UniqueId, tenantVerificationInfo.uniqueId,
                "Field uniqueId is not the expected.");
            Assert.IsTrue(tenantVerificationInfo.canMarkAsSent, "Field canMarkAsSent doesn't have the expected value.");

            Assert.IsFalse(tenantVerificationInfo.stepVerificationSentOutDone, "Field stepVerificationCodeSentOutDone doesn't have the expected value.");
            Assert.IsFalse(tenantVerificationInfo.stepVerificationReceivedDone, "Field stepVerificationReceivedDone doesn't have the expected value.");

            var retrievedTenantVerification = await DbProbe.TenantVerifications
                .Include(x => x.TenancyDetailsSubmission.Address)
                .SingleOrDefaultAsync(x => x.UniqueId.Equals(tenantVerificationInfo.uniqueId));
            Assert.AreEqual(retrievedTenantVerification.TenancyDetailsSubmission.Address.LocalityRegionPostcode(), tenantVerificationInfo.addressArea, 
                "Field addressArea is not the expected.");
        }

        [Test]
        public async Task GetUserOutgoingVerificationsSummary_SingleOutgoingVerificationJustSentOutTest()
        {
            var itemsLimit = 10;
            var isSent = true;
            var isComplete = false;

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", otherUserIpAddress);

            var random = new RandomWrapper(2015);
            var tenantVerification = await CreateTenantVerificationAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress, isSent, isComplete);

            var containerUnderTest = CreateContainer();
            SetupConfigForGetUserOutgoingVerificationsSummary(containerUnderTest, itemsLimit);
            var serviceUnderTest = containerUnderTest.Get<IOutgoingVerificationService>();

            var response = await serviceUnderTest.GetUserOutgoingVerificationsSummary(user.Id, false);

            Assert.AreEqual(1, response.tenantVerifications.Count,
                "The response should contain a single tenant verification.");

            var tenantVerificationInfo = response.tenantVerifications.Single();

            Assert.AreEqual(tenantVerification.UniqueId, tenantVerificationInfo.uniqueId,
                "Field uniqueId is not the expected.");
            Assert.IsFalse(tenantVerificationInfo.canMarkAsSent, "Field canMarkAsSent doesn't have the expected value.");

            Assert.IsTrue(tenantVerificationInfo.stepVerificationSentOutDone, "Field stepVerificationCodeSentOutDone doesn't have the expected value.");
            Assert.IsFalse(tenantVerificationInfo.stepVerificationReceivedDone, "Field stepVerificationReceivedDone doesn't have the expected value.");

            var retrievedTenantVerification = await DbProbe.TenantVerifications
                .Include(x => x.TenancyDetailsSubmission.Address)
                .SingleOrDefaultAsync(x => x.UniqueId.Equals(tenantVerificationInfo.uniqueId));
            Assert.AreEqual(retrievedTenantVerification.TenancyDetailsSubmission.Address.LocalityRegionPostcode(), tenantVerificationInfo.addressArea,
                "Field addressArea is not the expected.");
        }

        [Test]
        public async Task GetUserOutgoingVerificationsSummary_SingleOutgoingVerificationSentOutAndCompleteTest()
        {
            var itemsLimit = 10;
            var isSent = true;
            var isComplete = true;

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", otherUserIpAddress);

            var random = new RandomWrapper(2015);
            var tenantVerification = await CreateTenantVerificationAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress, isSent, isComplete);

            var containerUnderTest = CreateContainer();
            SetupConfigForGetUserOutgoingVerificationsSummary(containerUnderTest, itemsLimit);
            var serviceUnderTest = containerUnderTest.Get<IOutgoingVerificationService>();

            var response = await serviceUnderTest.GetUserOutgoingVerificationsSummary(user.Id, false);

            Assert.AreEqual(1, response.tenantVerifications.Count,
                "The response should contain a single tenant verification.");

            var tenantVerificationInfo = response.tenantVerifications.Single();

            Assert.AreEqual(tenantVerification.UniqueId, tenantVerificationInfo.uniqueId,
                "Field uniqueId is not the expected.");
            Assert.IsFalse(tenantVerificationInfo.canMarkAsSent, "Field canMarkAsSent doesn't have the expected value.");

            Assert.IsTrue(tenantVerificationInfo.stepVerificationSentOutDone, "Field stepVerificationCodeSentOutDone doesn't have the expected value.");
            Assert.IsTrue(tenantVerificationInfo.stepVerificationReceivedDone, "Field stepVerificationReceivedDone doesn't have the expected value.");

            var retrievedTenantVerification = await DbProbe.TenantVerifications
                .Include(x => x.TenancyDetailsSubmission.Address)
                .SingleOrDefaultAsync(x => x.UniqueId.Equals(tenantVerificationInfo.uniqueId));
            Assert.AreEqual(retrievedTenantVerification.TenancyDetailsSubmission.Address.LocalityRegionPostcode(), tenantVerificationInfo.addressArea,
                "Field addressArea is not the expected.");
        }

        [Test]
        public async Task GetUserOutgoingVerificationsSummary_SingleOutgoingVerificationCompleteButNotMarkedAsSentOutTest()
        {
            var itemsLimit = 10;
            var isSent = false;
            var isComplete = true;

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", otherUserIpAddress);

            var random = new RandomWrapper(2015);
            var tenantVerification = await CreateTenantVerificationAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress, isSent, isComplete);

            var containerUnderTest = CreateContainer();
            SetupConfigForGetUserOutgoingVerificationsSummary(containerUnderTest, itemsLimit);
            var serviceUnderTest = containerUnderTest.Get<IOutgoingVerificationService>();

            var response = await serviceUnderTest.GetUserOutgoingVerificationsSummary(user.Id, false);

            Assert.AreEqual(1, response.tenantVerifications.Count,
                "The response should contain a single tenant verification.");

            var tenantVerificationInfo = response.tenantVerifications.Single();

            Assert.AreEqual(tenantVerification.UniqueId, tenantVerificationInfo.uniqueId,
                "Field uniqueId is not the expected.");
            Assert.IsTrue(tenantVerificationInfo.canMarkAsSent, "Field canMarkAsSent doesn't have the expected value.");

            Assert.IsFalse(tenantVerificationInfo.stepVerificationSentOutDone, "Field stepVerificationCodeSentOutDone doesn't have the expected value.");
            Assert.IsTrue(tenantVerificationInfo.stepVerificationReceivedDone, "Field stepVerificationReceivedDone doesn't have the expected value.");

            var retrievedTenantVerification = await DbProbe.TenantVerifications
                .Include(x => x.TenancyDetailsSubmission.Address)
                .SingleOrDefaultAsync(x => x.UniqueId.Equals(tenantVerificationInfo.uniqueId));
            Assert.AreEqual(retrievedTenantVerification.TenancyDetailsSubmission.Address.LocalityRegionPostcode(), tenantVerificationInfo.addressArea,
                "Field addressArea is not the expected.");
        }

        #endregion

        #region GetUserOutgoingVerificationsSummaryWithCaching

        [Test]
        public async Task GetUserOutgoingVerificationsSummaryWithCaching_WithOutgoingVerificationsEqualToTheLimit_CachesTheSummary()
        {
            var itemsLimit = 3;
            var outgoingVerificationsToCreate = itemsLimit;
            var cachingPeriod = TimeSpan.FromSeconds(0.2);

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", otherUserIpAddress);

            var random = new RandomWrapper(2015);
            var tenantVerifications = new List<TenantVerification>();

            for (var i = 0; i < outgoingVerificationsToCreate; i++)
            {
                var tenantVerification = await CreateTenantVerificationAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress, false, false);
                tenantVerifications.Add(tenantVerification);
                await Task.Delay(_smallDelay);
            }
            var tenantVerificationsByCreationDescending = tenantVerifications.OrderByDescending(x => x.CreatedOn).ToList();


            // I create an outgoing verification for the other user and assign the submission to the user under test.
            // This is to test that the summary contains only verifications from the specific user.
            var otherUserOutgoingVerification = await CreateTenantVerificationAndSave(
                    random, helperContainer, otherUser.Id, otherUserIpAddress, user.Id, userIpAddress, false, false);
            Assert.IsNotNull(otherUserOutgoingVerification, "The outgoing verification created for the other user is null.");

            var containerUnderTest = CreateContainer();
            SetupConfigForGetUserOutgoingVerificationsSummaryWithCaching(containerUnderTest, itemsLimit, cachingPeriod);
            var serviceUnderTest = containerUnderTest.Get<IOutgoingVerificationService>();

            // Full summary
            var response1 = await serviceUnderTest.GetUserOutgoingVerificationsSummaryWithCaching(user.Id, false);

            Assert.IsNotNull(response1, "Response1 is null.");
            Assert.IsFalse(response1.moreItemsExist, "Field moreItemsExist on response1 is not the expected.");
            Assert.AreEqual(outgoingVerificationsToCreate, response1.tenantVerifications.Count,
                "Response1 should contain all tenant verifications.");
            for (var i = 0; i < outgoingVerificationsToCreate; i++)
            {
                Assert.AreEqual(tenantVerificationsByCreationDescending[i].UniqueId, response1.tenantVerifications[i].uniqueId,
                    string.Format("Response1: tenant verification at position {0} does not have the expected uniqueId.", i));
            }

            Assert.IsFalse(response1.tenantVerifications.Any(x => x.uniqueId.Equals(otherUserOutgoingVerification.UniqueId)),
                "Response1 should not contain the outgoing verification of the other user.");

            // Summary with limit
            var response2 = await serviceUnderTest.GetUserOutgoingVerificationsSummaryWithCaching(user.Id, true);

            Assert.IsNotNull(response2, "Response2 is null.");
            Assert.IsFalse(response2.moreItemsExist, "Field moreItemsExist on response2 is not the expected.");
            Assert.IsTrue(response2.tenantVerifications.Any(), "Field tenantVerifications on response2 should not be empty.");

            Assert.AreEqual(itemsLimit, response2.tenantVerifications.Count,
                "Response2 should contain a number of outgoing verifications equal to the limit.");
            for (var i = 0; i < itemsLimit; i++)
            {
                Assert.AreEqual(tenantVerificationsByCreationDescending[i].UniqueId, response2.tenantVerifications[i].uniqueId,
                    string.Format("Response2: tenant verification at position {0} does not have the expected uniqueId.", i));
            }

            Assert.IsFalse(response2.tenantVerifications.Any(x => x.uniqueId.Equals(otherUserOutgoingVerification.UniqueId)),
                "Response2 should not contain the outgoing verification of the other user.");

            KillDatabase(containerUnderTest);
            var serviceWithoutDatabase = containerUnderTest.Get<IOutgoingVerificationService>();

            // Full summary
            var response3 = await serviceWithoutDatabase.GetUserOutgoingVerificationsSummaryWithCaching(user.Id, false);

            Assert.IsNotNull(response3, "Response3 is null.");
            Assert.IsFalse(response3.moreItemsExist, "Field moreItemsExist on response3 is not the expected.");
            Assert.AreEqual(outgoingVerificationsToCreate, response3.tenantVerifications.Count,
                "Response3 should contain all tenant verifications.");
            for (var i = 0; i < outgoingVerificationsToCreate; i++)
            {
                Assert.AreEqual(tenantVerificationsByCreationDescending[i].UniqueId, response3.tenantVerifications[i].uniqueId,
                    string.Format("Response3: tenant verification at position {0} does not have the expected uniqueId.", i));
            }

            Assert.IsFalse(response3.tenantVerifications.Any(x => x.uniqueId.Equals(otherUserOutgoingVerification.UniqueId)),
                "Response3 should not contain the outgoing verification of the other user.");

            // Summary with limit
            var response4 = await serviceWithoutDatabase.GetUserOutgoingVerificationsSummaryWithCaching(user.Id, true);

            Assert.IsNotNull(response4, "Response4 is null.");
            Assert.IsFalse(response4.moreItemsExist, "Field moreItemsExist on response4 is not the expected.");
            Assert.IsTrue(response4.tenantVerifications.Any(), "Field tenantVerifications on response4 should not be empty.");

            Assert.AreEqual(itemsLimit, response4.tenantVerifications.Count,
                "Response4 should contain a number of outgoing verifications equal to the limit.");
            for (var i = 0; i < itemsLimit; i++)
            {
                Assert.AreEqual(tenantVerificationsByCreationDescending[i].UniqueId, response4.tenantVerifications[i].uniqueId,
                    string.Format("Response4: tenant verification at position {0} does not have the expected uniqueId.", i));
            }

            Assert.IsFalse(response4.tenantVerifications.Any(x => x.uniqueId.Equals(otherUserOutgoingVerification.UniqueId)),
                "Response4 should not contain the outgoing verification of the other user.");

            await Task.Delay(cachingPeriod);

            Assert.Throws<ArgumentNullException>(async () => await serviceWithoutDatabase.GetUserOutgoingVerificationsSummaryWithCaching(user.Id, false),
                "After the caching period is over, it should throw an exception because I have killed the  database. (limit items: false)");
            Assert.Throws<ArgumentNullException>(async () => await serviceWithoutDatabase.GetUserOutgoingVerificationsSummaryWithCaching(user.Id, true),
                "After the caching period is over, it should throw an exception because I have killed the  database. (limit items: true)");
        }

        #endregion

        #region Pick

        [Test]
        public async Task Pick_RejectedByAntiAbuseService()
        {
            var ipAddress = "1.2.3.4";
            var antiAbuseRejectionReason = "AntiAbuseService Rejection Reason";
            var helperContainer = CreateContainer();
            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);
            var countryId = CountryId.GB;

            var userIdUsedInAntiAbuse = string.Empty;
            var ipAddressUsedInAntiAbuse = string.Empty;
            CountryId? countryIdUsedInAntiAbuse = null;

            var userResidenceResponse = new GetResidenceResponse
            {
                Address = new Address { CountryId = EnumsHelper.CountryId.ToString(countryId) },
            };

            var container = CreateContainer();
            SetupAntiAbuseServiceResponse(container, (userId, ipAddr, cId) =>
            {
                userIdUsedInAntiAbuse = userId;
                ipAddressUsedInAntiAbuse = ipAddr;
                countryIdUsedInAntiAbuse = cId;

            }, new AntiAbuseServiceResponse()
            {
                IsRejected = true,
                RejectionReason = antiAbuseRejectionReason
            });

            SetupUserResidenceService(container, user.Id, userResidenceResponse);
            var service = container.Get<IOutgoingVerificationService>();

            var verificationUniqueId = Guid.NewGuid();
            var outcome = await service.Pick(user.Id, ipAddress, verificationUniqueId);

            Assert.IsTrue(outcome.IsRejected, "The field IsRejected on the outcome should be true.");
            Assert.AreEqual(antiAbuseRejectionReason, outcome.RejectionReason,
                "The RejectionReason on the outcome is not the expected.");
            Assert.AreEqual(user.Id, userIdUsedInAntiAbuse,
                "The UserId used in the call to AntiAbuseService is not the expected.");
            Assert.AreEqual(ipAddress, ipAddressUsedInAntiAbuse,
                "The IpAddress used in the call to AntiAbuseService is not the expected.");
            Assert.AreEqual(countryId, countryIdUsedInAntiAbuse,
                "The CountryId used in the call to AntiAbuseService is not the expected.");

            var retrievedTenantVerification = await DbProbe.TenantVerifications
                .SingleOrDefaultAsync(x => x.UniqueId.Equals(verificationUniqueId));
            Assert.IsNull(retrievedTenantVerification, "A TenantVerification should not be created.");
        }

        [Test]
        public async Task Pick_WhenThereAreNoSubmissions()
        {
            var minDegreesDistance = 0.001;

            var ipAddress = "1.1.1.1";
            var userResidenceCountryId = CountryId.GB;
            var userResidenceLatitude = 1.00;
            var userResidenceLongitude = 1.00;

            var random = new RandomWrapper(2015);

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var containerUnderTest = CreateContainer();
            SetupConfigForPick(containerUnderTest, minDegreesDistance);
            SetupAntiAbuseServiceResponseToNotRejected(containerUnderTest);
            await SetupUserResidenceService(
                random, containerUnderTest, user.Id, ipAddress, userResidenceCountryId, userResidenceLatitude, userResidenceLongitude);

            var serviceUnderTest = containerUnderTest.Get<IOutgoingVerificationService>();

            var verificationUniqueId = Guid.NewGuid();
            var outcome = await serviceUnderTest.Pick(user.Id, ipAddress, verificationUniqueId);

            Assert.IsTrue(outcome.IsRejected, "IsRejected field is not the expected.");
            Assert.AreEqual(OutgoingVerificationResources.Pick_NoVerificationAssignableToUser_RejectionMessage,
                outcome.RejectionReason, "RejectionReason is not the expected.");
            Assert.IsNull(outcome.VerificationUniqueId, "VerificationUniqueId is not the expected.");

            var retrievedTenantVerification = await DbProbe.TenantVerifications.SingleOrDefaultAsync(x => x.AssignedToId.Equals(user.Id));
            Assert.IsNull(retrievedTenantVerification, "A tenant verification should not be created.");
        }

        [Test]
        public async Task Pick_DoesNotPickFromDifferentCountry()
        {
            var minDegreesDistance = 0.001;

            var ipAddress = "1.1.1.1";
            var userResidenceCountryId = CountryId.GB;
            var userResidenceLatitude = 1.00;
            var userResidenceLongitude = 1.00;

            var addressIpAddress = "2.2.2.2";
            var submissionIpAddress = addressIpAddress;
            var addressCountryId = CountryId.GB;
            var submissionLatitude = 2.00;
            var submissionLongitude = 2.00;

            var random = new RandomWrapper(2015);

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);
            var userForSubmission = await CreateUser(helperContainer, "test2@test.com", submissionIpAddress);

            var submission = await CreateTenancyDetailsSubmissionAndSave(
                random, helperContainer, userForSubmission.Id, addressIpAddress, userForSubmission.Id, submissionIpAddress, addressCountryId, submissionLatitude, submissionLongitude);
            Assert.IsNotNull(submission, "Submission was not created during setup.");

            var containerUnderTest = CreateContainer();
            SetupConfigForPick(containerUnderTest, minDegreesDistance);
            SetupAntiAbuseServiceResponseToNotRejected(containerUnderTest);
            await SetupUserResidenceService(
                random, containerUnderTest, user.Id, ipAddress, userResidenceCountryId, userResidenceLatitude, userResidenceLongitude);
            
            var serviceUnderTest = containerUnderTest.Get<IOutgoingVerificationService>();

            var verificationUniqueId = Guid.NewGuid();
            var outcome = await serviceUnderTest.Pick(user.Id, ipAddress, verificationUniqueId);

            Assert.IsTrue(outcome.IsRejected, "IsRejected field is not the expected.");
            Assert.AreEqual(OutgoingVerificationResources.Pick_NoVerificationAssignableToUser_RejectionMessage,
                outcome.RejectionReason, "RejectionReason is not the expected.");
            Assert.IsNull(outcome.VerificationUniqueId, "VerificationUniqueId is not the expected.");

            var retrievedTenantVerification = await DbProbe.TenantVerifications.SingleOrDefaultAsync(x => x.AssignedToId.Equals(user.Id));
            Assert.IsNull(retrievedTenantVerification, "A tenant verification should not be created.");
        }

        [Test]
        public async Task Pick_DoesNotPickHiddenSubmissions()
        {
            var minDegreesDistance = 0.001;

            var ipAddress = "1.1.1.1";
            var userResidenceCountryId = CountryId.GB;
            var userResidenceLatitude = 1.00;
            var userResidenceLongitude = 1.00;

            var addressIpAddress = "2.2.2.2";
            var submissionIpAddress = addressIpAddress;
            var addressCountryId = CountryId.GB;
            var submissionLatitude = 2.00;
            var submissionLongitude = 2.00;

            var random = new RandomWrapper(2015);

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);
            var userForSubmission = await CreateUser(helperContainer, "test2@test.com", submissionIpAddress);

            var submission = await CreateTenancyDetailsSubmissionAndSave(
                random, helperContainer, userForSubmission.Id, addressIpAddress, userForSubmission.Id, submissionIpAddress, addressCountryId, 
                submissionLatitude, submissionLongitude, isHidden: true);
            Assert.IsNotNull(submission, "Submission was not created during setup.");

            var containerUnderTest = CreateContainer();
            SetupConfigForPick(containerUnderTest, minDegreesDistance);
            SetupAntiAbuseServiceResponseToNotRejected(containerUnderTest);
            await SetupUserResidenceService(
                random, containerUnderTest, user.Id, ipAddress, userResidenceCountryId, userResidenceLatitude, userResidenceLongitude);

            var serviceUnderTest = containerUnderTest.Get<IOutgoingVerificationService>();

            var verificationUniqueId = Guid.NewGuid();
            var outcome = await serviceUnderTest.Pick(user.Id, ipAddress, verificationUniqueId);

            Assert.IsTrue(outcome.IsRejected, "IsRejected field is not the expected.");
            Assert.AreEqual(OutgoingVerificationResources.Pick_NoVerificationAssignableToUser_RejectionMessage,
                outcome.RejectionReason, "RejectionReason is not the expected.");
            Assert.IsNull(outcome.VerificationUniqueId, "VerificationUniqueId is not the expected.");

            var retrievedTenantVerification = await DbProbe.TenantVerifications.SingleOrDefaultAsync(x => x.AssignedToId.Equals(user.Id));
            Assert.IsNull(retrievedTenantVerification, "A tenant verification should not be created.");
        }

        [Test]
        public async Task Pick_DoesNotPickSubmissionByTheSameUser()
        {
            // TODO_PANOS
        }

        [Test]
        public async Task Pick_DoesNotPickSubmissionByTheSameIpAddress()
        {
            // TODO_PANOS
        }

        [Test]
        public async Task Pick_DoesNotPickSubmissionForAddressByTheSameUser()
        {
            // TODO_PANOS
        }

        [Test]
        public async Task Pick_DoesNotPickSubmissionForAddressByTheSameIpAddress()
        {
            // TODO_PANOS
        }

        [Test]
        public async Task Pick_DoesNotPickSubmissionWithMaximumVerificationsAssigned()
        {
            // TODO_PANOS
        }

        [Test]
        public async Task Pick_DoesNotPickSubmissionAlreadyAssignedToUser()
        {
            // TODO_PANOS
        }

        [Test]
        public async Task Pick_DoesNotPickSubmissionAlreadyAssignedToIpAddress()
        {
            // TODO_PANOS
        }

        [Test]
        public async Task Pick_DoesNotPickSubmissionWithinMinLatitude()
        {
            // TODO_PANOS
        }

        [Test]
        public async Task Pick_DoesNotPickSubmissionWithinMinLongitude()
        {
            // TODO_PANOS
        }

        #endregion

        #region VerificationIsAssignedToUser

        [Test]
        public async Task VerificationIsAssignedToUser_Test()
        {
            var helperContainer = CreateContainer();
            var user1IpAddress = "1.2.3.4";
            var user1 = await CreateUser(helperContainer, "test1@test.com", user1IpAddress);
            var user2IpAddress = "1.2.3.5";
            var user2 = await CreateUser(helperContainer, "test2@test.com", user2IpAddress);

            var random = new RandomWrapper(2015);

            var user1verification = await CreateTenantVerificationAndSave(random, helperContainer,
                user1.Id, user1IpAddress, user2.Id, user2IpAddress, isSent: false, isComplete: true);
            Assert.IsNotNull(user1verification, "The submission created for user1 is null.");

            var user2verification = await CreateTenantVerificationAndSave(random, helperContainer,
                user2.Id, user2IpAddress, user1.Id, user1IpAddress, isSent: false, isComplete: true);
            Assert.IsNotNull(user2verification, "The submission created for user2 is null.");

            var containerUnderTest = CreateContainer();
            var serviceUnderTest = containerUnderTest.Get<IOutgoingVerificationService>();

            var user1verificationIsAssignedToUser1 = await serviceUnderTest.VerificationIsAssignedToUser(user1.Id, user1verification.UniqueId);
            Assert.IsTrue(user1verificationIsAssignedToUser1, "user1verification is assigned to user1.");

            var user1verificationIsAssignedToUser2 = await serviceUnderTest.VerificationIsAssignedToUser(user2.Id, user1verification.UniqueId);
            Assert.IsFalse(user1verificationIsAssignedToUser2, "user1verification is not assigned to user2.");

            var user2verificationIsAssignedToUser1 = await serviceUnderTest.VerificationIsAssignedToUser(user1.Id, user2verification.UniqueId);
            Assert.IsFalse(user2verificationIsAssignedToUser1, "user2verification is not assigned to user1.");

            var user2verificationIsAssignedToUser2 = await serviceUnderTest.VerificationIsAssignedToUser(user2.Id, user2verification.UniqueId);
            Assert.IsTrue(user2verificationIsAssignedToUser2, "user2verification is assigned to to user2.");
        }

        #endregion

        #region Constraints

        [Test]
        public async Task TenantVerification_SecretCodeIsUniqueWithinEachSubmission()
        {
            var secretCode = "secret";

            var container = CreateContainer();
            var dbContext = container.Get<IEpsilonContext>();

            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(container, "test@test.com", userIpAddress);
            var otherUserIpAddress = "1.2.3.5";
            var otherUser = await CreateUser(container, "test1@test.com", otherUserIpAddress);

            var random = new RandomWrapper(2015);

            var address = await AddressHelper.CreateRandomAddressAndSave(random, container, user.Id, userIpAddress, CountryId.GB);

            var tenancyDetailsSubmission = new TenancyDetailsSubmission
            {
                UniqueId = Guid.NewGuid(),
                AddressId = address.Id,
                UserId = user.Id,
                CreatedByIpAddress = userIpAddress
            };
            dbContext.TenancyDetailsSubmissions.Add(tenancyDetailsSubmission);
            await dbContext.SaveChangesAsync();

            var tenantVerification1 = new TenantVerification
            {
                UniqueId = Guid.NewGuid(),
                TenancyDetailsSubmissionId = tenancyDetailsSubmission.Id,
                AssignedToId = otherUser.Id,
                AssignedByIpAddress = otherUserIpAddress,
                SecretCode = secretCode
            };
            dbContext.TenantVerifications.Add(tenantVerification1);
            await dbContext.SaveChangesAsync();

            var tenantVerification2 = new TenantVerification
            {
                UniqueId = Guid.NewGuid(),
                TenancyDetailsSubmissionId = tenancyDetailsSubmission.Id,
                AssignedToId = otherUser.Id,
                AssignedByIpAddress = otherUserIpAddress,
                SecretCode = secretCode
            };
            dbContext.TenantVerifications.Add(tenantVerification2);

            Assert.Throws<DbUpdateException>(async () => await dbContext.SaveChangesAsync(), 
                "Saving a second verification with the same secret code and for the same tenancy details submission should throw.");
        }

        #endregion

        #region Private helper functions

        private static void SetupConfigForPick(IKernel container, double minDegreesDistance)
        {
            var mockConfig = new Mock<IOutgoingVerificationServiceConfig>();

            mockConfig.Setup(x => x.Pick_MinDegreesDistanceInAnyDirection).Returns(minDegreesDistance);

            container.Rebind<IOutgoingVerificationServiceConfig>().ToConstant(mockConfig.Object);
        }

        private static void SetupConfigForGetUserOutgoingVerificationsSummary(IKernel container, int itemsLimit)
        {
            var mockConfig = new Mock<IOutgoingVerificationServiceConfig>();

            mockConfig.Setup(x => x.MyOutgoingVerificationsSummary_ItemsLimit).Returns(itemsLimit);

            container.Rebind<IOutgoingVerificationServiceConfig>().ToConstant(mockConfig.Object);
        }

        private static void SetupConfigForGetUserOutgoingVerificationsSummaryWithCaching(IKernel container, int itemsLimit, TimeSpan cachingPeriod)
        {
            var mockConfig = new Mock<IOutgoingVerificationServiceConfig>();

            mockConfig.Setup(x => x.MyOutgoingVerificationsSummary_ItemsLimit).Returns(itemsLimit);
            mockConfig.Setup(x => x.MyOutgoingVerificationsSummary_CachingPeriod).Returns(cachingPeriod);

            container.Rebind<IOutgoingVerificationServiceConfig>().ToConstant(mockConfig.Object);
        }

        private static void SetupAntiAbuseServiceResponse(IKernel container, Action<string, string, CountryId> callback,
            AntiAbuseServiceResponse response)
        {
            var mockAntiAbuseService = new Mock<IAntiAbuseService>();
            mockAntiAbuseService.Setup(x => x.CanPickOutgoingVerification(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CountryId>()))
                .Callback(callback)
                .Returns(Task.FromResult(response));

            container.Rebind<IAntiAbuseService>().ToConstant(mockAntiAbuseService.Object);
        }

        private static void SetupAntiAbuseServiceResponseToNotRejected(IKernel container)
        {
            var response = new AntiAbuseServiceResponse { IsRejected = false };
            SetupAntiAbuseServiceResponse(container, (x, y, z) => { }, response);
        }

        private static void SetupUserResidenceService(IKernel container, string expectedUserId, GetResidenceResponse response)
        {
            var mockUserResidenceService = new Mock<IUserResidenceService>();
            mockUserResidenceService.Setup(x => x.GetResidence(It.IsAny<string>()))
                .Returns<string>(userId =>
                {
                    if (!userId.Equals(expectedUserId))
                        throw new Exception(string.Format(
                            "I was expecting userId '{0}' to be used in UserResidenceService but got '{1}' instead.", expectedUserId, userId));
                    return Task.FromResult(response);
                });

            container.Rebind<IUserResidenceService>().ToConstant(mockUserResidenceService.Object);
        }

        private async static Task SetupUserResidenceService(IRandomWrapper random, IKernel container, string expectedUserId, string userIpAddress, 
            CountryId countryId, double latitude, double longitude)
        {
            var mockUserResidenceService = new Mock<IUserResidenceService>();

            var address = await AddressHelper.CreateRandomAddressAndSave(random, container, expectedUserId, userIpAddress, countryId, latitude: latitude, longitude: longitude);

            var getResidenceResponse = new GetResidenceResponse()
            {
                Address = address
            };

            mockUserResidenceService.Setup(x => x.GetResidence(It.IsAny<string>()))
                .Returns<string>(userId =>
                {
                    if (!userId.Equals(expectedUserId))
                        throw new Exception(string.Format(
                            "I was expecting userId '{0}' to be used in UserResidenceService but got '{1}' instead.", expectedUserId, userId));
                    return Task.FromResult(getResidenceResponse);
                });

            container.Rebind<IUserResidenceService>().ToConstant(mockUserResidenceService.Object);
        }

        private static async Task<TenantVerification> CreateTenantVerificationAndSave(
            IRandomWrapper random, IKernel container, 
            string userId, string userIpAddress, 
            string userIdForSubmission, string userForSubmissionIpAddress,
            bool isSent, bool isComplete)
        {
            var clock = container.Get<IClock>();
            var dbContext = container.Get<IEpsilonContext>();

            var address = await AddressHelper.CreateRandomAddressAndSave(random, container, userIdForSubmission, userForSubmissionIpAddress, CountryId.GB);

            var tenancyDetailsSubmission = new TenancyDetailsSubmission
            {
                UniqueId = Guid.NewGuid(),
                AddressId = address.Id,
                UserId = userIdForSubmission,
                CreatedByIpAddress = userForSubmissionIpAddress
            };
            dbContext.TenancyDetailsSubmissions.Add(tenancyDetailsSubmission);

            var tenantVerification = new TenantVerification
            {
                TenancyDetailsSubmission = tenancyDetailsSubmission,
                UniqueId = Guid.NewGuid(),
                AssignedToId = userId,
                AssignedByIpAddress = userIpAddress,
                SecretCode = RandomStringHelper.GetString(random, AppConstant.SECRET_CODE_MAX_LENGTH, CharacterCase.Mixed)
            };
            if (isSent)
                tenantVerification.MarkedAsSentOn = clock.OffsetNow;
            if (isComplete)
                tenantVerification.VerifiedOn = clock.OffsetNow;

            dbContext.TenantVerifications.Add(tenantVerification);
            await dbContext.SaveChangesAsync();

            return tenantVerification;
        }

        private static async Task<TenancyDetailsSubmission> CreateTenancyDetailsSubmissionAndSave(
            IRandomWrapper random, IKernel container,
            string userIdForAddress, string ipAddressForAddress, string userIdForSubmission, string ipAddressForSubmission, CountryId countryId, double latitude, double longitude, bool isHidden = false)
        {
            var dbContext = container.Get<IEpsilonContext>();

            var address = await AddressHelper.CreateRandomAddressAndSave(
                random, container, userIdForAddress, ipAddressForAddress, countryId, latitude: latitude, longitude: longitude);

            var tenancyDetailsSubmission = new TenancyDetailsSubmission
            {
                UniqueId = Guid.NewGuid(),
                AddressId = address.Id,
                UserId = userIdForSubmission,
                CreatedByIpAddress = ipAddressForSubmission,
                IsHidden = isHidden
            };
            dbContext.TenancyDetailsSubmissions.Add(tenancyDetailsSubmission);
            await dbContext.SaveChangesAsync();
            
            return tenancyDetailsSubmission;
        }

        #endregion
    }
}
