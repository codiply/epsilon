using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Ninject;
using Moq;
using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Infrastructure.Primitives;
using NUnit.Framework;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Resources.Web.Submission;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Wrappers;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Resources.Logic.AntiAbuse;
using Epsilon.Resources.Logic.TenancyDetailsSubmission;
using Epsilon.Logic.Constants.Enums;
using Epsilon.IntegrationTests.TestHelpers;
using Epsilon.Logic.JsonModels;
using Epsilon.Logic.Wrappers.Interfaces;
using Epsilon.Logic.Entities.Interfaces;
using static Epsilon.Logic.Helpers.RandomStringHelper;
using Epsilon.Logic.Forms.Submission;
using Epsilon.Resources.Common;
using System.Data.Entity.Infrastructure;
using Epsilon.Logic.Constants;

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class TenancyDetailsSubmissionServiceTest : BaseIntegrationTestWithRollback
    {
        #region Create

        [Test]
        public async Task Create_ForNonExistingAddress()
        {
            var ipAddress = "1.2.3.4";
            var helperContainer = CreateContainer();
            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var userIdUsedInAntiAbuse = string.Empty;
            var ipAddressUsedInAntiAbuse = string.Empty;

            var container = CreateContainer();
            var disableFrequencyPerAddressCheck = false;
            var maxFrequencyPerAddress = new Frequency(1, TimeSpan.FromDays(1));
            SetupConfigForCreate(container, disableFrequencyPerAddressCheck, maxFrequencyPerAddress);
            SetupAntiAbuseServiceResponse(container, (userId, ipAddr) =>
                {
                    userIdUsedInAntiAbuse = userId;
                    ipAddressUsedInAntiAbuse = ipAddr;
                }, new AntiAbuseServiceResponse());
            var service = container.Get<ITenancyDetailsSubmissionService>();

            var submissionUniqueId = Guid.NewGuid();
            var addressUniqueId = Guid.NewGuid();
            var outcome = await service.Create(user.Id, ipAddress, submissionUniqueId, addressUniqueId);

            Assert.IsTrue(outcome.IsRejected, "The field IsRejected on the outcome should be true.");
            Assert.AreEqual(TenancyDetailsSubmissionResources.Create_AddressNotFoundMessage, outcome.RejectionReason,
                "The RejectionReason on the outcome is not the expected.");
            Assert.IsNullOrEmpty(userIdUsedInAntiAbuse, "The AntiAbuse service should not be called. (1)");
            Assert.IsNullOrEmpty(ipAddressUsedInAntiAbuse, "The AntiAbuse service should not be called. (2)");
        }

        [Test]
        public async Task Create_RejectedByAntiAbuseService()
        {
            var ipAddress = "1.2.3.4";
            var antiAbuseRejectionReason = "AntiAbuseService Rejection Reason";
            var helperContainer = CreateContainer();
            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var random = new RandomWrapper(2015);

            var address = await AddressHelper.CreateRandomAddressAndSave(random, helperContainer, user.Id, ipAddress, CountryId.GB);

            var userIdUsedInAntiAbuse = string.Empty;
            var ipAddressUsedInAntiAbuse = string.Empty;

            var container = CreateContainer();
            var disableFrequencyPerAddressCheck = true;
            var maxFrequencyPerAddress = new Frequency(1, TimeSpan.FromDays(0));
            SetupConfigForCreate(container, disableFrequencyPerAddressCheck, maxFrequencyPerAddress);
            SetupAntiAbuseServiceResponse(container, (userId, ipAddr) =>
                {
                    userIdUsedInAntiAbuse = userId;
                    ipAddressUsedInAntiAbuse = ipAddr;
                }, new AntiAbuseServiceResponse()
                {
                    IsRejected = true,
                    RejectionReason = antiAbuseRejectionReason
                });
            var service = container.Get<ITenancyDetailsSubmissionService>();

            var submissionUniqueId = Guid.NewGuid();
            var outcome = await service.Create(user.Id, ipAddress, submissionUniqueId, address.UniqueId);

            Assert.IsTrue(outcome.IsRejected, "The field IsRejected on the outcome should be true.");
            Assert.AreEqual(antiAbuseRejectionReason, outcome.RejectionReason,
                "The RejectionReason on the outcome is not the expected.");
            Assert.AreEqual(user.Id, userIdUsedInAntiAbuse, 
                "The UserId used in the call to AntiAbuseService is not the expected.");
            Assert.AreEqual(ipAddress, ipAddressUsedInAntiAbuse, 
                "The IpAddress used in the call to AntiAbuseService is not the expected.");

            var retrievedTenancyDetailsSubmission = await DbProbe.TenancyDetailsSubmissions
                .SingleOrDefaultAsync(x => x.UniqueId.Equals(submissionUniqueId));
            Assert.IsNull(retrievedTenancyDetailsSubmission, "A TenancyDetailsSubmission should not be created.");
        }

        [Test]
        public async Task Create_SuccessfulCase()
        {
            var ipAddress = "1.2.3.4";
            var helperContainer = CreateContainer();
            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var random = new RandomWrapper(2015);

            var address = await AddressHelper.CreateRandomAddressAndSave(random, helperContainer, user.Id, ipAddress, CountryId.GB);

            var userIdUsedInAntiAbuse = string.Empty;
            var ipAddressUsedInAntiAbuse = string.Empty;

            var container = CreateContainer();
            var disableFrequencyPerAddressCheck = true;
            var maxFrequencyPerAddress = new Frequency(1, TimeSpan.FromDays(0));
            SetupConfigForCreate(container, disableFrequencyPerAddressCheck, maxFrequencyPerAddress);
            SetupAntiAbuseServiceResponse(container, (userId, ipAddr) =>
            {
                userIdUsedInAntiAbuse = userId;
                ipAddressUsedInAntiAbuse = ipAddr;
            }, new AntiAbuseServiceResponse()
            {
                IsRejected = false
            });
            var service = container.Get<ITenancyDetailsSubmissionService>();

            var submissionUniqueId = Guid.NewGuid();
            var timeBefore = DateTimeOffset.Now;
            var outcome = await service.Create(user.Id, ipAddress, submissionUniqueId, address.UniqueId);

            Assert.IsFalse(outcome.IsRejected, "The field IsRejected on the outcome should be false.");
            Assert.AreEqual(submissionUniqueId, outcome.TenancyDetailsSubmissionUniqueId,
                "The TenancyDetailsSubmissionId on the outcome is not the expected.");
            Assert.AreEqual(user.Id, userIdUsedInAntiAbuse,
                "The UserId used in the call to AntiAbuseService is not the expected.");
            Assert.AreEqual(ipAddress, ipAddressUsedInAntiAbuse,
                "The IpAddress used in the call to AntiAbuseService is not the expected.");

            var timeAfter = DateTimeOffset.Now;

            var retrievedTenancyDetailsSubmission = await DbProbe.TenancyDetailsSubmissions
                .SingleOrDefaultAsync(x => x.UniqueId.Equals(submissionUniqueId));
            Assert.IsNotNull(retrievedTenancyDetailsSubmission, "A TenancyDetailsSubmission should be created.");
            Assert.AreEqual(address.Id, retrievedTenancyDetailsSubmission.AddressId,
                "The AddressId on the retrieved TenancyDetailsSubmission is not the expected.");
            Assert.AreEqual(ipAddress, retrievedTenancyDetailsSubmission.CreatedByIpAddress,
                "The CreatedByIpAddress on the retrieved TenancyDetailsSubmission is not the expected.");
            Assert.AreEqual(user.Id, retrievedTenancyDetailsSubmission.UserId,
                "The UserId on the retrieved TenancyDetailsSubmission is not the expected.");
            Assert.IsTrue(timeBefore <= retrievedTenancyDetailsSubmission.CreatedOn &&
                retrievedTenancyDetailsSubmission.CreatedOn <= timeAfter,
                "The CreatedOn field on the retrieved TenancyDetailsSubmission is not within the expected range.");

            // I try to use the same UniqueId again.
            Assert.Throws<DbUpdateException>(async () => await service.Create(user.Id, ipAddress, submissionUniqueId, address.UniqueId),
                "Using the same UniqueId twice should throw an exception because of the unique constraint on UniqueId.");
        }

        [Test]
        public async Task Create_MaxFrequencyPerAddressCheckTest()
        {
            var ipAddress = "1.2.3.4";
            var maxFrequencyPerAddressTimes = 1;
            var maxFrequencyPerAddressPeriod = TimeSpan.FromSeconds(0.2);
            var helperContainer = CreateContainer();
            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var random = new RandomWrapper(2015);

            var address = await AddressHelper.CreateRandomAddressAndSave(random, helperContainer, user.Id, ipAddress, CountryId.GB);

            var container = CreateContainer();
            var disableFrequencyPerAddressCheck = false;
            var maxFrequencyPerAddress = new Frequency(maxFrequencyPerAddressTimes, maxFrequencyPerAddressPeriod);
            SetupConfigForCreate(container, disableFrequencyPerAddressCheck, maxFrequencyPerAddress);
            SetupAntiAbuseServiceResponse(container, (userId, ipAddr) => { }, new AntiAbuseServiceResponse()
            {
                IsRejected = false
            });
            var service = container.Get<ITenancyDetailsSubmissionService>();

            var submissionUniqueId1 = Guid.NewGuid();
            var outcome1 = await service.Create(user.Id, ipAddress, submissionUniqueId1, address.UniqueId);
            Assert.IsFalse(outcome1.IsRejected, "The field IsRejected on outcome1 should be false.");

            var submissionUniqueId2 = Guid.NewGuid();
            var outcome2 = await service.Create(user.Id, ipAddress, submissionUniqueId2, address.UniqueId);
            Assert.IsTrue(outcome2.IsRejected, "The field IsRejected on outcome2 should be true.");
            Assert.AreEqual(TenancyDetailsSubmissionResources.Create_MaxFrequencyPerAddressCheck_RejectionMessage,
                outcome2.RejectionReason, "RejectionReason on outcome2 is not the expected.");

            await Task.Delay(maxFrequencyPerAddressPeriod);

            var submissionUniqueId3 = Guid.NewGuid();
            var outcome3 = await service.Create(user.Id, ipAddress, submissionUniqueId3, address.UniqueId);
            Assert.IsFalse(outcome3.IsRejected, "The field IsRejected on outcome3 should be false.");

            var retrievedTenancyDetailsSubmission1 = await DbProbe.TenancyDetailsSubmissions
                .SingleOrDefaultAsync(x => x.UniqueId.Equals(submissionUniqueId1));
            var retrievedTenancyDetailsSubmission2 = await DbProbe.TenancyDetailsSubmissions
                .SingleOrDefaultAsync(x => x.UniqueId.Equals(submissionUniqueId2));
            var retrievedTenancyDetailsSubmission3 = await DbProbe.TenancyDetailsSubmissions
                .SingleOrDefaultAsync(x => x.UniqueId.Equals(submissionUniqueId3));

            Assert.IsNotNull(retrievedTenancyDetailsSubmission1, "Submission 1 should create a TenancyDetailsSubmission.");
            Assert.IsNull(retrievedTenancyDetailsSubmission2, "Submission 2 should not create a TenancyDetailsSubmission.");
            Assert.IsNotNull(retrievedTenancyDetailsSubmission3, "Submission 3 should create a TenancyDetailsSubmission.");
        }

        #endregion

        #region SubmissionBelongsToUser

        [Test]
        public async Task SubmissionBelongsToUserTest()
        {
            var helperContainer = CreateContainer();
            var user1IpAddress = "1.2.3.4";
            var user1 = await CreateUser(helperContainer, "test1@test.com", user1IpAddress);
            var user2IpAddress = "1.2.3.5";
            var user2 = await CreateUser(helperContainer, "test2@test.com", user2IpAddress);

            var random = new RandomWrapper(2015);

            var user1submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user1.Id, user1IpAddress, user2.Id, user2IpAddress);
            Assert.IsNotNull(user1submission, "The submission created for user1 is null.");

            var user2submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user2.Id, user2IpAddress, user1.Id, user1IpAddress);
            Assert.IsNotNull(user2submission, "The submission created for user2 is null.");

            var containerUnderTest = CreateContainer();
            var serviceUnderTest = containerUnderTest.Get<ITenancyDetailsSubmissionService>();

            var user1submissionBelongsToUser1 = await serviceUnderTest.SubmissionBelongsToUser(user1.Id, user1submission.UniqueId);
            Assert.IsTrue(user1submissionBelongsToUser1, "user1submission belongs to user1.");

            var user1submissionBelongsToUser2 = await serviceUnderTest.SubmissionBelongsToUser(user2.Id, user1submission.UniqueId);
            Assert.IsFalse(user1submissionBelongsToUser2, "user1submission does not belong to user2.");

            var user2submissionBelongsToUser1 = await serviceUnderTest.SubmissionBelongsToUser(user1.Id, user2submission.UniqueId);
            Assert.IsFalse(user2submissionBelongsToUser1, "user2submission does not belong to user1.");

            var user2submissionBelongsToUser2 = await serviceUnderTest.SubmissionBelongsToUser(user2.Id, user2submission.UniqueId);
            Assert.IsTrue(user2submissionBelongsToUser2, "user2submission belongs to user2.");
        }

        #endregion

        #region GetUserSubmissionsSummary

        [Test]
        public async Task GetUserSubmissionSummary_ForUserWithoutSubmissions()
        {
            var helperContainer = CreateContainer();

            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);

            var random = new RandomWrapper(2015);

            // I create a submission for the other user and assign the verifications to the user under test.
            // This is to test that the summary contains only submissions from the specific user.
            var otherUserIpAddress = "1.2.3.5";
            var otherUser = await CreateUser(helperContainer, "test2@test.com", otherUserIpAddress);
            var otherUserSubmission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, otherUser.Id, otherUserIpAddress, user.Id, userIpAddress);
            Assert.IsNotNull(otherUserSubmission, "The submission created for the other user is null.");

            var containerUnderTest = CreateContainer();
            var serviceUnderTest = containerUnderTest.Get<ITenancyDetailsSubmissionService>();

            // Full summary
            var response1 = await serviceUnderTest.GetUserSubmissionsSummary(user.Id, false);

            Assert.IsNotNull(response1, "Response1 is null.");
            Assert.IsFalse(response1.moreItemsExist, "Field moreItemsExist on response1 is not the expected.");
            Assert.IsFalse(response1.tenancyDetailsSubmissions.Any(), "Field tenancyDetailsSubmissions on response1 should be empty.");

            // Summary with limit
            var response2 = await serviceUnderTest.GetUserSubmissionsSummary(user.Id, true);

            Assert.IsNotNull(response2, "Response2 is null.");
            Assert.IsFalse(response2.moreItemsExist, "Field moreItemsExist on response2 is not the expected.");
            Assert.IsFalse(response2.tenancyDetailsSubmissions.Any(), "Field tenancyDetailsSubmissions on response2 should be empty.");
        }

        [Test]
        public async Task GetUserSubmissionSummary_WithSubmissionsEqualToTheLimit_ItemsLimitIsNotRelevant()
        {
            var itemsLimit = 3;
            var submissionsToCreate = itemsLimit;

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", "11.12.13.14");

            var random = new RandomWrapper(2015);
            var submissions = new List<TenancyDetailsSubmission>();

            for (var i = 0; i < submissionsToCreate; i++)
            {
                var submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress);
                submissions.Add(submission);
            }
            var submissionByCreationDescending = submissions.OrderByDescending(x => x.CreatedOn).ToList();

            // I create a submission for the other user and assign the verifications to the user under test.
            // This is to test that the summary contains only submissions from the specific user.
            var otherUserSubmission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, otherUser.Id, otherUserIpAddress, user.Id, userIpAddress);
            Assert.IsNotNull(otherUserSubmission, "The submission created for the other user is null.");

            var containerUnderTest = CreateContainer();
            SetupConfigForGetUserSubmissionSummary(containerUnderTest, itemsLimit);
            var serviceUnderTest = containerUnderTest.Get<ITenancyDetailsSubmissionService>();

            // Full summary
            var response1 = await serviceUnderTest.GetUserSubmissionsSummary(user.Id, false);

            Assert.IsNotNull(response1, "Response1 is null.");
            Assert.IsFalse(response1.moreItemsExist, "Field moreItemsExist on response1 is not the expected.");
            Assert.AreEqual(submissionsToCreate, response1.tenancyDetailsSubmissions.Count,
                "Response1 should contain all submissions.");
            for (var i = 0; i < submissionsToCreate; i++)
            {
                Assert.AreEqual(submissionByCreationDescending[i].UniqueId, response1.tenancyDetailsSubmissions[i].uniqueId,
                    string.Format("Response1: submission at position {0} does not have the expected uniqueId.", i));
            }

            Assert.IsFalse(response1.tenancyDetailsSubmissions.Any(x => x.uniqueId.Equals(otherUserSubmission.UniqueId)),
                "Response1 should not contain the submission of the other user.");

            // Summary with limit
            var response2 = await serviceUnderTest.GetUserSubmissionsSummary(user.Id, true);

            Assert.IsNotNull(response2, "Response2 is null.");
            Assert.IsFalse(response2.moreItemsExist, "Field moreItemsExist on response2 is not the expected.");
            Assert.AreEqual(itemsLimit, response2.tenancyDetailsSubmissions.Count,
                "Response1 should contains a number of submissions equal to the limit.");
            for (var i = 0; i < itemsLimit; i++)
            {
                Assert.AreEqual(submissionByCreationDescending[i].UniqueId, response2.tenancyDetailsSubmissions[i].uniqueId, 
                    string.Format("Response2: submission at position {0} does not have the expected uniqueId.", i));
            }

            Assert.IsFalse(response2.tenancyDetailsSubmissions.Any(x => x.uniqueId.Equals(otherUserSubmission.UniqueId)),
                "Response2 should not contain the submission of the other user.");
        }

        [Test]
        public async Task GetUserSubmissionSummary_WithMoreSubmissionsThanTheLimit_ItemsLimitIsRespected()
        {
            var itemsLimit = 2;
            var submissionsToCreate = 3;

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", "11.12.13.14");

            var random = new RandomWrapper(2015);
            var submissions = new List<TenancyDetailsSubmission>();

            for (var i = 0; i < submissionsToCreate; i++)
            {
                var submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress);
                submissions.Add(submission);
            }
            var submissionByCreationDescending = submissions.OrderByDescending(x => x.CreatedOn).ToList();

            // I create a submission for the other user and assign the verifications to the user under test.
            // This is to test that the summary contains only submissions from the specific user.
            var otherUserSubmission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, otherUser.Id, otherUserIpAddress, user.Id, userIpAddress);
            Assert.IsNotNull(otherUserSubmission, "The submission created for the other user is null.");

            var containerUnderTest = CreateContainer();
            SetupConfigForGetUserSubmissionSummary(containerUnderTest, itemsLimit);
            var serviceUnderTest = containerUnderTest.Get<ITenancyDetailsSubmissionService>();

            // Full summary
            var response1 = await serviceUnderTest.GetUserSubmissionsSummary(user.Id, false);

            Assert.IsNotNull(response1, "Response1 is null.");
            Assert.IsFalse(response1.moreItemsExist, "Field moreItemsExist on response1 is not the expected.");
            Assert.AreEqual(submissionsToCreate, response1.tenancyDetailsSubmissions.Count,
                "Response1 should contain all submissions.");
            for (var i = 0; i < submissionsToCreate; i++)
            {
                Assert.AreEqual(submissionByCreationDescending[i].UniqueId, response1.tenancyDetailsSubmissions[i].uniqueId, 
                    string.Format("Response1: submission at position {0} does not have the expected uniqueId.", i));
            }

            Assert.IsFalse(response1.tenancyDetailsSubmissions.Any(x => x.uniqueId.Equals(otherUserSubmission.UniqueId)),
                "Response1 should not contain the submission of the other user.");

            // Summary with limit
            var response2 = await serviceUnderTest.GetUserSubmissionsSummary(user.Id, true);

            Assert.IsNotNull(response2, "Response2 is null.");
            Assert.IsTrue(response2.moreItemsExist, "Field moreItemsExist on response2 is not the expected.");
            Assert.AreEqual(itemsLimit, response2.tenancyDetailsSubmissions.Count,
                "Response2 should contains a number of submissions equal to the limit.");
            for (var i = 0; i < itemsLimit; i++)
            {
                Assert.AreEqual(submissionByCreationDescending[i].UniqueId, response2.tenancyDetailsSubmissions[i].uniqueId, 
                    string.Format("Response1: submission at position {0} does not have the expected uniqueId.", i));
            }

            Assert.IsFalse(response2.tenancyDetailsSubmissions.Any(x => x.uniqueId.Equals(otherUserSubmission.UniqueId)),
                "Response2 should not contain the submission of the other user.");
        }

        [Test]
        public async Task GetUserSubmissionSummary_SingleNewSubmissionTest()
        {
            var itemsLimit = 10;
            var justCreatedVerifications = 0;
            var sentVerifications = 0;
            var completeVerifications = 0;
            var areDetailsSubmitted = false;
            var hasMovedOut = false;

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", "11.12.13.14");

            var random = new RandomWrapper(2015);
            var submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress,
                    justCreatedVerifications, sentVerifications, completeVerifications, areDetailsSubmitted, hasMovedOut);

            var containerUnderTest = CreateContainer();
            SetupConfigForGetUserSubmissionSummary(containerUnderTest, itemsLimit);
            var serviceUnderTest = containerUnderTest.Get<ITenancyDetailsSubmissionService>();

            var response  = await serviceUnderTest.GetUserSubmissionsSummary(user.Id, false);

            Assert.AreEqual(1, response.tenancyDetailsSubmissions.Count,
                "The response should contain a single submission.");

            var submissionInfo = response.tenancyDetailsSubmissions.Single();

            Assert.AreEqual(submission.UniqueId, submissionInfo.uniqueId,
                "Field uniqueId is not the expected.");
            Assert.IsFalse(submissionInfo.canEnterVerificationCode, "Field canEnterVerificationCode doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.canSubmitTenancyDetails, "Field canSubmitTenancyDetails doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.canSubmitMoveOutDetails, "Field canSubmitMoveOutDetails doesn't have the expected value.");

            Assert.IsFalse(submissionInfo.stepVerificationCodeSentOutDone, "Field stepVerificationCodeSentOutDone doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.stepVerificationCodeEnteredDone, "Field stepVerificationCodeEnteredDone doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.stepTenancyDetailsSubmittedDone, "Field stepTenancyDetailsSubmittedDone doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.stepMoveOutDetailsSubmittedDone, "Field stepMoveOutDetailsSubmittedDone doesn't have the expected value.");

            var retrievedSubmission = await DbProbe.TenancyDetailsSubmissions
                .Include(x => x.Address).Include(x => x.Address.Country).SingleOrDefaultAsync(x => x.UniqueId.Equals(submissionInfo.uniqueId));
            Assert.AreEqual(retrievedSubmission.Address.FullAddress(), submissionInfo.displayAddress, "Field displayAddress is not the expected.");
        }

        [Test]
        public async Task GetUserSubmissionSummary_SingleSubmissionWithJustCreatedVerificationsTest()
        {
            var itemsLimit = 10;
            var justCreatedVerifications = 2;
            var sentVerifications = 0;
            var completeVerifications = 0;
            var areDetailsSubmitted = false;
            var hasMovedOut = false;

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", "11.12.13.14");

            var random = new RandomWrapper(2015);
            var submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress,
                    justCreatedVerifications, sentVerifications, completeVerifications, areDetailsSubmitted, hasMovedOut);

            var containerUnderTest = CreateContainer();
            SetupConfigForGetUserSubmissionSummary(containerUnderTest, itemsLimit);
            var serviceUnderTest = containerUnderTest.Get<ITenancyDetailsSubmissionService>();

            var response = await serviceUnderTest.GetUserSubmissionsSummary(user.Id, false);

            Assert.AreEqual(1, response.tenancyDetailsSubmissions.Count,
                "The response should contain a single submission.");

            var submissionInfo = response.tenancyDetailsSubmissions.Single();

            Assert.AreEqual(submission.UniqueId, submissionInfo.uniqueId,
                "Field uniqueId is not the expected.");
            Assert.IsTrue(submissionInfo.canEnterVerificationCode, "Field canEnterVerificationCode doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.canSubmitTenancyDetails, "Field canSubmitTenancyDetails doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.canSubmitMoveOutDetails, "Field canSubmitMoveOutDetails doesn't have the expected value.");

            Assert.IsFalse(submissionInfo.stepVerificationCodeSentOutDone, "Field stepVerificationCodeSentOutDone doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.stepVerificationCodeEnteredDone, "Field stepVerificationCodeEnteredDone doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.stepTenancyDetailsSubmittedDone, "Field stepTenancyDetailsSubmittedDone doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.stepMoveOutDetailsSubmittedDone, "Field stepMoveOutDetailsSubmittedDone doesn't have the expected value.");

            var retrievedSubmission = await DbProbe.TenancyDetailsSubmissions
                .Include(x => x.Address).Include(x => x.Address.Country).SingleOrDefaultAsync(x => x.UniqueId.Equals(submissionInfo.uniqueId));
            Assert.AreEqual(retrievedSubmission.Address.FullAddress(), submissionInfo.displayAddress, "Field displayAddress is not the expected.");
        }

        [Test]
        public async Task GetUserSubmissionSummary_SingleSubmissionWithSentVerificationsTest()
        {
            var itemsLimit = 10;
            var justCreatedVerifications = 0;
            var sentVerifications = 2;
            var completeVerifications = 0;
            var areDetailsSubmitted = false;
            var hasMovedOut = false;

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", "11.12.13.14");

            var random = new RandomWrapper(2015);
            var submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress,
                    justCreatedVerifications, sentVerifications, completeVerifications, areDetailsSubmitted, hasMovedOut);

            var containerUnderTest = CreateContainer();
            SetupConfigForGetUserSubmissionSummary(containerUnderTest, itemsLimit);
            var serviceUnderTest = containerUnderTest.Get<ITenancyDetailsSubmissionService>();

            var response = await serviceUnderTest.GetUserSubmissionsSummary(user.Id, false);

            Assert.AreEqual(1, response.tenancyDetailsSubmissions.Count,
                "The response should contain a single submission.");

            var submissionInfo = response.tenancyDetailsSubmissions.Single();

            Assert.AreEqual(submission.UniqueId, submissionInfo.uniqueId,
                "Field uniqueId is not the expected.");
            Assert.IsTrue(submissionInfo.canEnterVerificationCode, "Field canEnterVerificationCode doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.canSubmitTenancyDetails, "Field canSubmitTenancyDetails doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.canSubmitMoveOutDetails, "Field canSubmitMoveOutDetails doesn't have the expected value.");

            Assert.IsTrue(submissionInfo.stepVerificationCodeSentOutDone, "Field stepVerificationCodeSentOutDone doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.stepVerificationCodeEnteredDone, "Field stepVerificationCodeEnteredDone doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.stepTenancyDetailsSubmittedDone, "Field stepTenancyDetailsSubmittedDone doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.stepMoveOutDetailsSubmittedDone, "Field stepMoveOutDetailsSubmittedDone doesn't have the expected value.");

            var retrievedSubmission = await DbProbe.TenancyDetailsSubmissions
                .Include(x => x.Address).Include(x => x.Address.Country).SingleOrDefaultAsync(x => x.UniqueId.Equals(submissionInfo.uniqueId));
            Assert.AreEqual(retrievedSubmission.Address.FullAddress(), submissionInfo.displayAddress, "Field displayAddress is not the expected.");
        }

        [Test]
        public async Task GetUserSubmissionSummary_SingleSubmissionWithCompleteVerificationsTest()
        {
            var itemsLimit = 10;
            var justCreatedVerifications = 0;
            var sentVerifications = 0;
            var completeVerifications = 2;
            var areDetailsSubmitted = false;
            var hasMovedOut = false;

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", "11.12.13.14");

            var random = new RandomWrapper(2015);
            var submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress,
                    justCreatedVerifications, sentVerifications, completeVerifications, areDetailsSubmitted, hasMovedOut);

            var containerUnderTest = CreateContainer();
            SetupConfigForGetUserSubmissionSummary(containerUnderTest, itemsLimit);
            var serviceUnderTest = containerUnderTest.Get<ITenancyDetailsSubmissionService>();

            var response = await serviceUnderTest.GetUserSubmissionsSummary(user.Id, false);

            Assert.AreEqual(1, response.tenancyDetailsSubmissions.Count,
                "The response should contain a single submission.");

            var submissionInfo = response.tenancyDetailsSubmissions.Single();

            Assert.AreEqual(submission.UniqueId, submissionInfo.uniqueId,
                "Field uniqueId is not the expected.");
            Assert.IsFalse(submissionInfo.canEnterVerificationCode, "Field canEnterVerificationCode doesn't have the expected value.");
            Assert.IsTrue(submissionInfo.canSubmitTenancyDetails, "Field canSubmitTenancyDetails doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.canSubmitMoveOutDetails, "Field canSubmitMoveOutDetails doesn't have the expected value.");

            Assert.IsTrue(submissionInfo.stepVerificationCodeSentOutDone, "Field stepVerificationCodeSentOutDone doesn't have the expected value.");
            Assert.IsTrue(submissionInfo.stepVerificationCodeEnteredDone, "Field stepVerificationCodeEnteredDone doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.stepTenancyDetailsSubmittedDone, "Field stepTenancyDetailsSubmittedDone doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.stepMoveOutDetailsSubmittedDone, "Field stepMoveOutDetailsSubmittedDone doesn't have the expected value.");

            var retrievedSubmission = await DbProbe.TenancyDetailsSubmissions
                .Include(x => x.Address).Include(x => x.Address.Country).SingleOrDefaultAsync(x => x.UniqueId.Equals(submissionInfo.uniqueId));
            Assert.AreEqual(retrievedSubmission.Address.FullAddress(), submissionInfo.displayAddress, "Field displayAddress is not the expected.");
        }

        [Test]
        public async Task GetUserSubmissionSummary_SingleSubmissionWithSentAndCompleteVerificationsTest()
        {
            var itemsLimit = 10;
            var justCreatedVerifications = 0;
            var sentVerifications = 1;
            var completeVerifications = 1;
            var areDetailsSubmitted = false;
            var hasMovedOut = false;

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", "11.12.13.14");

            var random = new RandomWrapper(2015);
            var submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress,
                    justCreatedVerifications, sentVerifications, completeVerifications, areDetailsSubmitted, hasMovedOut);

            var containerUnderTest = CreateContainer();
            SetupConfigForGetUserSubmissionSummary(containerUnderTest, itemsLimit);
            var serviceUnderTest = containerUnderTest.Get<ITenancyDetailsSubmissionService>();

            var response = await serviceUnderTest.GetUserSubmissionsSummary(user.Id, false);

            Assert.AreEqual(1, response.tenancyDetailsSubmissions.Count,
                "The response should contain a single submission.");

            var submissionInfo = response.tenancyDetailsSubmissions.Single();

            Assert.AreEqual(submission.UniqueId, submissionInfo.uniqueId,
                "Field uniqueId is not the expected.");
            Assert.IsTrue(submissionInfo.canEnterVerificationCode, "Field canEnterVerificationCode doesn't have the expected value.");
            Assert.IsTrue(submissionInfo.canSubmitTenancyDetails, "Field canSubmitTenancyDetails doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.canSubmitMoveOutDetails, "Field canSubmitMoveOutDetails doesn't have the expected value.");

            Assert.IsTrue(submissionInfo.stepVerificationCodeSentOutDone, "Field stepVerificationCodeSentOutDone doesn't have the expected value.");
            Assert.IsTrue(submissionInfo.stepVerificationCodeEnteredDone, "Field stepVerificationCodeEnteredDone doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.stepTenancyDetailsSubmittedDone, "Field stepTenancyDetailsSubmittedDone doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.stepMoveOutDetailsSubmittedDone, "Field stepMoveOutDetailsSubmittedDone doesn't have the expected value.");

            var retrievedSubmission = await DbProbe.TenancyDetailsSubmissions
                .Include(x => x.Address).Include(x => x.Address.Country).SingleOrDefaultAsync(x => x.UniqueId.Equals(submissionInfo.uniqueId));
            Assert.AreEqual(retrievedSubmission.Address.FullAddress(), submissionInfo.displayAddress, "Field displayAddress is not the expected.");
        }

        [Test]
        public async Task GetUserSubmissionSummary_SingleSubmissionWithSubmittedDetailsTest()
        {
            var itemsLimit = 10;
            var justCreatedVerifications = 0;
            var sentVerifications = 0;
            var completeVerifications = 2;
            var areDetailsSubmitted = true;
            var hasMovedOut = false;

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", "11.12.13.14");

            var random = new RandomWrapper(2015);
            var submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress,
                    justCreatedVerifications, sentVerifications, completeVerifications, areDetailsSubmitted, hasMovedOut);

            var containerUnderTest = CreateContainer();
            SetupConfigForGetUserSubmissionSummary(containerUnderTest, itemsLimit);
            var serviceUnderTest = containerUnderTest.Get<ITenancyDetailsSubmissionService>();

            var response = await serviceUnderTest.GetUserSubmissionsSummary(user.Id, false);

            Assert.AreEqual(1, response.tenancyDetailsSubmissions.Count,
                "The response should contain a single submission.");

            var submissionInfo = response.tenancyDetailsSubmissions.Single();

            Assert.AreEqual(submission.UniqueId, submissionInfo.uniqueId,
                "Field uniqueId is not the expected.");
            Assert.IsFalse(submissionInfo.canEnterVerificationCode, "Field canEnterVerificationCode doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.canSubmitTenancyDetails, "Field canSubmitTenancyDetails doesn't have the expected value.");
            Assert.IsTrue(submissionInfo.canSubmitMoveOutDetails, "Field canSubmitMoveOutDetails doesn't have the expected value.");

            Assert.IsTrue(submissionInfo.stepVerificationCodeSentOutDone, "Field stepVerificationCodeSentOutDone doesn't have the expected value.");
            Assert.IsTrue(submissionInfo.stepVerificationCodeEnteredDone, "Field stepVerificationCodeEnteredDone doesn't have the expected value.");
            Assert.IsTrue(submissionInfo.stepTenancyDetailsSubmittedDone, "Field stepTenancyDetailsSubmittedDone doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.stepMoveOutDetailsSubmittedDone, "Field stepMoveOutDetailsSubmittedDone doesn't have the expected value.");

            var retrievedSubmission = await DbProbe.TenancyDetailsSubmissions
                .Include(x => x.Address).Include(x => x.Address.Country).SingleOrDefaultAsync(x => x.UniqueId.Equals(submissionInfo.uniqueId));
            Assert.AreEqual(retrievedSubmission.Address.FullAddress(), submissionInfo.displayAddress, "Field displayAddress is not the expected.");
        }

        [Test]
        public async Task GetUserSubmissionSummary_SingleSubmissionWithSubmittedDetailsAndSentVerificationsTest()
        {
            var itemsLimit = 10;
            var justCreatedVerifications = 0;
            var sentVerifications = 1;
            var completeVerifications = 1;
            var areDetailsSubmitted = true;
            var hasMovedOut = false;

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", "11.12.13.14");

            var random = new RandomWrapper(2015);
            var submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress,
                    justCreatedVerifications, sentVerifications, completeVerifications, areDetailsSubmitted, hasMovedOut);

            var containerUnderTest = CreateContainer();
            SetupConfigForGetUserSubmissionSummary(containerUnderTest, itemsLimit);
            var serviceUnderTest = containerUnderTest.Get<ITenancyDetailsSubmissionService>();

            var response = await serviceUnderTest.GetUserSubmissionsSummary(user.Id, false);

            Assert.AreEqual(1, response.tenancyDetailsSubmissions.Count,
                "The response should contain a single submission.");

            var submissionInfo = response.tenancyDetailsSubmissions.Single();

            Assert.AreEqual(submission.UniqueId, submissionInfo.uniqueId,
                "Field uniqueId is not the expected.");
            Assert.IsTrue(submissionInfo.canEnterVerificationCode, "Field canEnterVerificationCode doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.canSubmitTenancyDetails, "Field canSubmitTenancyDetails doesn't have the expected value.");
            Assert.IsTrue(submissionInfo.canSubmitMoveOutDetails, "Field canSubmitMoveOutDetails doesn't have the expected value.");

            Assert.IsTrue(submissionInfo.stepVerificationCodeSentOutDone, "Field stepVerificationCodeSentOutDone doesn't have the expected value.");
            Assert.IsTrue(submissionInfo.stepVerificationCodeEnteredDone, "Field stepVerificationCodeEnteredDone doesn't have the expected value.");
            Assert.IsTrue(submissionInfo.stepTenancyDetailsSubmittedDone, "Field stepTenancyDetailsSubmittedDone doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.stepMoveOutDetailsSubmittedDone, "Field stepMoveOutDetailsSubmittedDone doesn't have the expected value.");

            var retrievedSubmission = await DbProbe.TenancyDetailsSubmissions
                .Include(x => x.Address).Include(x => x.Address.Country).SingleOrDefaultAsync(x => x.UniqueId.Equals(submissionInfo.uniqueId));
            Assert.AreEqual(retrievedSubmission.Address.FullAddress(), submissionInfo.displayAddress, "Field displayAddress is not the expected.");
        }

        [Test]
        public async Task GetUserSubmissionSummary_SingleSubmissionAfterMoveOutTest()
        {
            var itemsLimit = 10;
            var justCreatedVerifications = 0;
            var sentVerifications = 0;
            var completeVerifications = 2;
            var areDetailsSubmitted = true;
            var hasMovedOut = true;

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", "11.12.13.14");

            var random = new RandomWrapper(2015);
            var submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress,
                    justCreatedVerifications, sentVerifications, completeVerifications, areDetailsSubmitted, hasMovedOut);

            var containerUnderTest = CreateContainer();
            SetupConfigForGetUserSubmissionSummary(containerUnderTest, itemsLimit);
            var serviceUnderTest = containerUnderTest.Get<ITenancyDetailsSubmissionService>();

            var response = await serviceUnderTest.GetUserSubmissionsSummary(user.Id, false);

            Assert.AreEqual(1, response.tenancyDetailsSubmissions.Count,
                "The response should contain a single submission.");

            var submissionInfo = response.tenancyDetailsSubmissions.Single();

            Assert.AreEqual(submission.UniqueId, submissionInfo.uniqueId,
                "Field uniqueId is not the expected.");
            Assert.IsFalse(submissionInfo.canEnterVerificationCode, "Field canEnterVerificationCode doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.canSubmitTenancyDetails, "Field canSubmitTenancyDetails doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.canSubmitMoveOutDetails, "Field canSubmitMoveOutDetails doesn't have the expected value.");

            Assert.IsTrue(submissionInfo.stepVerificationCodeSentOutDone, "Field stepVerificationCodeSentOutDone doesn't have the expected value.");
            Assert.IsTrue(submissionInfo.stepVerificationCodeEnteredDone, "Field stepVerificationCodeEnteredDone doesn't have the expected value.");
            Assert.IsTrue(submissionInfo.stepTenancyDetailsSubmittedDone, "Field stepTenancyDetailsSubmittedDone doesn't have the expected value.");
            Assert.IsTrue(submissionInfo.stepMoveOutDetailsSubmittedDone, "Field stepMoveOutDetailsSubmittedDone doesn't have the expected value.");

            var retrievedSubmission = await DbProbe.TenancyDetailsSubmissions
                .Include(x => x.Address).Include(x => x.Address.Country).SingleOrDefaultAsync(x => x.UniqueId.Equals(submissionInfo.uniqueId));
            Assert.AreEqual(retrievedSubmission.Address.FullAddress(), submissionInfo.displayAddress, "Field displayAddress is not the expected.");
        }

        [Test]
        public async Task GetUserSubmissionSummary_SingleSubmissionAfterMoveOutWithJustCreatedVerificationTest()
        {
            var itemsLimit = 10;
            var justCreatedVerifications = 0;
            var sentVerifications = 1;
            var completeVerifications = 1;
            var areDetailsSubmitted = true;
            var hasMovedOut = true;

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", "11.12.13.14");

            var random = new RandomWrapper(2015);
            var submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress,
                    justCreatedVerifications, sentVerifications, completeVerifications, areDetailsSubmitted, hasMovedOut);

            var containerUnderTest = CreateContainer();
            SetupConfigForGetUserSubmissionSummary(containerUnderTest, itemsLimit);
            var serviceUnderTest = containerUnderTest.Get<ITenancyDetailsSubmissionService>();

            var response = await serviceUnderTest.GetUserSubmissionsSummary(user.Id, false);

            Assert.AreEqual(1, response.tenancyDetailsSubmissions.Count,
                "The response should contain a single submission.");

            var submissionInfo = response.tenancyDetailsSubmissions.Single();

            Assert.AreEqual(submission.UniqueId, submissionInfo.uniqueId,
                "Field uniqueId is not the expected.");
            Assert.IsTrue(submissionInfo.canEnterVerificationCode, "Field canEnterVerificationCode doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.canSubmitTenancyDetails, "Field canSubmitTenancyDetails doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.canSubmitMoveOutDetails, "Field canSubmitMoveOutDetails doesn't have the expected value.");

            Assert.IsTrue(submissionInfo.stepVerificationCodeSentOutDone, "Field stepVerificationCodeSentOutDone doesn't have the expected value.");
            Assert.IsTrue(submissionInfo.stepVerificationCodeEnteredDone, "Field stepVerificationCodeEnteredDone doesn't have the expected value.");
            Assert.IsTrue(submissionInfo.stepTenancyDetailsSubmittedDone, "Field stepTenancyDetailsSubmittedDone doesn't have the expected value.");
            Assert.IsTrue(submissionInfo.stepMoveOutDetailsSubmittedDone, "Field stepMoveOutDetailsSubmittedDone doesn't have the expected value.");

            var retrievedSubmission = await DbProbe.TenancyDetailsSubmissions
                .Include(x => x.Address).Include(x => x.Address.Country).SingleOrDefaultAsync(x => x.UniqueId.Equals(submissionInfo.uniqueId));
            Assert.AreEqual(retrievedSubmission.Address.FullAddress(), submissionInfo.displayAddress, "Field displayAddress is not the expected.");
        }

        [Test]
        public async Task GetUserSubmissionSummary_SingleSubmissionAfterMoveOutWithSentVerificationTest()
        {
            var itemsLimit = 10;
            var justCreatedVerifications = 0;
            var sentVerifications = 1;
            var completeVerifications = 1;
            var areDetailsSubmitted = true;
            var hasMovedOut = true;

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", "11.12.13.14");

            var random = new RandomWrapper(2015);
            var submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress,
                    justCreatedVerifications, sentVerifications, completeVerifications, areDetailsSubmitted, hasMovedOut);

            var containerUnderTest = CreateContainer();
            SetupConfigForGetUserSubmissionSummary(containerUnderTest, itemsLimit);
            var serviceUnderTest = containerUnderTest.Get<ITenancyDetailsSubmissionService>();

            var request = new MySubmissionsSummaryRequest { limitItemsReturned = false };
            var response = await serviceUnderTest.GetUserSubmissionsSummary(user.Id, false);

            Assert.AreEqual(1, response.tenancyDetailsSubmissions.Count,
                "The response should contain a single submission.");

            var submissionInfo = response.tenancyDetailsSubmissions.Single();

            Assert.AreEqual(submission.UniqueId, submissionInfo.uniqueId,
                "Field uniqueId is not the expected.");
            Assert.IsTrue(submissionInfo.canEnterVerificationCode, "Field canEnterVerificationCode doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.canSubmitTenancyDetails, "Field canSubmitTenancyDetails doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.canSubmitMoveOutDetails, "Field canSubmitMoveOutDetails doesn't have the expected value.");

            Assert.IsTrue(submissionInfo.stepVerificationCodeSentOutDone, "Field stepVerificationCodeSentOutDone doesn't have the expected value.");
            Assert.IsTrue(submissionInfo.stepVerificationCodeEnteredDone, "Field stepVerificationCodeEnteredDone doesn't have the expected value.");
            Assert.IsTrue(submissionInfo.stepTenancyDetailsSubmittedDone, "Field stepTenancyDetailsSubmittedDone doesn't have the expected value.");
            Assert.IsTrue(submissionInfo.stepMoveOutDetailsSubmittedDone, "Field stepMoveOutDetailsSubmittedDone doesn't have the expected value.");

            var retrievedSubmission = await DbProbe.TenancyDetailsSubmissions
                .Include(x => x.Address).Include(x => x.Address.Country).SingleOrDefaultAsync(x => x.UniqueId.Equals(submissionInfo.uniqueId));
            Assert.AreEqual(retrievedSubmission.Address.FullAddress(), submissionInfo.displayAddress, "Field displayAddress is not the expected.");
        }

        #endregion

        #region EnterVerificationCode

        [Test]
        public async Task EnterVerificationCode_ForSameSecretCodeAccrossDifferentSubmissions()
        {
            var helperContainer = CreateContainer();

            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "1.2.3.5";
            var otherUser = await CreateUser(helperContainer, "test2@test.com", otherUserIpAddress);

            var secretCode = "secret";
            var clock = helperContainer.Get<IClock>();
            var random = new RandomWrapper(2015);

            var otherUserSubmission1 = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, otherUser.Id, otherUserIpAddress, user.Id, userIpAddress,
                    justCreatedVerifications: 0, sentVerifications: 1, completeVerifications: 0, 
                    areDetailsSubmitted: false, hasMovedOut: false, secretCode: secretCode);
            var otherUserSubmission1verification = otherUserSubmission1.TenantVerifications.Single();

            var thisUserSubmission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress,
                    justCreatedVerifications: 0, sentVerifications: 1, completeVerifications: 0,
                    areDetailsSubmitted: false, hasMovedOut: false, secretCode: secretCode);
            var thisUserSubmissionVerification = thisUserSubmission.TenantVerifications.Single();

            var otherUserSubmission2 = await CreateTenancyDetailsSubmissionAndSave(
                random, helperContainer, otherUser.Id, otherUserIpAddress, user.Id, userIpAddress,
                justCreatedVerifications: 0, sentVerifications: 1, completeVerifications: 0,
                areDetailsSubmitted: false, hasMovedOut: false, secretCode: secretCode);
            var otherUserSubmission2verification = otherUserSubmission2.TenantVerifications.Single();

            Assert.IsNull(otherUserSubmission1verification.VerifiedOn,
                "otherUserSubmission1verification.VerifiedOn should be null before entering the code.");
            Assert.IsNull(otherUserSubmission2verification.VerifiedOn,
                "otherUserSubmission2verification.VerifiedOn should be null before entering the code.");
            Assert.IsNull(thisUserSubmissionVerification.VerifiedOn,
                "thisUserSubmissionVerification.VerifiedOn should be null before entering the code.");

            var containerUderTest = CreateContainer();
            var serviceUnderTest = containerUderTest.Get<ITenancyDetailsSubmissionService>();

            var form = new VerificationCodeForm
            {
                TenancyDetailsSubmissionUniqueId = thisUserSubmission.UniqueId,
                VerificationCode = secretCode
            };
            var timeBefore = clock.OffsetNow;
            var outcome = await serviceUnderTest.EnterVerificationCode(user.Id, form);

            Assert.IsFalse(outcome.IsRejected, "IsRejected field on the outcome should be false.");
            var timeAfter = clock.OffsetNow;

            var retrievedOtherUserSubmission1verification = await DbProbe.TenantVerifications
                .SingleAsync(x => x.UniqueId.Equals(otherUserSubmission1verification.UniqueId));
            var retrievedOtherUserSubmission2verification = await DbProbe.TenantVerifications
                .SingleAsync(x => x.UniqueId.Equals(otherUserSubmission2verification.UniqueId));
            var retrievedThisUserSubmissionVerification = await DbProbe.TenantVerifications
                .SingleAsync(x => x.UniqueId.Equals(thisUserSubmissionVerification.UniqueId));

            Assert.IsNull(retrievedOtherUserSubmission1verification.VerifiedOn,
                "retrievedOtherUserSubmission1verification.VerifiedOn should be null after entering the code.");
            Assert.IsNull(retrievedOtherUserSubmission2verification.VerifiedOn,
                "retrievedOtherUserSubmission2verification.VerifiedOn should be null after entering the code.");
            Assert.IsNotNull(retrievedThisUserSubmissionVerification.VerifiedOn,
                "retrievedThisUserSubmissionVerification.VerifiedOn should not be null after entering the code.");
            Assert.IsTrue(timeBefore <= retrievedThisUserSubmissionVerification.VerifiedOn.Value 
                && retrievedThisUserSubmissionVerification.VerifiedOn.Value <= timeAfter,
                "retrievedThisUserSubmissionVerification.VerifiedOn is not in the expected range after entering the code.");
        }

        #endregion

        #region Actions tests in different scenarios

        [Test]
        public async Task ActionsTest_NewSubmissionWithoutVerifications()
        {
            var justCreatedVerifications = 0;
            var sentVerifications = 0;
            var completeVerifications = 0;
            var areDetailsSubmitted = false;
            var hasMovedOut = false;

            var helperContainer = CreateContainer();

            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "1.2.3.5";
            var otherUser = await CreateUser(helperContainer, "test2@test.com", otherUserIpAddress);

            var random = new RandomWrapper(2015);
            var clock = helperContainer.Get<IClock>();

            var submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress,
                    justCreatedVerifications, sentVerifications, completeVerifications, areDetailsSubmitted, hasMovedOut);

            var containerUnderTest = CreateContainer();
            var serviceUnderTest = containerUnderTest.Get<ITenancyDetailsSubmissionService>();

            var retrievedSubmissionAtPoint1 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint1, "Retrieved submission at point 1 is null.");
            Assert.IsNull(retrievedSubmissionAtPoint1.RentPerMonth, "Field RentPerMonth on retrieved submission at point 1 is not the expected.");
            Assert.IsNull(retrievedSubmissionAtPoint1.MoveOutDate, "Field MoveOutDate on retrieved submission at point 1 is not the expected.");

            // I try all invalid actions
            // EnterVerificationCode
            var verificationCodeForm = new VerificationCodeForm
            {
                TenancyDetailsSubmissionUniqueId = submission.UniqueId,
                VerificationCode = "some-code"
            };
            var enterVerificationCodeOutcome = await serviceUnderTest.EnterVerificationCode(user.Id, verificationCodeForm);
            Assert.IsTrue(enterVerificationCodeOutcome.IsRejected, "EnterVerificationCode outcome field IsRejected is not the expected.");
            Assert.AreEqual(CommonResources.GenericInvalidActionMessage, enterVerificationCodeOutcome.RejectionReason,
                "EnterVerificationCode outcome field RejectionReason is not the expected.");
            Assert.IsFalse(enterVerificationCodeOutcome.ReturnToForm, "EnterVerificationCode outcome field ReturnToForm is not the expected.");

            var enterVerificationCodeOutcomeForOtherUser = await serviceUnderTest.EnterVerificationCode(otherUser.Id, verificationCodeForm);
            Assert.IsTrue(enterVerificationCodeOutcomeForOtherUser.IsRejected, 
                "EnterVerificationCode outcome field IsRejected is not the expected when wrong user is used.");
            Assert.AreEqual(CommonResources.GenericInvalidRequestMessage, enterVerificationCodeOutcomeForOtherUser.RejectionReason, 
                "EnterVerificationCode outcome field RejectionReason is not the expected when wrong user is used.");
            Assert.IsFalse(enterVerificationCodeOutcomeForOtherUser.ReturnToForm, 
                "EnterVerificationCode outcome field ReturnToForm is not the expected when wrong user is used.");

            // SubmitTenancyDetails
            var tenancyDetailsForm = new TenancyDetailsForm
            {
                TenancyDetailsSubmissionUniqueId = submission.UniqueId,
                IsPartOfProperty = true,
                RentPerMonth = 100,
                MoveInDate =  clock.OffsetNow.UtcDateTime.AddDays(-10.0),
                NumberOfBedrooms = 3

            };
            var submitTenancyDetailsOutcome = await serviceUnderTest.SubmitTenancyDetails(user.Id, tenancyDetailsForm);
            Assert.IsTrue(submitTenancyDetailsOutcome.IsRejected, "SubmitTenancyDetails outcome field IsRejected is not the expected.");
            Assert.AreEqual(CommonResources.GenericInvalidActionMessage, submitTenancyDetailsOutcome.RejectionReason, 
                "SubmitTenancyDetails outcome field RejectionReason is not the expected.");
            Assert.IsFalse(submitTenancyDetailsOutcome.ReturnToForm, "SubmitTenancyDetails outcome field ReturnToForm is not the expected.");

            var retrievedSubmissionAtPoint2 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint2, "Retrieved submission at point 2 is null.");
            Assert.IsNull(retrievedSubmissionAtPoint2.RentPerMonth, "Field RentPerMonth on retrieved submission at point 2 is not the expected.");

            var submitTenancyDetailsOutcomeForOtherUser = await serviceUnderTest.SubmitTenancyDetails(otherUser.Id, tenancyDetailsForm);
            Assert.IsTrue(submitTenancyDetailsOutcomeForOtherUser.IsRejected,
                "SubmitTenancyDetails outcome field IsRejected is not the expected when wrong user is used.");
            Assert.AreEqual(CommonResources.GenericInvalidRequestMessage, submitTenancyDetailsOutcomeForOtherUser.RejectionReason, 
                "SubmitTenancyDetails outcome field RejectionReason is not the expected when wrong user is used.");
            Assert.IsFalse(submitTenancyDetailsOutcomeForOtherUser.ReturnToForm,
                "SubmitTenancyDetails outcome field ReturnToForm is not the expected when wrong user is used.");

            var retrievedSubmissionAtPoint3 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint3, "Retrieved submission at point 3 is null.");
            Assert.IsNull(retrievedSubmissionAtPoint3.RentPerMonth, "Field RentPerMonth on retrieved submission at point 3 is not the expected.");

            // SubmitMoveOutDetails
            var moveOutDetailsForm = new MoveOutDetailsForm
            {
                TenancyDetailsSubmissionUniqueId = submission.UniqueId,
                MoveOutDate = clock.OffsetNow.UtcDateTime.AddDays(1.0)

            };
            var submitMoveOutDetailsOutcome = await serviceUnderTest.SubmitMoveOutDetails(user.Id, moveOutDetailsForm);
            Assert.IsTrue(submitMoveOutDetailsOutcome.IsRejected, "SubmitMoveOutDetails outcome field IsRejected is not the expected.");
            Assert.AreEqual(CommonResources.GenericInvalidActionMessage, submitMoveOutDetailsOutcome.RejectionReason, 
                "SubmitMoveOutDetails outcome field RejectionReason is not the expected.");
            Assert.IsFalse(submitMoveOutDetailsOutcome.ReturnToForm, "SubmitMoveOutDetails outcome field ReturnToForm is not the expected.");

            var retrievedSubmissionAtPoint4 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint4, "Retrieved submission at point 4 is null.");
            Assert.IsNull(retrievedSubmissionAtPoint4.MoveOutDate, "Field MoveOutDate on retrieved submission at point 4 is not the expected.");

            var submitMoveOutDetailsOutcomeForOtherUser = await serviceUnderTest.SubmitMoveOutDetails(otherUser.Id, moveOutDetailsForm);
            Assert.IsTrue(submitMoveOutDetailsOutcomeForOtherUser.IsRejected,
                "SubmitMoveOutDetails outcome field IsRejected is not the expected when wrong user is used.");
            Assert.AreEqual(CommonResources.GenericInvalidRequestMessage, submitMoveOutDetailsOutcomeForOtherUser.RejectionReason, 
                "SubmitMoveOutDetails outcome field RejectionReason is not the expected when wrong user is used.");
            Assert.IsFalse(submitMoveOutDetailsOutcomeForOtherUser.ReturnToForm,
                "SubmitMoveOutDetails outcome field ReturnToForm is not the expected when wrong user is used.");

            var retrievedSubmissionAtPoint5 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint5, "Retrieved submission at point 5 is null.");
            Assert.IsNull(retrievedSubmissionAtPoint5.MoveOutDate, "Field MoveOutDate on retrieved submission at point 5 is not the expected.");
        }

        [Test]
        public async Task ActionsTest_SubmissionWithJustCreatedVerifications()
        {
            var justCreatedVerifications = 2;
            var sentVerifications = 0;
            var completeVerifications = 0;
            var areDetailsSubmitted = false;
            var hasMovedOut = false;

            var helperContainer = CreateContainer();

            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "1.2.3.5";
            var otherUser = await CreateUser(helperContainer, "test2@test.com", otherUserIpAddress);

            var random = new RandomWrapper(2015);
            var clock = helperContainer.Get<IClock>();

            var submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress,
                    justCreatedVerifications, sentVerifications, completeVerifications, areDetailsSubmitted, hasMovedOut);

            var containerUnderTest = CreateContainer();
            var serviceUnderTest = containerUnderTest.Get<ITenancyDetailsSubmissionService>();

            var retrievedSubmissionAtPoint1 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint1, "Retrieved submission at point 1 is null.");
            Assert.IsNull(retrievedSubmissionAtPoint1.RentPerMonth, "Field RentPerMonth on retrieved submission at point 1 is not the expected.");
            Assert.IsNull(retrievedSubmissionAtPoint1.MoveOutDate, "Field MoveOutDate on retrieved submission at point 1 is not the expected.");
            Assert.IsTrue(retrievedSubmissionAtPoint1.TenantVerifications.All(x => !x.VerifiedOn.HasValue),
                "At point 1 all verifications should have null VerifiedOn field.");
            Assert.IsTrue(retrievedSubmissionAtPoint1.TenantVerifications.All(x => !x.MarkedAsSentOn.HasValue),
                "At point 1 all verifications should have null MarkedAsSentOn field.");

            // I try all invalid actions first
            // SubmitTenancyDetails
            var tenancyDetailsForm = new TenancyDetailsForm
            {
                TenancyDetailsSubmissionUniqueId = submission.UniqueId,
                IsPartOfProperty = true,
                RentPerMonth = 100,
                MoveInDate = clock.OffsetNow.UtcDateTime.AddDays(-10.0),
                NumberOfBedrooms = 3

            };
            var submitTenancyDetailsOutcome = await serviceUnderTest.SubmitTenancyDetails(user.Id, tenancyDetailsForm);
            Assert.IsTrue(submitTenancyDetailsOutcome.IsRejected, "SubmitTenancyDetails outcome field IsRejected is not the expected.");
            Assert.AreEqual(CommonResources.GenericInvalidActionMessage, submitTenancyDetailsOutcome.RejectionReason,
                "SubmitTenancyDetails outcome field RejectionReason is not the expected.");
            Assert.IsFalse(submitTenancyDetailsOutcome.ReturnToForm, "SubmitTenancyDetails outcome field ReturnToForm is not the expected.");

            var retrievedSubmissionAtPoint2 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint2, "Retrieved submission at point 2 is null.");
            Assert.IsNull(retrievedSubmissionAtPoint2.RentPerMonth, "Field RentPerMonth on retrieved submission at point 2 is not the expected.");

            var submitTenancyDetailsOutcomeForOtherUser = await serviceUnderTest.SubmitTenancyDetails(otherUser.Id, tenancyDetailsForm);
            Assert.IsTrue(submitTenancyDetailsOutcomeForOtherUser.IsRejected,
                "SubmitTenancyDetails outcome field IsRejected is not the expected when wrong user is used.");
            Assert.AreEqual(CommonResources.GenericInvalidRequestMessage, submitTenancyDetailsOutcomeForOtherUser.RejectionReason, 
                "SubmitTenancyDetails outcome field RejectionReason is not the expected when wrong user is used.");
            Assert.IsFalse(submitTenancyDetailsOutcomeForOtherUser.ReturnToForm,
                "SubmitTenancyDetails outcome field ReturnToForm is not the expected when wrong user is used.");

            var retrievedSubmissionAtPoint3 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint3, "Retrieved submission at point 3 is null.");
            Assert.IsNull(retrievedSubmissionAtPoint3.RentPerMonth, "Field RentPerMonth on retrieved submission at point 3 is not the expected.");

            // SubmitMoveOutDetails
            var moveOutDetailsForm = new MoveOutDetailsForm
            {
                TenancyDetailsSubmissionUniqueId = submission.UniqueId,
                MoveOutDate = clock.OffsetNow.UtcDateTime.AddDays(3.0)

            };
            var submitMoveOutDetailsOutcome = await serviceUnderTest.SubmitMoveOutDetails(user.Id, moveOutDetailsForm);
            Assert.IsTrue(submitMoveOutDetailsOutcome.IsRejected, "SubmitMoveOutDetails outcome field IsRejected is not the expected.");
            Assert.AreEqual(CommonResources.GenericInvalidActionMessage, submitMoveOutDetailsOutcome.RejectionReason, 
                "SubmitMoveOutDetails outcome field RejectionReason is not the expected.");
            Assert.IsFalse(submitMoveOutDetailsOutcome.ReturnToForm, "SubmitMoveOutDetails outcome field ReturnToForm is not the expected.");

            var retrievedSubmissionAtPoint4 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint4, "Retrieved submission at point 4 is null.");
            Assert.IsNull(retrievedSubmissionAtPoint4.MoveOutDate, "Field MoveOutDate on retrieved submission at point 4 is not the expected.");

            var submitMoveOutDetailsOutcomeForOtherUser = await serviceUnderTest.SubmitMoveOutDetails(otherUser.Id, moveOutDetailsForm);
            Assert.IsTrue(submitMoveOutDetailsOutcomeForOtherUser.IsRejected,
                "SubmitMoveOutDetails outcome field IsRejected is not the expected when wrong user is used.");
            Assert.AreEqual(CommonResources.GenericInvalidRequestMessage, submitMoveOutDetailsOutcomeForOtherUser.RejectionReason, 
                "SubmitMoveOutDetails outcome field RejectionReason is not the expected when wrong user is used.");
            Assert.IsFalse(submitMoveOutDetailsOutcomeForOtherUser.ReturnToForm,
                "SubmitMoveOutDetails outcome field ReturnToForm is not the expected when wrong user is used.");

            var retrievedSubmissionAtPoint5 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint5, "Retrieved submission at point 5 is null.");
            Assert.IsNull(retrievedSubmissionAtPoint5.MoveOutDate, "Field MoveOutDate on retrieved submission at point 5 is not the expected.");

            // I now try the valid actions
            // EnterVerificationCode
            var verificationToUse = submission.TenantVerifications.First();
            var verificationCodeForm = new VerificationCodeForm
            {
                TenancyDetailsSubmissionUniqueId = submission.UniqueId,
                VerificationCode = string.Format("\t {0} \t ", verificationToUse.SecretCode.ToLower()) // I add some whitespace and change case.
            };

            // I try first the other user
            var enterVerificationCodeOutcomeForOtherUser = await serviceUnderTest.EnterVerificationCode(otherUser.Id, verificationCodeForm);
            Assert.IsTrue(enterVerificationCodeOutcomeForOtherUser.IsRejected,
                "EnterVerificationCode outcome field IsRejected is not the expected when wrong user is used.");
            Assert.AreEqual(CommonResources.GenericInvalidRequestMessage, enterVerificationCodeOutcomeForOtherUser.RejectionReason,
                "EnterVerificationCode outcome field RejectionReason is not the expected when wrong user is used.");
            Assert.IsFalse(enterVerificationCodeOutcomeForOtherUser.ReturnToForm,
                "EnterVerificationCode outcome field ReturnToForm is not the expected when wrong user is used.");

            var retrievedSubmissionAtPoint6 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint6, "Retrieved submission at point 6 is null.");
            var retrievedUsedVerificationAtPoint6 = 
                retrievedSubmissionAtPoint6.TenantVerifications.Single(x => x.UniqueId == verificationToUse.UniqueId);
            Assert.IsNull(retrievedUsedVerificationAtPoint6.VerifiedOn,
                "Field VerifiedOn on retrieved used verification at point 6 is not the expected.");

            var timeBefore = clock.OffsetNow;
            var enterVerificationCodeOutcome = await serviceUnderTest.EnterVerificationCode(user.Id, verificationCodeForm);
            Assert.IsFalse(enterVerificationCodeOutcome.IsRejected, "EnterVerificationCode outcome field IsRejected is not the expected.");
            Assert.IsNullOrEmpty(enterVerificationCodeOutcome.RejectionReason);
            Assert.IsFalse(enterVerificationCodeOutcome.ReturnToForm, "EnterVerificationCode outcome field ReturnToForm is not the expected.");
            var timeAfter = clock.OffsetNow;

            var retrievedSubmissionAtPoint7 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint7, "Retrieved submission at point 7 is null.");
            var retrievedUsedVerificationAtPoint7 =
                retrievedSubmissionAtPoint7.TenantVerifications.Single(x => x.UniqueId == verificationToUse.UniqueId);
            Assert.IsTrue(retrievedUsedVerificationAtPoint7.VerifiedOn.HasValue,
                "Field VerifiedOn on used verification retrieved at point 7 does not have a value.");
            Assert.IsTrue(timeBefore <= retrievedUsedVerificationAtPoint7.VerifiedOn.Value && retrievedUsedVerificationAtPoint7.VerifiedOn.Value <= timeAfter,
                "Field VerifiedOn on used verification retrieved at point 7 is not in the expected range.");
        }

        [Test]
        public async Task ActionsTest_SubmissionWithSentAndCompleteVerifications()
        {
            var justCreatedVerifications = 0;
            var sentVerifications = 1;
            var completeVerifications = 1;
            var areDetailsSubmitted = false;
            var hasMovedOut = false;

            var helperContainer = CreateContainer();

            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "1.2.3.5";
            var otherUser = await CreateUser(helperContainer, "test2@test.com", otherUserIpAddress);

            var random = new RandomWrapper(2015);
            var clock = helperContainer.Get<IClock>();

            var submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress,
                    justCreatedVerifications, sentVerifications, completeVerifications, areDetailsSubmitted, hasMovedOut);

            var containerUnderTest = CreateContainer();
            var serviceUnderTest = containerUnderTest.Get<ITenancyDetailsSubmissionService>();

            var retrievedSubmissionAtPoint1 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint1, "Retrieved submission at point 1 is null.");
            Assert.IsNull(retrievedSubmissionAtPoint1.RentPerMonth, "Field RentPerMonth on retrieved submission at point 1 is not the expected.");
            Assert.IsNull(retrievedSubmissionAtPoint1.MoveOutDate, "Field MoveOutDate on retrieved submission at point 1 is not the expected.");
            Assert.IsTrue(retrievedSubmissionAtPoint1.TenantVerifications.Any(x => !x.VerifiedOn.HasValue),
                "At point 1 some verifications should have null VerifiedOn field.");
            Assert.IsTrue(retrievedSubmissionAtPoint1.TenantVerifications.Any(x => x.VerifiedOn.HasValue),
                "At point 1 some verifications should not have null VerifiedOn field.");
            Assert.IsTrue(retrievedSubmissionAtPoint1.TenantVerifications.All(x => x.MarkedAsSentOn.HasValue),
                "At point 1 all verifications should have a value in MarkedAsSentOn field.");

            // I try all invalid actions first
            // SubmitMoveOutDetails
            var moveOutDetailsForm = new MoveOutDetailsForm
            {
                TenancyDetailsSubmissionUniqueId = submission.UniqueId,
                MoveOutDate = clock.OffsetNow.UtcDateTime.AddDays(3.0)

            };
            var submitMoveOutDetailsOutcome = await serviceUnderTest.SubmitMoveOutDetails(user.Id, moveOutDetailsForm);
            Assert.IsTrue(submitMoveOutDetailsOutcome.IsRejected, "SubmitMoveOutDetails outcome field IsRejected is not the expected.");
            Assert.AreEqual(CommonResources.GenericInvalidActionMessage, submitMoveOutDetailsOutcome.RejectionReason,
                "SubmitMoveOutDetails outcome field RejectionReason is not the expected.");
            Assert.IsFalse(submitMoveOutDetailsOutcome.ReturnToForm, "SubmitMoveOutDetails outcome field ReturnToForm is not the expected.");

            var retrievedSubmissionAtPoint2 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint2, "Retrieved submission at point 2 is null.");
            Assert.IsNull(retrievedSubmissionAtPoint2.MoveOutDate, "Field MoveOutDate on retrieved submission at point 2 is not the expected.");

            var submitMoveOutDetailsOutcomeForOtherUser = await serviceUnderTest.SubmitMoveOutDetails(otherUser.Id, moveOutDetailsForm);
            Assert.IsTrue(submitMoveOutDetailsOutcomeForOtherUser.IsRejected,
                "SubmitMoveOutDetails outcome field IsRejected is not the expected when wrong user is used.");
            Assert.AreEqual(CommonResources.GenericInvalidRequestMessage, submitMoveOutDetailsOutcomeForOtherUser.RejectionReason,
                "SubmitMoveOutDetails outcome field RejectionReason is not the expected when wrong user is used.");
            Assert.IsFalse(submitMoveOutDetailsOutcomeForOtherUser.ReturnToForm,
                "SubmitMoveOutDetails outcome field ReturnToForm is not the expected when wrong user is used.");

            var retrievedSubmissionAtPoint3 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint3, "Retrieved submission at point 3 is null.");
            Assert.IsNull(retrievedSubmissionAtPoint3.MoveOutDate, "Field MoveOutDate on retrieved submission at point 3 is not the expected.");

            // I now try the valid actions
            // EnterVerificationCode
            var verificationToUse = submission.TenantVerifications.Single(x => !x.VerifiedOn.HasValue);
            var completeVerification = submission.TenantVerifications.Single(x => x.VerifiedOn.HasValue);

            // I try an invalid code
            var verificationCodeFormInvalidCode = new VerificationCodeForm
            {
                TenancyDetailsSubmissionUniqueId = submission.UniqueId,
                VerificationCode = "invalid-code"
            };

            var enterVerificationCodeOutcomeInvalidCode = await serviceUnderTest.EnterVerificationCode(user.Id, verificationCodeFormInvalidCode);
            Assert.IsTrue(enterVerificationCodeOutcomeInvalidCode.IsRejected,
                "EnterVerificationCode outcome field IsRejected is not the expected when invalid code is used.");
            Assert.AreEqual(TenancyDetailsSubmissionResources.EnterVerification_InvalidVerificationCode_RejectionMessage, enterVerificationCodeOutcomeInvalidCode.RejectionReason,
                "EnterVerificationCode outcome field RejectionReason is not the expected when invalid code is used.");
            Assert.IsTrue(enterVerificationCodeOutcomeInvalidCode.ReturnToForm,
                "EnterVerificationCode outcome field ReturnToForm is not the expected when invalid code is used.");

            var retrievedSubmissionAtPoint4 = await RetrieveSubmission(submission.UniqueId);
            var retrievedVerificationToUseAtPoint4 =
                retrievedSubmissionAtPoint4.TenantVerifications.Single(x => x.UniqueId.Equals(verificationToUse.UniqueId));
            Assert.IsNull(retrievedVerificationToUseAtPoint4.VerifiedOn,
                "Field VerifiedOn on retrieved used verification at point 4 is not the expected.");

            // I try the secret code of the complete verification

            var verificationCodeFormPreviouslyEnteredCode = new VerificationCodeForm
            {
                TenancyDetailsSubmissionUniqueId = submission.UniqueId,
                VerificationCode = completeVerification.SecretCode
            };

            var enterVerificationCodeOutcomePreviouslyEnteredCode = 
                await serviceUnderTest.EnterVerificationCode(user.Id, verificationCodeFormPreviouslyEnteredCode);
            Assert.IsTrue(enterVerificationCodeOutcomePreviouslyEnteredCode.IsRejected,
                "EnterVerificationCode outcome field IsRejected is not the expected when previously entered code is used.");
            Assert.AreEqual(TenancyDetailsSubmissionResources.EnterVerification_VerificationAlreadyUsed_RejectionMessage, enterVerificationCodeOutcomePreviouslyEnteredCode.RejectionReason,
                "EnterVerificationCode outcome field RejectionReason is not the expected when previously entered code is used.");
            Assert.IsFalse(enterVerificationCodeOutcomePreviouslyEnteredCode.ReturnToForm,
                "EnterVerificationCode outcome field ReturnToForm is not the expected when previously code is used.");

            var retrievedSubmissionAtPoint5 = await RetrieveSubmission(submission.UniqueId);
            var retrievedVerificationToUseAtPoint5 =
                retrievedSubmissionAtPoint5.TenantVerifications.Single(x => x.UniqueId.Equals(verificationToUse.UniqueId));
            Assert.IsNull(retrievedVerificationToUseAtPoint5.VerifiedOn,
                "Field VerifiedOn on retrieved verification to use at point 5 is not the expected.");
            var retrievedCompleteVerificationAtPoint5 =
                retrievedSubmissionAtPoint5.TenantVerifications.Single(x => x.UniqueId.Equals(completeVerification.UniqueId));
            Assert.AreEqual(completeVerification.VerifiedOn.Value, retrievedCompleteVerificationAtPoint5.VerifiedOn.Value,
                "The VerifiedOn field should not be updated when a previously entered code is used.");

            // This is the right form.
            var verificationCodeForm = new VerificationCodeForm
            {
                TenancyDetailsSubmissionUniqueId = submission.UniqueId,
                VerificationCode = verificationToUse.SecretCode
            };

            // I try the right code but with the other user
            var enterVerificationCodeOutcomeForOtherUser = await serviceUnderTest.EnterVerificationCode(otherUser.Id, verificationCodeForm);
            Assert.IsTrue(enterVerificationCodeOutcomeForOtherUser.IsRejected,
                "EnterVerificationCode outcome field IsRejected is not the expected when wrong user is used.");
            Assert.AreEqual(CommonResources.GenericInvalidRequestMessage, enterVerificationCodeOutcomeForOtherUser.RejectionReason,
                "EnterVerificationCode outcome field RejectionReason is not the expected when wrong user is used.");
            Assert.IsFalse(enterVerificationCodeOutcomeForOtherUser.ReturnToForm,
                "EnterVerificationCode outcome field ReturnToForm is not the expected when wrong user is used.");

            var retrievedSubmissionAtPoint6 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint6, "Retrieved submission at point 6 is null.");
            var retrievedUsedVerificationAtPoint6 =
                retrievedSubmissionAtPoint6.TenantVerifications.Single(x => x.UniqueId == verificationToUse.UniqueId);
            Assert.IsNull(retrievedUsedVerificationAtPoint6.VerifiedOn,
                "Field VerifiedOn on retrieved used verification at point 6 is not the expected.");

            var timeBeforeEnterVerificationCode = clock.OffsetNow;
            var enterVerificationCodeOutcome = await serviceUnderTest.EnterVerificationCode(user.Id, verificationCodeForm);
            Assert.IsFalse(enterVerificationCodeOutcome.IsRejected, "EnterVerificationCode outcome field IsRejected is not the expected.");
            Assert.IsNullOrEmpty(enterVerificationCodeOutcome.RejectionReason);
            Assert.IsFalse(enterVerificationCodeOutcome.ReturnToForm, "EnterVerificationCode outcome field ReturnToForm is not the expected.");
            var timeAfterEnterVerificationCode = clock.OffsetNow;

            var retrievedSubmissionAtPoint7 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint7, "Retrieved submission at point 9 is null.");
            var retrievedUsedVerificationAtPoint7 =
                retrievedSubmissionAtPoint7.TenantVerifications.Single(x => x.UniqueId == verificationToUse.UniqueId);
            Assert.IsTrue(retrievedUsedVerificationAtPoint7.VerifiedOn.HasValue,
                "Field VerifiedOn on used verification retrieved at point 7 does not have a value.");
            Assert.IsTrue(timeBeforeEnterVerificationCode <= retrievedUsedVerificationAtPoint7.VerifiedOn.Value 
                && retrievedUsedVerificationAtPoint7.VerifiedOn.Value <= timeAfterEnterVerificationCode,
                "Field VerifiedOn on used verification retrieved at point 7 is not in the expected range.");

            // SubmitTenancyDetails
            var tenancyDetailsForm = new TenancyDetailsForm
            {
                TenancyDetailsSubmissionUniqueId = submission.UniqueId,
                IsPartOfProperty = true,
                RentPerMonth = 100,
                MoveInDate = clock.OffsetNow.UtcDateTime.AddDays(-10.0).Date,
                NumberOfBedrooms = 3

            };

            // I try the wrong user.
            var submitTenancyDetailsOutcomeForOtherUser = await serviceUnderTest.SubmitTenancyDetails(otherUser.Id, tenancyDetailsForm);
            Assert.IsTrue(submitTenancyDetailsOutcomeForOtherUser.IsRejected,
                "SubmitTenancyDetails outcome field IsRejected is not the expected when wrong user is used.");
            Assert.AreEqual(CommonResources.GenericInvalidRequestMessage, submitTenancyDetailsOutcomeForOtherUser.RejectionReason,
                "SubmitTenancyDetails outcome field RejectionReason is not the expected when wrong user is used.");
            Assert.IsFalse(submitTenancyDetailsOutcomeForOtherUser.ReturnToForm,
                "SubmitTenancyDetails outcome field ReturnToForm is not the expected when wrong user is used.");

            var retrievedSubmissionAtPoint8 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint8, "Retrieved submission at point 8 is null.");
            Assert.IsNull(retrievedSubmissionAtPoint8.RentPerMonth, "Field RentPerMonth on retrieved submission at point 8 is not the expected.");
            Assert.IsNull(retrievedSubmissionAtPoint8.SubmittedOn, "Field SubmittedOn on retrieved submission at point 8 is not the expected.");

            // I try the right user.
            var timeBeforeSubmitTenancyDetails = clock.OffsetNow;
            var submitTenancyDetailsOutcome = await serviceUnderTest.SubmitTenancyDetails(user.Id, tenancyDetailsForm);
            Assert.IsFalse(submitTenancyDetailsOutcome.IsRejected, "SubmitTenancyDetails outcome field IsRejected is not the expected.");
            Assert.IsNullOrEmpty(submitTenancyDetailsOutcome.RejectionReason,
                "SubmitTenancyDetails outcome field RejectionReason is not the expected.");
            Assert.IsFalse(submitTenancyDetailsOutcome.ReturnToForm, "SubmitTenancyDetails outcome field ReturnToForm is not the expected.");
            var timeAfterSubmitTenancyDetails = clock.OffsetNow;

            var retrievedSubmissionAtPoint9 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint3, "Retrieved submission at point 9 is null.");
            Assert.AreEqual(tenancyDetailsForm.IsPartOfProperty, retrievedSubmissionAtPoint9.IsPartOfProperty, 
                "Field IsPartOfProperty on retrieved submission at point 9 is not the expected.");
            Assert.AreEqual(tenancyDetailsForm.RentPerMonth, retrievedSubmissionAtPoint9.RentPerMonth,
                "Field Rent on retrieved submission at point 9 is not the expected.");
            Assert.AreEqual(tenancyDetailsForm.MoveInDate, retrievedSubmissionAtPoint9.MoveInDate,
                "Field MoveInDate on retrieved submission at point 9 is not the expected.");
            Assert.AreEqual(tenancyDetailsForm.NumberOfBedrooms, retrievedSubmissionAtPoint9.NumberOfBedrooms,
                "Field NumberOfBedrooms on retrieved submission at point 9 is not the expected.");
            Assert.IsTrue(retrievedSubmissionAtPoint9.SubmittedOn.HasValue,
                "Field SubmittedOn on retrieved submission at point 9 should have a value.");
            Assert.IsTrue(timeBeforeSubmitTenancyDetails <= retrievedSubmissionAtPoint9.SubmittedOn.Value
                && retrievedSubmissionAtPoint9.SubmittedOn.Value <= timeAfterSubmitTenancyDetails,
                "Field SubmittedOn on retrieved submission at point 9 is not within the expected range.");
        }

        [Test]
        public async Task ActionsTest_SubmissionWithSentAndCompleteVerificationsAndSubmittedDetails()
        {
            var justCreatedVerifications = 0;
            var sentVerifications = 1;
            var completeVerifications = 1;
            var areDetailsSubmitted = true;
            var hasMovedOut = false;

            var helperContainer = CreateContainer();

            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "1.2.3.5";
            var otherUser = await CreateUser(helperContainer, "test2@test.com", otherUserIpAddress);

            var random = new RandomWrapper(2015);
            var clock = helperContainer.Get<IClock>();

            var submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress,
                    justCreatedVerifications, sentVerifications, completeVerifications, areDetailsSubmitted, hasMovedOut);

            var containerUnderTest = CreateContainer();
            var serviceUnderTest = containerUnderTest.Get<ITenancyDetailsSubmissionService>();

            var retrievedSubmissionAtPoint1 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint1, "Retrieved submission at point 1 is null.");
            Assert.IsNotNull(retrievedSubmissionAtPoint1.SubmittedOn, "Field SubmittedOn on retrieved submission at point 1 is not the expected.");
            Assert.IsNotNull(retrievedSubmissionAtPoint1.RentPerMonth, "Field RentPerMonth on retrieved submission at point 1 is not the expected.");
            Assert.IsNull(retrievedSubmissionAtPoint1.MoveOutDate, "Field MoveOutDate on retrieved submission at point 1 is not the expected.");
            Assert.IsTrue(retrievedSubmissionAtPoint1.TenantVerifications.Any(x => !x.VerifiedOn.HasValue),
                "At point 1 some verifications should have null VerifiedOn field.");
            Assert.IsTrue(retrievedSubmissionAtPoint1.TenantVerifications.Any(x => x.VerifiedOn.HasValue),
                "At point 1 some verifications should not have null VerifiedOn field.");
            Assert.IsTrue(retrievedSubmissionAtPoint1.TenantVerifications.All(x => x.MarkedAsSentOn.HasValue),
                "At point 1 all verifications should have a value in MarkedAsSentOn field.");

            // SubmitTenancyDetails
            var tenancyDetailsForm = new TenancyDetailsForm
            {
                TenancyDetailsSubmissionUniqueId = submission.UniqueId,
                IsPartOfProperty = !submission.IsPartOfProperty.Value,
                RentPerMonth = submission.RentPerMonth.Value + 1,
                MoveInDate = submission.MoveInDate.Value.AddDays(3.0),
                NumberOfBedrooms = submission.NumberOfBedrooms.Value + 1

            };

            var submitTenancyDetailsOutcome = await serviceUnderTest.SubmitTenancyDetails(user.Id, tenancyDetailsForm);
            Assert.IsTrue(submitTenancyDetailsOutcome.IsRejected, "SubmitTenancyDetails outcome field IsRejected is not the expected.");
            Assert.AreEqual(CommonResources.GenericInvalidActionMessage, submitTenancyDetailsOutcome.RejectionReason,
                "SubmitTenancyDetails outcome field RejectionReason is not the expected.");
            Assert.IsFalse(submitTenancyDetailsOutcome.ReturnToForm, "SubmitTenancyDetails outcome field ReturnToForm is not the expected.");

            var retrievedSubmissionAtPoint2 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint2, "Retrieved submission at point 2 is null.");
            Assert.AreEqual(submission.IsPartOfProperty, retrievedSubmissionAtPoint2.IsPartOfProperty, 
                "Field IsPartOfProperty on retrieved submission at point 2 is not the expected.");
            Assert.AreEqual(submission.RentPerMonth, retrievedSubmissionAtPoint2.RentPerMonth, 
                "Field Rent on retrieved submission at point 2 is not the expected.");
            Assert.AreEqual(submission.MoveInDate, retrievedSubmissionAtPoint2.MoveInDate, 
                "Field MoveInDate on retrieved submission at point 2 is not the expected.");
            Assert.AreEqual(submission.NumberOfBedrooms, retrievedSubmissionAtPoint2.NumberOfBedrooms, 
                "Field NumberOfBedrooms on retrieved submission at point 2 is not the expected.");
            Assert.IsTrue(retrievedSubmissionAtPoint2.SubmittedOn.HasValue,
                "Field SubmittedOn on retrieved submission at point 2 should have a value.");
            Assert.AreEqual(submission.SubmittedOn, retrievedSubmissionAtPoint2.SubmittedOn,
                "Field SubmittedOn on retrieved submission at point 9 should not be updated.");

            // EnterVerificationCode
            var verificationToUse = submission.TenantVerifications.Single(x => !x.VerifiedOn.HasValue);
            var completeVerification = submission.TenantVerifications.Single(x => x.VerifiedOn.HasValue);

            // I try an invalid code
            var verificationCodeFormInvalidCode = new VerificationCodeForm
            {
                TenancyDetailsSubmissionUniqueId = submission.UniqueId,
                VerificationCode = "invalid-code"
            };

            var enterVerificationCodeOutcomeInvalidCode = await serviceUnderTest.EnterVerificationCode(user.Id, verificationCodeFormInvalidCode);
            Assert.IsTrue(enterVerificationCodeOutcomeInvalidCode.IsRejected,
                "EnterVerificationCode outcome field IsRejected is not the expected when invalid code is used.");
            Assert.AreEqual(TenancyDetailsSubmissionResources.EnterVerification_InvalidVerificationCode_RejectionMessage, enterVerificationCodeOutcomeInvalidCode.RejectionReason,
                "EnterVerificationCode outcome field RejectionReason is not the expected when invalid code is used.");
            Assert.IsTrue(enterVerificationCodeOutcomeInvalidCode.ReturnToForm,
                "EnterVerificationCode outcome field ReturnToForm is not the expected when invalid code is used.");

            var retrievedSubmissionAtPoint3 = await RetrieveSubmission(submission.UniqueId);
            var retrievedVerificationToUseAtPoint3 =
                retrievedSubmissionAtPoint3.TenantVerifications.Single(x => x.UniqueId.Equals(verificationToUse.UniqueId));
            Assert.IsNull(retrievedVerificationToUseAtPoint3.VerifiedOn,
                "Field VerifiedOn on retrieved used verification at point 3 is not the expected.");

            // I try the secret code of the complete verification

            var verificationCodeFormPreviouslyEnteredCode = new VerificationCodeForm
            {
                TenancyDetailsSubmissionUniqueId = submission.UniqueId,
                VerificationCode = completeVerification.SecretCode
            };

            var enterVerificationCodeOutcomePreviouslyEnteredCode =
                await serviceUnderTest.EnterVerificationCode(user.Id, verificationCodeFormPreviouslyEnteredCode);
            Assert.IsTrue(enterVerificationCodeOutcomePreviouslyEnteredCode.IsRejected,
                "EnterVerificationCode outcome field IsRejected is not the expected when previously entered code is used.");
            Assert.AreEqual(TenancyDetailsSubmissionResources.EnterVerification_VerificationAlreadyUsed_RejectionMessage, enterVerificationCodeOutcomePreviouslyEnteredCode.RejectionReason,
                "EnterVerificationCode outcome field RejectionReason is not the expected when previously entered code is used.");
            Assert.IsFalse(enterVerificationCodeOutcomePreviouslyEnteredCode.ReturnToForm,
                "EnterVerificationCode outcome field ReturnToForm is not the expected when previously code is used.");

            var retrievedSubmissionAtPoint4 = await RetrieveSubmission(submission.UniqueId);
            var retrievedVerificationToUseAtPoint4 =
                retrievedSubmissionAtPoint4.TenantVerifications.Single(x => x.UniqueId.Equals(verificationToUse.UniqueId));
            Assert.IsNull(retrievedVerificationToUseAtPoint4.VerifiedOn,
                "Field VerifiedOn on retrieved verification to use at point4 is not the expected.");
            var retrievedCompleteVerificationAtPoint4 =
                retrievedSubmissionAtPoint4.TenantVerifications.Single(x => x.UniqueId.Equals(completeVerification.UniqueId));
            Assert.AreEqual(completeVerification.VerifiedOn.Value, retrievedCompleteVerificationAtPoint4.VerifiedOn.Value,
                "The VerifiedOn field should not be updated when a previously entered code is used.");

            // This is the right form.
            var verificationCodeForm = new VerificationCodeForm
            {
                TenancyDetailsSubmissionUniqueId = submission.UniqueId,
                VerificationCode = verificationToUse.SecretCode
            };

            // I try the right code but with the other user
            var enterVerificationCodeOutcomeForOtherUser = await serviceUnderTest.EnterVerificationCode(otherUser.Id, verificationCodeForm);
            Assert.IsTrue(enterVerificationCodeOutcomeForOtherUser.IsRejected,
                "EnterVerificationCode outcome field IsRejected is not the expected when wrong user is used.");
            Assert.AreEqual(CommonResources.GenericInvalidRequestMessage, enterVerificationCodeOutcomeForOtherUser.RejectionReason,
                "EnterVerificationCode outcome field RejectionReason is not the expected when wrong user is used.");
            Assert.IsFalse(enterVerificationCodeOutcomeForOtherUser.ReturnToForm,
                "EnterVerificationCode outcome field ReturnToForm is not the expected when wrong user is used.");

            var retrievedSubmissionAtPoint5 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint5, "Retrieved submission at point 5 is null.");
            var retrievedUsedVerificationAtPoint5 =
                retrievedSubmissionAtPoint5.TenantVerifications.Single(x => x.UniqueId == verificationToUse.UniqueId);
            Assert.IsNull(retrievedUsedVerificationAtPoint5.VerifiedOn,
                "Field VerifiedOn on retrieved used verification at point 5 is not the expected.");

            var timeBeforeEnterVerificationCode = clock.OffsetNow;
            var enterVerificationCodeOutcome = await serviceUnderTest.EnterVerificationCode(user.Id, verificationCodeForm);
            Assert.IsFalse(enterVerificationCodeOutcome.IsRejected, "EnterVerificationCode outcome field IsRejected is not the expected.");
            Assert.IsNullOrEmpty(enterVerificationCodeOutcome.RejectionReason);
            Assert.IsFalse(enterVerificationCodeOutcome.ReturnToForm, "EnterVerificationCode outcome field ReturnToForm is not the expected.");
            var timeAfterEnterVerificationCode = clock.OffsetNow;

            var retrievedSubmissionAtPoint6 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint6, "Retrieved submission at point 6 is null.");
            var retrievedUsedVerificationAtPoint6 =
                retrievedSubmissionAtPoint6.TenantVerifications.Single(x => x.UniqueId == verificationToUse.UniqueId);
            Assert.IsTrue(retrievedUsedVerificationAtPoint6.VerifiedOn.HasValue,
                "Field VerifiedOn on used verification retrieved at point 6 does not have a value.");
            Assert.IsTrue(timeBeforeEnterVerificationCode <= retrievedUsedVerificationAtPoint6.VerifiedOn.Value
                && retrievedUsedVerificationAtPoint6.VerifiedOn.Value <= timeAfterEnterVerificationCode,
                "Field VerifiedOn on used verification retrieved at point 6 is not in the expected range.");

            // SubmitMoveOutDetails
            var moveOutDetailsForm = new MoveOutDetailsForm
            {
                TenancyDetailsSubmissionUniqueId = submission.UniqueId,
                MoveOutDate = clock.OffsetNow.UtcDateTime.AddDays(3.0).Date
            };

            // I try the wrong user
            var submitMoveOutDetailsOutcomeForOtherUser = await serviceUnderTest.SubmitMoveOutDetails(otherUser.Id, moveOutDetailsForm);
            Assert.IsTrue(submitMoveOutDetailsOutcomeForOtherUser.IsRejected,
                "SubmitMoveOutDetails outcome field IsRejected is not the expected when wrong user is used.");
            Assert.AreEqual(CommonResources.GenericInvalidRequestMessage, submitMoveOutDetailsOutcomeForOtherUser.RejectionReason,
                "SubmitMoveOutDetails outcome field RejectionReason is not the expected when wrong user is used.");
            Assert.IsFalse(submitMoveOutDetailsOutcomeForOtherUser.ReturnToForm,
                "SubmitMoveOutDetails outcome field ReturnToForm is not the expected when wrong user is used.");

            var retrievedSubmissionAtPoint7 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint7, "Retrieved submission at point 7 is null.");
            Assert.IsNull(retrievedSubmissionAtPoint7.MoveOutDate, "Field MoveOutDate on retrieved submission at point 7 is not the expected.");
            Assert.IsNull(retrievedSubmissionAtPoint7.MoveOutDateSubmittedOn, "Field MoveOutDateSubmittedOn on retrieved submission at point 7 is not the expected.");

            // I try the right user
            var timeBeforeSubmitMoveOutDetails = clock.OffsetNow;
            var submitMoveOutDetailsOutcome = await serviceUnderTest.SubmitMoveOutDetails(user.Id, moveOutDetailsForm);
            Assert.IsFalse(submitMoveOutDetailsOutcome.IsRejected, "SubmitMoveOutDetails outcome field IsRejected is not the expected.");
            Assert.IsNullOrEmpty(submitMoveOutDetailsOutcome.RejectionReason,
                "SubmitMoveOutDetails outcome field RejectionReason is not the expected.");
            Assert.IsFalse(submitMoveOutDetailsOutcome.ReturnToForm, "SubmitMoveOutDetails outcome field ReturnToForm is not the expected.");
            var timeAfterSubmitMoveOutDetails = clock.OffsetNow;

            var retrievedSubmissionAtPoint8 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint8, "Retrieved submission at point 8 is null.");
            Assert.IsNotNull(retrievedSubmissionAtPoint8.MoveOutDate, "Field MoveOutDate on retrieved submission at point 8 is null.");
            Assert.AreEqual(moveOutDetailsForm.MoveOutDate, retrievedSubmissionAtPoint8.MoveOutDate, 
                "Field MoveOutDate on retrieved submission at point 8 is not the expected.");
            Assert.IsTrue(timeBeforeSubmitMoveOutDetails <= retrievedSubmissionAtPoint8.MoveOutDateSubmittedOn.Value
                && retrievedSubmissionAtPoint8.MoveOutDateSubmittedOn.Value <= timeAfterSubmitMoveOutDetails,
                "Field MoveOutDateSubmittedOn on retrieved submission at point 8 is not within the expected range.");
        }

        [Test]
        public async Task ActionsTest_SubmissionWithSentAndCompleteVerificationsAndMoveOutDetailsSubmitted()
        {
            var justCreatedVerifications = 0;
            var sentVerifications = 1;
            var completeVerifications = 1;
            var areDetailsSubmitted = true;
            var hasMovedOut = true;

            var helperContainer = CreateContainer();

            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "1.2.3.5";
            var otherUser = await CreateUser(helperContainer, "test2@test.com", otherUserIpAddress);

            var random = new RandomWrapper(2015);
            var clock = helperContainer.Get<IClock>();

            var submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress,
                    justCreatedVerifications, sentVerifications, completeVerifications, areDetailsSubmitted, hasMovedOut);

            var containerUnderTest = CreateContainer();
            var serviceUnderTest = containerUnderTest.Get<ITenancyDetailsSubmissionService>();

            var retrievedSubmissionAtPoint1 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint1, "Retrieved submission at point 1 is null.");
            Assert.IsNotNull(retrievedSubmissionAtPoint1.SubmittedOn, "Field SubmittedOn on retrieved submission at point 1 is not the expected.");
            Assert.IsNotNull(retrievedSubmissionAtPoint1.RentPerMonth, "Field RentPerMonth on retrieved submission at point 1 is not the expected.");
            Assert.IsNotNull(retrievedSubmissionAtPoint1.MoveOutDate, "Field MoveOutDate on retrieved submission at point 1 is not the expected.");
            Assert.IsTrue(retrievedSubmissionAtPoint1.TenantVerifications.Any(x => !x.VerifiedOn.HasValue),
                "At point 1 some verifications should have null VerifiedOn field.");
            Assert.IsTrue(retrievedSubmissionAtPoint1.TenantVerifications.Any(x => x.VerifiedOn.HasValue),
                "At point 1 some verifications should not have null VerifiedOn field.");
            Assert.IsTrue(retrievedSubmissionAtPoint1.TenantVerifications.All(x => x.MarkedAsSentOn.HasValue),
                "At point 1 all verifications should have a value in MarkedAsSentOn field.");

            // SubmitTenancyDetails
            var tenancyDetailsForm = new TenancyDetailsForm
            {
                TenancyDetailsSubmissionUniqueId = submission.UniqueId,
                IsPartOfProperty = !submission.IsPartOfProperty.Value,
                RentPerMonth = submission.RentPerMonth.Value + 1,
                MoveInDate = submission.MoveInDate.Value.AddDays(3.0),
                NumberOfBedrooms = submission.NumberOfBedrooms.Value + 1

            };

            var submitTenancyDetailsOutcome = await serviceUnderTest.SubmitTenancyDetails(user.Id, tenancyDetailsForm);
            Assert.IsTrue(submitTenancyDetailsOutcome.IsRejected, "SubmitTenancyDetails outcome field IsRejected is not the expected.");
            Assert.AreEqual(CommonResources.GenericInvalidActionMessage, submitTenancyDetailsOutcome.RejectionReason,
                "SubmitTenancyDetails outcome field RejectionReason is not the expected.");
            Assert.IsFalse(submitTenancyDetailsOutcome.ReturnToForm, "SubmitTenancyDetails outcome field ReturnToForm is not the expected.");

            var retrievedSubmissionAtPoint2 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint2, "Retrieved submission at point 2 is null.");
            Assert.AreEqual(submission.IsPartOfProperty, retrievedSubmissionAtPoint2.IsPartOfProperty,
                "Field IsPartOfProperty on retrieved submission at point 2 is not the expected.");
            Assert.AreEqual(submission.RentPerMonth, retrievedSubmissionAtPoint2.RentPerMonth,
                "Field RentPerMonth on retrieved submission at point 2 is not the expected.");
            Assert.AreEqual(submission.MoveInDate, retrievedSubmissionAtPoint2.MoveInDate,
                "Field MoveInDate on retrieved submission at point 2 is not the expected.");
            Assert.AreEqual(submission.NumberOfBedrooms, retrievedSubmissionAtPoint2.NumberOfBedrooms,
                "Field NumberOfBedrooms on retrieved submission at point 2 is not the expected.");
            Assert.IsTrue(retrievedSubmissionAtPoint2.SubmittedOn.HasValue,
                "Field SubmittedOn on retrieved submission at point 2 should have a value.");
            Assert.AreEqual(submission.SubmittedOn, retrievedSubmissionAtPoint2.SubmittedOn,
                "Field SubmittedOn on retrieved submission at point 9 should not be updated.");

            // EnterVerificationCode
            var verificationToUse = submission.TenantVerifications.Single(x => !x.VerifiedOn.HasValue);
            var completeVerification = submission.TenantVerifications.Single(x => x.VerifiedOn.HasValue);

            // I try an invalid code
            var verificationCodeFormInvalidCode = new VerificationCodeForm
            {
                TenancyDetailsSubmissionUniqueId = submission.UniqueId,
                VerificationCode = "invalid-code"
            };

            var enterVerificationCodeOutcomeInvalidCode = await serviceUnderTest.EnterVerificationCode(user.Id, verificationCodeFormInvalidCode);
            Assert.IsTrue(enterVerificationCodeOutcomeInvalidCode.IsRejected,
                "EnterVerificationCode outcome field IsRejected is not the expected when invalid code is used.");
            Assert.AreEqual(TenancyDetailsSubmissionResources.EnterVerification_InvalidVerificationCode_RejectionMessage, enterVerificationCodeOutcomeInvalidCode.RejectionReason,
                "EnterVerificationCode outcome field RejectionReason is not the expected when invalid code is used.");
            Assert.IsTrue(enterVerificationCodeOutcomeInvalidCode.ReturnToForm,
                "EnterVerificationCode outcome field ReturnToForm is not the expected when invalid code is used.");

            var retrievedSubmissionAtPoint3 = await RetrieveSubmission(submission.UniqueId);
            var retrievedVerificationToUseAtPoint3 =
                retrievedSubmissionAtPoint3.TenantVerifications.Single(x => x.UniqueId.Equals(verificationToUse.UniqueId));
            Assert.IsNull(retrievedVerificationToUseAtPoint3.VerifiedOn,
                "Field VerifiedOn on retrieved used verification at point 3 is not the expected.");

            // I try the secret code of the complete verification

            var verificationCodeFormPreviouslyEnteredCode = new VerificationCodeForm
            {
                TenancyDetailsSubmissionUniqueId = submission.UniqueId,
                VerificationCode = completeVerification.SecretCode
            };

            var enterVerificationCodeOutcomePreviouslyEnteredCode =
                await serviceUnderTest.EnterVerificationCode(user.Id, verificationCodeFormPreviouslyEnteredCode);
            Assert.IsTrue(enterVerificationCodeOutcomePreviouslyEnteredCode.IsRejected,
                "EnterVerificationCode outcome field IsRejected is not the expected when previously entered code is used.");
            Assert.AreEqual(TenancyDetailsSubmissionResources.EnterVerification_VerificationAlreadyUsed_RejectionMessage, enterVerificationCodeOutcomePreviouslyEnteredCode.RejectionReason,
                "EnterVerificationCode outcome field RejectionReason is not the expected when previously entered code is used.");
            Assert.IsFalse(enterVerificationCodeOutcomePreviouslyEnteredCode.ReturnToForm,
                "EnterVerificationCode outcome field ReturnToForm is not the expected when previously code is used.");

            var retrievedSubmissionAtPoint4 = await RetrieveSubmission(submission.UniqueId);
            var retrievedVerificationToUseAtPoint4 =
                retrievedSubmissionAtPoint4.TenantVerifications.Single(x => x.UniqueId.Equals(verificationToUse.UniqueId));
            Assert.IsNull(retrievedVerificationToUseAtPoint4.VerifiedOn,
                "Field VerifiedOn on retrieved verification to use at point4 is not the expected.");
            var retrievedCompleteVerificationAtPoint4 =
                retrievedSubmissionAtPoint4.TenantVerifications.Single(x => x.UniqueId.Equals(completeVerification.UniqueId));
            Assert.AreEqual(completeVerification.VerifiedOn.Value, retrievedCompleteVerificationAtPoint4.VerifiedOn.Value,
                "The VerifiedOn field should not be updated when a previously entered code is used.");

            // This is the right form.
            var verificationCodeForm = new VerificationCodeForm
            {
                TenancyDetailsSubmissionUniqueId = submission.UniqueId,
                VerificationCode = verificationToUse.SecretCode
            };

            // I try the right code but with the other user
            var enterVerificationCodeOutcomeForOtherUser = await serviceUnderTest.EnterVerificationCode(otherUser.Id, verificationCodeForm);
            Assert.IsTrue(enterVerificationCodeOutcomeForOtherUser.IsRejected,
                "EnterVerificationCode outcome field IsRejected is not the expected when wrong user is used.");
            Assert.AreEqual(CommonResources.GenericInvalidRequestMessage, enterVerificationCodeOutcomeForOtherUser.RejectionReason,
                "EnterVerificationCode outcome field RejectionReason is not the expected when wrong user is used.");
            Assert.IsFalse(enterVerificationCodeOutcomeForOtherUser.ReturnToForm,
                "EnterVerificationCode outcome field ReturnToForm is not the expected when wrong user is used.");

            var retrievedSubmissionAtPoint5 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint5, "Retrieved submission at point 5 is null.");
            var retrievedUsedVerificationAtPoint5 =
                retrievedSubmissionAtPoint5.TenantVerifications.Single(x => x.UniqueId == verificationToUse.UniqueId);
            Assert.IsNull(retrievedUsedVerificationAtPoint5.VerifiedOn,
                "Field VerifiedOn on retrieved used verification at point 5 is not the expected.");

            var timeBeforeEnterVerificationCode = clock.OffsetNow;
            var enterVerificationCodeOutcome = await serviceUnderTest.EnterVerificationCode(user.Id, verificationCodeForm);
            Assert.IsFalse(enterVerificationCodeOutcome.IsRejected, "EnterVerificationCode outcome field IsRejected is not the expected.");
            Assert.IsNullOrEmpty(enterVerificationCodeOutcome.RejectionReason);
            Assert.IsFalse(enterVerificationCodeOutcome.ReturnToForm, "EnterVerificationCode outcome field ReturnToForm is not the expected.");
            var timeAfterEnterVerificationCode = clock.OffsetNow;

            var retrievedSubmissionAtPoint6 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint6, "Retrieved submission at point 6 is null.");
            var retrievedUsedVerificationAtPoint6 =
                retrievedSubmissionAtPoint6.TenantVerifications.Single(x => x.UniqueId == verificationToUse.UniqueId);
            Assert.IsTrue(retrievedUsedVerificationAtPoint6.VerifiedOn.HasValue,
                "Field VerifiedOn on used verification retrieved at point 6 does not have a value.");
            Assert.IsTrue(timeBeforeEnterVerificationCode <= retrievedUsedVerificationAtPoint6.VerifiedOn.Value
                && retrievedUsedVerificationAtPoint6.VerifiedOn.Value <= timeAfterEnterVerificationCode,
                "Field VerifiedOn on used verification retrieved at point 6 is not in the expected range.");

            // SubmitMoveOutDetails
            var moveOutDetailsForm = new MoveOutDetailsForm
            {
                TenancyDetailsSubmissionUniqueId = submission.UniqueId,
                MoveOutDate = submission.MoveOutDate.Value.AddDays(10)
            };

            // I try the wrong user
            var submitMoveOutDetailsOutcomeForOtherUser = await serviceUnderTest.SubmitMoveOutDetails(otherUser.Id, moveOutDetailsForm);
            Assert.IsTrue(submitMoveOutDetailsOutcomeForOtherUser.IsRejected,
                "SubmitMoveOutDetails outcome field IsRejected is not the expected when wrong user is used.");
            Assert.AreEqual(CommonResources.GenericInvalidRequestMessage, submitMoveOutDetailsOutcomeForOtherUser.RejectionReason,
                "SubmitMoveOutDetails outcome field RejectionReason is not the expected when wrong user is used.");
            Assert.IsFalse(submitMoveOutDetailsOutcomeForOtherUser.ReturnToForm,
                "SubmitMoveOutDetails outcome field ReturnToForm is not the expected when wrong user is used.");

            var retrievedSubmissionAtPoint7 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint7, "Retrieved submission at point 7 is null.");
            Assert.IsNotNull(retrievedSubmissionAtPoint7.MoveOutDate, 
                "Field MoveOutDate on retrieved submission at point 7 is null.");
            Assert.AreEqual(submission.MoveOutDate, retrievedSubmissionAtPoint7.MoveOutDate,
                "Field MoveOutDate on retrieved submission at point 7 is not the expected.");

            // I try the right user
            var submitMoveOutDetailsOutcome = await serviceUnderTest.SubmitMoveOutDetails(user.Id, moveOutDetailsForm);
            Assert.IsTrue(submitMoveOutDetailsOutcome.IsRejected, "SubmitMoveOutDetails outcome field IsRejected is not the expected.");
            Assert.AreEqual(CommonResources.GenericInvalidActionMessage, submitMoveOutDetailsOutcome.RejectionReason,
                "SubmitMoveOutDetails outcome field RejectionReason is not the expected.");
            Assert.IsFalse(submitMoveOutDetailsOutcome.ReturnToForm, "SubmitMoveOutDetails outcome field ReturnToForm is not the expected.");

            var retrievedSubmissionAtPoint8 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint8, "Retrieved submission at point 8 is null.");
            Assert.IsNotNull(retrievedSubmissionAtPoint8.MoveOutDate, "Field MoveOutDate on retrieved submission at point 8 is null.");
            Assert.AreEqual(submission.MoveOutDate, retrievedSubmissionAtPoint8.MoveOutDate.Value,
                "Field MoveOutDate on retrieved submission at point 8 is not the expected.");
            Assert.AreEqual(submission.MoveOutDateSubmittedOn, retrievedSubmissionAtPoint8.MoveOutDateSubmittedOn,
                "Field MoveOutDateSubmittedOn on retrieved submission at point 8 is not the expected.");
        }

        #endregion

        #region Private helper functions

        private async Task<TenancyDetailsSubmission> RetrieveSubmission(Guid uniqueId)
        {
            var container = CreateContainer();
            var dbContext = container.Get<IEpsilonContext>();
            return await dbContext.TenancyDetailsSubmissions
                .Include(x => x.TenantVerifications)
                .Include(x => x.Address)
                .SingleOrDefaultAsync(x => x.UniqueId == uniqueId);
        }

        private void SetupConfigForGetUserSubmissionSummary(IKernel container, int itemsLimit)
        {
            var mockConfig = new Mock<ITenancyDetailsSubmissionServiceConfig>();

            mockConfig.Setup(x => x.MySubmissionsSummary_ItemsLimit).Returns(itemsLimit);

            container.Rebind<ITenancyDetailsSubmissionServiceConfig>().ToConstant(mockConfig.Object);
        }

        private void SetupConfigForCreate(IKernel container,
            bool disableFrequencyPerAddressCheck, Frequency maxFrequencyPerAddress)
        {
            var mockConfig = new Mock<ITenancyDetailsSubmissionServiceConfig>();

            mockConfig.Setup(x => x.Create_DisableFrequencyPerAddressCheck).Returns(disableFrequencyPerAddressCheck);
            mockConfig.Setup(x => x.Create_MaxFrequencyPerAddress).Returns(maxFrequencyPerAddress);

            container.Rebind<ITenancyDetailsSubmissionServiceConfig>().ToConstant(mockConfig.Object);
        }

        private void SetupAntiAbuseServiceResponse(IKernel container, Action<string, string> callback,
            AntiAbuseServiceResponse response)
        {
            var mockAntiAbuseService = new Mock<IAntiAbuseService>();
            mockAntiAbuseService.Setup(x => x.CanCreateTenancyDetailsSubmission(It.IsAny<string>(), It.IsAny<string>()))
                .Callback(callback)
                .Returns(Task.FromResult(response));

            container.Rebind<IAntiAbuseService>().ToConstant(mockAntiAbuseService.Object);
        }

        private static async Task<TenancyDetailsSubmission> CreateTenancyDetailsSubmissionAndSave(
            IRandomWrapper random, IKernel container, 
            string userId, string userIpAddress,
            string userIdForVerfications, string userForVerificationsIpAddress,
            int justCreatedVerifications = 0, int sentVerifications = 0, int completeVerifications = 0, 
            bool areDetailsSubmitted = false, bool hasMovedOut = false, string secretCode = null)
        {
            if (secretCode != null && justCreatedVerifications + sentVerifications + completeVerifications != 1)
                throw new ArgumentException("If you choose your own secret secretCode then there should be only a single verification of any kind created.");

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

            if (areDetailsSubmitted)
            {
                tenancyDetailsSubmission.SubmittedOn = clock.OffsetNow;
                tenancyDetailsSubmission.RentPerMonth = random.Next(100, 1000);
                tenancyDetailsSubmission.IsPartOfProperty = random.NextDouble() >= 0.5;
                tenancyDetailsSubmission.MoveInDate = clock.OffsetNow.AddDays(-100.0 * random.NextDouble()).Date;
                tenancyDetailsSubmission.NumberOfBedrooms = random.Next(0, 5);
            }

            if (hasMovedOut)
            {
                tenancyDetailsSubmission.MoveOutDate = clock.OffsetNow.UtcDateTime.Date;
            }

            var verifications = new List<TenantVerification>();

            for (int i = 0; i < justCreatedVerifications; i++)
            {
                var verification = CreateTenantVerification(random, container, userIdForVerfications, userForVerificationsIpAddress, secretCode, isSent: false, isComplete: false);
                verifications.Add(verification);
            }

            for (int i = 0; i < sentVerifications; i++)
            {
                var verification = CreateTenantVerification(random, container, userIdForVerfications, userForVerificationsIpAddress, secretCode, isSent: true, isComplete: false);
                verifications.Add(verification);
            }

            for (int i = 0; i < completeVerifications; i++)
            {
                var verification = CreateTenantVerification(random, container, userIdForVerfications, userForVerificationsIpAddress, secretCode, isSent: true, isComplete: true);
                verifications.Add(verification);
            }

            tenancyDetailsSubmission.TenantVerifications = verifications;

            dbContext.TenancyDetailsSubmissions.Add(tenancyDetailsSubmission);
            await dbContext.SaveChangesAsync();
            return tenancyDetailsSubmission;
        }

        private static TenantVerification CreateTenantVerification(
            IRandomWrapper random, IKernel container, string userId, string userIpAddress, string secretCode, bool isSent, bool isComplete)
        {
            var clock = container.Get<IClock>();
            var dbContext = container.Get<IEpsilonContext>();

            var tenantVerification = new TenantVerification
            {
                UniqueId = Guid.NewGuid(),
                AssignedToId = userId,
                AssignedByIpAddress = userIpAddress,
                SecretCode = secretCode ?? RandomStringHelper.GetString(random, AppConstant.SECRET_CODE_MAX_LENGTH, CharacterCase.Mixed)
            };
            if (isSent)
                tenantVerification.MarkedAsSentOn = clock.OffsetNow;
            if (isComplete)
                tenantVerification.VerifiedOn = clock.OffsetNow;
            return tenantVerification;
        }

        #endregion
    }
}
