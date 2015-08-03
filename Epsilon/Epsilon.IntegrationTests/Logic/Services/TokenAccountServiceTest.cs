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
using System.Data.Entity;
using Epsilon.Logic.Constants;
using Moq;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Constants.Interfaces;
using System.Collections.Specialized;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Configuration.Interfaces;

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class TokenAccountServiceTest : BaseIntegrationTestWithRollback
    {
        [Test]
        public async Task GetBalance_ForNewUserWithoutTransactions_ReturnsZero()
        {
            var container = CreateContainer();
            var tokenAccountService = container.Get<ITokenAccountService>();
            // This also sets up the account for the user.
            var user = await CreateUser(container, "test@test.com", "1.2.3.4");
            var accountId = user.Id;

            var expectedBalance = 0.0M;
            var actualBalance = await tokenAccountService.GetBalance(accountId);

            Assert.AreEqual(expectedBalance, actualBalance);
        }

        [Test]
        public void GetBalance_ForNonExistingAccount_ThrowsArgumentException()
        {
            var container = CreateContainer();
            var tokenAccountService = container.Get<ITokenAccountService>();
            var accountId = "non-existing-account-id";

            Assert.Throws(typeof(ArgumentException), async () => await tokenAccountService.GetBalance(accountId));
        }

        [Test]
        public async Task GetBalance_AfterMakingHundredTransactionsAndTenSnapshots_ReturnsRightBalance()
        {
            var containerUnderTest = CreateContainer();
            var containerForVerification = CreateContainer();
            var amounts = new List<Decimal> { 200, -100, 300, -200, 400, -300, 500, -400, 600, -500 };
            var numberOfRounds = 10;
            var tokenAccountServiceUnderTest = containerUnderTest.Get<ITokenAccountService>();
            var tokenAccountServiceForVerification = containerForVerification.Get<ITokenAccountService>();
            // This also sets up the account for the user.
            var user = await CreateUser(containerUnderTest, "test@test.com", "1.2.3.4");
            var accountId = user.Id;

            Decimal expectedBalance = 0;
            Decimal actualBalance;
            for (int i = 0; i < numberOfRounds; i++)
            {
                foreach (var am in amounts)
                {
                    var tokenRewardKey = am < 0 ? TokenRewardKey.SpendPerPropertyDetailsAccess : TokenRewardKey.EarnPerVerificationMailSent;
                    await tokenAccountServiceUnderTest.MakeTransaction(user.Id, am, tokenRewardKey);
                    expectedBalance += am;
                    actualBalance = await tokenAccountServiceForVerification.GetBalance(accountId);
                    Assert.AreEqual(expectedBalance, actualBalance,
                        string.Format("The balance is not the expected in iteration {0} after transaction with amount {1}", i, am));
                }
                await tokenAccountServiceUnderTest.MakeSnapshot(user.Id);
            }

            actualBalance = await tokenAccountServiceForVerification.GetBalance(accountId);

            Assert.AreEqual(expectedBalance, actualBalance);
        }

        [Test]
        public async Task MakeTransaction_ForNonExistingAccount_ReturnsAccountNotFound()
        {
            var container = CreateContainer();
            var tokenAccountService = container.Get<ITokenAccountService>();
            var accountId = "non-existing-account-id";

            var status = await tokenAccountService.MakeTransaction(accountId, 100, TokenRewardKey.EarnPerVerificationCodeEntered);

            Assert.AreEqual(TokenAccountTransactionStatus.AccountNotFound, status);
        }

        [Test]
        public async Task MakeTransaction_ForAmountGreaterThanTheBalance_ReturnsInsufficientFundsAndBalanceDoesNotChange()
        {
            var containerUnderTest = CreateContainer();
            var containerForVerification = CreateContainer();

            var tokenAccountServiceUnderTest = containerUnderTest.Get<ITokenAccountService>();
            var tokenAccountServiceForVerification = containerForVerification.Get<ITokenAccountService>();
            // This also sets up the account for the user.
            var user = await CreateUser(containerUnderTest, "test@test.com", "1.2.3.4");
            var accountId = user.Id;

            var creditAmount = 100M;
            var debitAmount = -creditAmount - 1.0M;

            var creditStatus = await tokenAccountServiceUnderTest.MakeTransaction(accountId, creditAmount, TokenRewardKey.EarnPerVerificationCodeEntered);
            var balanceAfterCredit = await tokenAccountServiceForVerification.GetBalance(accountId);
            var debitStatus = await tokenAccountServiceUnderTest.MakeTransaction(accountId, debitAmount, TokenRewardKey.SpendPerPropertyDetailsAccess);
            var balanceAfterDebit = await tokenAccountServiceForVerification.GetBalance(accountId);

            Assert.AreEqual(TokenAccountTransactionStatus.Success, creditStatus,
                "Status returned by Credit transaction is not the expected.");
            Assert.AreEqual(creditAmount, balanceAfterCredit, "Balance after Credit transaction is not the expected.");
            Assert.AreEqual(TokenAccountTransactionStatus.InsufficientFunds, debitStatus,
                "Status returned by Debit transaction is not the expected.");
            Assert.AreEqual(creditAmount, balanceAfterDebit,
                "Balance after unsuccessful Debit transaction should be the same as before.");
        }

        [Test]
        public async Task MakeTransaction_SetsAmountTypeReferencesAndQuantityDefaultValuesCorrectly()
        {
            var container = CreateContainer();
            var tokenAccountService = container.Get<ITokenAccountService>();
            // This also sets up the account for the user.
            var user = await CreateUser(container, "test@test.com", "1.2.3.4");
            var accountId = user.Id;

            var amount = 100M;
            var tokenRewardKey = TokenRewardKey.SpendPerPropertyDetailsAccess;
            var expectedQuantity = 1;

            var status = await tokenAccountService.MakeTransaction(accountId, amount, tokenRewardKey);

            Assert.AreEqual(TokenAccountTransactionStatus.Success, status, "Transaction status was not Success.");

            var retrievedAcount = await DbProbe.TokenAccounts.Include(x => x.Transactions).SingleAsync(x => x.Id.Equals(accountId));
            var retrievedTransactions = retrievedAcount.Transactions;

            Assert.AreEqual(1, retrievedTransactions.Count, "The account should contain a single transaction.");

            var transaction = retrievedTransactions.Single();

            Assert.AreEqual(amount, transaction.Amount, "The amount on the transaction was not the expected.");
            Assert.AreEqual(EnumsHelper.TokenRewardKey.ToString(tokenRewardKey), transaction.RewardTypeKey, "The transaction RewardTypeKey was not the expected.");
            Assert.IsNull(transaction.InternalReference,
                "The internal reference on the transaction was not the expected.");
            Assert.IsNull(transaction.ExternalReference,
                "The external reference on the transaction was not the expected.");
            Assert.AreEqual(expectedQuantity, transaction.Quantity,
                "The quantity on the transaction was not the expected.");
        }

        [Test]
        public async Task MakeTransaction_SetsAmountTypeReferencesAndQuantityCorrectly()
        {
            var container = CreateContainer();
            var tokenAccountService = container.Get<ITokenAccountService>();
            // This also sets up the account for the user.
            var user = await CreateUser(container, "test@test.com", "1.2.3.4");
            var accountId = user.Id;

            var amount = 100M;
            var tokenRewardKey = TokenRewardKey.SpendPerPropertyDetailsAccess;
            var internalReference = Guid.NewGuid();
            var externalReference = "externa-reference";
            var quantity = 12;

            var status = await tokenAccountService
                .MakeTransaction(accountId, amount, tokenRewardKey, internalReference, externalReference, quantity);
            
            Assert.AreEqual(TokenAccountTransactionStatus.Success, status, "Transaction status was not Success.");

            var retrievedAcount = await DbProbe.TokenAccounts.Include(x => x.Transactions).SingleAsync(x => x.Id.Equals(accountId));
            var retrievedTransactions = retrievedAcount.Transactions;

            Assert.AreEqual(1, retrievedTransactions.Count, "The account should contain a single transaction.");

            var transaction = retrievedTransactions.Single();

            Assert.AreEqual(amount, transaction.Amount, "The amount on the transaction was not the expected.");
            Assert.AreEqual(EnumsHelper.TokenRewardKey.ToString(tokenRewardKey), transaction.RewardTypeKey, "The transaction RewardTypeKey was not the expected.");
            Assert.AreEqual(internalReference, transaction.InternalReference, 
                "The internal reference on the transaction was not the expected.");
            Assert.AreEqual(externalReference, transaction.ExternalReference,
                "The external reference on the transaction was not the expected.");
            Assert.AreEqual(quantity, transaction.Quantity,
                "The quantity on the transaction was not the expected.");
        }

        [Test]
        public async Task MakeTransaction_MakesNoSnapshotsDuringTheSnoozePeriod()
        {
            var container = CreateContainer();
            double snoozePeriodInHours = 1.0;
            int snapshotTransactionsThreshold = 2;
            int numberOfTransactions = 10;
            SetupContainer(container, snoozePeriodInHours, snapshotTransactionsThreshold);

            var tokenAccountService = container.Get<ITokenAccountService>();
            // This also sets up the account for the user.
            var user = await CreateUser(container, "test@test.com", "1.2.3.4");
            var accountId = user.Id;

            for (int i = 0; i < numberOfTransactions; i++)
            {
                await tokenAccountService.MakeTransaction(accountId, 100, TokenRewardKey.EarnPerVerificationCodeEntered);
            }

            var account = await DbProbe.TokenAccounts.Include(x => x.Snapshots).SingleAsync(x => x.Id.Equals(accountId));
            var snapshots = account.Snapshots;

            Assert.IsEmpty(snapshots, "There should be no snapshots created.");
        }

        [Test]
        public async Task MakeTransaction_MakesSnapshotWhenTheTresholdIsReached()
        {
            var container = CreateContainer();
            double snoozePeriodInHours = 0.0;
            int snapshotTransactionsThreshold = 2;
            SetupContainer(container, snoozePeriodInHours, snapshotTransactionsThreshold);

            var tokenAccountService = container.Get<ITokenAccountService>();
            // This also sets up the account for the user.
            var user = await CreateUser(container, "test@test.com", "1.2.3.4");
            var accountId = user.Id;

            for (int i = 0; i < snapshotTransactionsThreshold; i++)
            {
                await tokenAccountService.MakeTransaction(accountId, 100, TokenRewardKey.EarnPerVerificationCodeEntered);
            }

            var account = await DbProbe.TokenAccounts.Include(x => x.Snapshots).SingleAsync(x => x.Id.Equals(accountId));
            var snapshots = account.Snapshots;

            Assert.IsEmpty(snapshots, "There should be no snapshots created until the threshold is reached.");

            // I make one more transaction
            await tokenAccountService.MakeTransaction(accountId, 100, TokenRewardKey.EarnPerVerificationCodeEntered);
            account = await DbProbe.TokenAccounts.Include(x => x.Snapshots).SingleAsync(x => x.Id.Equals(accountId));
            snapshots = account.Snapshots;

            Assert.AreEqual(1, snapshots.Count, "There should be a single snapshot after the threshold is exceeded.");
        }

        [Test]
        public async Task MakeTransaction_TheNumberOfSnapshotsIsTheExpected()
        {
            var container = CreateContainer();
            double snoozePeriodInHours = 0.0;
            int snapshotTransactionsThreshold = 2;
            int expectedNumberOfSnapshots = 5;
            SetupContainer(container, snoozePeriodInHours, snapshotTransactionsThreshold);

            var tokenAccountService = container.Get<ITokenAccountService>();
            // This also sets up the account for the user.
            var user = await CreateUser(container, "test@test.com", "1.2.3.4");
            var accountId = user.Id;

            for (int i = 0; i < expectedNumberOfSnapshots * snapshotTransactionsThreshold; i++)
            {
                await tokenAccountService.MakeTransaction(accountId, 100, TokenRewardKey.EarnPerVerificationCodeEntered);
            }

            var account = await DbProbe.TokenAccounts.Include(x => x.Snapshots).SingleAsync(x => x.Id.Equals(accountId));
            var snapshots = account.Snapshots;

            Assert.AreEqual(expectedNumberOfSnapshots - 1, snapshots.Count,
                "The number of snapshots before the last transaction is not the expected.");

            // I make one more transaction
            await tokenAccountService.MakeTransaction(accountId, 100, TokenRewardKey.EarnPerVerificationCodeEntered);
            account = await DbProbe.TokenAccounts.Include(x => x.Snapshots).SingleAsync(x => x.Id.Equals(accountId));
            snapshots = account.Snapshots;

            Assert.AreEqual(expectedNumberOfSnapshots, snapshots.Count, "The final number of snapshots is not the expected.");
        }

        private static void SetupContainer(IKernel container, double snoozePeriodInHours, int snapshotTransactionsThreshold)
        {
            var mockTokenAccountServiceConfig = new Mock<ITokenAccountServiceConfig>();
            mockTokenAccountServiceConfig.Setup(x => x.SnapshotSnoozePeriod).Returns(TimeSpan.FromHours(snoozePeriodInHours));
            mockTokenAccountServiceConfig.Setup(x => x.SnapshotNumberOfTransactionsThreshold).Returns(snapshotTransactionsThreshold);
            container.Rebind<ITokenAccountServiceConfig>().ToConstant(mockTokenAccountServiceConfig.Object);
        }
    }
}
