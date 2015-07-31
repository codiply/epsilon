using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.Logic.Services.Interfaces;
using Moq;
using Ninject;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Helpers;
using static Epsilon.Logic.Helpers.RandomStringHelper;
using Epsilon.IntegrationTests.TestHelpers;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Wrappers;
using Epsilon.Logic.JsonModels;

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class OutgoingVerificationServiceTest : BaseIntegrationTestWithRollback
    {

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
            var request1 = new MyOutgoingVerificationsSummaryRequest { limitItemsReturned = false };
            var response1 = await serviceUnderTest.GetUserOutgoingVerificationsSummary(user.Id, request1);

            Assert.IsNotNull(response1, "Response1 is null.");
            Assert.IsFalse(response1.moreItemsExist, "Field moreItemsExist on response1 is not the expected.");
            Assert.IsFalse(response1.tenantVerifications.Any(), "Field tenantVerifications on response1 should be empty.");

            // Summary with limit
            var request2 = new MyOutgoingVerificationsSummaryRequest { limitItemsReturned = true };
            var response2 = await serviceUnderTest.GetUserOutgoingVerificationsSummary(user.Id, request2);

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
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", "11.12.13.14");

            var random = new RandomWrapper(2015);
            var tenantVerifications = new List<TenantVerification>();

            for (var i = 0; i < outgoingVerificationsToCreate; i++)
            {
                var tenantVerification = await CreateTenantVerificationAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress, false, false);
                tenantVerifications.Add(tenantVerification);
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
            var request1 = new MyOutgoingVerificationsSummaryRequest { limitItemsReturned = false };
            var response1 = await serviceUnderTest.GetUserOutgoingVerificationsSummary(user.Id, request1);

            Assert.IsNotNull(response1, "Response1 is null.");
            Assert.IsFalse(response1.moreItemsExist, "Field moreItemsExist on response1 is not the expected.");
            Assert.AreEqual(outgoingVerificationsToCreate, response1.tenantVerifications.Count,
                "Response1 should contain all tenant verifications.");
            for (var i = 0; i < outgoingVerificationsToCreate; i++)
            {
                Assert.AreEqual(response1.tenantVerifications[i].uniqueId, tenantVerificationsByCreationDescending[i].UniqueId,
                    string.Format("Response1: tenant verification at position {0} does not have the expected uniqueId.", i));
            }

            Assert.IsFalse(response1.tenantVerifications.Any(x => x.uniqueId.Equals(otherUserOutgoingVerification.UniqueId)),
                "Response1 should not contain the outgoing verification of the other user.");

            // Summary with limit
            var request2 = new MyOutgoingVerificationsSummaryRequest { limitItemsReturned = true };
            var response2 = await serviceUnderTest.GetUserOutgoingVerificationsSummary(user.Id, request2);

            Assert.IsNotNull(response2, "Response2 is null.");
            Assert.IsFalse(response2.moreItemsExist, "Field moreItemsExist on response2 is not the expected.");
            Assert.IsTrue(response2.tenantVerifications.Any(), "Field tenantVerifications on response2 should not be empty.");

            Assert.AreEqual(itemsLimit, response2.tenantVerifications.Count,
                "Response2 should contain a number of outgoing verifications equal to the limit.");
            for (var i = 0; i < itemsLimit; i++)
            {
                Assert.AreEqual(response2.tenantVerifications[i].uniqueId, tenantVerificationsByCreationDescending[i].UniqueId,
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
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", "11.12.13.14");

            var random = new RandomWrapper(2015);
            var tenantVerifications = new List<TenantVerification>();

            for (var i = 0; i < outgoingVerificationsToCreate; i++)
            {
                var tenantVerification = await CreateTenantVerificationAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress, false, false);
                tenantVerifications.Add(tenantVerification);
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
            var request1 = new MyOutgoingVerificationsSummaryRequest { limitItemsReturned = false };
            var response1 = await serviceUnderTest.GetUserOutgoingVerificationsSummary(user.Id, request1);

            Assert.IsNotNull(response1, "Response1 is null.");
            Assert.IsFalse(response1.moreItemsExist, "Field moreItemsExist on response1 is not the expected.");
            Assert.AreEqual(outgoingVerificationsToCreate, response1.tenantVerifications.Count,
                "Response1 should contain all tenant verifications.");
            for (var i = 0; i < outgoingVerificationsToCreate; i++)
            {
                Assert.AreEqual(response1.tenantVerifications[i].uniqueId, tenantVerificationsByCreationDescending[i].UniqueId,
                    string.Format("Response1: tenant verification at position {0} does not have the expected uniqueId.", i));
            }

            Assert.IsFalse(response1.tenantVerifications.Any(x => x.uniqueId.Equals(otherUserOutgoingVerification.UniqueId)),
                "Response1 should not contain the outgoing verification of the other user.");

            // Summary with limit
            var request2 = new MyOutgoingVerificationsSummaryRequest { limitItemsReturned = true };
            var response2 = await serviceUnderTest.GetUserOutgoingVerificationsSummary(user.Id, request2);

            Assert.IsNotNull(response2, "Response2 is null.");
            Assert.IsTrue(response2.moreItemsExist, "Field moreItemsExist on response2 is not the expected.");
            Assert.IsTrue(response2.tenantVerifications.Any(), "Field tenantVerifications on response2 should not be empty.");

            Assert.AreEqual(itemsLimit, response2.tenantVerifications.Count,
                "Response2 should contain a number of outgoing verifications equal to the limit.");
            for (var i = 0; i < itemsLimit; i++)
            {
                Assert.AreEqual(response2.tenantVerifications[i].uniqueId, tenantVerificationsByCreationDescending[i].UniqueId,
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
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", "11.12.13.14");

            var random = new RandomWrapper(2015);
            var tenantVerification = await CreateTenantVerificationAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress, isSent, isComplete);

            var containerUnderTest = CreateContainer();
            SetupConfigForGetUserOutgoingVerificationsSummary(containerUnderTest, itemsLimit);
            var serviceUnderTest = containerUnderTest.Get<IOutgoingVerificationService>();

            var request = new MyOutgoingVerificationsSummaryRequest { limitItemsReturned = false };
            var response = await serviceUnderTest.GetUserOutgoingVerificationsSummary(user.Id, request);

            Assert.AreEqual(1, response.tenantVerifications.Count,
                "The response should contain a single tenant verification.");

            var tenantVerificationInfo = response.tenantVerifications.Single();

            Assert.AreEqual(tenantVerification.UniqueId, tenantVerificationInfo.uniqueId,
                "Field uniqueId is not the expected.");
            Assert.IsTrue(tenantVerificationInfo.canMarkAsSent, "Field canMarkAsSent doesn't have the expected value.");

            Assert.IsFalse(tenantVerificationInfo.stepVerificationSentOutDone, "Field stepVerificationCodeSentOutDone doesn't have the expected value.");
            Assert.IsFalse(tenantVerificationInfo.stepVerificationReceivedDone, "Field stepVerificationReceivedDone doesn't have the expected value.");

            var retrievedTenantVerification = await DbProbe.TenantVerifications
                .SingleOrDefaultAsync(x => x.UniqueId.Equals(tenantVerificationInfo.uniqueId));
            Assert.AreEqual(retrievedTenantVerification.DisplayId(), tenantVerificationInfo.displayId, 
                "Field displayId is not the expected.");
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
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", "11.12.13.14");

            var random = new RandomWrapper(2015);
            var tenantVerification = await CreateTenantVerificationAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress, isSent, isComplete);

            var containerUnderTest = CreateContainer();
            SetupConfigForGetUserOutgoingVerificationsSummary(containerUnderTest, itemsLimit);
            var serviceUnderTest = containerUnderTest.Get<IOutgoingVerificationService>();

            var request = new MyOutgoingVerificationsSummaryRequest { limitItemsReturned = false };
            var response = await serviceUnderTest.GetUserOutgoingVerificationsSummary(user.Id, request);

            Assert.AreEqual(1, response.tenantVerifications.Count,
                "The response should contain a single tenant verification.");

            var tenantVerificationInfo = response.tenantVerifications.Single();

            Assert.AreEqual(tenantVerification.UniqueId, tenantVerificationInfo.uniqueId,
                "Field uniqueId is not the expected.");
            Assert.IsFalse(tenantVerificationInfo.canMarkAsSent, "Field canMarkAsSent doesn't have the expected value.");

            Assert.IsTrue(tenantVerificationInfo.stepVerificationSentOutDone, "Field stepVerificationCodeSentOutDone doesn't have the expected value.");
            Assert.IsFalse(tenantVerificationInfo.stepVerificationReceivedDone, "Field stepVerificationReceivedDone doesn't have the expected value.");

            var retrievedTenantVerification = await DbProbe.TenantVerifications
                .SingleOrDefaultAsync(x => x.UniqueId.Equals(tenantVerificationInfo.uniqueId));
            Assert.AreEqual(retrievedTenantVerification.DisplayId(), tenantVerificationInfo.displayId,
                "Field displayId is not the expected.");
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
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", "11.12.13.14");

            var random = new RandomWrapper(2015);
            var tenantVerification = await CreateTenantVerificationAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress, isSent, isComplete);

            var containerUnderTest = CreateContainer();
            SetupConfigForGetUserOutgoingVerificationsSummary(containerUnderTest, itemsLimit);
            var serviceUnderTest = containerUnderTest.Get<IOutgoingVerificationService>();

            var request = new MyOutgoingVerificationsSummaryRequest { limitItemsReturned = false };
            var response = await serviceUnderTest.GetUserOutgoingVerificationsSummary(user.Id, request);

            Assert.AreEqual(1, response.tenantVerifications.Count,
                "The response should contain a single tenant verification.");

            var tenantVerificationInfo = response.tenantVerifications.Single();

            Assert.AreEqual(tenantVerification.UniqueId, tenantVerificationInfo.uniqueId,
                "Field uniqueId is not the expected.");
            Assert.IsFalse(tenantVerificationInfo.canMarkAsSent, "Field canMarkAsSent doesn't have the expected value.");

            Assert.IsTrue(tenantVerificationInfo.stepVerificationSentOutDone, "Field stepVerificationCodeSentOutDone doesn't have the expected value.");
            Assert.IsTrue(tenantVerificationInfo.stepVerificationReceivedDone, "Field stepVerificationReceivedDone doesn't have the expected value.");

            var retrievedTenantVerification = await DbProbe.TenantVerifications
                .SingleOrDefaultAsync(x => x.UniqueId.Equals(tenantVerificationInfo.uniqueId));
            Assert.AreEqual(retrievedTenantVerification.DisplayId(), tenantVerificationInfo.displayId,
                "Field displayId is not the expected.");
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
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", "11.12.13.14");

            var random = new RandomWrapper(2015);
            var tenantVerification = await CreateTenantVerificationAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress, isSent, isComplete);

            var containerUnderTest = CreateContainer();
            SetupConfigForGetUserOutgoingVerificationsSummary(containerUnderTest, itemsLimit);
            var serviceUnderTest = containerUnderTest.Get<IOutgoingVerificationService>();

            var request = new MyOutgoingVerificationsSummaryRequest { limitItemsReturned = false };
            var response = await serviceUnderTest.GetUserOutgoingVerificationsSummary(user.Id, request);

            Assert.AreEqual(1, response.tenantVerifications.Count,
                "The response should contain a single tenant verification.");

            var tenantVerificationInfo = response.tenantVerifications.Single();

            Assert.AreEqual(tenantVerification.UniqueId, tenantVerificationInfo.uniqueId,
                "Field uniqueId is not the expected.");
            Assert.IsTrue(tenantVerificationInfo.canMarkAsSent, "Field canMarkAsSent doesn't have the expected value.");

            Assert.IsFalse(tenantVerificationInfo.stepVerificationSentOutDone, "Field stepVerificationCodeSentOutDone doesn't have the expected value.");
            Assert.IsTrue(tenantVerificationInfo.stepVerificationReceivedDone, "Field stepVerificationReceivedDone doesn't have the expected value.");

            var retrievedTenantVerification = await DbProbe.TenantVerifications
                .SingleOrDefaultAsync(x => x.UniqueId.Equals(tenantVerificationInfo.uniqueId));
            Assert.AreEqual(retrievedTenantVerification.DisplayId(), tenantVerificationInfo.displayId,
                "Field displayId is not the expected.");
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

            var userIdUsedInAntiAbuse = string.Empty;
            var ipAddressUsedInAntiAbuse = string.Empty;

            var container = CreateContainer();
            SetupAntiAbuseServiceResponse(container, (userId, ipAddr) =>
            {
                userIdUsedInAntiAbuse = userId;
                ipAddressUsedInAntiAbuse = ipAddr;
            }, new AntiAbuseServiceResponse()
            {
                IsRejected = true,
                RejectionReason = antiAbuseRejectionReason
            });
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

            var retrievedTenantVerification = await DbProbe.TenantVerifications
                .SingleOrDefaultAsync(x => x.UniqueId.Equals(verificationUniqueId));
            Assert.IsNull(retrievedTenantVerification, "A TenantVerification should not be created.");
        }

        #endregion

        #region Private helper functions

        private static void SetupConfigForGetUserOutgoingVerificationsSummary(IKernel container, int itemsLimit)
        {
            var mockConfig = new Mock<IOutgoingVerificationServiceConfig>();

            mockConfig.Setup(x => x.MyOutgoingVerificationsSummary_ItemsLimit).Returns(itemsLimit);

            container.Rebind<IOutgoingVerificationServiceConfig>().ToConstant(mockConfig.Object);
        }

        private static void SetupAntiAbuseServiceResponse(IKernel container, Action<string, string> callback,
            AntiAbuseServiceResponse response)
        {
            var mockAntiAbuseService = new Mock<IAntiAbuseService>();
            mockAntiAbuseService.Setup(x => x.CanPickOutgoingVerification(It.IsAny<string>(), It.IsAny<string>()))
                .Callback(callback)
                .Returns(Task.FromResult(response));

            container.Rebind<IAntiAbuseService>().ToConstant(mockAntiAbuseService.Object);
        }

        private static async Task<TenantVerification> CreateTenantVerificationAndSave(
            IRandomWrapper random, IKernel container, 
            string userId, string userIpAddress, 
            string userIdForSubmission, string userForSubmissionIpAddress,
            bool isSent, bool isComplete)
        {
            var clock = container.Get<IClock>();
            var dbContext = container.Get<IEpsilonContext>();

            var address = await AddressHelper.CreateRandomAddressAndSave(random, container, userId, userIpAddress, CountryId.GB);

            var tenancyDetailsSubmission = new TenancyDetailsSubmission
            {
                UniqueId = Guid.NewGuid(),
                AddressId = address.Id,
                UserId = userId,
                CreatedByIpAddress = userIpAddress,
            };

            var tenantVerification = new TenantVerification
            {
                TenancyDetailsSubmission = tenancyDetailsSubmission,
                UniqueId = Guid.NewGuid(),
                AssignedToId = userId,
                AssignedByIpAddress = userIpAddress,
                SecretCode = RandomStringHelper.GetString(random, 10, CharacterCase.Mixed)
            };
            if (isSent)
                tenantVerification.MarkedAsSentOn = clock.OffsetNow;
            if (isComplete)
                tenantVerification.VerifiedOn = clock.OffsetNow;

            dbContext.TenantVerifications.Add(tenantVerification);
            await dbContext.SaveChangesAsync();

            return tenantVerification;
        }

        #endregion
    }
}
