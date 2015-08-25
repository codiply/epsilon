using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Services;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Epsilon.UnitTests.Logic.Services
{
    [TestFixture]
    public class TokenAccountServiceTest
    {
        [Test]
        public async Task MakeTransaction_WrongQuantityTest()
        {
            var service = new TokenAccountService(null, null, null);

            var accountId = "any-account-id";

            var status1 = await service.MakeTransaction(accountId, 0.0M, TokenRewardKey.EarnPerTenancyDetailsSubmission,
                quantity: 0);
            Assert.AreEqual(TokenAccountTransactionStatus.WrongQuantity, status1,
                "Status1 was not the expected.");

            var status2 = await service.MakeTransaction(accountId, 0.0M, TokenRewardKey.EarnPerTenancyDetailsSubmission,
                quantity: -1);
            Assert.AreEqual(TokenAccountTransactionStatus.WrongQuantity, status2,
                "Status2 was not the expected.");
        }
    }
}
