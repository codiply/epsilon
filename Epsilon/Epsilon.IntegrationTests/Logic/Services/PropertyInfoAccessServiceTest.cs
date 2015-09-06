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
using System.Data.Entity;
using Epsilon.Resources.Logic.PropertyInfoAccess;
using Epsilon.Resources.Common;

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class PropertyInfoAccessServiceTest : BaseIntegrationTestWithRollback
    {
        private TimeSpan _smallDelay = TimeSpan.FromMilliseconds(20);

        #region Create

        [Test]
        public async Task Create_AddressDoesNotExist()
        {
            var helperContainer = CreateContainer();
            var ipAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var containerUnderTest = CreateContainer();
            var serviceUnderTest = containerUnderTest.Get<IPropertyInfoAccessService>();

            var nonExistentAddressUniqueId = Guid.NewGuid();
            var accessUniqueId = Guid.NewGuid();
            var outcome = await serviceUnderTest.Create(user.Id, ipAddress, accessUniqueId, nonExistentAddressUniqueId);

            Assert.IsTrue(outcome.IsRejected, "Outcome field IsRejected is not the expected.");
            Assert.AreEqual(PropertyInfoAccessResources.Create_AddressNotFoundMessage, outcome.RejectionReason, 
                "Outcome field RejectionReason is not the expected.");
            Assert.IsNull(outcome.PropertyInfoAccessUniqueId,
                "Outcome field PropertInfoAccessUniqueId is not the expected.");

            var retrievedPropertyInfoAccess = await DbProbe.PropertyInfoAccesses
                .SingleOrDefaultAsync(x => x.UniqueId.Equals(accessUniqueId));
            Assert.IsNull(retrievedPropertyInfoAccess, "A property info accesses should not be created.");
        }

        [Test]
        public async Task Create_ExistingUnexpiredAccessExists()
        {
            var expiryPeriodInDays = 1;

            var helperContainer = CreateContainer();
            var ipAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);
            var ipAddressForSubmission = "1.2.3.5";
            var userForSubmissions = await CreateUser(helperContainer, "test1@test.com", ipAddressForSubmission);


            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, expiryPeriodInDays: expiryPeriodInDays);
            var serviceUnderTest = containerUnderTest.Get<IPropertyInfoAccessService>();

            var random = new RandomWrapper(2015);

            var existingPropertyInfoAccess = await CreatePropertyInfoAccessAndSave(random, helperContainer,
                user.Id, ipAddress, userForSubmissions.Id, ipAddressForSubmission);

            Assert.IsNotNull(existingPropertyInfoAccess, "Existing property info access was not created.");

            var retrievedAddress = await DbProbe.Addresses.FindAsync(existingPropertyInfoAccess.AddressId);

            var accessUniqueId = Guid.NewGuid();

            var outcome = await serviceUnderTest.Create(user.Id, ipAddress, accessUniqueId, retrievedAddress.UniqueId);

            Assert.IsTrue(outcome.IsRejected, "Outcome field IsRejected is not the expected.");
            Assert.AreEqual(CommonResources.GenericInvalidActionMessage, outcome.RejectionReason,
                "Outcome field RejectionReason is not the expected.");
            Assert.IsNull(outcome.PropertyInfoAccessUniqueId,
                "Outcome field PropertInfoAccessUniqueId is not the expected.");

            var retrievedPropertyInfoAccess = await DbProbe.PropertyInfoAccesses
                .SingleOrDefaultAsync(x => x.UniqueId.Equals(accessUniqueId));
            Assert.IsNull(retrievedPropertyInfoAccess, "A property info accesses should not be created.");
        }

        [Test]
        public async Task Create_AddressHasNoCompleteSubmissions()
        {
            var helperContainer = CreateContainer();
            var ipAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var containerUnderTest = CreateContainer();
            var serviceUnderTest = containerUnderTest.Get<IPropertyInfoAccessService>();

            var random = new RandomWrapper(2015);

            var address = await AddressHelper.CreateRandomAddressAndSave(random, helperContainer, user.Id, ipAddress, CountryId.GB);

            var accessUniqueId = Guid.NewGuid();
            var outcome = await serviceUnderTest.Create(user.Id, ipAddress, accessUniqueId, address.UniqueId);

            Assert.IsTrue(outcome.IsRejected, "Outcome field IsRejected is not the expected.");
            Assert.AreEqual(CommonResources.GenericInvalidActionMessage, outcome.RejectionReason,
                "Outcome field RejectionReason is not the expected.");
            Assert.IsNull(outcome.PropertyInfoAccessUniqueId,
                "Outcome field PropertInfoAccessUniqueId is not the expected.");

            var retrievedPropertyInfoAccess = await DbProbe.PropertyInfoAccesses
                .SingleOrDefaultAsync(x => x.UniqueId.Equals(accessUniqueId));
            Assert.IsNull(retrievedPropertyInfoAccess, "A property info accesses should not be created.");
        }

        [Test]
        public async Task Create_InsufficientFunds()
        {
            var submissionsToCreate = 1;

            var helperContainer = CreateContainer();
            var ipAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var containerUnderTest = CreateContainer();
            var serviceUnderTest = containerUnderTest.Get<IPropertyInfoAccessService>();

            var random = new RandomWrapper(2015);

            var address = await AddressHelper.CreateRandomAddressAndSave(random, helperContainer, user.Id, ipAddress, CountryId.GB);
            var submissions = await CreateSubmissionsAndSave(random, helperContainer, address.Id, user.Id, ipAddress, submissionsToCreate);

            Assert.IsNotEmpty(submissions, "Submissions were not created.");

            var accessUniqueId = Guid.NewGuid();
            var outcome = await serviceUnderTest.Create(user.Id, ipAddress, accessUniqueId, address.UniqueId);

            Assert.IsTrue(outcome.IsRejected, "Outcome field IsRejected is not the expected.");
            Assert.AreEqual(CommonResources.InsufficientTokensErrorMessage, outcome.RejectionReason,
                "Outcome field RejectionReason is not the expected.");
            Assert.IsNull(outcome.PropertyInfoAccessUniqueId,
                "Outcome field PropertInfoAccessUniqueId is not the expected.");
           
            var retrievedPropertyInfoAccess = await DbProbe.PropertyInfoAccesses
                .SingleOrDefaultAsync(x => x.UniqueId.Equals(accessUniqueId));
            Assert.IsNull(retrievedPropertyInfoAccess, "A property info accesses should not be created.");
        }

        [Test]
        public async Task Create_Success()
        {
            var submissionsToCreate = 1;

            var helperContainer = CreateContainer();
            var ipAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var clock = helperContainer.Get<IClock>();

            var containerUnderTest = CreateContainer();
            var serviceUnderTest = containerUnderTest.Get<IPropertyInfoAccessService>();

            var random = new RandomWrapper(2015);

            var address = await AddressHelper.CreateRandomAddressAndSave(random, helperContainer, user.Id, ipAddress, CountryId.GB);
            var submissions = await CreateSubmissionsAndSave(random, helperContainer, address.Id, user.Id, ipAddress, submissionsToCreate);

            Assert.IsNotEmpty(submissions, "Submissions were not created.");

            var tokenRewardService = helperContainer.Get<ITokenRewardService>();

            var propertyInfoAccessCost =
                tokenRewardService.GetCurrentReward(TokenRewardKey.SpendPerPropertyInfoAccess).AbsValue;
            var tenancyDetailsSubmissionReward =
                tokenRewardService.GetCurrentReward(TokenRewardKey.EarnPerTenancyDetailsSubmission).AbsValue;

            Assert.That(tenancyDetailsSubmissionReward, Is.GreaterThanOrEqualTo(propertyInfoAccessCost),
                "This test assumes that by making a transaction for submitting tenancy details it will create enough funds to buy a property info access.");

            var userTokenService = helperContainer.Get<IUserTokenService>();
            var balance1 = await userTokenService.GetBalance(user.Id);
            Assert.AreEqual(0, balance1.balance, "Balance1 is not the expected.");
            // I add funds to spend later on.
            var tenancyDetailsSubmissionTransactionStatus = await userTokenService.MakeTransaction(user.Id, TokenRewardKey.EarnPerTenancyDetailsSubmission);
            Assert.AreEqual(TokenAccountTransactionStatus.Success, tenancyDetailsSubmissionTransactionStatus,
                "Adding funds failed.");

            var balance2 = await userTokenService.GetBalance(user.Id);
            Assert.AreEqual(tenancyDetailsSubmissionReward, balance2.balance, "Balance2 is not the expected.");

            var accessUniqueId = Guid.NewGuid();
            var timeBefore = clock.OffsetNow;
            var outcome = await serviceUnderTest.Create(user.Id, ipAddress, accessUniqueId, address.UniqueId);

            Assert.IsFalse(outcome.IsRejected, "Outcome field IsRejected is not the expected.");
            Assert.IsNullOrEmpty(outcome.RejectionReason, "Outcome field RejectionReason is not the expected.");
            Assert.AreEqual(accessUniqueId, outcome.PropertyInfoAccessUniqueId, 
                "Outcome field PropertInfoAccessUniqueId is not the expected.");

            var timeAfter = clock.OffsetNow;

            var retrievedPropertyInfoAccess = await DbProbe.PropertyInfoAccesses
                .SingleOrDefaultAsync(x => x.UniqueId.Equals(accessUniqueId));
            Assert.IsNotNull(retrievedPropertyInfoAccess, "A property info accesses should be created.");

            var rewardTypeKey = EnumsHelper.TokenRewardKey.ToString(TokenRewardKey.SpendPerPropertyInfoAccess);
            var retrievedTransaction = await DbProbe.TokenAccountTransactions
                .SingleOrDefaultAsync(x => x.AccountId.Equals(user.Id) && x.RewardTypeKey.Equals(rewardTypeKey));
            Assert.IsNotNull(retrievedTransaction, "A transaction for the property info access was not created.");
            Assert.AreEqual(accessUniqueId, retrievedTransaction.InternalReference,
                "The internal reference on the transaction was not the expected.");
            Assert.That(retrievedTransaction.MadeOn, Is.GreaterThanOrEqualTo(timeBefore),
                "RetrievedTransaction.MadeOn lower bound test failed.");
            Assert.That(retrievedTransaction.MadeOn, Is.LessThanOrEqualTo(timeAfter),
                "RetrievedTransaction.MadeOn upper bound test failed.");

            var balance3 = await userTokenService.GetBalance(user.Id);
            Assert.AreEqual(balance2.balance - propertyInfoAccessCost, balance3.balance, "Balance3 is not the expected.");
        }

        [Test]
        public async Task Create_Success_WithExistingdExpiredAccess()
        {

            var expiryPeriod = TimeSpan.FromSeconds(0.2);
            var expiryPeriodInDays = expiryPeriod.TotalDays;

            var helperContainer = CreateContainer();
            var ipAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);
            var userForSubmissionIpAddress = "1.2.3.5";
            var userForSubmission = await CreateUser(helperContainer, "test1@test.com", userForSubmissionIpAddress);

            var clock = helperContainer.Get<IClock>();

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, expiryPeriodInDays: expiryPeriodInDays);
            var serviceUnderTest = containerUnderTest.Get<IPropertyInfoAccessService>();

            var random = new RandomWrapper(2015);

            var existingPeropertyInfoAccess = await CreatePropertyInfoAccessAndSave(
                random, helperContainer, user.Id, ipAddress, userForSubmission.Id, userForSubmissionIpAddress);
            var retrievedAddresss = await DbProbe.Addresses.FindAsync(existingPeropertyInfoAccess.AddressId);

            var tokenRewardService = helperContainer.Get<ITokenRewardService>();

            var propertyInfoAccessCost =
                tokenRewardService.GetCurrentReward(TokenRewardKey.SpendPerPropertyInfoAccess).AbsValue;
            var tenancyDetailsSubmissionReward =
                tokenRewardService.GetCurrentReward(TokenRewardKey.EarnPerTenancyDetailsSubmission).AbsValue;

            Assert.That(tenancyDetailsSubmissionReward, Is.GreaterThanOrEqualTo(propertyInfoAccessCost),
                "This test assumes that by making a transaction for submitting tenancy details it will create enough funds to buy a property info access.");

            var userTokenService = helperContainer.Get<IUserTokenService>();
            // I add funds to spend later on.
            var tenancyDetailsSubmissionTransactionStatus = await userTokenService.MakeTransaction(user.Id, TokenRewardKey.EarnPerTenancyDetailsSubmission);
            Assert.AreEqual(TokenAccountTransactionStatus.Success, tenancyDetailsSubmissionTransactionStatus,
                "Adding funds failed.");

            var accessUniqueId = Guid.NewGuid();
            var failedOutcome = await serviceUnderTest.Create(user.Id, ipAddress, accessUniqueId, retrievedAddresss.UniqueId);
            Assert.IsTrue(failedOutcome.IsRejected, "The creation should be rejected before the expiry of the existing property info access.");

            // I wait until the existing property info access expires
            await Task.Delay(expiryPeriod);

            var timeBefore = clock.OffsetNow;
            var outcome = await serviceUnderTest.Create(user.Id, ipAddress, accessUniqueId, retrievedAddresss.UniqueId);

            Assert.IsFalse(outcome.IsRejected, "Outcome field IsRejected is not the expected.");
            Assert.IsNullOrEmpty(outcome.RejectionReason, "Outcome field RejectionReason is not the expected.");
            Assert.AreEqual(accessUniqueId, outcome.PropertyInfoAccessUniqueId,
                "Outcome field PropertInfoAccessUniqueId is not the expected.");

            var timeAfter = clock.OffsetNow;

            var retrievedPropertyInfoAccess = await DbProbe.PropertyInfoAccesses
                .SingleOrDefaultAsync(x => x.UniqueId.Equals(accessUniqueId));
            Assert.IsNotNull(retrievedPropertyInfoAccess, "A property info accesses should be created.");

            var rewardTypeKey = EnumsHelper.TokenRewardKey.ToString(TokenRewardKey.SpendPerPropertyInfoAccess);
            var retrievedTransaction = await DbProbe.TokenAccountTransactions
                .SingleOrDefaultAsync(x => x.AccountId.Equals(user.Id) && x.RewardTypeKey.Equals(rewardTypeKey));
            Assert.IsNotNull(retrievedTransaction, "A transaction for the property info access was not created.");
            Assert.AreEqual(accessUniqueId, retrievedTransaction.InternalReference,
                "The internal reference on the transaction was not the expected.");
            Assert.That(retrievedTransaction.MadeOn, Is.GreaterThanOrEqualTo(timeBefore),
                "RetrievedTransaction.MadeOn lower bound test failed.");
            Assert.That(retrievedTransaction.MadeOn, Is.LessThanOrEqualTo(timeAfter),
                "RetrievedTransaction.MadeOn upper bound test failed.");
        }

        #endregion

        #region GetInfo

        [Test]
        public async Task GetInfo_AccessDoesNotExist()
        {
            var helperContainer = CreateContainer();
            var ipAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);

            var containerUnderTest = CreateContainer();
            var serviceUnderTest = containerUnderTest.Get<IPropertyInfoAccessService>();

            var nonExistentAccessUniqueId = Guid.NewGuid();
            var outcome = await serviceUnderTest.GetInfo(user.Id, nonExistentAccessUniqueId);

            Assert.IsTrue(outcome.IsRejected, "IsRejected field on the outcome is not the expected.");
            Assert.AreEqual(CommonResources.GenericInvalidRequestMessage, outcome.RejectionReason,
                "RejectionReason field on the outcome is not the expected.");
            Assert.IsNull(outcome.PropertyInfo, "PropertyInfo field on the outcome should be null");
        }

        [Test]
        public async Task GetInfo_UnexpiredAccess()
        {
            var expiryPeriod = TimeSpan.FromDays(1);
            var expiryPeriodInDays = expiryPeriod.TotalDays;
            var submissionsToCreate = 3;

            var helperContainer = CreateContainer();
            var ipAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);
            var otherUserIpAddress = "1.2.3.5";
            var otherUser = await CreateUser(helperContainer, "test1@test.com", otherUserIpAddress);

            var random = new RandomWrapper(2015);

            var address = await AddressHelper.CreateRandomAddressAndSave(random, helperContainer, otherUser.Id, otherUserIpAddress, CountryId.GB);
            var submissions = await CreateSubmissionsAndSave(random, helperContainer, address.Id, otherUser.Id, otherUserIpAddress, submissionsToCreate);
            Assert.IsNotEmpty(submissions, "Submissions were not created.");

            var incompleteSubmissions = await CreateIncompleteSubmissionsAndSave(random, helperContainer, address.Id, otherUser.Id, otherUserIpAddress, submissionsToCreate);
            Assert.IsNotEmpty(submissions, "Incomplete submissions were not created.");

            var hiddenSubmissions = await CreateSubmissionsAndSave(
                random, helperContainer, address.Id, otherUser.Id, otherUserIpAddress, submissionsToCreate, submissionsAreHidden: true);
            Assert.IsNotEmpty(submissions, "Hidden submissions were not created.");

            var submissionsOrdered = submissions.OrderByDescending(x => x.SubmittedOn).ToList();

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, expiryPeriodInDays: expiryPeriodInDays);
            var serviceUnderTest = containerUnderTest.Get<IPropertyInfoAccessService>();

            var tokenRewardService = helperContainer.Get<ITokenRewardService>();

            var propertyInfoAccessCost =
                tokenRewardService.GetCurrentReward(TokenRewardKey.SpendPerPropertyInfoAccess).AbsValue;
            var tenancyDetailsSubmissionReward =
                tokenRewardService.GetCurrentReward(TokenRewardKey.EarnPerTenancyDetailsSubmission).AbsValue;

            Assert.That(tenancyDetailsSubmissionReward, Is.GreaterThanOrEqualTo(propertyInfoAccessCost),
                "This test assumes that by making a transaction for submitting tenancy details it will create enough funds to buy a property info access.");

            var userTokenService = helperContainer.Get<IUserTokenService>();
            // I add funds to spend later on.
            var tenancyDetailsSubmissionTransactionStatus = await userTokenService.MakeTransaction(user.Id, TokenRewardKey.EarnPerTenancyDetailsSubmission);
            Assert.AreEqual(TokenAccountTransactionStatus.Success, tenancyDetailsSubmissionTransactionStatus, "Adding funds failed.");

            // I create the property info access
            var accessUniqueId = Guid.NewGuid();
            var helperService = helperContainer.Get<IPropertyInfoAccessService>();
            var createOutcome = await helperService.Create(user.Id, ipAddress, accessUniqueId, address.UniqueId);
            Assert.IsFalse(createOutcome.IsRejected, "Property info access was not created.");

            var getInfoWithOtherUser = await serviceUnderTest.GetInfo(otherUser.Id, accessUniqueId);
            Assert.IsTrue(getInfoWithOtherUser.IsRejected, 
                "IsRejected field when trying to get the info with wrong user is not the expected.");
            Assert.AreEqual(CommonResources.GenericInvalidRequestMessage, getInfoWithOtherUser.RejectionReason,
                "RejectionReason field when trying to get the info with wrong user is not the expected.");
            Assert.IsNull(getInfoWithOtherUser.PropertyInfo,
                "PropertyInfo field when trying to get the info with wrong user is not the expected.");

            var getInfo = await serviceUnderTest.GetInfo(user.Id, accessUniqueId);
            Assert.IsFalse(getInfo.IsRejected,
                "IsRejected field when trying to get the info with the right user is not the expected.");
            Assert.IsNullOrEmpty(getInfo.RejectionReason,
                "RejectionReason field when trying to get the info with the right user is not the expected.");
            Assert.IsNotNull(getInfo.PropertyInfo,
                "PropertyInfo field when trying to get the info with the right user should not be null");

            var retrievedAddress = await DbProbe.Addresses.Include(x => x.Country).SingleAsync(x => x.Id.Equals(address.Id));

            Assert.IsNotNull(getInfo.PropertyInfo.MainProperty, "PropertyInfo.MainProperty should not be null.");
            Assert.AreEqual(retrievedAddress.FullAddress(), getInfo.PropertyInfo.MainProperty.DisplayAddress,
                "PropertyInfo.MainProperty.DisplayAddress is not the expected.");
            Assert.AreEqual(submissionsToCreate, getInfo.PropertyInfo.MainProperty.CompleteSubmissions.Count,
                "The number of complete submissions on the MainProperty is not the expected.");
            Assert.IsEmpty(getInfo.PropertyInfo.DuplicateProperties, "PropertyInfo.DuplicatProperties should be null.");

            for (var i = 0; i < submissionsToCreate; i++)
            {
                var submissionId = submissionsOrdered[i].Id;
                var expected = await DbProbe.TenancyDetailsSubmissions
                    .Include(x => x.Currency).SingleOrDefaultAsync(x => x.Id.Equals(submissionId));
                var actual = getInfo.PropertyInfo.MainProperty.CompleteSubmissions[i];

                Assert.AreEqual(expected.RentPerMonth, actual.RentPerMonth,
                    string.Format("RentPerMonth is not the expected for main property submission and i equal to '{0}'", i));
                Assert.AreEqual(expected.Currency.Symbol, actual.CurrencySymbol,
                    string.Format("CurrencySymbol is not the expected for main property submission and i equal to '{0}'", i));
                Assert.AreEqual(expected.NumberOfBedrooms, actual.NumberOfBedrooms,
                    string.Format("NumberOfBedrooms is not the expected for main property submission and i equal to '{0}'", i));
                Assert.AreEqual(expected.IsFurnished, actual.IsFurnished,
                    string.Format("IsFurnished is not the expected for main property submission and i equal to '{0}'", i));
                Assert.AreEqual(expected.IsPartOfProperty, actual.IsPartOfProperty,
                    string.Format("IsPartOfProperty is not the expected for main property submission and i equal to '{0}'", i));
                Assert.AreEqual(expected.SubmittedOn, actual.SubmittedOn,
                    string.Format("SubmittedOn is not the expected for main property submission and i equal to '{0}'", i));
                Assert.AreEqual(expected.LandlordRating, actual.LandlordRating,
                    string.Format("LandlordRating is not the expected for main property submission and i equal to '{0}'", i));
                Assert.AreEqual(expected.NeighboursRating, actual.NeighboursRating,
                    string.Format("NeighboursRating is not the expected for main property submission and i equal to '{0}'", i));
                Assert.AreEqual(expected.PropertyConditionRating, actual.PropertyConditionRating,
                    string.Format("PropertyConditionRating is not the expected for main property submission and i equal to '{0}'", i));
            }
        }

        [Test]
        public async Task GetInfo_ExpiredAccess()
        {
            var expiryPeriod = TimeSpan.FromSeconds(0.2);
            var expiryPeriodInDays = expiryPeriod.TotalDays;
            var submissionsToCreate = 3;

            var helperContainer = CreateContainer();
            var ipAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);
            var otherUserIpAddress = "1.2.3.5";
            var otherUser = await CreateUser(helperContainer, "test1@test.com", otherUserIpAddress);

            var random = new RandomWrapper(2015);

            var address = await AddressHelper.CreateRandomAddressAndSave(random, helperContainer, otherUser.Id, otherUserIpAddress, CountryId.GB);
            var submissions = await CreateSubmissionsAndSave(random, helperContainer, address.Id, otherUser.Id, otherUserIpAddress, submissionsToCreate);

            Assert.IsNotEmpty(submissions, "Submissions were not created.");

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, expiryPeriodInDays: expiryPeriodInDays);
            var serviceUnderTest = containerUnderTest.Get<IPropertyInfoAccessService>();

            var tokenRewardService = helperContainer.Get<ITokenRewardService>();

            var propertyInfoAccessCost =
                tokenRewardService.GetCurrentReward(TokenRewardKey.SpendPerPropertyInfoAccess).AbsValue;
            var tenancyDetailsSubmissionReward =
                tokenRewardService.GetCurrentReward(TokenRewardKey.EarnPerTenancyDetailsSubmission).AbsValue;

            Assert.That(tenancyDetailsSubmissionReward, Is.GreaterThanOrEqualTo(propertyInfoAccessCost),
                "This test assumes that by making a transaction for submitting tenancy details it will create enough funds to buy a property info access.");

            var userTokenService = helperContainer.Get<IUserTokenService>();
            // I add funds to spend later on.
            var tenancyDetailsSubmissionTransactionStatus = await userTokenService.MakeTransaction(user.Id, TokenRewardKey.EarnPerTenancyDetailsSubmission);
            Assert.AreEqual(TokenAccountTransactionStatus.Success, tenancyDetailsSubmissionTransactionStatus, "Adding funds failed.");

            // I create the property info access
            var accessUniqueId = Guid.NewGuid();
            var helperService = helperContainer.Get<IPropertyInfoAccessService>();
            var createOutcome = await helperService.Create(user.Id, ipAddress, accessUniqueId, address.UniqueId);
            Assert.IsFalse(createOutcome.IsRejected, "Property info access was not created.");

            var getInfoBeforeExpiry = await serviceUnderTest.GetInfo(user.Id, accessUniqueId);
            Assert.IsFalse(getInfoBeforeExpiry.IsRejected,
                "IsRejected field before expiry is not the expected.");
            Assert.IsNullOrEmpty(getInfoBeforeExpiry.RejectionReason,
                "RejectionReason field before expiry is not the expected.");
            Assert.IsNotNull(getInfoBeforeExpiry.PropertyInfo,
                "PropertyInfo field before expiry is not the expected.");

            await Task.Delay(expiryPeriod);

            var getInfoAfterExpiry = await serviceUnderTest.GetInfo(user.Id, accessUniqueId);
            Assert.IsTrue(getInfoAfterExpiry.IsRejected,
                "IsRejected field after expiry is not the expected.");
            Assert.AreEqual(CommonResources.GenericInvalidActionMessage, getInfoAfterExpiry.RejectionReason,
                "RejectionReason field after expiry is not the expected.");
            Assert.IsNull(getInfoAfterExpiry.PropertyInfo,
                "PropertyInfo field after expiry is not the expected.");
        }

        [Test]
        public async Task GetInfo_WithDuplicateAddresses()
        {
            var expiryPeriod = TimeSpan.FromDays(1);
            var expiryPeriodInDays = expiryPeriod.TotalDays;
            var submissionsToCreate = 3;
            var distinctAddressCode = "distinct-address-code";

            var helperContainer = CreateContainer();
            var ipAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", ipAddress);
            var otherUserIpAddress = "1.2.3.5";
            var otherUser = await CreateUser(helperContainer, "test1@test.com", otherUserIpAddress);

            var random = new RandomWrapper(2015);

            var address = await AddressHelper.CreateRandomAddressAndSave(
                random, helperContainer, otherUser.Id, otherUserIpAddress, CountryId.GB, distinctAddressCode: distinctAddressCode);
            var submissions = await CreateSubmissionsAndSave(random, helperContainer, address.Id, otherUser.Id, otherUserIpAddress, submissionsToCreate);
            Assert.IsNotEmpty(submissions, "Submissions were not created.");

            var duplicateAddress1 = await AddressHelper.CreateRandomAddressAndSave(
                random, helperContainer, otherUser.Id, otherUserIpAddress, CountryId.GB, distinctAddressCode: distinctAddressCode);
            var submissionsDuplicateAddress1 = await CreateSubmissionsAndSave(
                random, helperContainer, duplicateAddress1.Id, otherUser.Id, otherUserIpAddress, submissionsToCreate);
            Assert.IsNotEmpty(submissions, "Submissions for duplicateAddress1 were not created.");

            var duplicateAddress2 = await AddressHelper.CreateRandomAddressAndSave(
                random, helperContainer, otherUser.Id, otherUserIpAddress, CountryId.GB, distinctAddressCode: distinctAddressCode);
            var submissionsDuplicateAddress2 = await CreateSubmissionsAndSave(
                random, helperContainer, duplicateAddress2.Id, otherUser.Id, otherUserIpAddress, submissionsToCreate);
            Assert.IsNotEmpty(submissions, "Submissions for duplicateAddress2 were not created.");

            // I test that duplicate address without submissions does not show up in the property info.
            var duplicateAddressNoSubmissions = await AddressHelper.CreateRandomAddressAndSave(
                random, helperContainer, otherUser.Id, otherUserIpAddress, CountryId.GB, distinctAddressCode: distinctAddressCode);
            Assert.IsNotNull(duplicateAddressNoSubmissions, "Duplicate address without submissions was not created.");

            // I test that hidden address without submissions does not show up in the property info.
            var hiddenAddress = await AddressHelper.CreateRandomAddressAndSave(
                random, helperContainer, otherUser.Id, otherUserIpAddress, CountryId.GB, distinctAddressCode: distinctAddressCode, isHidden:true);
            var submissionsHiddenAddress = await CreateSubmissionsAndSave(
                random, helperContainer, hiddenAddress.Id, otherUser.Id, otherUserIpAddress, submissionsToCreate);
            Assert.IsNotEmpty(submissionsHiddenAddress, "Submissions for hiddenAddress were not created.");

            var submissionsOrdered = submissions.OrderByDescending(x => x.SubmittedOn).ToList();
            var submissionsDuplicateAddress1Ordered = submissionsDuplicateAddress1.OrderByDescending(x => x.SubmittedOn).ToList();
            var submissionsDuplicateAddress2Ordered = submissionsDuplicateAddress2.OrderByDescending(x => x.SubmittedOn).ToList();

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, expiryPeriodInDays: expiryPeriodInDays);
            var serviceUnderTest = containerUnderTest.Get<IPropertyInfoAccessService>();

            var tokenRewardService = helperContainer.Get<ITokenRewardService>();

            var propertyInfoAccessCost =
                tokenRewardService.GetCurrentReward(TokenRewardKey.SpendPerPropertyInfoAccess).AbsValue;
            var tenancyDetailsSubmissionReward =
                tokenRewardService.GetCurrentReward(TokenRewardKey.EarnPerTenancyDetailsSubmission).AbsValue;

            Assert.That(tenancyDetailsSubmissionReward, Is.GreaterThanOrEqualTo(propertyInfoAccessCost),
                "This test assumes that by making a transaction for submitting tenancy details it will create enough funds to buy a property info access.");

            var userTokenService = helperContainer.Get<IUserTokenService>();
            // I add funds to spend later on.
            var tenancyDetailsSubmissionTransactionStatus = await userTokenService.MakeTransaction(user.Id, TokenRewardKey.EarnPerTenancyDetailsSubmission);
            Assert.AreEqual(TokenAccountTransactionStatus.Success, tenancyDetailsSubmissionTransactionStatus, "Adding funds failed.");

            // I create the property info access
            var accessUniqueId = Guid.NewGuid();
            var helperService = helperContainer.Get<IPropertyInfoAccessService>();
            var createOutcome = await helperService.Create(user.Id, ipAddress, accessUniqueId, address.UniqueId);
            Assert.IsFalse(createOutcome.IsRejected, "Property info access was not created.");

            var getInfo = await serviceUnderTest.GetInfo(user.Id, accessUniqueId);
            Assert.IsFalse(getInfo.IsRejected,
                "IsRejected field when trying to get the info with the right user is not the expected.");
            Assert.IsNullOrEmpty(getInfo.RejectionReason,
                "RejectionReason field when trying to get the info with the right user is not the expected.");
            Assert.IsNotNull(getInfo.PropertyInfo,
                "PropertyInfo field when trying to get the info with the right user should not be null");

            var retrievedAddress = await DbProbe.Addresses.Include(x => x.Country).SingleAsync(x => x.Id.Equals(address.Id));
            var retrievedDuplicateAddress1 = await DbProbe.Addresses
                .Include(x => x.Country).SingleAsync(x => x.Id.Equals(duplicateAddress1.Id));
            var retrievedDuplicateAddress2 = await DbProbe.Addresses
                .Include(x => x.Country).SingleAsync(x => x.Id.Equals(duplicateAddress2.Id));

            Assert.IsNotNull(getInfo.PropertyInfo.MainProperty, "PropertyInfo.MainProperty should not be null.");
            Assert.AreEqual(retrievedAddress.FullAddress(), getInfo.PropertyInfo.MainProperty.DisplayAddress,
                "PropertyInfo.MainProperty.DisplayAddress is not the expected.");
            Assert.AreEqual(submissionsToCreate, getInfo.PropertyInfo.MainProperty.CompleteSubmissions.Count,
                "The number of complete submissions on the MainProperty is not the expected.");
            Assert.AreEqual(2, getInfo.PropertyInfo.DuplicateProperties.Count, 
                "The number of duplicate properties is not the expected.");

            for (var i = 0; i < submissionsToCreate; i++)
            {
                var submissionId = submissionsOrdered[i].Id;
                var expected = await DbProbe.TenancyDetailsSubmissions
                    .Include(x => x.Currency).SingleOrDefaultAsync(x => x.Id.Equals(submissionId));
                var actual = getInfo.PropertyInfo.MainProperty.CompleteSubmissions[i];

                Assert.AreEqual(expected.RentPerMonth, actual.RentPerMonth,
                    string.Format("RentPerMonth is not the expected for main property submission and i equal to '{0}'", i));
                Assert.AreEqual(expected.Currency.Symbol, actual.CurrencySymbol,
                    string.Format("CurrencySymbol is not the expected for main property submission and i equal to '{0}'", i));
                Assert.AreEqual(expected.NumberOfBedrooms, actual.NumberOfBedrooms,
                    string.Format("NumberOfBedrooms is not the expected for main property submission and i equal to '{0}'", i));
                Assert.AreEqual(expected.IsFurnished, actual.IsFurnished,
                    string.Format("IsFurnished is not the expected for main property submission and i equal to '{0}'", i));
                Assert.AreEqual(expected.IsPartOfProperty, actual.IsPartOfProperty,
                    string.Format("IsPartOfProperty is not the expected for main property submission and i equal to '{0}'", i));
                Assert.AreEqual(expected.SubmittedOn, actual.SubmittedOn,
                    string.Format("SubmittedOn is not the expected for main property submission and i equal to '{0}'", i));
                Assert.AreEqual(expected.LandlordRating, actual.LandlordRating,
                    string.Format("LandlordRating is not the expected for main property submission and i equal to '{0}'", i));
                Assert.AreEqual(expected.NeighboursRating, actual.NeighboursRating,
                    string.Format("NeighboursRating is not the expected for main property submission and i equal to '{0}'", i));
                Assert.AreEqual(expected.PropertyConditionRating, actual.PropertyConditionRating,
                    string.Format("PropertyConditionRating is not the expected for main property submission and i equal to '{0}'", i));
            }

            var duplicateProperties = new List<Address> { duplicateAddress1, duplicateAddress2 }.OrderBy(x => x.CreatedOn).ToList();

            // Duplicate property 1
            var duplicateProperty1 = getInfo.PropertyInfo.DuplicateProperties[0];
            for (var i = 0; i < submissionsToCreate; i++)
            {
                var submissionId = submissionsDuplicateAddress1Ordered[i].Id;
                var expected = await DbProbe.TenancyDetailsSubmissions
                    .Include(x => x.Currency).SingleOrDefaultAsync(x => x.Id.Equals(submissionId));
                var actual = duplicateProperty1.CompleteSubmissions[i];

                Assert.AreEqual(expected.RentPerMonth, actual.RentPerMonth,
                    string.Format("RentPerMonth is not the expected for main property submission and i equal to '{0}'", i));
                Assert.AreEqual(expected.Currency.Symbol, actual.CurrencySymbol,
                    string.Format("CurrencySymbol is not the expected for main property submission and i equal to '{0}'", i));
                Assert.AreEqual(expected.NumberOfBedrooms, actual.NumberOfBedrooms,
                    string.Format("NumberOfBedrooms is not the expected for main property submission and i equal to '{0}'", i));
                Assert.AreEqual(expected.IsFurnished, actual.IsFurnished,
                    string.Format("IsFurnished is not the expected for main property submission and i equal to '{0}'", i));
                Assert.AreEqual(expected.IsPartOfProperty, actual.IsPartOfProperty,
                    string.Format("IsPartOfProperty is not the expected for main property submission and i equal to '{0}'", i));
                Assert.AreEqual(expected.SubmittedOn, actual.SubmittedOn,
                    string.Format("SubmittedOn is not the expected for main property submission and i equal to '{0}'", i));
                Assert.AreEqual(expected.LandlordRating, actual.LandlordRating,
                    string.Format("LandlordRating is not the expected for main property submission and i equal to '{0}'", i));
                Assert.AreEqual(expected.NeighboursRating, actual.NeighboursRating,
                    string.Format("NeighboursRating is not the expected for main property submission and i equal to '{0}'", i));
                Assert.AreEqual(expected.PropertyConditionRating, actual.PropertyConditionRating,
                    string.Format("PropertyConditionRating is not the expected for main property submission and i equal to '{0}'", i));
            }

            // Duplicate property 2
            var duplicateProperty2 = getInfo.PropertyInfo.DuplicateProperties[1];
            for (var i = 0; i < submissionsToCreate; i++)
            {
                var submissionId = submissionsDuplicateAddress2Ordered[i].Id;
                var expected = await DbProbe.TenancyDetailsSubmissions
                    .Include(x => x.Currency).SingleOrDefaultAsync(x => x.Id.Equals(submissionId));
                var actual = duplicateProperty2.CompleteSubmissions[i];

                Assert.AreEqual(expected.RentPerMonth, actual.RentPerMonth,
                    string.Format("RentPerMonth is not the expected for main property submission and i equal to '{0}'", i));
                Assert.AreEqual(expected.Currency.Symbol, actual.CurrencySymbol,
                    string.Format("CurrencySymbol is not the expected for main property submission and i equal to '{0}'", i));
                Assert.AreEqual(expected.NumberOfBedrooms, actual.NumberOfBedrooms,
                    string.Format("NumberOfBedrooms is not the expected for main property submission and i equal to '{0}'", i));
                Assert.AreEqual(expected.IsFurnished, actual.IsFurnished,
                    string.Format("IsFurnished is not the expected for main property submission and i equal to '{0}'", i));
                Assert.AreEqual(expected.IsPartOfProperty, actual.IsPartOfProperty,
                    string.Format("IsPartOfProperty is not the expected for main property submission and i equal to '{0}'", i));
                Assert.AreEqual(expected.SubmittedOn, actual.SubmittedOn,
                    string.Format("SubmittedOn is not the expected for main property submission and i equal to '{0}'", i));
                Assert.AreEqual(expected.LandlordRating, actual.LandlordRating,
                    string.Format("LandlordRating is not the expected for main property submission and i equal to '{0}'", i));
                Assert.AreEqual(expected.NeighboursRating, actual.NeighboursRating,
                    string.Format("NeighboursRating is not the expected for main property submission and i equal to '{0}'", i));
                Assert.AreEqual(expected.PropertyConditionRating, actual.PropertyConditionRating,
                    string.Format("PropertyConditionRating is not the expected for main property submission and i equal to '{0}'", i));
            }
        }

        #endregion

        #region GetExistingUnexpiredAccessUniqueId

        [Test]
        public async Task GetExistingUnexpiredAccessUniqueId_AccessTest()
        {
            var expiryPeriod = TimeSpan.FromDays(1);
            var expiryPeriodInDays = expiryPeriod.TotalDays;

            var helperContainer = CreateContainer();

            var ipAddress1 = "1.2.3.4";
            var user1 = await CreateUser(helperContainer, "test1@test.com", ipAddress1);
            var ipAddress2 = "1.2.3.5";
            var user2 = await CreateUser(helperContainer, "test2@test.com", ipAddress2);

            var random = new RandomWrapper(2015);

            var propertyInfoAccess1 = await CreatePropertyInfoAccessAndSave(
                random, helperContainer, user1.Id, ipAddress1, user2.Id, ipAddress2);
            var propertyInfoAccess2 = await CreatePropertyInfoAccessAndSave(
                random, helperContainer, user2.Id, ipAddress2, user1.Id, ipAddress1);

            var retrievedAddress1 = await DbProbe.Addresses.FindAsync(propertyInfoAccess1.AddressId);
            var retrievedAddress2 = await DbProbe.Addresses.FindAsync(propertyInfoAccess2.AddressId);

            var nonExistentAddressId1 = Guid.NewGuid();

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, expiryPeriodInDays: expiryPeriodInDays);
            var serviceUnderTest = containerUnderTest.Get<IPropertyInfoAccessService>();

            var response1 = await serviceUnderTest.GetExistingUnexpiredAccessUniqueId(user1.Id, nonExistentAddressId1);

            Assert.IsNull(response1, "Response1 is not the expected.");

            var response2 = await serviceUnderTest.GetExistingUnexpiredAccessUniqueId(user1.Id, retrievedAddress1.UniqueId);
            Assert.AreEqual(propertyInfoAccess1.UniqueId, response2, "Response2 is not the expected.");
            var response3 = await serviceUnderTest.GetExistingUnexpiredAccessUniqueId(user1.Id, retrievedAddress2.UniqueId);
            Assert.IsNull(response3, "Response3 is not the expected.");
            var response4 = await serviceUnderTest.GetExistingUnexpiredAccessUniqueId(user2.Id, retrievedAddress1.UniqueId);
            Assert.IsNull(response4, "Response4 is not the expected.");
            var response5 = await serviceUnderTest.GetExistingUnexpiredAccessUniqueId(user2.Id, retrievedAddress2.UniqueId);
            Assert.AreEqual(propertyInfoAccess2.UniqueId, response5, "Response5 is not the expected.");
        }

        [Test]
        public async Task GetExistingUnexpiredAccessUniqueId_ExpiryTest()
        {
            var expiryPeriod = TimeSpan.FromSeconds(0.01);
            var expiryPeriodInDays = expiryPeriod.TotalDays;

            var helperContainer = CreateContainer();

            var ipAddress1 = "1.2.3.4";
            var user1 = await CreateUser(helperContainer, "test1@test.com", ipAddress1);
            var ipAddress2 = "1.2.3.5";
            var user2 = await CreateUser(helperContainer, "test2@test.com", ipAddress2);

            var random = new RandomWrapper(2015);

            var propertyInfoAccess1 = await CreatePropertyInfoAccessAndSave(
                random, helperContainer, user1.Id, ipAddress1, user2.Id, ipAddress2);
            var propertyInfoAccess2 = await CreatePropertyInfoAccessAndSave(
                random, helperContainer, user2.Id, ipAddress2, user1.Id, ipAddress1);

            var retrievedAddress1 = await DbProbe.Addresses.FindAsync(propertyInfoAccess1.AddressId);
            var retrievedAddress2 = await DbProbe.Addresses.FindAsync(propertyInfoAccess2.AddressId);

            // I wait until all accesses get expired.
            await Task.Delay(expiryPeriod);

            var nonExistentAddressId1 = Guid.NewGuid();

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, expiryPeriodInDays: expiryPeriodInDays);
            var serviceUnderTest = containerUnderTest.Get<IPropertyInfoAccessService>();

            var response1 = await serviceUnderTest.GetExistingUnexpiredAccessUniqueId(user1.Id, nonExistentAddressId1);

            Assert.IsNull(response1, "Response1 is not the expected.");

            var response2 = await serviceUnderTest.GetExistingUnexpiredAccessUniqueId(user1.Id, retrievedAddress1.UniqueId);
            Assert.IsNull(response2, "Response2 is not the expected.");
            var response3 = await serviceUnderTest.GetExistingUnexpiredAccessUniqueId(user1.Id, retrievedAddress2.UniqueId);
            Assert.IsNull(response3, "Response3 is not the expected.");
            var response4 = await serviceUnderTest.GetExistingUnexpiredAccessUniqueId(user2.Id, retrievedAddress1.UniqueId);
            Assert.IsNull(response4, "Response4 is not the expected.");
            var response5 = await serviceUnderTest.GetExistingUnexpiredAccessUniqueId(user2.Id, retrievedAddress2.UniqueId);
            Assert.IsNull(response5, "Response5 is not the expected.");
        }

        #endregion

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
            var expiryPeriodInDays = 1.0;

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
            SetupConfig(containerUnderTest, expiryPeriodInDays, itemsLimit);
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

        [Test]
        public async Task GetUserExploredPropertiesSummary_WithMorePropertyInfoAccessesThanTheLimit_ItemsLimitIsRespected()
        {
            var itemsLimit = 2;
            var propertyInfoAccessesToCreate = 3;
            var expiryPeriodInDays = 1.0;

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", otherUserIpAddress);

            var random = new RandomWrapper(2015);
            var propertyInfoAccesses = new List<PropertyInfoAccess>();

            for (var i = 0; i < propertyInfoAccessesToCreate; i++)
            {
                var propertyInfoAccess = await CreatePropertyInfoAccessAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress);
                propertyInfoAccesses.Add(propertyInfoAccess);
                await Task.Delay(_smallDelay);
            }
            var propertyInfoAccesseByCreationDescending = propertyInfoAccesses.OrderByDescending(x => x.CreatedOn).ToList();

            // I create a property info access for the other user and assign the submission to the user under test.
            // This is to test that the summary contains only verifications from the specific user.
            var otherUserPropertyInfoAccess = await CreatePropertyInfoAccessAndSave(
                    random, helperContainer, otherUser.Id, otherUserIpAddress, user.Id, userIpAddress);
            Assert.IsNotNull(otherUserPropertyInfoAccess, "The property info access created for the other user is null.");

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, expiryPeriodInDays, itemsLimit);
            var serviceUnderTest = containerUnderTest.Get<IPropertyInfoAccessService>();

            // Full summary
            var response1 = await serviceUnderTest.GetUserExploredPropertiesSummary(user.Id, false);

            Assert.IsNotNull(response1, "Response1 is null.");
            Assert.IsFalse(response1.moreItemsExist, "Field moreItemsExist on response1 is not the expected.");
            Assert.AreEqual(propertyInfoAccessesToCreate, response1.exploredProperties.Count,
                "Response1 should contain all explored properties.");
            for (var i = 0; i < propertyInfoAccessesToCreate; i++)
            {
                Assert.AreEqual(propertyInfoAccesseByCreationDescending[i].UniqueId, response1.exploredProperties[i].accessUniqueId,
                    string.Format("Response1: explored property at position {0} does not have the expected uniqueId.", i));
            }

            Assert.IsFalse(response1.exploredProperties.Any(x => x.accessUniqueId.Equals(otherUserPropertyInfoAccess.UniqueId)),
                "Response1 should not contain the property info access of the other user.");

            // Summary with limit
            var response2 = await serviceUnderTest.GetUserExploredPropertiesSummary(user.Id, true);

            Assert.IsNotNull(response2, "Response2 is null.");
            Assert.IsTrue(response2.moreItemsExist, "Field moreItemsExist on response2 is not the expected.");
            Assert.IsTrue(response2.exploredProperties.Any(), "Field exploredProperties on response2 should not be empty.");

            Assert.AreEqual(itemsLimit, response2.exploredProperties.Count,
                "Response2 should contain a number of explored properties equal to the limit.");
            for (var i = 0; i < itemsLimit; i++)
            {
                Assert.AreEqual(propertyInfoAccesseByCreationDescending[i].UniqueId, response2.exploredProperties[i].accessUniqueId,
                    string.Format("Response2: explored property at position {0} does not have the expected uniqueId.", i));
            }

            Assert.IsFalse(response2.exploredProperties.Any(x => x.accessUniqueId.Equals(otherUserPropertyInfoAccess.UniqueId)),
                "Response1 should not contain the property info access of the other user.");
        }

        [Test]
        public async Task GetUserExploredPropertiesSummary_SingleExploredProperty_ExpiryTest()
        {
            var itemsLimit = 10;
            var expiryPeriod = TimeSpan.FromSeconds(0.3);
            var expiryPeriodInDays = expiryPeriod.TotalDays;

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", otherUserIpAddress);

            var random = new RandomWrapper(2015);
            var propertyInfoAccess = await CreatePropertyInfoAccessAndSave(
                    random, helperContainer, user.Id, userIpAddress, otherUser.Id, otherUserIpAddress);

            var containerUnderTest = CreateContainer();
            SetupConfig(containerUnderTest, expiryPeriodInDays, itemsLimit);
            var serviceUnderTest = containerUnderTest.Get<IPropertyInfoAccessService>();

            var response = await serviceUnderTest.GetUserExploredPropertiesSummary(user.Id, false);

            Assert.AreEqual(1, response.exploredProperties.Count,
                "The response should contain a single explored property.");

            var exploredProperty = response.exploredProperties.Single();

            Assert.AreEqual(propertyInfoAccess.UniqueId, exploredProperty.accessUniqueId,
                "Field accessUniqueId is not the expected.");
            Assert.IsTrue(exploredProperty.canViewInfo, "Field canViewInfo doesn't have the expected value.");

            var retrievedPropertyInfoAccess = await DbProbe.PropertyInfoAccesses
                .Include(x => x.Address)
                .Include(x => x.Address.Country)
                .SingleOrDefaultAsync(x => x.UniqueId.Equals(propertyInfoAccess.UniqueId));
            Assert.AreEqual(retrievedPropertyInfoAccess.CreatedOn + expiryPeriod, exploredProperty.expiresOn,
                "Field expiresOn is not the expected.");
            Assert.AreEqual(retrievedPropertyInfoAccess.Address.FullAddress(), exploredProperty.displayAddress,
                "Field displayAddress is not the expected.");

            await Task.Delay(expiryPeriod);

            var responseAfterExpiry = await serviceUnderTest.GetUserExploredPropertiesSummary(user.Id, false);

            Assert.IsEmpty(responseAfterExpiry.exploredProperties, "The explored property should not appear on the summary after the expiry.");
        }

        #endregion

        #region GetUserExploredPropertiesSummaryWithCaching

        [Test]
        public async Task GetUserExploredPropertiesSummaryWithCaching_WithPropertyInfoAccessesEqualToTheLimit_CachesTheSummary()
        {
            var itemsLimit = 3;
            var propertyInfoAccessesToCreate = itemsLimit;
            var expiryPeriodInDays = 1.0;
            var cachingPeriod = TimeSpan.FromSeconds(0.2);

            var helperContainer = CreateContainer();
            var userIpAddress = "1.2.3.4";
            var user = await CreateUser(helperContainer, "test@test.com", userIpAddress);
            var otherUserIpAddress = "11.12.13.14";
            var otherUser = await CreateUser(helperContainer, "other-user@test.com", otherUserIpAddress);

            var random = new RandomWrapper(2015);
            var propertyInfoAccesses = new List<PropertyInfoAccess>();

            for (var i = 0; i < propertyInfoAccessesToCreate; i++)
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
            SetupConfig(containerUnderTest, expiryPeriodInDays, itemsLimit, cachingPeriod);
            var serviceUnderTest = containerUnderTest.Get<IPropertyInfoAccessService>();

            // Full summary
            var response1 = await serviceUnderTest.GetUserExploredPropertiesSummaryWithCaching(user.Id, false);

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
            var response2 = await serviceUnderTest.GetUserExploredPropertiesSummaryWithCaching(user.Id, true);

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

            KillDatabase(containerUnderTest);
            var serviceWithoutDatabase = containerUnderTest.Get<IPropertyInfoAccessService>();

            // Full summary
            var response3 = await serviceWithoutDatabase.GetUserExploredPropertiesSummaryWithCaching(user.Id, false);

            Assert.IsNotNull(response3, "Response3 is null.");
            Assert.IsFalse(response3.moreItemsExist, "Field moreItemsExist on response3 is not the expected.");
            Assert.AreEqual(propertyInfoAccessesToCreate, response3.exploredProperties.Count,
                "Response3 should contain all property info accesses.");
            for (var i = 0; i < propertyInfoAccessesToCreate; i++)
            {
                Assert.AreEqual(propertyInfoAccessesByCreationDescending[i].UniqueId, response3.exploredProperties[i].accessUniqueId,
                    string.Format("Response3: explored property at position {0} does not have the expected uniqueId.", i));
            }

            Assert.IsFalse(response3.exploredProperties.Any(x => x.accessUniqueId.Equals(otherUserPropertyInfoAccess.UniqueId)),
                "Response3 should not contain the property info acccess of the other user.");

            // Summary with limit
            var response4 = await serviceWithoutDatabase.GetUserExploredPropertiesSummaryWithCaching(user.Id, true);

            Assert.IsNotNull(response4, "Response4 is null.");
            Assert.IsFalse(response4.moreItemsExist, "Field moreItemsExist on response4 is not the expected.");
            Assert.IsTrue(response4.exploredProperties.Any(), "Field exploredProperties on response4 should not be empty.");

            Assert.AreEqual(itemsLimit, response4.exploredProperties.Count,
                "Response4 should contain a number of explored properties equal to the limit.");
            for (var i = 0; i < itemsLimit; i++)
            {
                Assert.AreEqual(propertyInfoAccessesByCreationDescending[i].UniqueId, response4.exploredProperties[i].accessUniqueId,
                    string.Format("Response4: explored property at position {0} does not have the expected uniqueId.", i));
            }

            Assert.IsFalse(response4.exploredProperties.Any(x => x.accessUniqueId.Equals(otherUserPropertyInfoAccess.UniqueId)),
                "Response4 should not contain the property info access of the other user.");

            await Task.Delay(cachingPeriod);

            Assert.Throws<ArgumentNullException>(async () => await serviceWithoutDatabase.GetUserExploredPropertiesSummaryWithCaching(user.Id, false),
                "After the caching period is over, it should throw an exception because I have killed the  database. (limit items: false)");
            Assert.Throws<ArgumentNullException>(async () => await serviceWithoutDatabase.GetUserExploredPropertiesSummaryWithCaching(user.Id, true),
                "After the caching period is over, it should throw an exception because I have killed the  database. (limit items: true)");

        }

        #endregion

        #region Private helper functions
        
        private static void SetupConfig(
            IKernel container, double? expiryPeriodInDays = null, int? itemsLimit = null, TimeSpan? cachingPeriod = null)
        {
            var mockConfig = new Mock<IPropertyInfoAccessServiceConfig>();

            if (itemsLimit.HasValue)
                mockConfig.Setup(x => x.MyExploredPropertiesSummary_ItemsLimit).Returns(itemsLimit.Value);
            if (expiryPeriodInDays.HasValue)
                mockConfig.Setup(x => x.ExpiryPeriodInDays).Returns(expiryPeriodInDays.Value);
            if (cachingPeriod.HasValue)
                mockConfig.Setup(x => x.MyExploredPropertiesSummary_CachingPeriod).Returns(cachingPeriod.Value);

            container.Rebind<IPropertyInfoAccessServiceConfig>().ToConstant(mockConfig.Object);
        }

        private static async Task<PropertyInfoAccess> CreatePropertyInfoAccessAndSave(
            IRandomWrapper random, IKernel container,
            string userId, string userIpAddress,
            string userIdForSubmission, string userForSubmissionIpAddress, int numberOfSubmissions = 1, 
            CountryId countryId = CountryId.GB, CurrencyId currencyId = CurrencyId.GBP)
        {
            var clock = container.Get<IClock>();

            var address = await AddressHelper.CreateRandomAddressAndSave(
                random, container, userIdForSubmission, userForSubmissionIpAddress, countryId);

            var dbContext = container.Get<IEpsilonContext>();

            for (var i = 0; i < numberOfSubmissions; i++)
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

        private static async Task<IList<TenancyDetailsSubmission>> CreateSubmissionsAndSave(
            IRandomWrapper random, IKernel container, long addressId,
            string userId, string userIpAddress, int numberOfSubmissions = 1, bool submissionsAreHidden = false, 
            CountryId countryId = CountryId.GB, CurrencyId currencyId = CurrencyId.GBP)
        {
            var clock = container.Get<IClock>();

            var dbContext = container.Get<IEpsilonContext>();

            var submissions = new List<TenancyDetailsSubmission>();

            for (var i = 0; i < numberOfSubmissions; i++)
            {
                var tenancyDetailsSubmission = new TenancyDetailsSubmission
                {
                    UniqueId = Guid.NewGuid(),
                    AddressId = addressId,
                    UserId = userId,
                    CreatedByIpAddress = userIpAddress,
                    RentPerMonth = random.Next(100, 1000),
                    NumberOfBedrooms = (byte)random.Next(0, 20),
                    IsFurnished = random.NextDouble() < 0.5,
                    IsPartOfProperty = random.NextDouble() < 0.5,
                    LandlordRating = (byte)random.Next(1, 6),
                    PropertyConditionRating = (byte)random.Next(1, 6),
                    NeighboursRating = (byte)random.Next(1, 6),
                    CurrencyId = EnumsHelper.CurrencyId.ToString(currencyId),
                    SubmittedOn = clock.OffsetNow,
                    IsHidden = submissionsAreHidden
                };
                dbContext.TenancyDetailsSubmissions.Add(tenancyDetailsSubmission);
                submissions.Add(tenancyDetailsSubmission);
            }

            await dbContext.SaveChangesAsync();

            return submissions;
        }

        private static async Task<IList<TenancyDetailsSubmission>> CreateIncompleteSubmissionsAndSave(
            IRandomWrapper random, IKernel container, long addressId,
            string userId, string userIpAddress, int numberOfSubmissions = 1,
            CountryId countryId = CountryId.GB, CurrencyId currencyId = CurrencyId.GBP)
        {
            var clock = container.Get<IClock>();

            var dbContext = container.Get<IEpsilonContext>();

            var submissions = new List<TenancyDetailsSubmission>();

            for (var i = 0; i < numberOfSubmissions; i++)
            {
                var tenancyDetailsSubmission = new TenancyDetailsSubmission
                {
                    UniqueId = Guid.NewGuid(),
                    AddressId = addressId,
                    UserId = userId,
                    CreatedByIpAddress = userIpAddress
                };
                dbContext.TenancyDetailsSubmissions.Add(tenancyDetailsSubmission);
                submissions.Add(tenancyDetailsSubmission);
            }

            await dbContext.SaveChangesAsync();

            return submissions;
        }

        #endregion
    }
}
