using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.Logic.Services.Interfaces;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Constants.Enums;

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class CoinAccountServiceTest : BaseIntegrationTestWithRollback
    {
        [Test]
        public async Task GetBalance_ForNewUserWithoutTransactions_ReturnsZero()
        {
            var container = CreateContainer();
            var coinAccountService = container.Get<ICoinAccountService>();
            var user = await CreateUser(container, "test@test.com");
            var accountId = user.Id;
            
            var expectedBalance = 0.0M;
            var actualBalance = await coinAccountService.GetBalance(accountId);

            Assert.AreEqual(expectedBalance, actualBalance);
        }

        [Test]
        public void GetBalance_ForNonExistingAccount_ThrowsArgumentException()
        {
            var container = CreateContainer();
            var coinAccountService = container.Get<ICoinAccountService>();
            var accountId = "non-existing-account-id";

            Assert.Throws(typeof(ArgumentException), async () => await coinAccountService.GetBalance(accountId));
        }

        [Test]
        public async Task GetBalance_AfterMakingHundredTransactionsAndTenSnapshots_ReturnsRightBalance()
        {
            var containerUnderTest = CreateContainer();
            var containerForVerification = CreateContainer();
            var amounts = new List<Decimal> { 200, -100, 300, -200, 400, -300, 500, -400, 600, -500 };
            var numberOfRounds = 10;
            var coinAccountServiceUnderTest = containerUnderTest.Get<ICoinAccountService>();
            var coinAccountServiceForVerification = containerForVerification.Get<ICoinAccountService>();
            var user = await CreateUser(containerUnderTest, "test@test.com");
            var accountId = user.Id;

            for (int i = 0; i < numberOfRounds; i++)
            {
                foreach (var am in amounts)
                {
                    var typeId = am < 0 ? CoinAccountTransactionTypeId.DEBIT : CoinAccountTransactionTypeId.CREDIT;
                    await coinAccountServiceUnderTest.MakeTransaction(user.Id, am, typeId, "");
                }
                await coinAccountServiceUnderTest.MakeSnapshot(user.Id);
            }

            var expectdBalance = amounts.Sum() * numberOfRounds;
            var actualBalance = await coinAccountServiceForVerification.GetBalance(accountId);

            Assert.AreEqual(expectdBalance, actualBalance);
        }

        [Test]
        public async Task MakeTransaction_ForNonExistingAccount_ReturnsAccountNotFound()
        {
            var container = CreateContainer();
            var coinAccountService = container.Get<ICoinAccountService>();
            var accountId = "non-existing-account-id";

            var status = await coinAccountService.MakeTransaction(accountId, 100, CoinAccountTransactionTypeId.CREDIT, "");

            Assert.AreEqual(CoinAccountTransactionStatus.AccountNotFound, status);
        }

        [Test]
        public async Task MakeTransaction_ForAmountGreaterThanTheBalance_ReturnsInsufficientFundsAndBalanceDoesNotChange()
        {
            var containerUnderTest = CreateContainer();
            var containerForVerification = CreateContainer();

            var coinAccountServiceUnderTest = containerUnderTest.Get<ICoinAccountService>();
            var coinAccountServiceForVerification = containerForVerification.Get<ICoinAccountService>();

            var user = await CreateUser(containerUnderTest, "test@test.com");
            var accountId = user.Id;

            var creditAmount = 100M;
            var debitAmount = -creditAmount - 1.0M;

            var creditStatus = await coinAccountServiceUnderTest.MakeTransaction(accountId, creditAmount, CoinAccountTransactionTypeId.CREDIT, "");
            var balanceAfterCredit = await coinAccountServiceForVerification.GetBalance(accountId);
            var debitStatus = await coinAccountServiceUnderTest.MakeTransaction(accountId, debitAmount, CoinAccountTransactionTypeId.DEBIT, "");
            var balanceAfterDebit = await coinAccountServiceForVerification.GetBalance(accountId);

            Assert.AreEqual(CoinAccountTransactionStatus.Success, creditStatus,
                "Status returned by Credit transaction is not the expected.");
            Assert.AreEqual(creditAmount, balanceAfterCredit, "Balance after Credit transaction is not the expected.");
            Assert.AreEqual(CoinAccountTransactionStatus.InsufficientFunds, debitStatus,
                "Status returned by Debit transaction is not the expected.");
            Assert.AreEqual(creditAmount, balanceAfterDebit, 
                "Balance after unsuccessful Debit transaction should be the same as before.");
        }

        // TODO_PANOS: Test that the transaction sets the type and the reference correctly.
        // TODO_PANOS: Test snapshots are made as part of transactions and are well timed.
    }
}
