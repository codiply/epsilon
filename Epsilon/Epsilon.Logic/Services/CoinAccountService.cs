using Epsilon.Logic.Constants;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Transactions;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Constants.Interfaces;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Configuration.Interfaces;

namespace Epsilon.Logic.Services
{
    public class CoinAccountService : ICoinAccountService
    {
        private readonly IClock _clock;
        private readonly ICoinAccountServiceConfig _coinAccountServiceConfig;
        private readonly IEpsilonContext _dbContext;

        public CoinAccountService(
            IClock clock,
            ICoinAccountServiceConfig coinAccountServiceConfig,
            IEpsilonContext dbContext)
        {
            _clock = clock;
            _coinAccountServiceConfig = coinAccountServiceConfig;
            _dbContext = dbContext;
        }

        public async Task CreateAccount(string accountId)
        {
            var newAccount = new CoinAccount
            {
                Id = accountId,
                LastSnapshotOn = _clock.OffsetNow
            };
            _dbContext.CoinAccounts.Add(newAccount);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<CoinAccountTransactionStatus> MakeTransaction(
            string accountId, 
            Decimal amount,
            CoinAccountTransactionTypeId transactionTypeId, 
            string reference)
        {
            var account = await _dbContext.CoinAccounts.FindAsync(accountId);

            if (account == null)
                return CoinAccountTransactionStatus.AccountNotFound;

            if (await IsTimeToMakeSnapshot(account))
                await MakeSnapshot(account.Id);

            if (amount < 0)
            {
                var currentBalance = await GetBalance(account.Id);
                if (currentBalance + amount < 0.0M)
                    return CoinAccountTransactionStatus.InsufficientFunds;
            }

            // NOTE: I am checking for sufficient funds above and making the transaction below.
            //       It is still possible to get another transaction in between that will result 
            //       in spending more than the balance. In this rare event we will just get a 
            //       negative balance for this account, which is acceptable.

            var transaction = new CoinAccountTransaction
            {
                AccountId = accountId,
                Amount = amount,
                TypeId = EnumsHelper.CoinAccountTransactionTypeId.ToString(transactionTypeId),
                Reference = reference
            };

            _dbContext.CoinAccountTransactions.Add(transaction);
            await _dbContext.SaveChangesAsync();

            return CoinAccountTransactionStatus.Success;
        }

        public async Task<Decimal> GetBalance(string accountId)
        {
            var account = await _dbContext.CoinAccounts.FindAsync(accountId);

            if (account == null)
                throw new ArgumentException(string.Format("No account found for acountId: '{0}'", accountId));

            var lastSnapshot = await GetLastSnapshot(account.Id);

            if (lastSnapshot == null)
            {
                var sumOfAmounts= await _dbContext.CoinAccountTransactions
                    .Where(tr => tr.AccountId.Equals(account.Id))
                    .Select(tr => tr.Amount)
                    .DefaultIfEmpty(0.0M)
                    .SumAsync();

                return sumOfAmounts;
            }
            else
            {
                var sumOfAmountsAfterSnapshot = await _dbContext.CoinAccountTransactions
                    .Where(tr => tr.AccountId.Equals(account.Id))
                    .Where(tr => lastSnapshot.MadeOn <= tr.MadeOn)
                    .Select(tr => tr.Amount)
                    .DefaultIfEmpty(0.0M)
                    .SumAsync();
                return lastSnapshot.Balance + sumOfAmountsAfterSnapshot;
            }
        }

        public async Task MakeSnapshot(string accountId)
        {
            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var account = await _dbContext.CoinAccounts.FindAsync(accountId);

                if (account == null)
                    throw new ArgumentException(string.Format("No account found for acountId: '{0}'", accountId));

                var lastSnapshot = await GetLastSnapshot(account.Id);

                var newSnapshot = new CoinAccountSnapshot
                {
                    AccountId = accountId,
                    IsFinalised = false
                };
                _dbContext.CoinAccountSnapshots.Add(newSnapshot);
                await _dbContext.SaveChangesAsync();

                decimal newSnapshotBalance;
                if (lastSnapshot == null)
                {
                    newSnapshotBalance = await _dbContext.CoinAccountTransactions
                        .Where(tr => tr.AccountId.Equals(account.Id))
                        .Where(tr => tr.MadeOn < newSnapshot.MadeOn)
                        .Select(tr => tr.Amount)
                        .DefaultIfEmpty(0.0M)
                        .SumAsync();
                }
                else
                {
                    var newTransactionsSum = await _dbContext.CoinAccountTransactions
                        .Where(tr => tr.AccountId.Equals(account.Id))
                        .Where(tr => lastSnapshot.MadeOn <= tr.MadeOn && tr.MadeOn < newSnapshot.MadeOn)
                        .Select(tr => tr.Amount)
                        .DefaultIfEmpty(0.0M)
                        .SumAsync();
                    newSnapshotBalance = lastSnapshot.Balance + newTransactionsSum;
                }

                newSnapshot.Balance = newSnapshotBalance;
                newSnapshot.IsFinalised = true;
                _dbContext.Entry(newSnapshot).State = EntityState.Modified;

                account.LastSnapshotOn = newSnapshot.MadeOn;
                _dbContext.Entry(account).State = EntityState.Modified;

                await _dbContext.SaveChangesAsync();

                transaction.Complete();
            }
        }

        private async Task<CoinAccountSnapshot> GetLastSnapshot(string accountId)
        {
            var lastSnapshot = await _dbContext.CoinAccountSnapshots
                .Where(x => x.IsFinalised)
                .Where(x => x.AccountId.Equals(accountId))
                .OrderByDescending(x => x.MadeOn)
                .FirstOrDefaultAsync();
            return lastSnapshot;
        }

        private async Task<bool> IsTimeToMakeSnapshot(CoinAccount account)
        {
            var timeElapsedSinceLastSnapshot = _clock.OffsetNow - account.LastSnapshotOn;
            var snoozePeriod = _coinAccountServiceConfig.SnapshotSnoozePeriod;
            if (timeElapsedSinceLastSnapshot < snoozePeriod)
                return false;

            var transactionsThreshold = _coinAccountServiceConfig.SnapshotNumberOfTransactionsThreshold;

            var lastSnapshot = await GetLastSnapshot(account.Id);

            int numberOfTransactionsSinceLastSnapshot;
            if (lastSnapshot == null)
            {
                numberOfTransactionsSinceLastSnapshot = await _dbContext.CoinAccountTransactions
                    .Where(tr => tr.AccountId.Equals(account.Id))
                    .CountAsync();
            }
            else
            {
                numberOfTransactionsSinceLastSnapshot = await _dbContext.CoinAccountTransactions
                    .Where(tr => tr.AccountId.Equals(account.Id))
                    .Where(tr => lastSnapshot.MadeOn <= tr.MadeOn)
                    .CountAsync();
            }
            return numberOfTransactionsSinceLastSnapshot >= transactionsThreshold;
        }
    }
}
