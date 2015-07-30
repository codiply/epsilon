﻿using System;
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
            Assert.AreEqual(SubmissionResources.UseAddressConfirmed_AddressNotFoundMessage, outcome.RejectionReason,
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

        #region GetUserSubmissionsSummary

        [Test]
        public async Task GetUserSubmissionSummary_ForUserWithoutSubmissions()
        {
            var helperContainer = CreateContainer();
            var user = await CreateUser(helperContainer, "test@test.com", "1.2.3.4");

            var containerUnderTest = CreateContainer();
            var serviceUnderTest = containerUnderTest.Get<ITenancyDetailsSubmissionService>();


            // Full summary
            var request1 = new MySubmissionsSummaryRequest { limitItemsReturned = false };
            var response1 = await serviceUnderTest.GetUserSubmissionsSummary(user.Id, request1);

            Assert.IsNotNull(response1, "Response1 is null.");
            Assert.IsFalse(response1.moreItemsExist, "Field moreItemsExist on response1 is not the expected.");
            Assert.IsFalse(response1.tenancyDetailsSubmissions.Any(), "Field tenancyDetailsSubmissions on response1 should be empty.");

            // Summary with limit
            var request2 = new MySubmissionsSummaryRequest { limitItemsReturned = true };
            var response2 = await serviceUnderTest.GetUserSubmissionsSummary(user.Id, request2);

            Assert.IsNotNull(response2, "Response2 is null.");
            Assert.IsFalse(response2.moreItemsExist, "Field moreItemsExist on response2 is not the expected.");
            Assert.IsFalse(response2.tenancyDetailsSubmissions.Any(), "Field tenancyDetailsSubmissions on response2 should be empty.");
        }

        [Test]
        public async Task GetUserSubmissionSummary_WithMoreSubmissionsThanLimit_ItemsLimitIsRespected()
        {
            var itemsLimit = 2;
            var submissionsToCreate = 3;

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", "11.12.13.14");


            var random = new RandomWrapper(2015);

            for (var i = 0; i < submissionsToCreate; i++)
            {
                var submission = await CreateTenancyDetailsSubmissionAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress);
                Assert.IsNotNull(submission, string.Format("Submission created for i {0} is null.", i));
            }

            var containerUnderTest = CreateContainer();
            SetupConfigForGetUserSubmissionSummary(containerUnderTest, itemsLimit);
            var serviceUnderTest = containerUnderTest.Get<ITenancyDetailsSubmissionService>();

            // Full summary
            var request1 = new MySubmissionsSummaryRequest { limitItemsReturned = false };
            var response1 = await serviceUnderTest.GetUserSubmissionsSummary(user.Id, request1);

            Assert.IsNotNull(response1, "Response1 is null.");
            Assert.IsFalse(response1.moreItemsExist, "Field moreItemsExist on response1 is not the expected.");
            Assert.AreEqual(submissionsToCreate, response1.tenancyDetailsSubmissions.Count,
                "Response1 should contain all submissions.");

            // Summary with limit
            var request2 = new MySubmissionsSummaryRequest { limitItemsReturned = true };
            var response2 = await serviceUnderTest.GetUserSubmissionsSummary(user.Id, request2);

            Assert.IsNotNull(response2, "Response2 is null.");
            Assert.IsTrue(response2.moreItemsExist, "Field moreItemsExist on response2 is not the expected.");
            Assert.AreEqual(itemsLimit, response2.tenancyDetailsSubmissions.Count,
                "Response1 should contains a number of submissions equal to the limit.");
        }

        #endregion

        #region Private helper functions

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
            string otherUserId, string otherUserIpAddress,
            int outstandingVerifications = 0, int completeVerification = 0, bool isSubmitted = false, bool hasMovedOut = false)
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

            if (isSubmitted)
            {
                tenancyDetailsSubmission.SubmittedOn = clock.OffsetNow;
            }

            if (hasMovedOut)
            {
                tenancyDetailsSubmission.MoveOutDate = clock.OffsetNow.UtcDateTime;
            }

            var verifications = new List<TenantVerification>();

            for (int i = 0; i < outstandingVerifications; i++)
            {
                var verification = CreateTenantVerification(random, container, otherUserId, otherUserIpAddress, isComplete: false);
                verifications.Add(verification);
            }

            for (int i = 0; i < completeVerification; i++)
            {
                var verification = CreateTenantVerification(random, container, otherUserId, otherUserIpAddress, isComplete: true);
                verifications.Add(verification);
            }

            tenancyDetailsSubmission.TenantVerifications = verifications;

            dbContext.TenancyDetailsSubmissions.Add(tenancyDetailsSubmission);
            await dbContext.SaveChangesAsync();
            return tenancyDetailsSubmission;
        }

        private static TenantVerification CreateTenantVerification(
            IRandomWrapper random, IKernel container, string userId, string userIpAddress, bool isComplete)
        {
            var clock = container.Get<IClock>();
            var dbContext = container.Get<IEpsilonContext>();

            var tenantVerification = new TenantVerification
            {
                UniqueId = Guid.NewGuid(),
                AssignedToId = userId,
                AssignedByIpAddress = userIpAddress,
                SecretCode = "secret-code"
            };
            if (isComplete)
                tenantVerification.VerifiedOn = clock.OffsetNow;
            return tenantVerification;
        }

        #endregion
    }
}
