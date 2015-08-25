using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Services.Interfaces;
using Ninject;
using NUnit.Framework;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class UserTokenServiceTest : BaseIntegrationTestWithRollback
    {
        [Test]
        public async Task ScenarioTest()
        {
            var tokenRewardService = CreateContainer().Get<ITokenRewardService>();
            var spendKey = TokenRewardKey.SpendPerPropertyInfoAccess;
            var earnKey = TokenRewardKey.EarnPerTenancyDetailsSubmission;
            var spendAmount = tokenRewardService.GetCurrentReward(spendKey).Value;
            var earnAmount = tokenRewardService.GetCurrentReward(earnKey).Value;
            var earnQuantity = 11;
            var internalReference = Guid.NewGuid();
            var externalReference = "external-reference";

            // This also sets up the account for the user.
            var user = await CreateUser(CreateContainer(), "test@test.com", "1.2.3.4");

            var service1 = CreateContainer().Get<IUserTokenService>();
            var response1 = await service1.GetBalance(user.Id);
            Assert.AreEqual(0.0M, response1.balance, "Balance on response1 should be zero.");

            var sufficientFundsExist1 = await service1.SufficientFundsExistForTransaction(user.Id, spendKey);
            Assert.IsFalse(sufficientFundsExist1, "There shouldn't be sufficient funds to spend with zero balance.");

            // Make unsuccessful transaction
            var service2 = CreateContainer().Get<IUserTokenService>();
            var outcome2 = await service2.MakeTransaction(user.Id, spendKey);
            Assert.AreEqual(TokenAccountTransactionStatus.InsufficientFunds, outcome2, "Outcome2 is not the expected.");
            var response2 = await service2.GetBalance(user.Id);
            Assert.AreEqual(0.0M, response2.balance, "Balance on response2 should be zero.");

            var service3 = CreateContainer().Get<IUserTokenService>();
            var sufficientFundsExist3 = await service3.SufficientFundsExistForTransaction(user.Id, earnKey);
            Assert.IsTrue(sufficientFundsExist3, "There should be sufficient funds when earning tokens.");
            var outcome3 = await service3.MakeTransaction(user.Id, earnKey, 
                internalReference: internalReference, externalReference: externalReference, quantity: earnQuantity);
            Assert.AreEqual(TokenAccountTransactionStatus.Success, outcome3, "Outcome3 is not the expected.");
            var response3 = await service3.GetBalance(user.Id);
            Assert.AreEqual(earnAmount * earnQuantity, response3.balance, "Balance on response3 is not the expected.");

            var retrievedTransaction = await DbProbe.TokenAccountTransactions
                .Where(x => x.AccountId.Equals(user.Id))
                .SingleOrDefaultAsync();

            Assert.IsNotNull(retrievedTransaction, "The transaction was not found in the database.");
            Assert.AreEqual(internalReference, retrievedTransaction.InternalReference,
                "The internal reference is not the expected.");
            Assert.AreEqual(externalReference, retrievedTransaction.ExternalReference,
                "The external reference is not the expected.");
        }
    }
}
