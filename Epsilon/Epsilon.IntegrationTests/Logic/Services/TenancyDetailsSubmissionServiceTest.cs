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

            var submissionId = Guid.NewGuid();
            var addressId = Guid.NewGuid();
            var outcome = await service.Create(user.Id, ipAddress, submissionId, addressId);

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

            var address = await CreateRandomAddress(helperContainer, user.Id, ipAddress);

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

            var submissionId = Guid.NewGuid();
            var outcome = await service.Create(user.Id, ipAddress, submissionId, address.Id);

            Assert.IsTrue(outcome.IsRejected, "The field IsRejected on the outcome should be true.");
            Assert.AreEqual(antiAbuseRejectionReason, outcome.RejectionReason,
                "The RejectionReason on the outcome is not the expected.");
            Assert.AreEqual(user.Id, userIdUsedInAntiAbuse, 
                "The UserId used in the call to AntiAbuseService is not the expected.");
            Assert.AreEqual(ipAddress, ipAddressUsedInAntiAbuse, 
                "The IpAddress used in the call to AntiAbuseService is not the expected.");

            var retrievedTenancyDetailsSubmission = await DbProbe.TenancyDetailsSubmissions.FindAsync(submissionId);
            Assert.IsNull(retrievedTenancyDetailsSubmission, "A TenancyDetailsSubmission should not be created.");
        }

        [Test]
        public async Task Create_SuccessfulCase()
        {
            var ipAddress = "1.2.3.4";
            var helperContainer = CreateContainer();
            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var address = await CreateRandomAddress(helperContainer, user.Id, ipAddress);

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

            var submissionId = Guid.NewGuid();
            var timeBefore = DateTimeOffset.Now;
            var outcome = await service.Create(user.Id, ipAddress, submissionId, address.Id);

            Assert.IsFalse(outcome.IsRejected, "The field IsRejected on the outcome should be false.");
            Assert.AreEqual(submissionId, outcome.TenancyDetailsSubmissionId,
                "The TenancyDetailsSubmissionId on the outcome is not the expected.");
            Assert.AreEqual(user.Id, userIdUsedInAntiAbuse,
                "The UserId used in the call to AntiAbuseService is not the expected.");
            Assert.AreEqual(ipAddress, ipAddressUsedInAntiAbuse,
                "The IpAddress used in the call to AntiAbuseService is not the expected.");

            var timeAfter = DateTimeOffset.Now;

            var retrievedTenancyDetailsSubmission = await DbProbe.TenancyDetailsSubmissions.FindAsync(submissionId);
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

        #endregion

        #region Private helper functions

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

        private async Task<Address> CreateRandomAddress(IKernel container, string userId, string userIpAddress)
        {
            var random = new RandomWrapper();
            var randomFieldLength = 10;
            var address = new Address
            {
                Id = Guid.NewGuid(),
                Line1 = RandomStringHelper.GetAlphaNumericString(random, randomFieldLength, RandomStringHelper.CharacterCase.Mixed),
                Line2 = RandomStringHelper.GetAlphaNumericString(random, randomFieldLength, RandomStringHelper.CharacterCase.Mixed),
                Line3 = RandomStringHelper.GetAlphaNumericString(random, randomFieldLength, RandomStringHelper.CharacterCase.Mixed),
                Line4 = RandomStringHelper.GetAlphaNumericString(random, randomFieldLength, RandomStringHelper.CharacterCase.Mixed),
                Locality = RandomStringHelper.GetAlphaNumericString(random, randomFieldLength, RandomStringHelper.CharacterCase.Mixed),
                Region = RandomStringHelper.GetAlphaNumericString(random, randomFieldLength, RandomStringHelper.CharacterCase.Mixed),
                Postcode = RandomStringHelper.GetAlphaNumericString(random, randomFieldLength, RandomStringHelper.CharacterCase.Mixed),
                CountryId = "GB",
                CreatedById = userId,
                CreatedByIpAddress = userIpAddress
            };
            var dbContext = container.Get<IEpsilonContext>();
            dbContext.Addresses.Add(address);
            await dbContext.SaveChangesAsync();
            return address;
        }

        #endregion
    }
}
