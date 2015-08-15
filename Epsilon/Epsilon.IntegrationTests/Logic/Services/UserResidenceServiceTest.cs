using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.IntegrationTests.TestHelpers;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Wrappers;
using Epsilon.Logic.Wrappers.Interfaces;
using Ninject;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Epsilon.Logic.Helpers.RandomStringHelper;

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class UserResidenceServiceTest : BaseIntegrationTestWithRollback
    {
        [Test]
        public async Task GetResidence_ForUserWithNoSubmissions()
        {
            var container = CreateContainer();
            var user = await CreateUser(container, "test@test.com", "1.2.3.4");

            var service = container.Get<IUserResidenceService>();

            var response = await service.GetResidence(user.Id);

            Assert.IsTrue(response.HasNoSubmissions, "The value for response field HasNoSubmissions is not the expected");
            Assert.IsNull(response.Address, "The value for response field Address should be null.");
            Assert.IsFalse(response.IsVerified, "The value for response field IsVerified is not the expected");
        }

        [Test]
        public async Task GetResidence_ForUserOneUnVerifiedSubmissionReturnsTheSubmission()
        {
            var helperContainer = CreateContainer();
            var ipAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var containerUnderTest = CreateContainer();
            var service = containerUnderTest.Get<IUserResidenceService>();

            var random = new RandomWrapper(2015);

            var countryId = CountryId.GB;
            var address = await AddressHelper.CreateRandomAddressAndSave(random, helperContainer, user.Id, ipAddress, countryId);
            var numberOfIncompleteVerifications = 0;
            var submission = await CreateUnverifiedTenancyDetailsSubmission(
                random, helperContainer, user.Id, ipAddress, address.Id, numberOfIncompleteVerifications);

            var response = await service.GetResidence(user.Id);

            Assert.IsFalse(response.HasNoSubmissions, "The value for response field HasNoSubmissions is not the expected");
            Assert.IsNotNull(response.Address, "The value for response field Address should not be null.");
            Assert.AreEqual(address.Id, response.Address.Id, "The Address.Id field on the response is not the expected.");
            Assert.IsFalse(response.IsVerified, "The value for response field IsVerified is not the expected.");
            Assert.IsNotNull(response.Address.Country, "The Address.Country field on the response should not be null.");
            Assert.AreEqual(countryId, response.Address.Country.IdAsEnum, "The Address.Country on the response is not the expected.");
            Assert.IsNotNull(response.Address.Geometry, "The Address.Geometry field on the response should not be null.");
            Assert.AreEqual(address.Geometry.Latitude, response.Address.Geometry.Latitude,
                "The Address.Geometry.Latitude field on the response is not the expected.");
            Assert.AreEqual(address.Geometry.Longitude, response.Address.Geometry.Longitude,
                "The Address.Geometry.Longitude field on the response is not the expected.");
        }

        [Test]
        public async Task GetResidence_ForUserWithSeveralUnVerifiedSubmissionsReturnsTheLatest()
        {
            var helperContainer = CreateContainer();
            var ipAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var containerUnderTest = CreateContainer();
            var service = containerUnderTest.Get<IUserResidenceService>();

            var random = new RandomWrapper(2015);

            var countryId1 = CountryId.GB;
            var address1 = await AddressHelper.CreateRandomAddressAndSave(random, helperContainer, user.Id, ipAddress, countryId1);
            var numberOfIncompleteVerifications1 = 1;
            var submission1 = await CreateUnverifiedTenancyDetailsSubmission(
                random, helperContainer, user.Id, ipAddress, address1.Id, numberOfIncompleteVerifications1);

            var countryId2 = CountryId.GR;
            var numberOfIncompleteVerifications2 = 2;
            var address2 = await AddressHelper.CreateRandomAddressAndSave(random, helperContainer, user.Id, ipAddress, countryId2);
            var submission2 = await CreateUnverifiedTenancyDetailsSubmission(
                random, helperContainer, user.Id, ipAddress, address2.Id, numberOfIncompleteVerifications2);

            var response = await service.GetResidence(user.Id);

            Assert.IsFalse(response.HasNoSubmissions, "The value for response field HasNoSubmissions is not the expected");
            Assert.IsNotNull(response.Address, "The value for response field Address should not be null.");
            Assert.AreEqual(address2.Id, response.Address.Id, "The response should contain the second address.");
            Assert.IsFalse(response.IsVerified, "The value for response field IsVerified is not the expected.");
            Assert.IsNotNull(response.Address.Country, "The Address.Country field on the response should not be null.");
            Assert.AreEqual(countryId2, response.Address.Country.IdAsEnum,"The Address.Country on the response is not the expected.");
            Assert.IsNotNull(response.Address.Geometry, "The Address.Geometry field on the response should not be null.");
            Assert.AreEqual(address2.Geometry.Latitude, response.Address.Geometry.Latitude, 
                "The Address.Geometry.Latitude field on the response is not the expected.");
            Assert.AreEqual(address2.Geometry.Longitude, response.Address.Geometry.Longitude,
                "The Address.Geometry.Longitude field on the response is not the expected.");
        }

        [Test]
        public async Task GetResidence_ForUserOneVerifiedSubmissionReturnsTheSubmission()
        {
            var helperContainer = CreateContainer();
            var ipAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var containerUnderTest = CreateContainer();
            var service = containerUnderTest.Get<IUserResidenceService>();

            var random = new RandomWrapper(2015);

            var countryId = CountryId.GB;
            var address = await AddressHelper.CreateRandomAddressAndSave(random, helperContainer, user.Id, ipAddress, countryId);
            var numberOfIncompleteVerifications = 0;
            var numberOfCompleteVerifications = 1;
            var submission = await CreateVerifiedTenancyDetailsSubmission(
                random, helperContainer, user.Id, ipAddress, address.Id, numberOfCompleteVerifications, numberOfIncompleteVerifications);

            var response = await service.GetResidence(user.Id);

            Assert.IsFalse(response.HasNoSubmissions, "The value for response field HasNoSubmissions is not the expected");
            Assert.IsNotNull(response.Address, "The value for response field Address should not be null.");
            Assert.AreEqual(address.Id, response.Address.Id, "The Address.Id field on the response is not the expected.");
            Assert.IsTrue(response.IsVerified, "The value for response field IsVerified is not the expected.");
            Assert.IsNotNull(response.Address.Country, "The Address.Country field on the response should not be null.");
            Assert.AreEqual(countryId, response.Address.Country.IdAsEnum, "The Address.Country on the response is not the expected.");
            Assert.IsNotNull(response.Address.Geometry, "The Address.Geometry field on the response should not be null.");
            Assert.AreEqual(address.Geometry.Latitude, response.Address.Geometry.Latitude,
                "The Address.Geometry.Latitude field on the response is not the expected.");
            Assert.AreEqual(address.Geometry.Longitude, response.Address.Geometry.Longitude,
                "The Address.Geometry.Longitude field on the response is not the expected.");
        }

        [Test]
        public async Task GetResidence_ForUserWithSeveralVerifiedSubmissionsReturnsTheLatest()
        {
            var helperContainer = CreateContainer();
            var ipAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var containerUnderTest = CreateContainer();
            var service = containerUnderTest.Get<IUserResidenceService>();

            var random = new RandomWrapper(2015);

            var countryId1 = CountryId.GB;
            var address1 = await AddressHelper.CreateRandomAddressAndSave(random, helperContainer, user.Id, ipAddress, countryId1);
            var numberOfCompleteVerifications1 = 1;
            var numberOfIncompleteVerifications1 = 0;
            var submission1 = await CreateVerifiedTenancyDetailsSubmission(
                random, helperContainer, user.Id, ipAddress, address1.Id, numberOfCompleteVerifications1, numberOfIncompleteVerifications1);

            var countryId2 = CountryId.GR;
            var numberOfCompleteVerifications2 = 1;
            var numberOfIncompleteVerifications2 = 1;
            var address2 = await AddressHelper.CreateRandomAddressAndSave(random, helperContainer, user.Id, ipAddress, countryId2);
            var submission2 = await CreateVerifiedTenancyDetailsSubmission(
                random, helperContainer, user.Id, ipAddress, address2.Id, numberOfCompleteVerifications2, numberOfIncompleteVerifications2);

            var response = await service.GetResidence(user.Id);

            Assert.IsFalse(response.HasNoSubmissions, "The value for response field HasNoSubmissions is not the expected");
            Assert.IsNotNull(response.Address, "The value for response field Address should not be null.");
            Assert.AreEqual(address2.Id, response.Address.Id, "The response should contain the second address.");
            Assert.IsTrue(response.IsVerified, "The value for response field IsVerified is not the expected.");
            Assert.IsNotNull(response.Address.Country, "The Address.Country field on the response should not be null.");
            Assert.AreEqual(countryId2, response.Address.Country.IdAsEnum, "The Address.Country on the response is not the expected.");
            Assert.IsNotNull(response.Address.Geometry, "The Address.Geometry field on the response should not be null.");
            Assert.AreEqual(address2.Geometry.Latitude, response.Address.Geometry.Latitude,
                "The Address.Geometry.Latitude field on the response is not the expected.");
            Assert.AreEqual(address2.Geometry.Longitude, response.Address.Geometry.Longitude,
                "The Address.Geometry.Longitude field on the response is not the expected.");
        }

        [Test]
        public async Task GetResidence_ForUserWithSeveralVerifiedAndVerifiedSubmissionsReturnsTheLatestVerified()
        {
            var helperContainer = CreateContainer();
            var ipAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var containerUnderTest = CreateContainer();
            var service = containerUnderTest.Get<IUserResidenceService>();

            var random = new RandomWrapper(2015);

            // Submission 1 verified
            var countryId1 = CountryId.GB;
            var verifiedAddress1 = await AddressHelper.CreateRandomAddressAndSave(random, helperContainer, user.Id, ipAddress, countryId1);
            var numberOfIncompleteVerifications1 = 0;
            var numberOfCompleteVerifications1 = 1;
            var verifiedSubmission1 = await CreateVerifiedTenancyDetailsSubmission(
                random, helperContainer, user.Id, ipAddress, verifiedAddress1.Id, numberOfCompleteVerifications1, numberOfIncompleteVerifications1);
            
            // Submission 2 unverified
            var countryId2 = CountryId.GR;
            var numberOfIncompleteVerifications2 = 1;
            var unverifiedAddress2 = await AddressHelper.CreateRandomAddressAndSave(random, helperContainer, user.Id, ipAddress, countryId2);
            var submission2 = await CreateUnverifiedTenancyDetailsSubmission(
                random, helperContainer, user.Id, ipAddress, unverifiedAddress2.Id, numberOfIncompleteVerifications2);

            // Submission 3 verified
            var countryId3 = CountryId.GB;
            var verifiedAddress3 = await AddressHelper.CreateRandomAddressAndSave(random, helperContainer, user.Id, ipAddress, countryId3);
            var numberOfIncompleteVerifications3 = 0;
            var numberOfCompleteVerifications3 = 1;
            var verifiedSubmission3 = await CreateVerifiedTenancyDetailsSubmission(
                random, helperContainer, user.Id, ipAddress, verifiedAddress3.Id, numberOfCompleteVerifications3, numberOfIncompleteVerifications3);

            // Submission 4 unverified
            var countryId4 = CountryId.GR;
            var numberOfIncompleteVerifications4 = 1;
            var unverifiedAddress4 = await AddressHelper.CreateRandomAddressAndSave(random, helperContainer, user.Id, ipAddress, countryId4);
            var submission4 = await CreateUnverifiedTenancyDetailsSubmission(
                random, helperContainer, user.Id, ipAddress, unverifiedAddress4.Id, numberOfIncompleteVerifications4);

            var response = await service.GetResidence(user.Id);

            Assert.IsFalse(response.HasNoSubmissions, "The value for response field HasNoSubmissions is not the expected");
            Assert.IsNotNull(response.Address, "The value for response field Address should not be null.");
            Assert.AreEqual(verifiedAddress3.Id, response.Address.Id, "The response should contain the second address.");
            Assert.IsTrue(response.IsVerified, "The value for response field IsVerified is not the expected.");
            Assert.IsNotNull(response.Address.Country, "The Address.Country field on the response should not be null.");
            Assert.AreEqual(countryId3, response.Address.Country.IdAsEnum, "The Address.Country on the response is not the expected.");
            Assert.IsNotNull(response.Address.Geometry, "The Address.Geometry field on the response should not be null.");
            Assert.AreEqual(verifiedAddress3.Geometry.Latitude, response.Address.Geometry.Latitude,
                "The Address.Geometry.Latitude field on the response is not the expected.");
            Assert.AreEqual(verifiedAddress3.Geometry.Longitude, response.Address.Geometry.Longitude,
                "The Address.Geometry.Longitude field on the response is not the expected.");
        }

        private async static Task<TenancyDetailsSubmission> CreateUnverifiedTenancyDetailsSubmission(
            IRandomWrapper random, IKernel container, string userId, string userIpAddress, long addressId, int numberOfIncompleteVerifications)
        {
            var verifications = new List<TenantVerification>();
            for (var i = 0; i < numberOfIncompleteVerifications; i++)
            {
                var verification = new TenantVerification
                {
                    UniqueId = Guid.NewGuid(),
                    AssignedToId = userId,
                    AssignedByIpAddress = userIpAddress,
                    SecretCode = RandomStringHelper.GetString(random, AppConstant.SECRET_CODE_MAX_LENGTH, CharacterCase.Mixed),
                    VerifiedOn = null
                };
                verifications.Add(verification);
            }

            var submission = new TenancyDetailsSubmission
            {
                UniqueId = Guid.NewGuid(),
                UserId = userId,
                CreatedByIpAddress = userIpAddress,
                AddressId = addressId,
                TenantVerifications = verifications
            };

            var dbContext = container.Get<IEpsilonContext>();
            dbContext.TenancyDetailsSubmissions.Add(submission);
            await dbContext.SaveChangesAsync();
            
            return submission;
        }

        private async static Task<TenancyDetailsSubmission> CreateVerifiedTenancyDetailsSubmission(
            IRandomWrapper random, IKernel container, string userId, string userIpAddress, long addressId, 
            int numberOfCompleteVerifications, int numberOfIncompleteVerifications)
        {
            var clock = container.Get<IClock>();

            var verifications = new List<TenantVerification>();
            for (var i = 0; i < numberOfIncompleteVerifications; i++)
            {
                var verification = new TenantVerification
                {
                    UniqueId = Guid.NewGuid(),
                    AssignedToId = userId,
                    AssignedByIpAddress = userIpAddress,
                    SecretCode = RandomStringHelper.GetString(random, AppConstant.SECRET_CODE_MAX_LENGTH, CharacterCase.Mixed),
                    VerifiedOn = null
                };
                verifications.Add(verification);
            }

            for (var i = 0; i < numberOfCompleteVerifications; i++)
            {
                var verification = new TenantVerification
                {
                    UniqueId = Guid.NewGuid(),
                    AssignedToId = userId,
                    AssignedByIpAddress = userIpAddress,
                    SecretCode = RandomStringHelper.GetString(random, AppConstant.SECRET_CODE_MAX_LENGTH, CharacterCase.Mixed),
                    VerifiedOn = clock.OffsetNow
                };
                verifications.Add(verification);
            }

            var submission = new TenancyDetailsSubmission
            {
                UniqueId = Guid.NewGuid(),
                UserId = userId,
                CreatedByIpAddress = userIpAddress,
                AddressId = addressId,
                TenantVerifications = verifications
            };

            var dbContext = container.Get<IEpsilonContext>();
            dbContext.TenancyDetailsSubmissions.Add(submission);
            await dbContext.SaveChangesAsync();

            return submission;
        }
    }
}
