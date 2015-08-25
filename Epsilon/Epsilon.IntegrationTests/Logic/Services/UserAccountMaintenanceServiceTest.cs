using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.IntegrationTests.TestHelpers;
using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Wrappers;
using Epsilon.Logic.Wrappers.Interfaces;
using Moq;
using Ninject;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Epsilon.Logic.Helpers.RandomStringHelper;

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class UserAccountMaintenanceServiceTest : BaseIntegrationTestWithRollback
    {
        [Test]
        public async Task DoMaintenance_DoesNotThrow()
        {
            var email = "test@test.com";
            var container = CreateContainer();

            Exception exceptionLogged = null;
            string adminAlertKeyUsed = null;
            AdminEventLogKey? adminEventLogKeyUsed = null;
            Dictionary<string, object> extraInfoUsed = null;
            SetupElmahHelper(container, ex => exceptionLogged = ex);
            SetupAdminAlertService(container, (key, doNotUseDatabase) =>
            {
                adminAlertKeyUsed = key;
            });
            SetupAdminEventLogService(container, (key, extraInfo) =>
            {
                adminEventLogKeyUsed = key;
                extraInfoUsed = extraInfo;
            });

            // I create a service here because there are components like the 
            // DbAppSettingsService that need the database.
            var throwAwayService = container.Get<IUserAccountMaintenanceService>();
            // I now kill the database so that an exception is thrown when I use the service.
            KillDatabase(container);
            var service = container.Get<IUserAccountMaintenanceService>();
            var success = await service.DoMaintenance(email);

            Assert.IsFalse(success, "The return value of DoMaintenance is not the expected.");
            Assert.IsNotNull(exceptionLogged, "No exception was logged via the ElmahHelper.");
            Assert.AreEqual(AdminAlertKey.UserAccountMaintenanceThrewException, adminAlertKeyUsed,
                "AdminAlertKey used is not the expected.");
            Assert.AreEqual(AdminEventLogKey.UserAccountMaintenanceThrewException, adminEventLogKeyUsed,
                "AdminEventLogKey used is not the expected.");
            Assert.IsNotNull(extraInfoUsed, "No ExtraInfo was used.");
            Assert.AreEqual(email, extraInfoUsed[AdminEventLogExtraInfoKey.MaintenanceUserEmail],
                "MaintenanceUserEmail on ExtraInfo was not the expected.");
        }

        [Test]
        public async Task CheckForUnrewardedOutgoingVerifications_TwoUnrewardedVerifications_PeriodNotElapsed()
        {
            var disableOutgoingVerificationRewardSendersIfNoneUsed = false;
            var outgoingVerificationRewardSendersIfNoneUsedAfterPeriod = TimeSpan.FromDays(30);

            var ipAddress = "irrelevant";

            var helperContainer = CreateContainer();
            var userForSubmission = await CreateUser(helperContainer, "test1@test.com", ipAddress);
            var userForOutgoingVerification1 = await CreateUser(helperContainer, "test2@test.com", ipAddress);
            var userForOutgoingVerification2 = await CreateUser(helperContainer, "test3@test.com", ipAddress);

            var random = new RandomWrapper(2015);

            var submission = await CreateSubmissionAndSave(random, helperContainer, userForSubmission.Id, ipAddress);
            var verification1 = await CreateTenantVerificationAndSave(random, helperContainer, submission,
                userForOutgoingVerification1.Id, ipAddress, isSent: true, isComplete: false);
            var verification2 = await CreateTenantVerificationAndSave(random, helperContainer, submission,
                userForOutgoingVerification2.Id, ipAddress, isSent: true, isComplete: false);

            var userTokenService = helperContainer.Get<IUserTokenService>();
            var balanceSender1before = await userTokenService.GetBalance(userForOutgoingVerification1.Id);
            var balanceSender2before = await userTokenService.GetBalance(userForOutgoingVerification2.Id);

            var container = CreateContainer();
            SetupConfig(container, disableOutgoingVerificationRewardSendersIfNoneUsed, outgoingVerificationRewardSendersIfNoneUsedAfterPeriod);
            var service = container.Get<IUserAccountMaintenanceService>();
            var success = await service.DoMaintenance(userForOutgoingVerification1.UserName);

            Assert.IsTrue(success, "The return value of DoMaintenance is not the expected.");

            var helperContainer2 = CreateContainer();
            var userTokenService2 = helperContainer2.Get<IUserTokenService>();
            var balanceSender1after = await userTokenService2.GetBalance(userForOutgoingVerification1.Id);
            var balanceSender2after = await userTokenService2.GetBalance(userForOutgoingVerification2.Id);

            Assert.AreEqual(balanceSender1before.balance, balanceSender1after.balance, "Balance for sender 1 changed.");
            Assert.AreEqual(balanceSender2before.balance, balanceSender2after.balance, "Balance for sender 2 changed.");

            var retrievedVerification1 = await DbProbe.TenantVerifications.FindAsync(verification1.Id);
            var retrievedVerification2 = await DbProbe.TenantVerifications.FindAsync(verification2.Id);

            Assert.IsNull(retrievedVerification1.SenderRewardedOn, 
                "SenderRewardedOn should be null on retrievedVerification1.");
            Assert.IsNull(retrievedVerification2.SenderRewardedOn,
                "SenderRewardedOn should be null on retrievedVerification2.");
        }

        [Test]
        public async Task CheckForUnrewardedOutgoingVerifications_TwoUnrewardedVerifications_PeriodIsElapsed()
        {
            var disableOutgoingVerificationRewardSendersIfNoneUsed = false;
            var outgoingVerificationRewardSendersIfNoneUsedAfterPeriod = TimeSpan.FromSeconds(0.1);

            var ipAddress = "irrelevant";

            var helperContainer = CreateContainer();
            var userForSubmission = await CreateUser(helperContainer, "test1@test.com", ipAddress);
            var userForOutgoingVerification1 = await CreateUser(helperContainer, "test2@test.com", ipAddress);
            var userForOutgoingVerification2 = await CreateUser(helperContainer, "test3@test.com", ipAddress);

            var random = new RandomWrapper(2015);

            var submission = await CreateSubmissionAndSave(random, helperContainer, userForSubmission.Id, ipAddress);
            var verification1 = await CreateTenantVerificationAndSave(random, helperContainer, submission,
                userForOutgoingVerification1.Id, ipAddress, isSent: true, isComplete: false);
            var verification2 = await CreateTenantVerificationAndSave(random, helperContainer, submission,
                userForOutgoingVerification2.Id, ipAddress, isSent: true, isComplete: false);

            var userTokenService = helperContainer.Get<IUserTokenService>();
            var balanceSender1before = await userTokenService.GetBalance(userForOutgoingVerification1.Id);
            var balanceSender2before = await userTokenService.GetBalance(userForOutgoingVerification2.Id);

            await Task.Delay(outgoingVerificationRewardSendersIfNoneUsedAfterPeriod);

            var container = CreateContainer();
            SetupConfig(container, disableOutgoingVerificationRewardSendersIfNoneUsed, outgoingVerificationRewardSendersIfNoneUsedAfterPeriod);
            var service = container.Get<IUserAccountMaintenanceService>();
            var clock = container.Get<IClock>();
            var timeBefore = clock.OffsetNow;
            var success = await service.DoMaintenance(userForOutgoingVerification1.UserName);
            var timeAfter = clock.OffsetNow;

            Assert.IsTrue(success, "The return value of DoMaintenance is not the expected.");

            var helperContainer2 = CreateContainer();
            var userTokenService2 = helperContainer2.Get<IUserTokenService>();
            var balanceSender1after = await userTokenService2.GetBalance(userForOutgoingVerification1.Id);
            var balanceSender2after = await userTokenService2.GetBalance(userForOutgoingVerification2.Id);

            var reward = helperContainer2.Get<ITokenRewardService>().GetCurrentReward(TokenRewardKey.EarnPerVerificationMailSent);

            Assert.AreEqual(balanceSender1before.balance + reward.Value, balanceSender1after.balance, "Balance for sender 1 changed.");
            Assert.AreEqual(balanceSender2before.balance + reward.Value, balanceSender2after.balance, "Balance for sender 2 changed.");

            var retrievedVerification1 = await DbProbe.TenantVerifications.FindAsync(verification1.Id);
            var retrievedVerification2 = await DbProbe.TenantVerifications.FindAsync(verification2.Id);

            Assert.IsNotNull(retrievedVerification1.SenderRewardedOn,
                "SenderRewardedOn should not be null on retrievedVerification1.");
            Assert.IsNotNull(retrievedVerification2.SenderRewardedOn,
                "SenderRewardedOn should not be null on retrievedVerification2.");

            Assert.That(retrievedVerification1.SenderRewardedOn, Is.GreaterThanOrEqualTo(timeBefore),
                "SenderRewardedOn on retrieved verificiation 1 should be greater than or equal to timeBefore.");
            Assert.That(retrievedVerification1.SenderRewardedOn, Is.LessThanOrEqualTo(timeAfter),
                "SenderRewardedOn on retrieved verificiation 1 should be less than or equal to timeAfter.");

            Assert.That(retrievedVerification2.SenderRewardedOn, Is.GreaterThanOrEqualTo(timeBefore),
                "SenderRewardedOn on retrieved verificiation 2 should be greater than or equal to timeBefore.");
            Assert.That(retrievedVerification2.SenderRewardedOn, Is.LessThanOrEqualTo(timeAfter),
                "SenderRewardedOn on retrieved verificiation 2 should be less than or equal to timeAfter.");
        }

        [Test]
        public async Task CheckForUnrewardedOutgoingVerifications_TwoUnrewardedVerifications_PeriodIsElapsed_RewardMaintenanceDisabled()
        {
            var disableOutgoingVerificationRewardSendersIfNoneUsed = true;
            var outgoingVerificationRewardSendersIfNoneUsedAfterPeriod = TimeSpan.FromSeconds(0.1);

            var ipAddress = "irrelevant";

            var helperContainer = CreateContainer();
            var userForSubmission = await CreateUser(helperContainer, "test1@test.com", ipAddress);
            var userForOutgoingVerification1 = await CreateUser(helperContainer, "test2@test.com", ipAddress);
            var userForOutgoingVerification2 = await CreateUser(helperContainer, "test3@test.com", ipAddress);

            var random = new RandomWrapper(2015);

            var submission = await CreateSubmissionAndSave(random, helperContainer, userForSubmission.Id, ipAddress);
            var verification1 = await CreateTenantVerificationAndSave(random, helperContainer, submission,
                userForOutgoingVerification1.Id, ipAddress, isSent: true, isComplete: false);
            var verification2 = await CreateTenantVerificationAndSave(random, helperContainer, submission,
                userForOutgoingVerification2.Id, ipAddress, isSent: true, isComplete: false);

            var userTokenService = helperContainer.Get<IUserTokenService>();
            var balanceSender1before = await userTokenService.GetBalance(userForOutgoingVerification1.Id);
            var balanceSender2before = await userTokenService.GetBalance(userForOutgoingVerification2.Id);

            await Task.Delay(outgoingVerificationRewardSendersIfNoneUsedAfterPeriod);

            var container = CreateContainer();
            SetupConfig(container, disableOutgoingVerificationRewardSendersIfNoneUsed, outgoingVerificationRewardSendersIfNoneUsedAfterPeriod);
            var service = container.Get<IUserAccountMaintenanceService>();
            var clock = container.Get<IClock>();
            var timeBefore = clock.OffsetNow;
            var success = await service.DoMaintenance(userForOutgoingVerification1.UserName);
            var timeAfter = clock.OffsetNow;

            Assert.IsTrue(success, "The return value of DoMaintenance is not the expected.");

            var helperContainer2 = CreateContainer();
            var userTokenService2 = helperContainer2.Get<IUserTokenService>();
            var balanceSender1after = await userTokenService2.GetBalance(userForOutgoingVerification1.Id);
            var balanceSender2after = await userTokenService2.GetBalance(userForOutgoingVerification2.Id);

            var reward = helperContainer2.Get<ITokenRewardService>().GetCurrentReward(TokenRewardKey.EarnPerVerificationMailSent);

            Assert.AreEqual(balanceSender1before.balance, balanceSender1after.balance, "Balance for sender 1 changed.");
            Assert.AreEqual(balanceSender2before.balance, balanceSender2after.balance, "Balance for sender 2 changed.");

            var retrievedVerification1 = await DbProbe.TenantVerifications.FindAsync(verification1.Id);
            var retrievedVerification2 = await DbProbe.TenantVerifications.FindAsync(verification2.Id);

            Assert.IsNull(retrievedVerification1.SenderRewardedOn,
                "SenderRewardedOn should be null on retrievedVerification1.");
            Assert.IsNull(retrievedVerification2.SenderRewardedOn,
                "SenderRewardedOn should be null on retrievedVerification2.");
        }

        [Test]
        public async Task CheckForUnrewardedOutgoingVerifications_SingleUnrewardedVerification_PeriodIsElapsed()
        {
            var disableOutgoingVerificationRewardSendersIfNoneUsed = false;
            var outgoingVerificationRewardSendersIfNoneUsedAfterPeriod = TimeSpan.FromSeconds(0.1);

            var ipAddress = "irrelevant";

            var helperContainer = CreateContainer();
            var userForSubmission = await CreateUser(helperContainer, "test1@test.com", ipAddress);
            var userForOutgoingVerification1 = await CreateUser(helperContainer, "test2@test.com", ipAddress);

            var random = new RandomWrapper(2015);

            var submission = await CreateSubmissionAndSave(random, helperContainer, userForSubmission.Id, ipAddress);
            var verification1 = await CreateTenantVerificationAndSave(random, helperContainer, submission,
                userForOutgoingVerification1.Id, ipAddress, isSent: true, isComplete: false);

            var userTokenService = helperContainer.Get<IUserTokenService>();
            var balanceSender1before = await userTokenService.GetBalance(userForOutgoingVerification1.Id);

            await Task.Delay(outgoingVerificationRewardSendersIfNoneUsedAfterPeriod);

            var container = CreateContainer();
            SetupConfig(container, disableOutgoingVerificationRewardSendersIfNoneUsed, outgoingVerificationRewardSendersIfNoneUsedAfterPeriod);
            var service = container.Get<IUserAccountMaintenanceService>();
            var clock = container.Get<IClock>();
            var timeBefore = clock.OffsetNow;
            var success = await service.DoMaintenance(userForOutgoingVerification1.UserName);
            var timeAfter = clock.OffsetNow;

            Assert.IsTrue(success, "The return value of DoMaintenance is not the expected.");

            var helperContainer2 = CreateContainer();
            var userTokenService2 = helperContainer2.Get<IUserTokenService>();
            var balanceSender1after = await userTokenService2.GetBalance(userForOutgoingVerification1.Id);

            var reward = helperContainer2.Get<ITokenRewardService>().GetCurrentReward(TokenRewardKey.EarnPerVerificationMailSent);

            Assert.AreEqual(balanceSender1before.balance, balanceSender1after.balance, "Balance for sender 1 changed.");

            var retrievedVerification1 = await DbProbe.TenantVerifications.FindAsync(verification1.Id);

            Assert.IsNull(retrievedVerification1.SenderRewardedOn,
                "SenderRewardedOn should be null on retrievedVerification1.");
        }

        #region Private Helpers

        private static void SetupConfig(IKernel container,
            bool disableOutgoingVerificationRewardSendersIfNoneUsed,
            TimeSpan outgoingVerificationRewardSendersIfNoneUsedAfterPeriod)
        {
            var mockConfig = new Mock<IUserAccountMaintenanceServiceConfig>();

            mockConfig.Setup(x => x.DisableRewardOutgoingVerificationSendersIfNoneUsedAfterCertainPeriod)
                .Returns(disableOutgoingVerificationRewardSendersIfNoneUsed);
            mockConfig.Setup(x => x.OutgoingVerification_RewardSendersIfNoneUsed_AfterPeriod)
                .Returns(outgoingVerificationRewardSendersIfNoneUsedAfterPeriod);

            container.Rebind<IUserAccountMaintenanceServiceConfig>().ToConstant(mockConfig.Object);
        }

        private static async Task<TenancyDetailsSubmission> CreateSubmissionAndSave(
            IRandomWrapper random, IKernel container,
            string userId, string userIpAddress)
        {
            var dbContext = container.Get<IEpsilonContext>();

            var address = await AddressHelper.CreateRandomAddressAndSave(random, container, userId, userIpAddress, CountryId.GB);

            var tenancyDetailsSubmission = new TenancyDetailsSubmission
            {
                UniqueId = Guid.NewGuid(),
                AddressId = address.Id,
                UserId = userId,
                CreatedByIpAddress = userIpAddress,
            };

            dbContext.TenancyDetailsSubmissions.Add(tenancyDetailsSubmission);
            await dbContext.SaveChangesAsync();

            return tenancyDetailsSubmission;
        }

        private static async Task<TenantVerification> CreateTenantVerificationAndSave(
            IRandomWrapper random, IKernel container,
            TenancyDetailsSubmission tenancyDetailsSubmission,
            string userId, string userIpAddress,
            bool isSent, bool isComplete)
        {
            var clock = container.Get<IClock>();
            var dbContext = container.Get<IEpsilonContext>();

            var tenantVerification = new TenantVerification
            {
                TenancyDetailsSubmissionId = tenancyDetailsSubmission.Id,
                UniqueId = Guid.NewGuid(),
                AssignedToId = userId,
                AssignedByIpAddress = userIpAddress,
                SecretCode = RandomStringHelper.GetString(random, AppConstant.SECRET_CODE_MAX_LENGTH, CharacterCase.Mixed)
            };
            if (isSent)
                tenantVerification.MarkedAsSentOn = clock.OffsetNow;
            if (isComplete)
            {
                tenantVerification.VerifiedOn = clock.OffsetNow;
                tenantVerification.SenderRewardedOn = clock.OffsetNow;
            }

            dbContext.TenantVerifications.Add(tenantVerification);
            await dbContext.SaveChangesAsync();

            return tenantVerification;
        }

        #endregion
    }
}
