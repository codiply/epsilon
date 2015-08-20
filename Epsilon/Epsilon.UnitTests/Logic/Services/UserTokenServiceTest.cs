using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.UnitTests.Logic.Services
{
    [TestFixture]
    public class UserTokenServiceTest
    {
        [Test]
        public async Task MakeTransaction_WrongQuantityTest()
        {
            var service = new UserTokenService(null, null, null, null, null);

            var accountId = "any-account-id";

            var status1 = await service.MakeTransaction(accountId, TokenRewardKey.EarnPerTenancyDetailsSubmission,
                Guid.NewGuid(), quantity: 0);
            Assert.AreEqual(TokenAccountTransactionStatus.WrongQuantity, status1,
                "Status1 was not the expected.");

            var status2 = await service.MakeTransaction(accountId, TokenRewardKey.EarnPerTenancyDetailsSubmission,
                Guid.NewGuid(), quantity: -1);
            Assert.AreEqual(TokenAccountTransactionStatus.WrongQuantity, status2,
                "Status2 was not the expected.");
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentException))]
        public async Task SufficientFundsExistForTransaction_ZeroQuantityTest()
        {
            var service = new UserTokenService(null, null, null, null, null);

            var accountId = "any-account-id";

            await service.SufficientFundsExistForTransaction(
                accountId, TokenRewardKey.EarnPerTenancyDetailsSubmission, quantity: 0);
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentException))]
        public async Task SufficientFundsExistForTransaction_NegativeQuantityTest()
        {
            var service = new UserTokenService(null, null, null, null, null);

            var accountId = "any-account-id";

            await service.SufficientFundsExistForTransaction(
                accountId, TokenRewardKey.EarnPerTenancyDetailsSubmission, quantity: -1);
        }
    }
}
