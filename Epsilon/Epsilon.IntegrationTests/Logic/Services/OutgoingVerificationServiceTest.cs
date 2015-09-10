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
using Epsilon.Resources.Common;
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
            SetupConfig(containerUnderTest, itemsLimit: itemsLimit);
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
            SetupConfig(containerUnderTest, itemsLimit: itemsLimit);
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
            SetupConfig(containerUnderTest, itemsLimit: itemsLimit);
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
            SetupConfig(containerUnderTest, itemsLimit: itemsLimit);
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
            SetupConfig(containerUnderTest, itemsLimit: itemsLimit);
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
            SetupConfig(containerUnderTest, itemsLimit: itemsLimit);
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
            SetupConfig(containerUnderTest, itemsLimit: itemsLimit, cachingPeriod: cachingPeriod);
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

        #region GetInstructions

        [Test]
        public async Task GetInstructions_VerificationDoesNotExist()
        {
            var helperContainer = CreateContainer();

            var ipAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var containerUnderTest = CreateContainer();
            var serviceUnderTest = containerUnderTest.Get<IOutgoingVerificationService>();

            var nonExistentVerificationUniqueId = Guid.NewGuid();
            var instructions = await serviceUnderTest.GetInstructions(user.Id, nonExistentVerificationUniqueId);

            Assert.IsTrue(instructions.IsRejected, "IsRejected field is not the expected.");
            Assert.AreEqual(CommonResources.GenericInvalidRequestMessage, instructions.RejectionReason,
                "RejectionReason field is not the expected.");
            Assert.IsNull(instructions.Instructions, "Instructions field is not the expected.");
        }

        [Test, Combinatorial]
        public async Task GetInstructions_ExpiryPeriodTest(
            [Values(false, true)] bool markedAsSent,
            [Values(false, true)] bool otherUserMarkedAddressAsInvalid)
        {
            var markedAddressAsInvalid = false;
            var expiryPeriod = TimeSpan.FromSeconds(0.3);
            var expiryPeriodInDays = expiryPeriod.TotalDays;

            var helperContainer = CreateContainer();

            var ipAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);
            var ipAddressForSubmission = "2.3.4.5";
            var userForSubmission = await CreateUser(helperContainer, "test1@test.com", ipAddressForSubmission);

            var random = new RandomWrapper(2015);

            var verification = await CreateTenantVerificationAndSave(
                random, helperContainer, user.Id, ipAddress, userForSubmission.Id, ipAddressForSubmission,
                isSent: markedAsSent, markedAddressAsInvalid: markedAddressAsInvalid, otherUserMarkedAddressAsInvalid: otherUserMarkedAddressAsInvalid);

            var retrievedVerification = await DbProbe.TenantVerifications
                .Include(x => x.TenancyDetailsSubmission)
                .Include(x => x.TenancyDetailsSubmission.Address)
                .Include(x => x.TenancyDetailsSubmission.Address.Country)
                .SingleOrDefaultAsync(x => x.UniqueId.Equals(verification.UniqueId));

            Assert.IsNotNull(retrievedVerification, "Verification was not created.");

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, expiryPeriodInDays: expiryPeriodInDays);
            var serviceUnderTest = containerUnderTest.Get<IOutgoingVerificationService>();

            var instructionsWithWrongUser = await serviceUnderTest.GetInstructions(userForSubmission.Id, verification.UniqueId);

            var instructionsBeforeExpiry = await serviceUnderTest.GetInstructions(user.Id, verification.UniqueId);

            // I wait until the verification expires.
            await Task.Delay(expiryPeriod);

            var instructionsAfterExpiry = await serviceUnderTest.GetInstructions(user.Id, verification.UniqueId);

            Assert.IsTrue(instructionsWithWrongUser.IsRejected, "IsRejected field is not the expected when using the wrong user.");
            Assert.AreEqual(CommonResources.GenericInvalidRequestMessage, instructionsWithWrongUser.RejectionReason,
                "RejectionReason field is not the expected when using the wrong user.");
            Assert.IsNull(instructionsWithWrongUser.Instructions, "Instructions field should not be null when using the wrong user.");

            Assert.IsFalse(instructionsBeforeExpiry.IsRejected, "IsRejected field is not the expected before expiry.");
            Assert.IsNullOrEmpty(instructionsBeforeExpiry.RejectionReason, "RejectionReason field is not the expected before expiry.");
            Assert.IsNotNull(instructionsBeforeExpiry.Instructions, "Instructions field should not be null before expiry.");

            Assert.AreEqual(!markedAsSent, instructionsBeforeExpiry.Instructions.CanMarkAddressAsInvalid, "CanMarkAddressAsInvalid is not the expected.");
            Assert.AreEqual(!markedAsSent, instructionsBeforeExpiry.Instructions.CanMarkAsSent, "CanMarkAsSent is not the expected.");
            Assert.AreEqual(otherUserMarkedAddressAsInvalid, instructionsBeforeExpiry.Instructions.OtherUserHasMarkedAddressAsInvalid,
                "OtherUserHasMarkedAddressAsInvalid is not the expected.");

            Assert.AreEqual(
                retrievedVerification.TenancyDetailsSubmission.Address.Line1,
                instructionsBeforeExpiry.Instructions.RecipientAddress.Line1,
                "Instructions.RecipientAddress.Line1 is not the expected.");
            Assert.AreEqual(
                retrievedVerification.TenancyDetailsSubmission.Address.Line2,
                instructionsBeforeExpiry.Instructions.RecipientAddress.Line2,
                "Instructions.RecipientAddress.Line2 is not the expected.");
            Assert.AreEqual(
                retrievedVerification.TenancyDetailsSubmission.Address.Line3,
                instructionsBeforeExpiry.Instructions.RecipientAddress.Line3,
                "Instructions.RecipientAddress.Line3 is not the expected.");
            Assert.AreEqual(
                retrievedVerification.TenancyDetailsSubmission.Address.Line4,
                instructionsBeforeExpiry.Instructions.RecipientAddress.Line4,
                "Instructions.RecipientAddress.Line1 is not the expected.");
            Assert.AreEqual(
                retrievedVerification.TenancyDetailsSubmission.Address.Locality,
                instructionsBeforeExpiry.Instructions.RecipientAddress.Locality,
                "Instructions.RecipientAddress.Locality is not the expected.");
            Assert.AreEqual(
                retrievedVerification.TenancyDetailsSubmission.Address.Region,
                instructionsBeforeExpiry.Instructions.RecipientAddress.Region,
                "Instructions.RecipientAddress.Region is not the expected.");
            Assert.AreEqual(
                retrievedVerification.TenancyDetailsSubmission.Address.Postcode,
                instructionsBeforeExpiry.Instructions.RecipientAddress.Postcode,
                "Instructions.RecipientAddress.Postcode is not the expected.");
            Assert.AreEqual(
                retrievedVerification.TenancyDetailsSubmission.Address.Country.EnglishName,
                instructionsBeforeExpiry.Instructions.RecipientAddress.CountryEnglishName,
                "Instructions.RecipientAddress.CountryEnglishName is not the expected.");
            Assert.AreEqual(
                retrievedVerification.TenancyDetailsSubmission.Address.Country.LocalName,
                instructionsBeforeExpiry.Instructions.RecipientAddress.CountryLocalName,
                "Instructions.RecipientAddress.CountryLocalName is not the expected.");

            Assert.AreEqual(retrievedVerification.TenancyDetailsSubmission.Address.CountryIdAsEnum(),
                instructionsBeforeExpiry.Instructions.MessageArguments.CountryId, "Instructions.MessageArguments.CountryId is not the expected.");
            Assert.AreEqual(retrievedVerification.SecretCode,
                instructionsBeforeExpiry.Instructions.MessageArguments.SecretCode, "Instructions.MessageArguments.SecretCode is not the expected.");

            Assert.IsTrue(instructionsAfterExpiry.IsRejected, "IsRejected field is not the expected after expiry.");
            Assert.AreEqual(CommonResources.GenericInvalidActionMessage, instructionsAfterExpiry.RejectionReason,
                "RejectionReason field is not the expected after expiry.");
            Assert.IsNull(instructionsAfterExpiry.Instructions,
                "Instructions field should be null after expiry.");
        }

        [Test, Combinatorial]
        public async Task GetInstructions_ExpiryPeriodTest_MarkedAddressAsInvalid(
            [Values(false, true)] bool markedAsSent,
            [Values(false, true)] bool otherUserMarkedAddressAsInvalid)
        {
            var markedAddressAsInvalid = true;
            var expiryPeriod = TimeSpan.FromSeconds(0.3);
            var expiryPeriodInDays = expiryPeriod.TotalDays;

            var helperContainer = CreateContainer();

            var ipAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);
            var ipAddressForSubmission = "2.3.4.5";
            var userForSubmission = await CreateUser(helperContainer, "test1@test.com", ipAddressForSubmission);

            var random = new RandomWrapper(2015);

            var verification = await CreateTenantVerificationAndSave(
                random, helperContainer, user.Id, ipAddress, userForSubmission.Id, ipAddressForSubmission,
                isSent: markedAsSent, markedAddressAsInvalid: markedAddressAsInvalid, otherUserMarkedAddressAsInvalid: otherUserMarkedAddressAsInvalid);

            var retrievedVerification = await DbProbe.TenantVerifications
                .Include(x => x.TenancyDetailsSubmission)
                .Include(x => x.TenancyDetailsSubmission.Address)
                .Include(x => x.TenancyDetailsSubmission.Address.Country)
                .SingleOrDefaultAsync(x => x.UniqueId.Equals(verification.UniqueId));

            Assert.IsNotNull(retrievedVerification, "Verification was not created.");

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, expiryPeriodInDays: expiryPeriodInDays);
            var serviceUnderTest = containerUnderTest.Get<IOutgoingVerificationService>();

            var instructionsWithWrongUser = await serviceUnderTest.GetInstructions(userForSubmission.Id, verification.UniqueId);

            var instructionsBeforeExpiry = await serviceUnderTest.GetInstructions(user.Id, verification.UniqueId);

            // I wait until the verification expires.
            await Task.Delay(expiryPeriod);

            var instructionsAfterExpiry = await serviceUnderTest.GetInstructions(user.Id, verification.UniqueId);

            Assert.IsTrue(instructionsWithWrongUser.IsRejected, "IsRejected field is not the expected when using the wrong user.");
            Assert.AreEqual(CommonResources.GenericInvalidRequestMessage, instructionsWithWrongUser.RejectionReason,
                "RejectionReason field is not the expected when using the wrong user.");
            Assert.IsNull(instructionsWithWrongUser.Instructions, "Instructions field should not be null when using the wrong user.");

            Assert.IsTrue(instructionsBeforeExpiry.IsRejected, "IsRejected field is not the expected before expiry.");
            Assert.AreEqual(CommonResources.GenericInvalidActionMessage, instructionsBeforeExpiry.RejectionReason, "RejectionReason field is not the expected before expiry.");
            Assert.IsNull(instructionsBeforeExpiry.Instructions, "Instructions field should be null before expiry.");
            
            Assert.IsTrue(instructionsAfterExpiry.IsRejected, "IsRejected field is not the expected after expiry.");
            Assert.AreEqual(CommonResources.GenericInvalidActionMessage, instructionsAfterExpiry.RejectionReason,
                "RejectionReason field is not the expected after expiry.");
            Assert.IsNull(instructionsAfterExpiry.Instructions,
                "Instructions field should be null after expiry.");
        }

        #endregion

        #region GetVerificationMessage

        [Test]
        public async Task GetVerificationMessage_VerificationDoesNotExist()
        {
            var helperContainer = CreateContainer();

            var ipAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var containerUnderTest = CreateContainer();
            var serviceUnderTest = containerUnderTest.Get<IOutgoingVerificationService>();

            var nonExistentVerificationUniqueId = Guid.NewGuid();
            var message = await serviceUnderTest.GetVerificationMessage(user.Id, nonExistentVerificationUniqueId);

            Assert.IsTrue(message.IsRejected, "IsRejected field is not the expected.");
            Assert.AreEqual(CommonResources.GenericInvalidRequestMessage, message.RejectionReason, 
                "RejectionReason field is not the expected.");
            Assert.IsNull(message.MessageArguments, "MessageArguments field is not the expected.");
        }

        [Test]
        public async Task GetVerificationMessage_ExpiryPeriodTest()
        {
            var expiryPeriod = TimeSpan.FromSeconds(0.4);
            var expiryPeriodInDays = expiryPeriod.TotalDays;

            var helperContainer = CreateContainer();

            var ipAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);
            var ipAddressForSubmission = "2.3.4.5";
            var userForSubmission = await CreateUser(helperContainer, "test1@test.com", ipAddressForSubmission);

            var random = new RandomWrapper(2015);

            var verification = await CreateTenantVerificationAndSave(
                random, helperContainer, user.Id, ipAddress, userForSubmission.Id, ipAddressForSubmission);

            var retrievedVerification = await DbProbe.TenantVerifications
                .Include(x => x.TenancyDetailsSubmission)
                .Include(x => x.TenancyDetailsSubmission.Address)
                .SingleOrDefaultAsync(x => x.UniqueId.Equals(verification.UniqueId));

            Assert.IsNotNull(retrievedVerification, "Verification was not created.");

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, expiryPeriodInDays: expiryPeriodInDays);
            var serviceUnderTest = containerUnderTest.Get<IOutgoingVerificationService>();

            var messageWithWrongUser = await serviceUnderTest.GetVerificationMessage(userForSubmission.Id, verification.UniqueId);

            var messageBeforeExpiry = await serviceUnderTest.GetVerificationMessage(user.Id, verification.UniqueId);

            // I wait until the verification expires.
            await Task.Delay(expiryPeriod);

            var messageAfterExpiry = await serviceUnderTest.GetVerificationMessage(user.Id, verification.UniqueId);

            Assert.IsTrue(messageWithWrongUser.IsRejected, "IsRejected field is not the expected when using the wrong user.");
            Assert.AreEqual(CommonResources.GenericInvalidRequestMessage, messageWithWrongUser.RejectionReason, 
                "RejectionReason field is not the expected when using the wrong user.");
            Assert.IsNull(messageWithWrongUser.MessageArguments, "MessageArguments field should not be null when using the wrong user.");

            Assert.IsFalse(messageBeforeExpiry.IsRejected, "IsRejected field is not the expected before expiry.");
            Assert.IsNullOrEmpty(messageBeforeExpiry.RejectionReason, "RejectionReason field is not the expected before expiry.");
            Assert.IsNotNull(messageBeforeExpiry.MessageArguments, "MessageArguments field should not be null before expiry.");
            Assert.AreEqual(retrievedVerification.TenancyDetailsSubmission.Address.CountryIdAsEnum(),
                messageBeforeExpiry.MessageArguments.CountryId, "CountryId is not the expected.");
            Assert.AreEqual(retrievedVerification.SecretCode,
                messageBeforeExpiry.MessageArguments.SecretCode, "SecretCode is not the expected.");

            Assert.IsTrue(messageAfterExpiry.IsRejected, "IsRejected field is not the expected after expiry.");
            Assert.AreEqual(CommonResources.GenericInvalidActionMessage, messageAfterExpiry.RejectionReason, 
                "RejectionReason field is not the expected after expiry.");
            Assert.IsNull(messageAfterExpiry.MessageArguments, 
                "MessageArguments field should be null after expiry.");
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
            var verificationsPerTenancyDetailsSubmission = 2;

            var ipAddress = "1.1.1.1";
            var userResidenceCountryId = CountryId.GB;
            var userResidenceLatitude = 1.00;
            var userResidenceLongitude = 1.00;

            var random = new RandomWrapper(2015);

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            // I create this submission for the UserResidenceService, so that the user has a residence address.
            var submissionForUserResidence = await CreateTenancyDetailsSubmissionAndSave(
                random, helperContainer, 
                user.Id, ipAddress, // Address
                user.Id, ipAddress, // Submission
                userResidenceCountryId, userResidenceLatitude, userResidenceLongitude);
            Assert.IsNotNull(submissionForUserResidence, "Submission for user residence was not created during the setup.");

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, 
                minDegreesDistance: minDegreesDistance, 
                verificationsPerTenancyDetailsSubmission: verificationsPerTenancyDetailsSubmission);
            SetupAntiAbuseServiceResponseToNotRejected(containerUnderTest);

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
            var verificationsPerTenancyDetailsSubmission = 2;

            var ipAddress = "1.1.1.1";
            var userResidenceCountryId = CountryId.GB;
            var userResidenceLatitude = 1.00;
            var userResidenceLongitude = 1.00;

            var addressIpAddress = "2.2.2.2";
            var submissionIpAddress = addressIpAddress;
            var addressCountryId = CountryId.GR;
            var submissionLatitude = 2.00;
            var submissionLongitude = 2.00;

            var random = new RandomWrapper(2015);

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);
            var userForSubmission = await CreateUser(helperContainer, "test2@test.com", submissionIpAddress);

            // I create this submission for the UserResidenceService, so that the user has a residence address.
            var submissionForUserResidence = await CreateTenancyDetailsSubmissionAndSave(
                random, helperContainer, 
                user.Id, ipAddress, // Address
                user.Id, ipAddress, // Submission
                userResidenceCountryId, userResidenceLatitude, userResidenceLongitude);
            Assert.IsNotNull(submissionForUserResidence, "Submission for user residence was not created during the setup.");

            // This is the actual submission that can be picked up for outgoing verifcation.
            var submission = await CreateTenancyDetailsSubmissionAndSave(
                random, helperContainer, 
                userForSubmission.Id, addressIpAddress, // Address
                userForSubmission.Id, submissionIpAddress, // Submission
                addressCountryId, submissionLatitude, submissionLongitude);
            Assert.IsNotNull(submission, "Submission was not created during setup.");

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, 
                minDegreesDistance: minDegreesDistance, 
                verificationsPerTenancyDetailsSubmission: verificationsPerTenancyDetailsSubmission);
            SetupAntiAbuseServiceResponseToNotRejected(containerUnderTest);
            
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
        public async Task Pick_NoUserResidenceBecauseNoSubmissions()
        {
            var minDegreesDistance = 0.001;
            var verificationsPerTenancyDetailsSubmission = 2;

            var ipAddress = "1.1.1.1";
            
            var addressIpAddress = "2.2.2.2";
            var submissionIpAddress = addressIpAddress;
            var addressCountryId = CountryId.GB;
            var submissionLatitude = 2.00;
            var submissionLongitude = 2.00;

            var random = new RandomWrapper(2015);

            var helperContainer = CreateContainer();
            
            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);
            var userForSubmission = await CreateUser(helperContainer, "test2@test.com", submissionIpAddress);

            // This is the submission that can be picked up for outgoing verifcation.
            var submission = await CreateTenancyDetailsSubmissionAndSave(
                random, helperContainer,
                userForSubmission.Id, addressIpAddress, // Address
                userForSubmission.Id, submissionIpAddress, // Submission
                addressCountryId, submissionLatitude, submissionLongitude);
            Assert.IsNotNull(submission, "Submission was not created during setup.");

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, 
                minDegreesDistance: minDegreesDistance, 
                verificationsPerTenancyDetailsSubmission: verificationsPerTenancyDetailsSubmission);
            SetupAntiAbuseServiceResponseToNotRejected(containerUnderTest);

            var serviceUnderTest = containerUnderTest.Get<IOutgoingVerificationService>();

            var verificationUniqueId = Guid.NewGuid();
            var outcome = await serviceUnderTest.Pick(user.Id, ipAddress, verificationUniqueId);

            Assert.IsTrue(outcome.IsRejected, "IsRejected field is not the expected.");
            Assert.AreEqual(OutgoingVerificationResources.Pick_CannotDetermineUserResidenceErrorMessage,
                outcome.RejectionReason, "RejectionReason is not the expected.");
            Assert.IsNull(outcome.VerificationUniqueId, "VerificationUniqueId is not the expected.");

            var retrievedTenantVerification = await DbProbe.TenantVerifications.SingleOrDefaultAsync(x => x.AssignedToId.Equals(user.Id));
            Assert.IsNull(retrievedTenantVerification, "A tenant verification should not be created.");
        }

        [Test]
        public async Task Pick_Success()
        {
            var minDegreesDistance = 0.001;
            var verificationsPerTenancyDetailsSubmission = 2;

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
            var clock = helperContainer.Get<IClock>();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);
            var userForSubmission = await CreateUser(helperContainer, "test2@test.com", submissionIpAddress);
            // I create this submission for the UserResidenceService, so that the user has a residence address.
            var submissionForUserResidence = await CreateTenancyDetailsSubmissionAndSave(
                random, helperContainer,
                user.Id, ipAddress, // Address
                user.Id, ipAddress, // Submission
                userResidenceCountryId, userResidenceLatitude, userResidenceLongitude);
            Assert.IsNotNull(submissionForUserResidence, "Submission for user residence was not created during the setup.");

            // This is the actual submission that can be picked up for outgoing verifcation.
            var submission = await CreateTenancyDetailsSubmissionAndSave(
                random, helperContainer,
                userForSubmission.Id, addressIpAddress, // Address
                userForSubmission.Id, submissionIpAddress, // Submission
                addressCountryId, submissionLatitude, submissionLongitude);
            Assert.IsNotNull(submission, "Submission was not created during setup.");

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, 
                minDegreesDistance: minDegreesDistance, 
                verificationsPerTenancyDetailsSubmission: verificationsPerTenancyDetailsSubmission);
            SetupAntiAbuseServiceResponseToNotRejected(containerUnderTest);

            var serviceUnderTest = containerUnderTest.Get<IOutgoingVerificationService>();

            var timeBefore = clock.OffsetNow;
            var verificationUniqueId = Guid.NewGuid();
            var outcome = await serviceUnderTest.Pick(user.Id, ipAddress, verificationUniqueId);

            Assert.IsFalse(outcome.IsRejected, "IsRejected field is not the expected.");
            Assert.IsNullOrEmpty(outcome.RejectionReason, "RejectionReason is not the expected.");
            Assert.AreEqual(verificationUniqueId, outcome.VerificationUniqueId, "VerificationUniqueId is not the expected.");

            var timeAfter = clock.OffsetNow;

            var retrievedTenantVerification = await DbProbe.TenantVerifications.SingleOrDefaultAsync(x => x.AssignedToId.Equals(user.Id));
            Assert.IsNotNull(retrievedTenantVerification, "A tenant verification should be created.");
            Assert.That(retrievedTenantVerification.CreatedOn, Is.GreaterThanOrEqualTo(timeBefore),
                "CreatedOn on the retrieved tenant verification is not the expected.");
            Assert.That(retrievedTenantVerification.CreatedOn, Is.LessThanOrEqualTo(timeAfter),
                "CreatedOn on the retrieved tenant verification is not the expected.");
            Assert.AreEqual(submission.Id, retrievedTenantVerification.TenancyDetailsSubmissionId,
                "TenancyDetailsSubmissionId on the retrieved tenant verification is not the expected.");
            Assert.AreEqual(ipAddress, retrievedTenantVerification.AssignedByIpAddress,
                "AssignedByIpAddress on the retrieved tenant verification is not the expected.");
        }

        [Test]
        public async Task Pick_DoesNotPickHiddenSubmissions()
        {
            var minDegreesDistance = 0.001;
            var verificationsPerTenancyDetailsSubmission = 2;

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
            // I create this submission for the UserResidenceService, so that the user has a residence address.
            var submissionForUserResidence = await CreateTenancyDetailsSubmissionAndSave(
                random, helperContainer, 
                user.Id, ipAddress, // Address
                user.Id, ipAddress, // Submission
                userResidenceCountryId, userResidenceLatitude, userResidenceLongitude);
            Assert.IsNotNull(submissionForUserResidence, "Submission for user residence was not created during the setup.");

            // This is the actual submission that can be picked up for outgoing verifcation.
            var submission = await CreateTenancyDetailsSubmissionAndSave(
                random, helperContainer, 
                userForSubmission.Id, addressIpAddress, // Address
                userForSubmission.Id, submissionIpAddress, // Submission
                addressCountryId, submissionLatitude, submissionLongitude, isHidden: true);
            Assert.IsNotNull(submission, "Submission was not created during setup.");

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, 
                minDegreesDistance: minDegreesDistance, 
                verificationsPerTenancyDetailsSubmission: verificationsPerTenancyDetailsSubmission);
            SetupAntiAbuseServiceResponseToNotRejected(containerUnderTest);
            
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
            var minDegreesDistance = 0.001;
            var verificationsPerTenancyDetailsSubmission = 2;

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
            // I create this submission for the UserResidenceService, so that the user has a residence address.
            var submissionForUserResidence = await CreateTenancyDetailsSubmissionAndSave(
                random, helperContainer, 
                user.Id, ipAddress, // Address
                user.Id, ipAddress, // Submission
                userResidenceCountryId, userResidenceLatitude, userResidenceLongitude);
            Assert.IsNotNull(submissionForUserResidence, "Submission for user residence was not created during the setup.");

            // This is the actual submission that can be picked up for outgoing verifcation.
            var submission = await CreateTenancyDetailsSubmissionAndSave(
                random, helperContainer, 
                userForSubmission.Id, addressIpAddress, // Address 
                user.Id, submissionIpAddress, // Submission
                addressCountryId, submissionLatitude, submissionLongitude);
            Assert.IsNotNull(submission, "Submission was not created during setup.");

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, 
                minDegreesDistance: minDegreesDistance, 
                verificationsPerTenancyDetailsSubmission: verificationsPerTenancyDetailsSubmission);
            SetupAntiAbuseServiceResponseToNotRejected(containerUnderTest);

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
        public async Task Pick_DoesNotPickSubmissionByTheSameIpAddress()
        {
            var minDegreesDistance = 0.001;
            var verificationsPerTenancyDetailsSubmission = 2;

            var ipAddress = "1.1.1.1";
            var userResidenceCountryId = CountryId.GB;
            var userResidenceLatitude = 1.00;
            var userResidenceLongitude = 1.00;

            var addressIpAddress = "2.2.2.2";
            var submissionIpAddress = ipAddress;
            var addressCountryId = CountryId.GB;
            var submissionLatitude = 2.00;
            var submissionLongitude = 2.00;

            var random = new RandomWrapper(2015);

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);
            var userForSubmission = await CreateUser(helperContainer, "test2@test.com", submissionIpAddress);
            // I create this submission for the UserResidenceService, so that the user has a residence address.
            var submissionForUserResidence = await CreateTenancyDetailsSubmissionAndSave(
                random, helperContainer,
                user.Id, ipAddress, // Address
                user.Id, ipAddress, // Submission
                userResidenceCountryId, userResidenceLatitude, userResidenceLongitude);
            Assert.IsNotNull(submissionForUserResidence, "Submission for user residence was not created during the setup.");

            // This is the actual submission that can be picked up for outgoing verifcation.
            var submission = await CreateTenancyDetailsSubmissionAndSave(
                random, helperContainer,
                userForSubmission.Id, addressIpAddress, // Address 
                userForSubmission.Id, submissionIpAddress, // Submission
                addressCountryId, submissionLatitude, submissionLongitude);
            Assert.IsNotNull(submission, "Submission was not created during setup.");

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest,
                minDegreesDistance: minDegreesDistance, 
                verificationsPerTenancyDetailsSubmission: verificationsPerTenancyDetailsSubmission);
            SetupAntiAbuseServiceResponseToNotRejected(containerUnderTest);

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
        public async Task Pick_DoesNotPickSubmissionForAddressByTheSameUser()
        {
            var minDegreesDistance = 0.001;
            var verificationsPerTenancyDetailsSubmission = 2;

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
            // I create this submission for the UserResidenceService, so that the user has a residence address.
            var submissionForUserResidence = await CreateTenancyDetailsSubmissionAndSave(
                random, helperContainer,
                user.Id, ipAddress, // Address
                user.Id, ipAddress, // Submission
                userResidenceCountryId, userResidenceLatitude, userResidenceLongitude);
            Assert.IsNotNull(submissionForUserResidence, "Submission for user residence was not created during the setup.");

            // This is the actual submission that can be picked up for outgoing verifcation.
            var submission = await CreateTenancyDetailsSubmissionAndSave(
                random, helperContainer,
                user.Id, addressIpAddress, // Address 
                userForSubmission.Id, submissionIpAddress, // Submission
                addressCountryId, submissionLatitude, submissionLongitude);
            Assert.IsNotNull(submission, "Submission was not created during setup.");

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest,
                minDegreesDistance: minDegreesDistance, 
                verificationsPerTenancyDetailsSubmission: verificationsPerTenancyDetailsSubmission);
            SetupAntiAbuseServiceResponseToNotRejected(containerUnderTest);

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
        public async Task Pick_DoesNotPickSubmissionForAddressByTheSameIpAddress()
        {
            var minDegreesDistance = 0.001;
            var verificationsPerTenancyDetailsSubmission = 2;

            var ipAddress = "1.1.1.1";
            var userResidenceCountryId = CountryId.GB;
            var userResidenceLatitude = 1.00;
            var userResidenceLongitude = 1.00;

            var addressIpAddress = ipAddress;
            var submissionIpAddress = "2.2.2.2";
            var addressCountryId = CountryId.GB;
            var submissionLatitude = 2.00;
            var submissionLongitude = 2.00;

            var random = new RandomWrapper(2015);

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);
            var userForSubmission = await CreateUser(helperContainer, "test2@test.com", submissionIpAddress);
            // I create this submission for the UserResidenceService, so that the user has a residence address.
            var submissionForUserResidence = await CreateTenancyDetailsSubmissionAndSave(
                random, helperContainer,
                user.Id, ipAddress, // Address
                user.Id, ipAddress, // Submission
                userResidenceCountryId, userResidenceLatitude, userResidenceLongitude);
            Assert.IsNotNull(submissionForUserResidence, "Submission for user residence was not created during the setup.");

            // This is the actual submission that can be picked up for outgoing verifcation.
            var submission = await CreateTenancyDetailsSubmissionAndSave(
                random, helperContainer,
                userForSubmission.Id, addressIpAddress, // Address 
                userForSubmission.Id, submissionIpAddress, // Submission
                addressCountryId, submissionLatitude, submissionLongitude);
            Assert.IsNotNull(submission, "Submission was not created during setup.");

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, 
                minDegreesDistance: minDegreesDistance, 
                verificationsPerTenancyDetailsSubmission: verificationsPerTenancyDetailsSubmission);
            SetupAntiAbuseServiceResponseToNotRejected(containerUnderTest);

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
        public async Task Pick_DoesNotPickSubmissionWithMaximumVerificationsAssigned()
        {
            var minDegreesDistance = 0.001;
            var verificationsPerTenancyDetailsSubmission = 2;

            var ipAddress = "1.1.1.1";
            var userResidenceCountryId = CountryId.GB;
            var userResidenceLatitude = 1.00;
            var userResidenceLongitude = 1.00;

            var addressIpAddress = "2.2.2.2";
            var submissionIpAddress = addressIpAddress;
            var addressCountryId = CountryId.GB;
            var submissionLatitude = 2.00;
            var submissionLongitude = 2.00;

            var existingVerificationsIpAddress = "3.3.3.3";

            var random = new RandomWrapper(2015);

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);
            var userForSubmission = await CreateUser(helperContainer, "test2@test.com", submissionIpAddress);
            var userForExistingVerifications = await CreateUser(helperContainer, "test3@test.com", existingVerificationsIpAddress);
            // I create this submission for the UserResidenceService, so that the user has a residence address.
            var submissionForUserResidence = await CreateTenancyDetailsSubmissionAndSave(
                random, helperContainer,
                user.Id, ipAddress, // Address
                user.Id, ipAddress, // Submission
                userResidenceCountryId, userResidenceLatitude, userResidenceLongitude);
            Assert.IsNotNull(submissionForUserResidence, "Submission for user residence was not created during the setup.");

            // This is the actual submission that can be picked up for outgoing verifcation.
            var submission = await CreateTenancyDetailsSubmissionAndSave(
                random, helperContainer,
                userForSubmission.Id, addressIpAddress, // Address 
                userForSubmission.Id, submissionIpAddress, // Submission
                addressCountryId, submissionLatitude, submissionLongitude);
            Assert.IsNotNull(submission, "Submission was not created during setup.");
            var existingVerifications = await AssingVerifications(
                random, helperContainer, submission.Id,
                userForExistingVerifications.Id, existingVerificationsIpAddress, verificationsPerTenancyDetailsSubmission);
            Assert.IsNotEmpty(existingVerifications, "Existing verifications were not created.");

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, 
                minDegreesDistance: minDegreesDistance, 
                verificationsPerTenancyDetailsSubmission: verificationsPerTenancyDetailsSubmission);
            SetupAntiAbuseServiceResponseToNotRejected(containerUnderTest);

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
        public async Task Pick_DoesNotPickSubmissionAlreadyAssignedToUser()
        {
            var minDegreesDistance = 0.001;
            var verificationsPerTenancyDetailsSubmission = 2;

            var ipAddress = "1.1.1.1";
            var userResidenceCountryId = CountryId.GB;
            var userResidenceLatitude = 1.00;
            var userResidenceLongitude = 1.00;

            var addressIpAddress = "2.2.2.2";
            var submissionIpAddress = addressIpAddress;
            var addressCountryId = CountryId.GB;
            var submissionLatitude = 2.00;
            var submissionLongitude = 2.00;

            var existingVerificationsIpAddress = "3.3.3.3";

            var random = new RandomWrapper(2015);

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);
            var userForSubmission = await CreateUser(helperContainer, "test2@test.com", submissionIpAddress);
            // I create this submission for the UserResidenceService, so that the user has a residence address.
            var submissionForUserResidence = await CreateTenancyDetailsSubmissionAndSave(
                random, helperContainer,
                user.Id, ipAddress, // Address
                user.Id, ipAddress, // Submission
                userResidenceCountryId, userResidenceLatitude, userResidenceLongitude);
            Assert.IsNotNull(submissionForUserResidence, "Submission for user residence was not created during the setup.");

            // This is the actual submission that can be picked up for outgoing verifcation.
            var submission = await CreateTenancyDetailsSubmissionAndSave(
                random, helperContainer,
                userForSubmission.Id, addressIpAddress, // Address 
                userForSubmission.Id, submissionIpAddress, // Submission
                addressCountryId, submissionLatitude, submissionLongitude);
            Assert.IsNotNull(submission, "Submission was not created during setup.");
            var existingVerifications = await AssingVerifications(
                random, helperContainer, submission.Id,
                user.Id, existingVerificationsIpAddress, numberOfTenantVerifications: 1);
            Assert.IsNotEmpty(existingVerifications, "Existing verifications were not created.");

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, 
                minDegreesDistance: minDegreesDistance, 
                verificationsPerTenancyDetailsSubmission: verificationsPerTenancyDetailsSubmission);
            SetupAntiAbuseServiceResponseToNotRejected(containerUnderTest);

            var serviceUnderTest = containerUnderTest.Get<IOutgoingVerificationService>();

            var verificationUniqueId = Guid.NewGuid();
            var outcome = await serviceUnderTest.Pick(user.Id, ipAddress, verificationUniqueId);

            Assert.IsTrue(outcome.IsRejected, "IsRejected field is not the expected.");
            Assert.AreEqual(OutgoingVerificationResources.Pick_NoVerificationAssignableToUser_RejectionMessage,
                outcome.RejectionReason, "RejectionReason is not the expected.");
            Assert.IsNull(outcome.VerificationUniqueId, "VerificationUniqueId is not the expected.");

            var retrievedTenantVerification = await DbProbe.TenantVerifications.SingleOrDefaultAsync(x => x.UniqueId.Equals(verificationUniqueId));
            Assert.IsNull(retrievedTenantVerification, "A tenant verification should not be created.");
        }

        [Test]
        public async Task Pick_DoesNotPickSubmissionAlreadyAssignedToIpAddress()
        {
            var minDegreesDistance = 0.001;
            var verificationsPerTenancyDetailsSubmission = 2;

            var ipAddress = "1.1.1.1";
            var userResidenceCountryId = CountryId.GB;
            var userResidenceLatitude = 1.00;
            var userResidenceLongitude = 1.00;

            var addressIpAddress = "2.2.2.2";
            var submissionIpAddress = addressIpAddress;
            var addressCountryId = CountryId.GB;
            var submissionLatitude = 2.00;
            var submissionLongitude = 2.00;

            var existingVerificationsIpAddress = "3.3.3.3";

            var random = new RandomWrapper(2015);

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);
            var userForSubmission = await CreateUser(helperContainer, "test2@test.com", submissionIpAddress);
            var userForExistingVerifications = await CreateUser(helperContainer, "test3@test.com", existingVerificationsIpAddress);
            // I create this submission for the UserResidenceService, so that the user has a residence address.
            var submissionForUserResidence = await CreateTenancyDetailsSubmissionAndSave(
                random, helperContainer,
                user.Id, ipAddress, // Address
                user.Id, ipAddress, // Submission
                userResidenceCountryId, userResidenceLatitude, userResidenceLongitude);
            Assert.IsNotNull(submissionForUserResidence, "Submission for user residence was not created during the setup.");

            // This is the actual submission that can be picked up for outgoing verifcation.
            var submission = await CreateTenancyDetailsSubmissionAndSave(
                random, helperContainer,
                userForSubmission.Id, addressIpAddress, // Address 
                userForSubmission.Id, submissionIpAddress, // Submission
                addressCountryId, submissionLatitude, submissionLongitude);
            Assert.IsNotNull(submission, "Submission was not created during setup.");
            var existingVerifications = await AssingVerifications(
                random, helperContainer, submission.Id,
                userForExistingVerifications.Id, ipAddress, verificationsPerTenancyDetailsSubmission);
            Assert.IsNotEmpty(existingVerifications, "Existing verifications were not created.");

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, 
                minDegreesDistance: minDegreesDistance, 
                verificationsPerTenancyDetailsSubmission: verificationsPerTenancyDetailsSubmission);
            SetupAntiAbuseServiceResponseToNotRejected(containerUnderTest);

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
        public async Task Pick_DoesNotPickSubmissionWithinMinDegreesDistance()
        {
            var minDegreesDistance = 1.00;
            var verificationsPerTenancyDetailsSubmission = 2;

            var ipAddress = "1.1.1.1";
            var userResidenceCountryId = CountryId.GB;
            var userResidenceLatitude = 1.00;
            var userResidenceLongitude = 1.00;

            var addressIpAddress = "2.2.2.2";
            var submissionIpAddress = addressIpAddress;
            var addressCountryId = CountryId.GB;
            var submissionLatitude = 0.01;
            var submissionLongitude = 0.01;

            var random = new RandomWrapper(2015);

            var helperContainer = CreateContainer();

            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);
            var userForSubmission = await CreateUser(helperContainer, "test2@test.com", submissionIpAddress);
            // I create this submission for the UserResidenceService, so that the user has a residence address.
            var submissionForUserResidence = await CreateTenancyDetailsSubmissionAndSave(
                random, helperContainer,
                user.Id, ipAddress, // Address
                user.Id, ipAddress, // Submission
                userResidenceCountryId, userResidenceLatitude, userResidenceLongitude);
            Assert.IsNotNull(submissionForUserResidence, "Submission for user residence was not created during the setup.");

            // This is the actual submission that can be picked up for outgoing verifcation.
            var submission = await CreateTenancyDetailsSubmissionAndSave(
                random, helperContainer,
                userForSubmission.Id, addressIpAddress, // Address 
                userForSubmission.Id, submissionIpAddress, // Submission
                addressCountryId, submissionLatitude, submissionLongitude);
            Assert.IsNotNull(submission, "Submission was not created during setup.");

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, 
                minDegreesDistance: minDegreesDistance, 
                verificationsPerTenancyDetailsSubmission: verificationsPerTenancyDetailsSubmission);
            SetupAntiAbuseServiceResponseToNotRejected(containerUnderTest);

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

        private static void SetupConfig(IKernel container,
            int? itemsLimit = null,
            TimeSpan? cachingPeriod = null,
            double? minDegreesDistance = null, 
            int? verificationsPerTenancyDetailsSubmission = null,
            double? expiryPeriodInDays = null)
        {
            var mockConfig = new Mock<IOutgoingVerificationServiceConfig>();

            if (itemsLimit.HasValue)
                mockConfig.Setup(x => x.MyOutgoingVerificationsSummary_ItemsLimit).Returns(itemsLimit.Value);
            if (cachingPeriod.HasValue)
                mockConfig.Setup(x => x.MyOutgoingVerificationsSummary_CachingPeriod).Returns(cachingPeriod.Value);
            if (minDegreesDistance.HasValue)
                mockConfig.Setup(x => x.Pick_MinDegreesDistanceInAnyDirection).Returns(minDegreesDistance.Value);
            if (verificationsPerTenancyDetailsSubmission.HasValue)
                mockConfig.Setup(x => x.VerificationsPerTenancyDetailsSubmission).Returns(verificationsPerTenancyDetailsSubmission.Value);
            if (expiryPeriodInDays.HasValue)
                mockConfig.Setup(x => x.Instructions_ExpiryPeriodInDays).Returns(expiryPeriodInDays.Value);

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

        private static async Task<TenantVerification> CreateTenantVerificationAndSave(
            IRandomWrapper random, IKernel container, 
            string userId, string userIpAddress, 
            string userIdForSubmission, string userForSubmissionIpAddress,
            bool isSent = false,
            bool isComplete = false,
            bool markedAddressAsInvalid = false, 
            bool otherUserMarkedAddressAsInvalid = false)
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
            if (markedAddressAsInvalid)
                tenantVerification.MarkedAddressAsInvalidOn = clock.OffsetNow;

            dbContext.TenantVerifications.Add(tenantVerification);

            if (otherUserMarkedAddressAsInvalid)
            {
                var otherUserIpAddress = RandomIpAddress(random);
                var otherUser = await CreateUser(container, RandomEmail(random), otherUserIpAddress);
                var otherTenantVerification = new TenantVerification
                {
                    TenancyDetailsSubmission = tenancyDetailsSubmission,
                    UniqueId = Guid.NewGuid(),
                    AssignedToId = otherUser.Id,
                    AssignedByIpAddress = otherUserIpAddress,
                    SecretCode = RandomStringHelper.GetString(random, AppConstant.SECRET_CODE_MAX_LENGTH, CharacterCase.Mixed),
                    MarkedAddressAsInvalidOn = clock.OffsetNow
                };
                dbContext.TenantVerifications.Add(otherTenantVerification);
            }

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

        private static async Task<List<TenantVerification>> AssingVerifications(
            IRandomWrapper random, IKernel container, long tenancyDetailsSubmissionId, string userId, string userIpAddress, int numberOfTenantVerifications)
        {
            var dbContext = container.Get<IEpsilonContext>();

            var verifications = new List<TenantVerification>();
            for (var i = 0; i < numberOfTenantVerifications; i++)
            {
                var verification = new TenantVerification()
                {
                    TenancyDetailsSubmissionId = tenancyDetailsSubmissionId,
                    UniqueId = Guid.NewGuid(),
                    AssignedToId = userId,
                    AssignedByIpAddress = userIpAddress,
                    SecretCode = RandomStringHelper.GetString(random, AppConstant.SECRET_CODE_MAX_LENGTH, CharacterCase.Mixed)
                };
                dbContext.TenantVerifications.Add(verification);
                verifications.Add(verification);
            }
            await dbContext.SaveChangesAsync();
            return verifications;
        }

        #endregion
    }
}
