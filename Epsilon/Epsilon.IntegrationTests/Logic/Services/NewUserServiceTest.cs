using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Services.Interfaces;
using Ninject;
using NUnit.Framework;
using System;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class NewUserServiceTest : BaseIntegrationTestWithRollback
    { 
        [Test]
        public async Task Setup_CreatesTokenAccount()
        {
            var container = CreateContainer();
            var ipAddress = "1.2.3.5";
            var languageId = "en";
            var user = await CreateUser(container, "test@test.com", ipAddress, false);

            var timeBefore = DateTimeOffset.Now;

            var service = container.Get<INewUserService>();
            await service.Setup(user.Id, ipAddress, languageId);

            var tokenAccountId = user.Id;
            var tokenAccount = await DbProbe.TokenAccounts.FindAsync(tokenAccountId);

            Assert.IsNotNull(tokenAccount, "The token account was not found.");
            var timeAfter = DateTimeOffset.Now;
            Assert.IsTrue(timeBefore <= tokenAccount.CreatedOn && tokenAccount.CreatedOn <= timeAfter, 
                "The field CreatedOn was not set.");

            var tokenAccountService = container.Get<ITokenAccountService>();
            var balance = await tokenAccountService.GetBalance(tokenAccountId);
            Assert.AreEqual(0.0M, balance, "Balance of new token account was expected to be zero.");
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
