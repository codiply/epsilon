using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.IntegrationTests.TestHelpers;
using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Forms.Submission;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Infrastructure.Primitives;
using Epsilon.Logic.JsonModels;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Wrappers;
using Epsilon.Logic.Wrappers.Interfaces;
using Epsilon.Resources.Common;
using Epsilon.Resources.Logic.TenancyDetailsSubmission;
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
            CountryId? countryIdUsedInAntiAbuse = null;

            var container = CreateContainer();
            var disableFrequencyPerAddressCheck = false;
            var maxFrequencyPerAddress = new Frequency(1, TimeSpan.FromDays(1));
            SetupConfigForCreate(container, disableFrequencyPerAddressCheck, maxFrequencyPerAddress);
            SetupAntiAbuseServiceResponse(container, (userId, ipAddr, cId) =>
                {
                    userIdUsedInAntiAbuse = userId;
                    ipAddressUsedInAntiAbuse = ipAddr;
                    countryIdUsedInAntiAbuse = cId;
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
            Assert.IsNull(countryIdUsedInAntiAbuse, "The AntiAbuse service should not be called. (3)");
        }

        [Test]
        public async Task Create_RejectedByAntiAbuseService()
        {
            var ipAddress = "1.2.3.4";
            var antiAbuseRejectionReason = "AntiAbuseService Rejection Reason";
            var helperContainer = CreateContainer();
            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);
            var countryId = CountryId.GB;

            var random = new RandomWrapper(2015);

            var address = await AddressHelper.CreateRandomAddressAndSave(random, helperContainer, user.Id, ipAddress, countryId);

            var userIdUsedInAntiAbuse = string.Empty;
            var ipAddressUsedInAntiAbuse = string.Empty;
            CountryId? countryIdUsedInAntiAbuse = null;

            var container = CreateContainer();
            var disableFrequencyPerAddressCheck = true;
            var maxFrequencyPerAddress = new Frequency(1, TimeSpan.FromDays(0));
            SetupConfigForCreate(container, disableFrequencyPerAddressCheck, maxFrequencyPerAddress);
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
            Assert.AreEqual(countryId, countryIdUsedInAntiAbuse,
                "The CountryId used in the call to AntiAbuseService is not the expected.");

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
            var countryId = CountryId.GB;

            var random = new RandomWrapper(2015);

            var address = await AddressHelper.CreateRandomAddressAndSave(random, helperContainer, user.Id, ipAddress, countryId);

            var userIdUsedInAntiAbuse = string.Empty;
            var ipAddressUsedInAntiAbuse = string.Empty;
            CountryId? countryIdUsedInAntiAbuse = null;

            var container = CreateContainer();
            var disableFrequencyPerAddressCheck = true;
            var maxFrequencyPerAddress = new Frequency(1, TimeSpan.FromDays(0));
            SetupConfigForCreate(container, disableFrequencyPerAddressCheck, maxFrequencyPerAddress);
            SetupAntiAbuseServiceResponse(container, (userId, ipAddr, cId) =>
            {
                userIdUsedInAntiAbuse = userId;
                ipAddressUsedInAntiAbuse = ipAddr;
                countryIdUsedInAntiAbuse = cId;
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
            Assert.AreEqual(countryId, countryIdUsedInAntiAbuse,
                "The CountryId used in the call to AntiAbuseService is not the expected.");

            Assert.IsTrue(outcome.UiAlerts.Any(x => x.Message.Equals(TenancyDetailsSubmissionResources.Create_SuccessMessage)),
                "SuccessMessage was not found among the UiAlertts.");

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
            SetupAntiAbuseServiceResponse(container, (userId, ipAddr, cId) => { }, new AntiAbuseServiceResponse()
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
        public async Task SubmissionBelongsToUser_Test()
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

        #region GetSubmissionAddress

        [Test]
        public async Task GetSubmissionAddress_ForNonExistingSubmission()
        {
            var helperContainer = CreateContainer();
            var user = await CreateUser(helperContainer, "test@test.com", "1.2.3.4");

            var containerUnderTest = CreateContainer();
            var service = containerUnderTest.Get<ITenancyDetailsSubmissionService>();

            var submissionId = Guid.NewGuid();

            var outcome = await service.GetSubmissionAddress(user.Id, submissionId);

            Assert.IsTrue(outcome.SubmissionNotFound, "SubmissionNotFound is not the expected.");
            Assert.IsNull(outcome.Address, "Address is not the expected.");
        }

        [Test]
        public async Task GetSubmissionAddress_ForExistingSubmission()
        {
            var helperContainer = CreateContainer();
            var ipAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);
            var otherIpAddress = "1.2.3.5";
            var otherUser = await CreateUser(helperContainer, "test2@test.com", otherIpAddress);

            var random = new RandomWrapper(2015);

            var submission = await CreateTenancyDetailsSubmissionAndSave(random, helperContainer, user.Id, ipAddress,
                otherUser.Id, otherIpAddress, justCreatedVerifications: 1);

            var containerUnderTest = CreateContainer();
            var service = containerUnderTest.Get<ITenancyDetailsSubmissionService>();

            // I access it via the correct user.
            var outcome1 = await service.GetSubmissionAddress(user.Id, submission.UniqueId);

            var retrievedAddress = await DbProbe.Addresses.FindAsync(submission.AddressId);

            Assert.IsFalse(outcome1.SubmissionNotFound, "SubmissionNotFound on outcome1 is not the expected.");
            Assert.IsNotNull(outcome1.Address, "Address on outcome1 is not the expected.");
            Assert.AreEqual(submission.AddressId, outcome1.Address.Id, "Address Id on outcome1 is not the expected.");
            Assert.AreEqual(retrievedAddress.Line1, outcome1.Address.Line1, "Address Line1 is not the expected.");
            Assert.AreEqual(retrievedAddress.Postcode, outcome1.Address.Postcode, "Address Postcode is not the expected.");
            Assert.AreEqual(retrievedAddress.CountryId, outcome1.Address.CountryId, "Address CountryId is not the expected.");
            Assert.IsNotNull(outcome1.Address.Country, "Address.Country is not included in the result.");

            // I try to acccess it via the other user.
            var outcome2 = await service.GetSubmissionAddress(otherUser.Id, submission.UniqueId);

            Assert.IsTrue(outcome2.SubmissionNotFound, "SubmissionNotFound on outcome2 is not the expected.");
            Assert.IsNull(outcome2.Address, "Address on outcome2 is not the expected.");
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
            var sentRewardedVerifications = 0;
            var completeVerifications = 0;
            var areDetailsSubmitted = false;

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", "11.12.13.14");

            var random = new RandomWrapper(2015);
            var submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress,
                    justCreatedVerifications, sentVerifications, sentRewardedVerifications, completeVerifications, areDetailsSubmitted);

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

            Assert.IsFalse(submissionInfo.stepVerificationCodeSentOutDone, "Field stepVerificationCodeSentOutDone doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.stepVerificationCodeEnteredDone, "Field stepVerificationCodeEnteredDone doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.stepTenancyDetailsSubmittedDone, "Field stepTenancyDetailsSubmittedDone doesn't have the expected value.");
            
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
            var sentRewardedVerifications = 0;
            var completeVerifications = 0;
            var areDetailsSubmitted = false;

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", "11.12.13.14");

            var random = new RandomWrapper(2015);
            var submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress,
                    justCreatedVerifications, sentVerifications, sentRewardedVerifications, completeVerifications, areDetailsSubmitted);

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
            
            Assert.IsFalse(submissionInfo.stepVerificationCodeSentOutDone, "Field stepVerificationCodeSentOutDone doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.stepVerificationCodeEnteredDone, "Field stepVerificationCodeEnteredDone doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.stepTenancyDetailsSubmittedDone, "Field stepTenancyDetailsSubmittedDone doesn't have the expected value.");
            
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
            var sentRewardedVerifications = 0;
            var completeVerifications = 0;
            var areDetailsSubmitted = false;

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", "11.12.13.14");

            var random = new RandomWrapper(2015);
            var submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress,
                    justCreatedVerifications, sentVerifications, sentRewardedVerifications, completeVerifications, areDetailsSubmitted);

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
            
            Assert.IsTrue(submissionInfo.stepVerificationCodeSentOutDone, "Field stepVerificationCodeSentOutDone doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.stepVerificationCodeEnteredDone, "Field stepVerificationCodeEnteredDone doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.stepTenancyDetailsSubmittedDone, "Field stepTenancyDetailsSubmittedDone doesn't have the expected value.");
            
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
            var sentRewardedVerifications = 0;
            var completeVerifications = 2;
            var areDetailsSubmitted = false;

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", "11.12.13.14");

            var random = new RandomWrapper(2015);
            var submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress,
                    justCreatedVerifications, sentVerifications, sentRewardedVerifications, completeVerifications, areDetailsSubmitted);

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
            
            Assert.IsTrue(submissionInfo.stepVerificationCodeSentOutDone, "Field stepVerificationCodeSentOutDone doesn't have the expected value.");
            Assert.IsTrue(submissionInfo.stepVerificationCodeEnteredDone, "Field stepVerificationCodeEnteredDone doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.stepTenancyDetailsSubmittedDone, "Field stepTenancyDetailsSubmittedDone doesn't have the expected value.");
            
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
            var sentRewardedVerifications = 0;
            var completeVerifications = 1;
            var areDetailsSubmitted = false;

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", "11.12.13.14");

            var random = new RandomWrapper(2015);
            var submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress,
                    justCreatedVerifications, sentVerifications, sentRewardedVerifications, completeVerifications, areDetailsSubmitted);

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
            
            Assert.IsTrue(submissionInfo.stepVerificationCodeSentOutDone, "Field stepVerificationCodeSentOutDone doesn't have the expected value.");
            Assert.IsTrue(submissionInfo.stepVerificationCodeEnteredDone, "Field stepVerificationCodeEnteredDone doesn't have the expected value.");
            Assert.IsFalse(submissionInfo.stepTenancyDetailsSubmittedDone, "Field stepTenancyDetailsSubmittedDone doesn't have the expected value.");
            
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
            var sentRewardedVerifications = 0;
            var completeVerifications = 2;
            var areDetailsSubmitted = true;

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", "11.12.13.14");

            var random = new RandomWrapper(2015);
            var submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress,
                    justCreatedVerifications, sentVerifications, sentRewardedVerifications, completeVerifications, areDetailsSubmitted);

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
            
            Assert.IsTrue(submissionInfo.stepVerificationCodeSentOutDone, "Field stepVerificationCodeSentOutDone doesn't have the expected value.");
            Assert.IsTrue(submissionInfo.stepVerificationCodeEnteredDone, "Field stepVerificationCodeEnteredDone doesn't have the expected value.");
            Assert.IsTrue(submissionInfo.stepTenancyDetailsSubmittedDone, "Field stepTenancyDetailsSubmittedDone doesn't have the expected value.");
           
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
            var sentRewardedVerifications = 0;
            var completeVerifications = 1;
            var areDetailsSubmitted = true;

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", "11.12.13.14");

            var random = new RandomWrapper(2015);
            var submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress,
                    justCreatedVerifications, sentVerifications, sentRewardedVerifications, completeVerifications, areDetailsSubmitted);

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
            
            Assert.IsTrue(submissionInfo.stepVerificationCodeSentOutDone, "Field stepVerificationCodeSentOutDone doesn't have the expected value.");
            Assert.IsTrue(submissionInfo.stepVerificationCodeEnteredDone, "Field stepVerificationCodeEnteredDone doesn't have the expected value.");
            Assert.IsTrue(submissionInfo.stepTenancyDetailsSubmittedDone, "Field stepTenancyDetailsSubmittedDone doesn't have the expected value.");
            
            var retrievedSubmission = await DbProbe.TenancyDetailsSubmissions
                .Include(x => x.Address).Include(x => x.Address.Country).SingleOrDefaultAsync(x => x.UniqueId.Equals(submissionInfo.uniqueId));
            Assert.AreEqual(retrievedSubmission.Address.FullAddress(), submissionInfo.displayAddress, "Field displayAddress is not the expected.");
        }

        
        [Test]
        public async Task GetUserSubmissionSummary_SingleSubmissionAfterTenancyDetailsSubmittedWithJustCreatedVerificationTest()
        {
            var itemsLimit = 10;
            var justCreatedVerifications = 0;
            var sentVerifications = 1;
            var sentRewardedVerifications = 0;
            var completeVerifications = 1;
            var areDetailsSubmitted = true;

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", "11.12.13.14");

            var random = new RandomWrapper(2015);
            var submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress,
                    justCreatedVerifications, sentVerifications, sentRewardedVerifications, completeVerifications, areDetailsSubmitted);

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
            
            Assert.IsTrue(submissionInfo.stepVerificationCodeSentOutDone, "Field stepVerificationCodeSentOutDone doesn't have the expected value.");
            Assert.IsTrue(submissionInfo.stepVerificationCodeEnteredDone, "Field stepVerificationCodeEnteredDone doesn't have the expected value.");
            Assert.IsTrue(submissionInfo.stepTenancyDetailsSubmittedDone, "Field stepTenancyDetailsSubmittedDone doesn't have the expected value.");
            
            var retrievedSubmission = await DbProbe.TenancyDetailsSubmissions
                .Include(x => x.Address).Include(x => x.Address.Country).SingleOrDefaultAsync(x => x.UniqueId.Equals(submissionInfo.uniqueId));
            Assert.AreEqual(retrievedSubmission.Address.FullAddress(), submissionInfo.displayAddress, "Field displayAddress is not the expected.");
        }

        [Test]
        public async Task GetUserSubmissionSummary_SingleSubmissionWithSubmittedDetailsAndSentVerificationTest()
        {
            var itemsLimit = 10;
            var justCreatedVerifications = 0;
            var sentVerifications = 1;
            var sentRewardedVerifications = 0;
            var completeVerifications = 1;
            var areDetailsSubmitted = true;

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", "11.12.13.14");

            var random = new RandomWrapper(2015);
            var submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress,
                    justCreatedVerifications, sentVerifications, sentRewardedVerifications, completeVerifications, areDetailsSubmitted);

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
            
            Assert.IsTrue(submissionInfo.stepVerificationCodeSentOutDone, "Field stepVerificationCodeSentOutDone doesn't have the expected value.");
            Assert.IsTrue(submissionInfo.stepVerificationCodeEnteredDone, "Field stepVerificationCodeEnteredDone doesn't have the expected value.");
            Assert.IsTrue(submissionInfo.stepTenancyDetailsSubmittedDone, "Field stepTenancyDetailsSubmittedDone doesn't have the expected value.");
            
            var retrievedSubmission = await DbProbe.TenancyDetailsSubmissions
                .Include(x => x.Address).Include(x => x.Address.Country).SingleOrDefaultAsync(x => x.UniqueId.Equals(submissionInfo.uniqueId));
            Assert.AreEqual(retrievedSubmission.Address.FullAddress(), submissionInfo.displayAddress, "Field displayAddress is not the expected.");
        }

        #endregion

        #region GetUserSubmissionSummaryWithCaching

        [Test]
        public async Task GetUserSubmissionSummaryWithCaching_WithSubmissionsEqualToTheLimit_CachesTheSummary()
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
            var response1 = await serviceUnderTest.GetUserSubmissionsSummaryWithCaching(user.Id, false);

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
            var response2 = await serviceUnderTest.GetUserSubmissionsSummaryWithCaching(user.Id, true);

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

            KillDatabase(containerUnderTest);
            var serviceWithoutDatabase = containerUnderTest.Get<ITenancyDetailsSubmissionService>();

            // Full summary
            var response3 = await serviceUnderTest.GetUserSubmissionsSummaryWithCaching(user.Id, false);

            Assert.IsNotNull(response3, "Response3 is null.");
            Assert.IsFalse(response3.moreItemsExist, "Field moreItemsExist on response3 is not the expected.");
            Assert.AreEqual(submissionsToCreate, response3.tenancyDetailsSubmissions.Count,
                "Response3 should contain all submissions.");
            for (var i = 0; i < submissionsToCreate; i++)
            {
                Assert.AreEqual(submissionByCreationDescending[i].UniqueId, response3.tenancyDetailsSubmissions[i].uniqueId,
                    string.Format("Response3: submission at position {0} does not have the expected uniqueId.", i));
            }

            Assert.IsFalse(response3.tenancyDetailsSubmissions.Any(x => x.uniqueId.Equals(otherUserSubmission.UniqueId)),
                "Response3 should not contain the submission of the other user.");

            // Summary with limit
            var response4 = await serviceUnderTest.GetUserSubmissionsSummaryWithCaching(user.Id, true);

            Assert.IsNotNull(response4, "Response4 is null.");
            Assert.IsFalse(response4.moreItemsExist, "Field moreItemsExist on response4 is not the expected.");
            Assert.AreEqual(itemsLimit, response4.tenancyDetailsSubmissions.Count,
                "Response4 should contains a number of submissions equal to the limit.");
            for (var i = 0; i < itemsLimit; i++)
            {
                Assert.AreEqual(submissionByCreationDescending[i].UniqueId, response4.tenancyDetailsSubmissions[i].uniqueId,
                    string.Format("Response4: submission at position {0} does not have the expected uniqueId.", i));
            }

            Assert.IsFalse(response4.tenancyDetailsSubmissions.Any(x => x.uniqueId.Equals(otherUserSubmission.UniqueId)),
                "Response4 should not contain the submission of the other user.");
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
                    areDetailsSubmitted: false, secretCode: secretCode);
            var otherUserSubmission1verification = otherUserSubmission1.TenantVerifications.Single();

            var thisUserSubmission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress,
                    justCreatedVerifications: 0, sentVerifications: 1, completeVerifications: 0,
                    areDetailsSubmitted: false, secretCode: secretCode);
            var thisUserSubmissionVerification = thisUserSubmission.TenantVerifications.Single();

            var otherUserSubmission2 = await CreateTenancyDetailsSubmissionAndSave(
                random, helperContainer, otherUser.Id, otherUserIpAddress, user.Id, userIpAddress,
                justCreatedVerifications: 0, sentVerifications: 1, completeVerifications: 0,
                areDetailsSubmitted: false, secretCode: secretCode);
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
            var sentRewardedVerifications = 0;
            var completeVerifications = 0;
            var areDetailsSubmitted = false;

            var helperContainer = CreateContainer();

            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "1.2.3.5";
            var otherUser = await CreateUser(helperContainer, "test2@test.com", otherUserIpAddress);

            var random = new RandomWrapper(2015);
            var clock = helperContainer.Get<IClock>();

            var submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress,
                    justCreatedVerifications, sentVerifications, sentRewardedVerifications, completeVerifications, areDetailsSubmitted);

            var containerUnderTest = CreateContainer();
            SetupClockDisableLuckySender(containerUnderTest);
            var serviceUnderTest = containerUnderTest.Get<ITenancyDetailsSubmissionService>();

            var retrievedSubmissionAtPoint1 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint1, "Retrieved submission at point 1 is null.");
            Assert.IsNull(retrievedSubmissionAtPoint1.RentPerMonth, "Field RentPerMonth on retrieved submission at point 1 is not the expected.");
            
            // I try all invalid actions first
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
                IsFurnished = true,
                RentPerMonth = 100,
                NumberOfBedrooms = 3,
                PropertyConditionRating = 1,
                LandlordRating = 2,
                NeighboursRating = 3
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
        }

        [Test]
        public async Task ActionsTest_SubmissionWithJustCreatedVerifications()
        {
            var justCreatedVerifications = 2;
            var sentVerifications = 0;
            var sentRewardedVerifications = 0;
            var completeVerifications = 0;
            var areDetailsSubmitted = false;

            var helperContainer = CreateContainer();

            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "1.2.3.5";
            var otherUser = await CreateUser(helperContainer, "test2@test.com", otherUserIpAddress);

            var random = new RandomWrapper(2015);
            var clock = helperContainer.Get<IClock>();

            var submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress,
                    justCreatedVerifications, sentVerifications, sentRewardedVerifications, completeVerifications, areDetailsSubmitted);

            var containerUnderTest = CreateContainer();
            SetupClockDisableLuckySender(containerUnderTest);
            var serviceUnderTest = containerUnderTest.Get<ITenancyDetailsSubmissionService>();

            var retrievedSubmissionAtPoint1 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint1, "Retrieved submission at point 1 is null.");
            Assert.IsNull(retrievedSubmissionAtPoint1.RentPerMonth, "Field RentPerMonth on retrieved submission at point 1 is not the expected.");
            Assert.IsTrue(retrievedSubmissionAtPoint1.TenantVerifications.All(x => !x.VerifiedOn.HasValue),
                "At point 1 all verifications should have null VerifiedOn field.");
            Assert.IsTrue(retrievedSubmissionAtPoint1.TenantVerifications.All(x => !x.MarkedAsSentOn.HasValue),
                "At point 1 all verifications should have null MarkedAsSentOn field.");

            // I try invalid actions first
            // SubmitTenancyDetails
            var tenancyDetailsForm = new TenancyDetailsForm
            {
                TenancyDetailsSubmissionUniqueId = submission.UniqueId,
                IsPartOfProperty = true,
                IsFurnished = true,
                RentPerMonth = 100,
                NumberOfBedrooms = 3,
                PropertyConditionRating = 1,
                LandlordRating = 2,
                NeighboursRating = 3
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

            var retrievedSubmissionAtPoint4 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint4, "Retrieved submission at point 4 is null.");
            var retrievedUsedVerificationAtPoint4 = 
                retrievedSubmissionAtPoint4.TenantVerifications.Single(x => x.UniqueId == verificationToUse.UniqueId);
            Assert.IsNull(retrievedUsedVerificationAtPoint4.VerifiedOn,
                "Field VerifiedOn on retrieved used verification at point 4 is not the expected.");

            var timeBefore = clock.OffsetNow;
            var enterVerificationCodeOutcome = await serviceUnderTest.EnterVerificationCode(user.Id, verificationCodeForm);
            Assert.IsFalse(enterVerificationCodeOutcome.IsRejected, "EnterVerificationCode outcome field IsRejected is not the expected.");
            Assert.IsNullOrEmpty(enterVerificationCodeOutcome.RejectionReason);
            Assert.IsFalse(enterVerificationCodeOutcome.ReturnToForm, "EnterVerificationCode outcome field ReturnToForm is not the expected.");
            var timeAfter = clock.OffsetNow;

            var retrievedSubmissionAtPoint5 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint5, "Retrieved submission at point 5 is null.");
            var retrievedUsedVerificationAtPoint5 =
                retrievedSubmissionAtPoint5.TenantVerifications.Single(x => x.UniqueId == verificationToUse.UniqueId);
            Assert.IsTrue(retrievedUsedVerificationAtPoint5.VerifiedOn.HasValue,
                "Field VerifiedOn on used verification retrieved at point 5 does not have a value.");
            Assert.IsTrue(timeBefore <= retrievedUsedVerificationAtPoint5.VerifiedOn.Value && retrievedUsedVerificationAtPoint5.VerifiedOn.Value <= timeAfter,
                "Field VerifiedOn on used verification retrieved at point 5 is not in the expected range.");
        }

        [Test]
        public async Task ActionsTest_SubmissionWithSentAndCompleteVerifications()
        {
            var justCreatedVerifications = 0;
            var sentVerifications = 1;
            var sentRewardedVerifications = 0;
            var completeVerifications = 1;
            var areDetailsSubmitted = false;
            var countryId = CountryId.GB;

            var helperContainer = CreateContainer();

            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "1.2.3.5";
            var otherUser = await CreateUser(helperContainer, "test2@test.com", otherUserIpAddress);

            var random = new RandomWrapper(2015);
            var clock = helperContainer.Get<IClock>();

            var submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress,
                    justCreatedVerifications, sentVerifications, sentRewardedVerifications, completeVerifications, areDetailsSubmitted, countryId: countryId);

            var containerUnderTest = CreateContainer();
            SetupClockDisableLuckySender(containerUnderTest);
            var serviceUnderTest = containerUnderTest.Get<ITenancyDetailsSubmissionService>();
            var userTokenService = containerUnderTest.Get<IUserTokenService>();
            var tokenRewardService = containerUnderTest.Get<ITokenRewardService>();

            var retrievedSubmissionAtPoint1 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint1, "Retrieved submission at point 1 is null.");
            Assert.IsNull(retrievedSubmissionAtPoint1.RentPerMonth, "Field RentPerMonth on retrieved submission at point 1 is not the expected.");
            Assert.IsTrue(retrievedSubmissionAtPoint1.TenantVerifications.Any(x => !x.VerifiedOn.HasValue),
                "At point 1 some verifications should have null VerifiedOn field.");
            Assert.IsTrue(retrievedSubmissionAtPoint1.TenantVerifications.Any(x => x.VerifiedOn.HasValue),
                "At point 1 some verifications should not have null VerifiedOn field.");
            Assert.IsTrue(retrievedSubmissionAtPoint1.TenantVerifications.All(x => x.MarkedAsSentOn.HasValue),
                "At point 1 all verifications should have a value in MarkedAsSentOn field.");

            var userBalanceAtPoint1 = await userTokenService.GetBalance(user.Id);
            var otherUserBalanceAtPoint1 = await userTokenService.GetBalance(otherUser.Id);
            Assert.AreEqual(0, userBalanceAtPoint1.balance, "User balance at point 1 is not the expected.");
            Assert.AreEqual(0, otherUserBalanceAtPoint1.balance, "Other User balance at point 1 is not the expected.");

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

            var retrievedSubmissionAtPoint2 = await RetrieveSubmission(submission.UniqueId);
            var retrievedVerificationToUseAtPoint2 =
                retrievedSubmissionAtPoint2.TenantVerifications.Single(x => x.UniqueId.Equals(verificationToUse.UniqueId));
            Assert.IsNull(retrievedVerificationToUseAtPoint2.VerifiedOn,
                "Field VerifiedOn on retrieved used verification at point 2 is not the expected.");

            var userBalanceAtPoint2 = await userTokenService.GetBalance(user.Id);
            var otherUserBalanceAtPoint2 = await userTokenService.GetBalance(otherUser.Id);
            Assert.AreEqual(0, userBalanceAtPoint2.balance, "User balance at point 2 is not the expected.");
            Assert.AreEqual(0, otherUserBalanceAtPoint2.balance, "Other User balance at point 2 is not the expected.");

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

            var retrievedSubmissionAtPoint3 = await RetrieveSubmission(submission.UniqueId);
            var retrievedVerificationToUseAtPoint3 =
                retrievedSubmissionAtPoint3.TenantVerifications.Single(x => x.UniqueId.Equals(verificationToUse.UniqueId));
            Assert.IsNull(retrievedVerificationToUseAtPoint3.VerifiedOn,
                "Field VerifiedOn on retrieved verification to use at point 3 is not the expected.");
            Assert.IsNull(retrievedVerificationToUseAtPoint3.SenderRewardedOn,
                "Field SenderRewardedOn on retrieved verification to use at point 3 is not the expected.");
            var retrievedCompleteVerificationAtPoint3 =
                retrievedSubmissionAtPoint3.TenantVerifications.Single(x => x.UniqueId.Equals(completeVerification.UniqueId));
            Assert.AreEqual(completeVerification.VerifiedOn.Value, retrievedCompleteVerificationAtPoint3.VerifiedOn.Value,
                "The VerifiedOn field should not be updated when a previously entered code is used.");

            var userBalanceAtPoint3 = await userTokenService.GetBalance(user.Id);
            var otherUserBalanceAtPoint3 = await userTokenService.GetBalance(otherUser.Id);
            Assert.AreEqual(0, userBalanceAtPoint3.balance, "User balance at point 3 is not the expected.");
            Assert.AreEqual(0, otherUserBalanceAtPoint3.balance, "Other User balance at point 3 is not the expected.");

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

            var retrievedSubmissionAtPoint4 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint4, "Retrieved submission at point 4 is null.");
            var retrievedUsedVerificationAtPoint4 =
                retrievedSubmissionAtPoint4.TenantVerifications.Single(x => x.UniqueId == verificationToUse.UniqueId);
            Assert.IsNull(retrievedUsedVerificationAtPoint4.VerifiedOn,
                "Field VerifiedOn on retrieved used verification at point 4 is not the expected.");

            var userBalanceAtPoint4 = await userTokenService.GetBalance(user.Id);
            var otherUserBalanceAtPoint4 = await userTokenService.GetBalance(otherUser.Id);
            Assert.AreEqual(0, userBalanceAtPoint4.balance, "User balance at point 4 is not the expected.");
            Assert.AreEqual(0, otherUserBalanceAtPoint4.balance, "Other User balance at point 4 is not the expected.");

            var timeBeforeEnterVerificationCode = clock.OffsetNow;
            var enterVerificationCodeOutcome = await serviceUnderTest.EnterVerificationCode(user.Id, verificationCodeForm);
            Assert.IsFalse(enterVerificationCodeOutcome.IsRejected, "EnterVerificationCode outcome field IsRejected is not the expected.");
            Assert.IsNullOrEmpty(enterVerificationCodeOutcome.RejectionReason);
            Assert.IsFalse(enterVerificationCodeOutcome.ReturnToForm, "EnterVerificationCode outcome field ReturnToForm is not the expected.");
            Assert.IsTrue(enterVerificationCodeOutcome.UiAlerts.Any(x => x.Message.Equals(CommonResources.TokenAccountCreditedForThisAction)),
                "A message for crediting tokens was not found on the UiAlerts of the EnterVerificationCode outcome.");
            
            var timeAfterEnterVerificationCode = clock.OffsetNow;

            var retrievedSubmissionAtPoint5 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint5, "Retrieved submission at point 5 is null.");
            var retrievedUsedVerificationAtPoint5 =
                retrievedSubmissionAtPoint5.TenantVerifications.Single(x => x.UniqueId == verificationToUse.UniqueId);
            Assert.IsTrue(retrievedUsedVerificationAtPoint5.VerifiedOn.HasValue,
                "Field VerifiedOn on used verification retrieved at point 5 does not have a value.");
            Assert.IsTrue(timeBeforeEnterVerificationCode <= retrievedUsedVerificationAtPoint5.VerifiedOn.Value 
                && retrievedUsedVerificationAtPoint5.VerifiedOn.Value <= timeAfterEnterVerificationCode,
                "Field VerifiedOn on used verification retrieved at point 5 is not in the expected range.");
            Assert.IsTrue(timeBeforeEnterVerificationCode <= retrievedUsedVerificationAtPoint5.SenderRewardedOn.Value
                && retrievedUsedVerificationAtPoint5.SenderRewardedOn.Value <= timeAfterEnterVerificationCode,
                 "Field SenderRewardedOn on used verification retrieved at point 5 is not in the expected range.");

            var rewardForEnteringCode = tokenRewardService.GetCurrentReward(TokenRewardKey.EarnPerVerificationCodeEntered);
            var rewardForVerificationSender = tokenRewardService.GetCurrentReward(TokenRewardKey.EarnPerVerificationMailSent);

            var userBalanceAtPoint5 = await userTokenService.GetBalance(user.Id);
            var otherUserBalanceAtPoint5 = await userTokenService.GetBalance(otherUser.Id);
            Assert.AreEqual(rewardForEnteringCode.Value, userBalanceAtPoint5.balance, "User balance at point 5 is not the expected.");
            Assert.AreEqual(rewardForVerificationSender.Value, otherUserBalanceAtPoint5.balance, "Other User balance at point 5 is not the expected.");

            var earnPerVerificationMailSentRewardTypeKey = EnumsHelper.TokenRewardKey.ToString(TokenRewardKey.EarnPerVerificationMailSent);
            var retrievedTokenTransactionForVerificationSender = await DbProbe.TokenAccountTransactions
                .SingleOrDefaultAsync(x => x.AccountId.Equals(otherUser.Id) &&
                                      x.RewardTypeKey.Equals(earnPerVerificationMailSentRewardTypeKey));
            Assert.IsNotNull(earnPerVerificationMailSentRewardTypeKey,
                "No token transaction was created for the sender of the verification mail.");
            Assert.AreEqual(verificationToUse.UniqueId, retrievedTokenTransactionForVerificationSender.InternalReference,
                "The internal reference on the token transaction for the sender of the verification mail is not the expected.");

            var earnPerVerificationCodeEnteredRewardTypeKey = EnumsHelper.TokenRewardKey.ToString(TokenRewardKey.EarnPerVerificationCodeEntered);
            var retrievedTokenTransactionForEnteringCode = await DbProbe.TokenAccountTransactions
                .SingleOrDefaultAsync(x => x.AccountId.Equals(user.Id) &&
                                      x.RewardTypeKey.Equals(earnPerVerificationCodeEnteredRewardTypeKey));
            Assert.IsNotNull(retrievedTokenTransactionForEnteringCode,
                "No token transaction was created for entering the verification code.");
            Assert.AreEqual(submission.UniqueId, retrievedTokenTransactionForEnteringCode.InternalReference,
                "The internal reference on the token transaction for entering the verification code is not the expected.");

            // SubmitTenancyDetails
            var tenancyDetailsForm = new TenancyDetailsForm
            {
                TenancyDetailsSubmissionUniqueId = submission.UniqueId,
                IsPartOfProperty = true,
                IsFurnished = true,
                RentPerMonth = 100,
                NumberOfBedrooms = 3,
                PropertyConditionRating = 1,
                LandlordRating = 2,
                NeighboursRating = 3
            };

            // I try the wrong user.
            var submitTenancyDetailsOutcomeForOtherUser = await serviceUnderTest.SubmitTenancyDetails(otherUser.Id, tenancyDetailsForm);
            Assert.IsTrue(submitTenancyDetailsOutcomeForOtherUser.IsRejected,
                "SubmitTenancyDetails outcome field IsRejected is not the expected when wrong user is used.");
            Assert.AreEqual(CommonResources.GenericInvalidRequestMessage, submitTenancyDetailsOutcomeForOtherUser.RejectionReason,
                "SubmitTenancyDetails outcome field RejectionReason is not the expected when wrong user is used.");
            Assert.IsFalse(submitTenancyDetailsOutcomeForOtherUser.ReturnToForm,
                "SubmitTenancyDetails outcome field ReturnToForm is not the expected when wrong user is used.");
            Assert.IsTrue(submitTenancyDetailsOutcomeForOtherUser.UiAlerts == null || submitTenancyDetailsOutcomeForOtherUser.UiAlerts.Any(x => x.Message.Equals(TenancyDetailsSubmissionResources.SubmitTenancyDetails_SuccessMessage)),
                "A success message should not be on the UiAlerts of the SubmitTenancyDetails outcome when wrong user is used.");

            var retrievedSubmissionAtPoint6 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint6, "Retrieved submission at point 6 is null.");
            Assert.IsNull(retrievedSubmissionAtPoint6.RentPerMonth, "Field RentPerMonth on retrieved submission at point 6 is not the expected.");
            Assert.IsNull(retrievedSubmissionAtPoint6.SubmittedOn, "Field SubmittedOn on retrieved submission at point 6 is not the expected.");
            Assert.IsNull(retrievedSubmissionAtPoint6.CurrencyId,
                "Field CurrencyId on retrieved submission at point 6 is not the expected.");

            var userBalanceAtPoint6 = await userTokenService.GetBalance(user.Id);
            var otherUserBalanceAtPoint6 = await userTokenService.GetBalance(otherUser.Id);
            Assert.AreEqual(userBalanceAtPoint5.balance, userBalanceAtPoint6.balance, "User balance at point 6 is not the expected.");
            Assert.AreEqual(otherUserBalanceAtPoint5.balance, otherUserBalanceAtPoint6.balance, "Other User balance at point 6 is not the expected.");

            var earnPerTenancyDetailsSubmissionRewardTypeKey = EnumsHelper.TokenRewardKey.ToString(TokenRewardKey.EarnPerTenancyDetailsSubmission);
            var retrievedTokenTransactionForSubmissionForOtherUser = await DbProbe.TokenAccountTransactions
                .SingleOrDefaultAsync(x => x.AccountId.Equals(otherUser.Id) && 
                                           x.RewardTypeKey.Equals(earnPerTenancyDetailsSubmissionRewardTypeKey));
            Assert.IsNull(retrievedTokenTransactionForSubmissionForOtherUser, 
                "There should be no transaction created when trying to submit details with the other user.");

            // I try the right user.
            var timeBeforeSubmitTenancyDetails = clock.OffsetNow;
            var submitTenancyDetailsOutcome = await serviceUnderTest.SubmitTenancyDetails(user.Id, tenancyDetailsForm);
            Assert.IsFalse(submitTenancyDetailsOutcome.IsRejected, "SubmitTenancyDetails outcome field IsRejected is not the expected.");
            Assert.IsNullOrEmpty(submitTenancyDetailsOutcome.RejectionReason,
                "SubmitTenancyDetails outcome field RejectionReason is not the expected.");
            Assert.IsFalse(submitTenancyDetailsOutcome.ReturnToForm, "SubmitTenancyDetails outcome field ReturnToForm is not the expected.");
            Assert.IsTrue(submitTenancyDetailsOutcome.UiAlerts.Any(x => x.Message.Equals(TenancyDetailsSubmissionResources.SubmitTenancyDetails_SuccessMessage)),
                "A success message was not found on the UiAlerts of the SubmitTenancyDetails outcome.");
            var timeAfterSubmitTenancyDetails = clock.OffsetNow;

            var retrievedSubmissionAtPoint7 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint7, "Retrieved submission at point 7 is null.");
            Assert.AreEqual(tenancyDetailsForm.IsPartOfProperty, retrievedSubmissionAtPoint7.IsPartOfProperty, 
                "Field IsPartOfProperty on retrieved submission at point 7 is not the expected.");
            Assert.AreEqual(tenancyDetailsForm.RentPerMonth, retrievedSubmissionAtPoint7.RentPerMonth,
                "Field Rent on retrieved submission at point 7 is not the expected.");
            Assert.AreEqual(tenancyDetailsForm.IsFurnished, retrievedSubmissionAtPoint7.IsFurnished,
                "Field IsFurnished on retrieved submission at point 7 is not the expected.");
            Assert.AreEqual(tenancyDetailsForm.NumberOfBedrooms, retrievedSubmissionAtPoint7.NumberOfBedrooms,
                "Field NumberOfBedrooms on retrieved submission at point 7 is not the expected.");
            Assert.AreEqual(tenancyDetailsForm.PropertyConditionRating, retrievedSubmissionAtPoint7.PropertyConditionRating,
                "Field PropertyConditionRating on retrieved submission at point 7 is not the expected.");
            Assert.AreEqual(tenancyDetailsForm.LandlordRating, retrievedSubmissionAtPoint7.LandlordRating,
                "Field LandlordRating on retrieved submission at point 7 is not the expected.");
            Assert.AreEqual(tenancyDetailsForm.NeighboursRating, retrievedSubmissionAtPoint7.NeighboursRating,
                "Field NeighboursRating on retrieved submission at point 7 is not the expected.");
            Assert.IsTrue(retrievedSubmissionAtPoint7.SubmittedOn.HasValue,
                "Field SubmittedOn on retrieved submission at point 7 should have a value.");
            Assert.IsTrue(timeBeforeSubmitTenancyDetails <= retrievedSubmissionAtPoint7.SubmittedOn.Value
                && retrievedSubmissionAtPoint7.SubmittedOn.Value <= timeAfterSubmitTenancyDetails,
                "Field SubmittedOn on retrieved submission at point 7 is not within the expected range.");
            Assert.AreEqual(EnumsHelper.CurrencyId.ToString(CurrencyId.GBP), retrievedSubmissionAtPoint7.CurrencyId,
                "Field CurrencyId on retrieved submission at point 7 is not the expected.");

            var rewardForSubmittingTenancyDetails = tokenRewardService.GetCurrentReward(TokenRewardKey.EarnPerTenancyDetailsSubmission);

            var userBalanceAtPoint7 = await userTokenService.GetBalance(user.Id);
            var otherUserBalanceAtPoint7 = await userTokenService.GetBalance(otherUser.Id);
            Assert.AreEqual(userBalanceAtPoint6.balance + rewardForSubmittingTenancyDetails.Value, userBalanceAtPoint7.balance, "User balance at point 7 is not the expected.");
            Assert.AreEqual(otherUserBalanceAtPoint6.balance, otherUserBalanceAtPoint7.balance, "Other User balance at point 7 is not the expected.");

            var retrievedTokenTransactionForSubmission = await DbProbe.TokenAccountTransactions
                .SingleOrDefaultAsync(x => x.AccountId.Equals(user.Id) &&
                                           x.RewardTypeKey.Equals(earnPerTenancyDetailsSubmissionRewardTypeKey));
            Assert.IsNotNull(retrievedTokenTransactionForSubmission,
                "No token transaction was created for submitting the tenancy details");
            Assert.AreEqual(submission.UniqueId, retrievedTokenTransactionForSubmission.InternalReference,
                "The internal reference on the token transaction for submitting the tenancy details is not the expected.");
        }

        [Test]
        public async Task ActionsTest_SubmissionWithSentAndCompleteVerifications_LuckySender()
        {
            var justCreatedVerifications = 0;
            var sentVerifications = 1;
            var sentRewardedVerifications = 0;
            var completeVerifications = 1;
            var areDetailsSubmitted = false;
            var countryId = CountryId.GB;

            var helperContainer = CreateContainer();

            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "1.2.3.5";
            var otherUser = await CreateUser(helperContainer, "test2@test.com", otherUserIpAddress);

            var random = new RandomWrapper(2015);
            var clock = helperContainer.Get<IClock>();

            var submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress,
                    justCreatedVerifications, sentVerifications, sentRewardedVerifications, completeVerifications, areDetailsSubmitted, countryId: countryId);

            var containerUnderTest = CreateContainer();
            SetupClockAlwaysLuckySender(containerUnderTest);
            var serviceUnderTest = containerUnderTest.Get<ITenancyDetailsSubmissionService>();
            var userTokenService = containerUnderTest.Get<IUserTokenService>();
            var tokenRewardService = containerUnderTest.Get<ITokenRewardService>();

            var retrievedSubmissionAtPoint1 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint1, "Retrieved submission at point 1 is null.");
            Assert.IsNull(retrievedSubmissionAtPoint1.RentPerMonth, "Field RentPerMonth on retrieved submission at point 1 is not the expected.");
            Assert.IsTrue(retrievedSubmissionAtPoint1.TenantVerifications.Any(x => !x.VerifiedOn.HasValue),
                "At point 1 some verifications should have null VerifiedOn field.");
            Assert.IsTrue(retrievedSubmissionAtPoint1.TenantVerifications.Any(x => x.VerifiedOn.HasValue),
                "At point 1 some verifications should not have null VerifiedOn field.");
            Assert.IsTrue(retrievedSubmissionAtPoint1.TenantVerifications.All(x => x.MarkedAsSentOn.HasValue),
                "At point 1 all verifications should have a value in MarkedAsSentOn field.");

            var userBalanceAtPoint1 = await userTokenService.GetBalance(user.Id);
            var otherUserBalanceAtPoint1 = await userTokenService.GetBalance(otherUser.Id);
            Assert.AreEqual(0, userBalanceAtPoint1.balance, "User balance at point 1 is not the expected.");
            Assert.AreEqual(0, otherUserBalanceAtPoint1.balance, "Other User balance at point 1 is not the expected.");

            // EnterVerificationCode
            var verificationToUse = submission.TenantVerifications.Single(x => !x.VerifiedOn.HasValue);
            var completeVerification = submission.TenantVerifications.Single(x => x.VerifiedOn.HasValue);

            var userBalanceAtPoint2 = await userTokenService.GetBalance(user.Id);
            var otherUserBalanceAtPoint2 = await userTokenService.GetBalance(otherUser.Id);
            Assert.AreEqual(0, userBalanceAtPoint2.balance, "User balance at point 2 is not the expected.");
            Assert.AreEqual(0, otherUserBalanceAtPoint2.balance, "Other User balance at point 2 is not the expected.");

            var verificationCodeForm = new VerificationCodeForm
            {
                TenancyDetailsSubmissionUniqueId = submission.UniqueId,
                VerificationCode = verificationToUse.SecretCode
            };

            var userBalanceAtPoint4 = await userTokenService.GetBalance(user.Id);
            var otherUserBalanceAtPoint4 = await userTokenService.GetBalance(otherUser.Id);
            Assert.AreEqual(0, userBalanceAtPoint4.balance, "User balance at point 4 is not the expected.");
            Assert.AreEqual(0, otherUserBalanceAtPoint4.balance, "Other User balance at point 4 is not the expected.");

            var enterVerificationCodeOutcome = await serviceUnderTest.EnterVerificationCode(user.Id, verificationCodeForm);
            Assert.IsFalse(enterVerificationCodeOutcome.IsRejected, "EnterVerificationCode outcome field IsRejected is not the expected.");
            Assert.IsNullOrEmpty(enterVerificationCodeOutcome.RejectionReason);
            Assert.IsFalse(enterVerificationCodeOutcome.ReturnToForm, "EnterVerificationCode outcome field ReturnToForm is not the expected.");
            Assert.IsTrue(enterVerificationCodeOutcome.UiAlerts.Any(x => x.Message.Equals(CommonResources.TokenAccountCreditedForThisAction)),
                "A message for crediting tokens was not found on the UiAlerts of the EnterVerificationCode outcome.");

            var retrievedSubmissionAtPoint5 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint5, "Retrieved submission at point 5 is null.");
            var retrievedUsedVerificationAtPoint5 =
                retrievedSubmissionAtPoint5.TenantVerifications.Single(x => x.UniqueId == verificationToUse.UniqueId);
            Assert.IsTrue(retrievedUsedVerificationAtPoint5.VerifiedOn.HasValue,
                "Field VerifiedOn on used verification retrieved at point 5 does not have a value.");

            var rewardForEnteringCode = tokenRewardService.GetCurrentReward(TokenRewardKey.EarnPerVerificationCodeEntered);
            var rewardForVerificationSender = tokenRewardService.GetCurrentReward(TokenRewardKey.EarnPerVerificationMailSent);
            var rewardForLuckySender = tokenRewardService.GetCurrentReward(TokenRewardKey.EarnPerVerificationLuckySender);

            var userBalanceAtPoint5 = await userTokenService.GetBalance(user.Id);
            var otherUserBalanceAtPoint5 = await userTokenService.GetBalance(otherUser.Id);
            Assert.AreEqual(rewardForEnteringCode.Value, userBalanceAtPoint5.balance, "User balance at point 5 is not the expected.");
            Assert.AreEqual(rewardForVerificationSender.Value + rewardForLuckySender.Value, otherUserBalanceAtPoint5.balance, 
                "Other User balance at point 5 is not the expected.");

            var earnPerVerificationMailSentRewardTypeKey = EnumsHelper.TokenRewardKey.ToString(TokenRewardKey.EarnPerVerificationMailSent);
            var retrievedTokenTransactionForVerificationSender = await DbProbe.TokenAccountTransactions
                .SingleOrDefaultAsync(x => x.AccountId.Equals(otherUser.Id) &&
                                      x.RewardTypeKey.Equals(earnPerVerificationMailSentRewardTypeKey));
            Assert.IsNotNull(retrievedTokenTransactionForVerificationSender,
                "No token transaction was created for the sender of the verification mail.");
            Assert.AreEqual(verificationToUse.UniqueId, retrievedTokenTransactionForVerificationSender.InternalReference,
                "The internal reference on the token transaction for the sender of the verification mail is not the expected.");

            var earnPerVerificationCodeEnteredRewardTypeKey = EnumsHelper.TokenRewardKey.ToString(TokenRewardKey.EarnPerVerificationCodeEntered);
            var retrievedTokenTransactionForEnteringCode = await DbProbe.TokenAccountTransactions
                .SingleOrDefaultAsync(x => x.AccountId.Equals(user.Id) &&
                                      x.RewardTypeKey.Equals(earnPerVerificationCodeEnteredRewardTypeKey));
            Assert.IsNotNull(retrievedTokenTransactionForEnteringCode,
                "No token transaction was created for entering the verification code.");
            Assert.AreEqual(submission.UniqueId, retrievedTokenTransactionForEnteringCode.InternalReference,
                "The internal reference on the token transaction for entering the verification code is not the expected.");

            var earnPerVerificationLuckySenderRewardTypeKey = EnumsHelper.TokenRewardKey.ToString(TokenRewardKey.EarnPerVerificationLuckySender);
            var retrievedTokenTransactionForLuckySender = await DbProbe.TokenAccountTransactions
                .SingleOrDefaultAsync(x => x.AccountId.Equals(otherUser.Id) &&
                                      x.RewardTypeKey.Equals(earnPerVerificationLuckySenderRewardTypeKey));
            Assert.IsNotNull(retrievedTokenTransactionForLuckySender,
                "No token transaction was created for lucky sender.");
            Assert.AreEqual(verificationToUse.UniqueId, retrievedTokenTransactionForLuckySender.InternalReference,
                "The internal reference on the token transaction for lucky sender is not the expected.");
        }

        [Test]
        public async Task ActionsTest_SubmissionWithSentAndCompleteVerifications_SenderAlreadyRewarded()
        {
            var justCreatedVerifications = 0;
            var sentVerifications = 0;
            var sentRewardedVerifications = 1;
            var completeVerifications = 1;
            var areDetailsSubmitted = false;
            var countryId = CountryId.GB;

            var helperContainer = CreateContainer();

            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "1.2.3.5";
            var otherUser = await CreateUser(helperContainer, "test2@test.com", otherUserIpAddress);

            var random = new RandomWrapper(2015);
            var clock = helperContainer.Get<IClock>();

            var submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress,
                    justCreatedVerifications, sentVerifications, sentRewardedVerifications, completeVerifications, areDetailsSubmitted, countryId: countryId);

            var containerUnderTest = CreateContainer();
            SetupClockDisableLuckySender(containerUnderTest);
            var serviceUnderTest = containerUnderTest.Get<ITenancyDetailsSubmissionService>();
            var userTokenService = containerUnderTest.Get<IUserTokenService>();
            var tokenRewardService = containerUnderTest.Get<ITokenRewardService>();

            var retrievedSubmissionAtPoint1 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint1, "Retrieved submission at point 1 is null.");
            Assert.IsNull(retrievedSubmissionAtPoint1.RentPerMonth, "Field RentPerMonth on retrieved submission at point 1 is not the expected.");
            Assert.IsTrue(retrievedSubmissionAtPoint1.TenantVerifications.Any(x => !x.VerifiedOn.HasValue),
                "At point 1 some verifications should have null VerifiedOn field.");
            Assert.IsTrue(retrievedSubmissionAtPoint1.TenantVerifications.Any(x => x.VerifiedOn.HasValue),
                "At point 1 some verifications should not have null VerifiedOn field.");
            Assert.IsTrue(retrievedSubmissionAtPoint1.TenantVerifications.All(x => x.MarkedAsSentOn.HasValue),
                "At point 1 all verifications should have a value in MarkedAsSentOn field.");

            var userBalanceAtPoint1 = await userTokenService.GetBalance(user.Id);
            var otherUserBalanceAtPoint1 = await userTokenService.GetBalance(otherUser.Id);
            Assert.AreEqual(0, userBalanceAtPoint1.balance, "User balance at point 1 is not the expected.");
            Assert.AreEqual(0, otherUserBalanceAtPoint1.balance, "Other User balance at point 1 is not the expected.");

            // EnterVerificationCode
            var verificationToUse = submission.TenantVerifications.Single(x => !x.VerifiedOn.HasValue);
            var completeVerification = submission.TenantVerifications.Single(x => x.VerifiedOn.HasValue);

            var userBalanceAtPoint2 = await userTokenService.GetBalance(user.Id);
            var otherUserBalanceAtPoint2 = await userTokenService.GetBalance(otherUser.Id);
            Assert.AreEqual(0, userBalanceAtPoint2.balance, "User balance at point 2 is not the expected.");
            Assert.AreEqual(0, otherUserBalanceAtPoint2.balance, "Other User balance at point 2 is not the expected.");

            var verificationCodeForm = new VerificationCodeForm
            {
                TenancyDetailsSubmissionUniqueId = submission.UniqueId,
                VerificationCode = verificationToUse.SecretCode
            };

            var userBalanceAtPoint3 = await userTokenService.GetBalance(user.Id);
            var otherUserBalanceAtPoint3 = await userTokenService.GetBalance(otherUser.Id);
            Assert.AreEqual(0, userBalanceAtPoint3.balance, "User balance at point 3 is not the expected.");
            Assert.AreEqual(0, otherUserBalanceAtPoint3.balance, "Other User balance at point 3 is not the expected.");

            var enterVerificationCodeOutcome = await serviceUnderTest.EnterVerificationCode(user.Id, verificationCodeForm);
            Assert.IsFalse(enterVerificationCodeOutcome.IsRejected, "EnterVerificationCode outcome field IsRejected is not the expected.");
            Assert.IsNullOrEmpty(enterVerificationCodeOutcome.RejectionReason);
            Assert.IsFalse(enterVerificationCodeOutcome.ReturnToForm, "EnterVerificationCode outcome field ReturnToForm is not the expected.");
            Assert.IsTrue(enterVerificationCodeOutcome.UiAlerts.Any(x => x.Message.Equals(CommonResources.TokenAccountCreditedForThisAction)),
                "A message for crediting tokens was not found on the UiAlerts of the EnterVerificationCode outcome.");

            var retrievedSubmissionAtPoint5 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint5, "Retrieved submission at point 5 is null.");
            var retrievedUsedVerificationAtPoint5 =
                retrievedSubmissionAtPoint5.TenantVerifications.Single(x => x.UniqueId == verificationToUse.UniqueId);
            Assert.IsTrue(retrievedUsedVerificationAtPoint5.VerifiedOn.HasValue,
                "Field VerifiedOn on used verification retrieved at point 5 does not have a value.");

            var rewardForEnteringCode = tokenRewardService.GetCurrentReward(TokenRewardKey.EarnPerVerificationCodeEntered);
            var rewardForVerificationSender = tokenRewardService.GetCurrentReward(TokenRewardKey.EarnPerVerificationMailSent);

            var userBalanceAtPoint4 = await userTokenService.GetBalance(user.Id);
            var otherUserBalanceAtPoint4 = await userTokenService.GetBalance(otherUser.Id);
            Assert.AreEqual(rewardForEnteringCode.Value, userBalanceAtPoint4.balance, "User balance at point 4 is not the expected.");
            Assert.AreEqual(0, otherUserBalanceAtPoint4.balance,
                "Other User balance at point 4 is not the expected.");

            var earnPerVerificationMailSentRewardTypeKey = EnumsHelper.TokenRewardKey.ToString(TokenRewardKey.EarnPerVerificationMailSent);
            var retrievedTokenTransactionForVerificationSender = await DbProbe.TokenAccountTransactions
                .SingleOrDefaultAsync(x => x.AccountId.Equals(otherUser.Id) &&
                                      x.RewardTypeKey.Equals(earnPerVerificationMailSentRewardTypeKey));
            Assert.IsNull(retrievedTokenTransactionForVerificationSender,
                "No token transaction should be created for the sender of the verification mail if already rewarded.");

            var earnPerVerificationCodeEnteredRewardTypeKey = EnumsHelper.TokenRewardKey.ToString(TokenRewardKey.EarnPerVerificationCodeEntered);
            var retrievedTokenTransactionForEnteringCode = await DbProbe.TokenAccountTransactions
                .SingleOrDefaultAsync(x => x.AccountId.Equals(user.Id) &&
                                      x.RewardTypeKey.Equals(earnPerVerificationCodeEnteredRewardTypeKey));
            Assert.IsNotNull(retrievedTokenTransactionForEnteringCode,
                "No token transaction was created for entering the verification code.");
            Assert.AreEqual(submission.UniqueId, retrievedTokenTransactionForEnteringCode.InternalReference,
                "The internal reference on the token transaction for entering the verification code is not the expected.");
        }

        [Test]
        public async Task ActionsTest_SubmissionWithSentAndCompleteVerificationsAndSubmittedDetails()
        {
            var justCreatedVerifications = 0;
            var sentVerifications = 1;
            var sentRewardedVerifications = 0;
            var completeVerifications = 1;
            var areDetailsSubmitted = true;

            var helperContainer = CreateContainer();

            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "1.2.3.5";
            var otherUser = await CreateUser(helperContainer, "test2@test.com", otherUserIpAddress);

            var random = new RandomWrapper(2015);
            var clock = helperContainer.Get<IClock>();

            var submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress,
                    justCreatedVerifications, sentVerifications, sentRewardedVerifications, completeVerifications, areDetailsSubmitted);

            var containerUnderTest = CreateContainer();
            SetupClockDisableLuckySender(containerUnderTest);
            var serviceUnderTest = containerUnderTest.Get<ITenancyDetailsSubmissionService>();

            var retrievedSubmissionAtPoint1 = await RetrieveSubmission(submission.UniqueId);
            Assert.IsNotNull(retrievedSubmissionAtPoint1, "Retrieved submission at point 1 is null.");
            Assert.IsNotNull(retrievedSubmissionAtPoint1.SubmittedOn, "Field SubmittedOn on retrieved submission at point 1 is not the expected.");
            Assert.IsNotNull(retrievedSubmissionAtPoint1.RentPerMonth, "Field RentPerMonth on retrieved submission at point 1 is not the expected.");
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
                IsFurnished = !submission.IsFurnished.Value,
                RentPerMonth = submission.RentPerMonth.Value + 1,
                NumberOfBedrooms = (byte)(submission.NumberOfBedrooms.Value + 1),
                PropertyConditionRating = (byte)(submission.PropertyConditionRating.Value + 1),
                LandlordRating = (byte)(submission.LandlordRating.Value + 1),
                NeighboursRating = (byte)(submission.NeighboursRating.Value + 1)
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
            Assert.AreEqual(submission.IsFurnished, retrievedSubmissionAtPoint2.IsFurnished, 
                "Field IsFurnished on retrieved submission at point 2 is not the expected.");
            Assert.AreEqual(submission.NumberOfBedrooms, retrievedSubmissionAtPoint2.NumberOfBedrooms, 
                "Field NumberOfBedrooms on retrieved submission at point 2 is not the expected.");
            Assert.AreEqual(submission.PropertyConditionRating, retrievedSubmissionAtPoint2.PropertyConditionRating,
                "Field PropertyConditionRating on retrieved submission at point 2 is not the expected.");
            Assert.AreEqual(submission.LandlordRating, retrievedSubmissionAtPoint2.LandlordRating,
                "Field LandlordRating on retrieved submission at point 2 is not the expected.");
            Assert.AreEqual(submission.NeighboursRating, retrievedSubmissionAtPoint2.NeighboursRating,
                "Field NeighboursRating on retrieved submission at point 2 is not the expected.");
            Assert.IsTrue(retrievedSubmissionAtPoint2.SubmittedOn.HasValue,
                "Field SubmittedOn on retrieved submission at point 2 should have a value.");
            Assert.AreEqual(submission.SubmittedOn, retrievedSubmissionAtPoint2.SubmittedOn,
                "Field SubmittedOn on retrieved submission at point 2 should not be updated.");
            Assert.IsNull(retrievedSubmissionAtPoint2.CurrencyId,
                "Field CurrencyId on retrieved submission at point 2 is not the expected.");

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

        private void SetupAntiAbuseServiceResponse(IKernel container, Action<string, string, CountryId> callback,
            AntiAbuseServiceResponse response)
        {
            var mockAntiAbuseService = new Mock<IAntiAbuseService>();
            mockAntiAbuseService.Setup(x => x.CanCreateTenancyDetailsSubmission(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CountryId>()))
                .Callback(callback)
                .Returns(Task.FromResult(response));

            container.Rebind<IAntiAbuseService>().ToConstant(mockAntiAbuseService.Object);
        }

        private void SetupClockAlwaysLuckySender(IKernel container)
        {
            var realClock = container.Get<IClock>();
            var mockClock = new Mock<IClock>();

            mockClock.Setup(x => x.OffsetNow)
                .Returns(() =>
                {
                    var now = realClock.OffsetNow;
                    return now.AddMilliseconds(-(now.Millisecond % 100));
                });

            mockClock.Setup(x => x.OffsetUtcNow)
                .Returns(() =>
                {
                    var now = realClock.OffsetUtcNow;
                    return now.AddMilliseconds(-(now.Millisecond % 100));
                });

            container.Rebind<IClock>().ToConstant(mockClock.Object);
        }

        private void SetupClockDisableLuckySender(IKernel container)
        {
            var realClock = container.Get<IClock>();
            var mockClock = new Mock<IClock>();

            mockClock.Setup(x => x.OffsetNow)
                .Returns(() =>
                {
                    var now = realClock.OffsetNow;
                    if (now.Millisecond % 100 == 0)
                        return now.AddMilliseconds(1);
                    return now;
                });

            mockClock.Setup(x => x.OffsetUtcNow)
                .Returns(() =>
                {
                    var now = realClock.OffsetUtcNow;
                    if (now.Millisecond % 100 == 0)
                        return now.AddMilliseconds(1);
                    return now;
                });

            container.Rebind<IClock>().ToConstant(mockClock.Object);
        }

        private static async Task<TenancyDetailsSubmission> CreateTenancyDetailsSubmissionAndSave(
            IRandomWrapper random, IKernel container, 
            string userId, string userIpAddress,
            string userIdForVerfications, string userForVerificationsIpAddress,
            int justCreatedVerifications = 0, int sentVerifications = 0, int sentRewardedVerifications = 0, int completeVerifications = 0, 
            bool areDetailsSubmitted = false, string secretCode = null, CountryId countryId = CountryId.GB)
        {
            if (secretCode != null && justCreatedVerifications + sentVerifications + sentRewardedVerifications + completeVerifications != 1)
                throw new ArgumentException("If you choose your own secret secretCode then there should be only a single verification of any kind created.");

            var clock = container.Get<IClock>();
            var dbContext = container.Get<IEpsilonContext>();

            var address = await AddressHelper.CreateRandomAddressAndSave(random, container, userId, userIpAddress, countryId);
            
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
                tenancyDetailsSubmission.IsFurnished = random.NextDouble() >= 0.5;
                tenancyDetailsSubmission.NumberOfBedrooms = (byte)random.Next(0, 5);
                tenancyDetailsSubmission.PropertyConditionRating = (byte)random.Next(1, 5);
                tenancyDetailsSubmission.LandlordRating = (byte)random.Next(1, 5);
                tenancyDetailsSubmission.NeighboursRating = (byte)random.Next(1, 5);
            }

            var verifications = new List<TenantVerification>();

            for (int i = 0; i < justCreatedVerifications; i++)
            {
                var verification = CreateTenantVerification(random, container, userIdForVerfications, userForVerificationsIpAddress, secretCode, 
                    isSent: false, isComplete: false, isSenderRewarded: false);
                verifications.Add(verification);
            }

            for (int i = 0; i < sentVerifications; i++)
            {
                var verification = CreateTenantVerification(random, container, userIdForVerfications, userForVerificationsIpAddress, secretCode, 
                    isSent: true, isComplete: false, isSenderRewarded: false);
                verifications.Add(verification);
            }

            for (int i = 0; i < sentRewardedVerifications; i++)
            {
                var verification = CreateTenantVerification(random, container, userIdForVerfications, userForVerificationsIpAddress, secretCode,
                    isSent: true, isComplete: false, isSenderRewarded: true);
                verifications.Add(verification);
            }

            for (int i = 0; i < completeVerifications; i++)
            {
                var verification = CreateTenantVerification(random, container, userIdForVerfications, userForVerificationsIpAddress, secretCode, 
                    isSent: true, isComplete: true, isSenderRewarded: true);
                verifications.Add(verification);
            }

            tenancyDetailsSubmission.TenantVerifications = verifications;

            dbContext.TenancyDetailsSubmissions.Add(tenancyDetailsSubmission);
            await dbContext.SaveChangesAsync();
            return tenancyDetailsSubmission;
        }

        private static TenantVerification CreateTenantVerification(
            IRandomWrapper random, IKernel container, string userId, string userIpAddress, string secretCode, bool isSent, bool isComplete, bool isSenderRewarded)
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
            if (isSenderRewarded)
                tenantVerification.SenderRewardedOn = clock.OffsetNow;
            return tenantVerification;
        }

        #endregion
    }
}
