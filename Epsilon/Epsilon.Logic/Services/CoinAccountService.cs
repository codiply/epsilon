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

namespace Epsilon.Logic.Services
{
    public class CoinAccountService : ICoinAccountService
    {
        private readonly IClock _clock;
        private readonly IEpsilonContext _dbContext;

        public CoinAccountService(
            IClock clock,
            IEpsilonContext dbContext)
        {
            _clock = clock;
            _dbContext = dbContext;
        }

        public async Task<CoinAccountTransaction> MakeTransaction(
            string accountId, 
            Decimal amount,
            CoinAccountTransactionTypeId transactionTypeId, 
            string reference)
        {
            if (await IsTimeToMakeSnapshot(accountId))
                await MakeSnapshot(accountId);

            var transaction = new CoinAccountTransaction
            {
                AccountId = accountId,
                Amount = amount,
                TypeId = transactionTypeId.ToString(),
                Reference = reference
            };

            _dbContext.CoinAccountTransactions.Add(transaction);
            await _dbContext.SaveChangesAsync();
            return transaction;
        }

        public async Task<Decimal> GetBalance(string accountId)
        {
            var lastSnapshot = await GetLastSnapshot(accountId);

            if (lastSnapshot == null)
            {
                var sumOfAmounts= await _dbContext.CoinAccountTransactions
                    .Where(x => x.AccountId.Equals(accountId))
                    .SumAsync(x => x.Amount);

                return sumOfAmounts;
            }
            else
            {
                var sumOfAmountsAfterSnapshot = await _dbContext.CoinAccountTransactions
                    .Where(tr => tr.AccountId.Equals(accountId))
                    .Where(tr => lastSnapshot.MadeOn <= tr.MadeOn)
                    .SumAsync(x => x.Amount);
                return lastSnapshot.Balance + sumOfAmountsAfterSnapshot;
            }
        }

        public async Task MakeSnapshot(string accountId)
        {
            using (new TransactionScope())
            {
                var lastSnapshot = await GetLastSnapshot(accountId);

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
                        .Where(tr => tr.AccountId.Equals(accountId))
                        .Where(tr => tr.MadeOn < newSnapshot.MadeOn)
                        .SumAsync(x => x.Amount);
                }
                else
                {
                    newSnapshotBalance = await _dbContext.CoinAccountTransactions
                        .Where(tr => tr.AccountId.Equals(accountId))
                        .Where(tr => lastSnapshot.MadeOn <= tr.MadeOn && tr.MadeOn < newSnapshot.MadeOn)
                        .SumAsync(tr => tr.Amount);
                }

                newSnapshot.Balance = newSnapshotBalance;
                newSnapshot.IsFinalised = true;

                _dbContext.Entry(newSnapshot).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
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

        private async Task<bool> IsTimeToMakeSnapshot(string accountId)
        {
            // TODO:
            return false;
        }
    }
}
