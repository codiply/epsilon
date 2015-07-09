using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.Logic.Services.Interfaces;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using System.Data.Entity;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Constants.Enums;

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class NewUserServiceTest : BaseIntegrationTestWithRollback
    { 
        [Test]
        public async Task Setup_CreatesCoinAccount()
        {
            var container = CreateContainer();
            var ipAddress = "1.2.3.5";
            var languageId = "en";
            var user = await CreateUser(container, "test@test.com", ipAddress, false);

            var timeBefore = DateTimeOffset.Now;

            var service = container.Get<INewUserService>();
            await service.Setup(user.Id, ipAddress, languageId);

            var coinAccountId = user.Id;
            var coinAccount = await DbProbe.CoinAccounts.FindAsync(coinAccountId);

            Assert.IsNotNull(coinAccount, "The coin account was not found.");
            var timeAfter = DateTimeOffset.Now;
            Assert.IsTrue(timeBefore <= coinAccount.CreatedOn && coinAccount.CreatedOn <= timeAfter, 
                "The field CreatedOn was not set.");

            var coinAccountService = container.Get<ICoinAccountService>();
            var balance = await coinAccountService.GetBalance(coinAccountId);
            Assert.AreEqual(0.0M, balance, "Balance of new coin account was expected to be zero.");
        }

        [Test]
        public async Task Setup_CreatesUserPreference()
        {
            var container = CreateContainer();
            var ipAddress = "1.2.3.5";
            var languageId = "en";
            var user = await CreateUser(container, "test@test.com", ipAddress, false);
            
            var service = container.Get<INewUserService>();
            await service.Setup(user.Id, ipAddress, languageId);

            var userPreferenceId = user.Id;
            var userPreference = await DbProbe.UserPreferences.FindAsync(userPreferenceId);

            Assert.IsNotNull(userPreference, "The UserPreference was not found.");
            Assert.AreEqual(userPreference.LanguageId, languageId, "Language was not set in UserPreference.");
        }

        [Test]
        public async Task Setup_RecordsIpAddressActivity()
        {
            var container = CreateContainer();
            var ipAddress = "1.2.3.5";
            var languageId = "en";
            var user = await CreateUser(container, "test@test.com", ipAddress, false);

            var timeBefore = DateTimeOffset.Now;

            var service = container.Get<INewUserService>();
            await service.Setup(user.Id, ipAddress, languageId);

            var ipAddressActivity = await DbProbe.IpAddressActivities
                .SingleOrDefaultAsync(x => x.UserId.Equals(user.Id));

            Assert.IsNotNull(ipAddressActivity, "The IpAddressActivity was not found.");
            var timeAfter = DateTimeOffset.Now;
            Assert.IsTrue(timeBefore <= ipAddressActivity.RecordedOn && ipAddressActivity.RecordedOn <= timeAfter,
                "The field RecordedOn was not set.");
            Assert.AreEqual(IpAddressActivityType.Registration, ipAddressActivity.ActivityTypeAsEnum,
                "The IpAddressActivity has wrong ActivityType.");
            Assert.AreEqual(ipAddress, ipAddressActivity.IpAddress,
                "The IpAddressActivity has wrong IpAddress");
        }
    }
}
