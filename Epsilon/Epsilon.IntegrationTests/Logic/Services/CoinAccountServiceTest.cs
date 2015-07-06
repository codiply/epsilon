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

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class CoinAccountServiceTest : BaseIntegrationTestWithRollback
    {
        [Test]
        public async Task GetBalance_ForNewUserWithoutTransactions_ReturnsZero()
        {
            var container = CreateContainer();
            var coinAccountService = container.Get<ICoinAccountService>();
            // This also sets up the account for the user.
            var user = await CreateUser(container, "test@test.com", "1.2.3.4");
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
            // This also sets up the account for the user.
            var user = await CreateUser(containerUnderTest, "test@test.com", "1.2.3.4");
            var accountId = user.Id;

            Decimal expectedBalance = 0;
            Decimal actualBalance;
            for (int i = 0; i < numberOfRounds; i++)
            {
                foreach (var am in amounts)
                {
                    var typeId = am < 0 ? CoinAccountTransactionTypeId.DEBIT : CoinAccountTransactionTypeId.CREDIT;
                    await coinAccountServiceUnderTest.MakeTransaction(user.Id, am, typeId, "");
                    expectedBalance += am;
                    actualBalance = await coinAccountServiceForVerification.GetBalance(accountId);
                    Assert.AreEqual(expectedBalance, actualBalance, 
                        String.Format("The balance is not the expected in iteration {0} after transaction with amount {1}", i, am));
                }
                await coinAccountServiceUnderTest.MakeSnapshot(user.Id);
            }

            actualBalance = await coinAccountServiceForVerification.GetBalance(accountId);

            Assert.AreEqual(expectedBalance, actualBalance);
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
            // This also sets up the account for the user.
            var user = await CreateUser(containerUnderTest, "test@test.com", "1.2.3.4");
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

        [Test]
        public async Task MakeTransaction_SetsAmountTypeAndReferenceCorrectly()
        {
            var container = CreateContainer();
            var coinAccountService = container.Get<ICoinAccountService>();
            // This also sets up the account for the user.
            var user = await CreateUser(container, "test@test.com", "1.2.3.4");
            var accountId = user.Id;

            var amount = 100M;
            var transactionTypeId = CoinAccountTransactionTypeId.CREDIT;
            var reference = "Test-Reference";

            var status = await coinAccountService.MakeTransaction(accountId, amount, transactionTypeId, reference);

            Assert.AreEqual(CoinAccountTransactionStatus.Success, status, "Transaction status was not Success.");

            var retrievedAcount = await DbProbe.CoinAccounts.Include(x => x.Transactions).SingleAsync(x => x.Id.Equals(accountId));
            var retrievedTransactions = retrievedAcount.Transactions;

            Assert.AreEqual(1, retrievedTransactions.Count, "The account should contain a single transaction.");

            var transaction = retrievedTransactions.Single();

            Assert.AreEqual(amount, transaction.Amount, "The amount on the transaction was not the expected.");
            Assert.AreEqual(EnumsHelper.CoinAccountTransactionTypeId.ToString(transactionTypeId), transaction.TypeId, "The transaction TypeId was not the expected.");
            Assert.AreEqual(reference, transaction.Reference, "The reference on the transaction was not the expected.");
        }

        [Test]
        public async Task MakeTransaction_MakesNoSnapshotsDuringTheSnoozePeriod()
        {
            var container = CreateContainer();
            double snoozePeriodInHours = 1.0;
            int snapshotTransactionsThreshold = 2;
            int numberOfTransactions = 10;
            SetupContainer(container, snoozePeriodInHours, snapshotTransactionsThreshold);

            var coinAccountService = container.Get<ICoinAccountService>();
            // This also sets up the account for the user.
            var user = await CreateUser(container, "test@test.com", "1.2.3.4");
            var accountId = user.Id;

            for (int i = 0; i < numberOfTransactions; i++)
            {
                await coinAccountService.MakeTransaction(accountId, 100, CoinAccountTransactionTypeId.CREDIT, "reference");
            }

            var account = await DbProbe.CoinAccounts.Include(x => x.Snapshots).SingleAsync(x => x.Id.Equals(accountId));
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

            var coinAccountService = container.Get<ICoinAccountService>();
            // This also sets up the account for the user.
            var user = await CreateUser(container, "test@test.com", "1.2.3.4");
            var accountId = user.Id;

            for (int i = 0; i < snapshotTransactionsThreshold; i++)
            {
                await coinAccountService.MakeTransaction(accountId, 100, CoinAccountTransactionTypeId.CREDIT, "reference");
            }

            var account = await DbProbe.CoinAccounts.Include(x => x.Snapshots).SingleAsync(x => x.Id.Equals(accountId));
            var snapshots = account.Snapshots;

            Assert.IsEmpty(snapshots, "There should be no snapshots created until the threshold is reached.");

            // I make one more transaction
            await coinAccountService.MakeTransaction(accountId, 100, CoinAccountTransactionTypeId.CREDIT, "reference");
            account = await DbProbe.CoinAccounts.Include(x => x.Snapshots).SingleAsync(x => x.Id.Equals(accountId));
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

            var coinAccountService = container.Get<ICoinAccountService>();
            // This also sets up the account for the user.
            var user = await CreateUser(container, "test@test.com", "1.2.3.4");
            var accountId = user.Id;

            for (int i = 0; i < expectedNumberOfSnapshots * snapshotTransactionsThreshold; i++)
            {
                await coinAccountService.MakeTransaction(accountId, 100, CoinAccountTransactionTypeId.CREDIT, "reference");
            }

            var account = await DbProbe.CoinAccounts.Include(x => x.Snapshots).SingleAsync(x => x.Id.Equals(accountId));
            var snapshots = account.Snapshots;

            Assert.AreEqual(expectedNumberOfSnapshots - 1, snapshots.Count, 
                "The number of snapshots before the last transaction is not the expected.");

            // I make one more transaction
            await coinAccountService.MakeTransaction(accountId, 100, CoinAccountTransactionTypeId.CREDIT, "reference");
            account = await DbProbe.CoinAccounts.Include(x => x.Snapshots).SingleAsync(x => x.Id.Equals(accountId));
            snapshots = account.Snapshots;

            Assert.AreEqual(expectedNumberOfSnapshots, snapshots.Count, "The final number of snapshots is not the expected.");
        }

        private static void SetupContainer(IKernel container, double snoozePeriodInHours, int snapshotTransactionsThreshold)
        {
            var settings = new NameValueCollection();
            settings.Add(AppSettingsKey.CoinAccountSnapshotSnoozePeriodInHours, snoozePeriodInHours.ToString());
            settings.Add(AppSettingsKey.CoinAccountSnapshotNumberOfTransactionsThreshold, snapshotTransactionsThreshold.ToString());
            container.Rebind<NameValueCollection>().ToConstant(settings).WhenInjectedExactlyInto<AppSettingsHelper>();

            var mockAppSettingsDefaultValue = new Mock<IAppSettingsDefaultValue>();
            mockAppSettingsDefaultValue.Setup(x => x.CoinAccountSnapshotSnoozePeriodInHours).Returns(snoozePeriodInHours);
            mockAppSettingsDefaultValue.Setup(x => x.CoinAccountSnapshotNumberOfTransactionsThreshold)
                .Returns(snapshotTransactionsThreshold);
            container.Rebind<IAppSettingsDefaultValue>().ToConstant(mockAppSettingsDefaultValue.Object);            
        }
    }
}
