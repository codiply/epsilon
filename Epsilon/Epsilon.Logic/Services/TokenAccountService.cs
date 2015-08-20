using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.JsonModels;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace Epsilon.Logic.Services
{
    public class TokenAccountService : ITokenAccountService
    {
        private readonly IClock _clock;
        private readonly ITokenAccountServiceConfig _tokenAccountServiceConfig;
        private readonly IEpsilonContext _dbContext;

        public TokenAccountService(
            IClock clock,
            ITokenAccountServiceConfig tokenAccountServiceConfig,
            IEpsilonContext dbContext)
        {
            _clock = clock;
            _tokenAccountServiceConfig = tokenAccountServiceConfig;
            _dbContext = dbContext;
        }

        public async Task CreateAccount(string accountId)
        {
            var newAccount = new TokenAccount
            {
                Id = accountId,
                LastSnapshotOn = _clock.OffsetNow
            };
            _dbContext.TokenAccounts.Add(newAccount);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> SufficientFundsExistForTransaction(string accountId, decimal amount)
        {
            // TODO_TEST_PANOS: whole function
            if (amount >= 0.0M)
                return true;

            var balance = await GetBalance(accountId);

            return balance + amount >= 0.0M;
        }

        public async Task<TokenAccountTransactionStatus> MakeTransaction(
            string accountId, 
            decimal amount,
            TokenRewardKey tokenRewardKey,
            Guid? internalReference = null,
            string externalReference = null,
            int quantity = 1)
        {
            // TODO_TEST_PANOS
            if (quantity < 1)
                return TokenAccountTransactionStatus.WrongQuantity;

            var account = await _dbContext.TokenAccounts.FindAsync(accountId);

            if (account == null)
                return TokenAccountTransactionStatus.AccountNotFound;

            if (await IsTimeToMakeSnapshot(account))
                await MakeSnapshot(account.Id);

            if (amount < 0)
            {
                var currentBalance = await GetBalance(account.Id);
                if (currentBalance + amount < 0.0M)
                    return TokenAccountTransactionStatus.InsufficientFunds;
            }

            // NOTE: I am checking for sufficient funds above and making the transaction below.
            //       It is still possible to get another transaction in between that will result 
            //       in spending more than the balance. In this rare event we will just get a 
            //       negative balance for this account, which is acceptable.

            var transaction = new TokenAccountTransaction
            {
                AccountId = accountId,
                Amount = amount,
                Quantity = quantity,
                RewardTypeKey = EnumsHelper.TokenRewardKey.ToString(tokenRewardKey),
                ExternalReference = externalReference,
                InternalReference = internalReference
            };

            _dbContext.TokenAccountTransactions.Add(transaction);
            await _dbContext.SaveChangesAsync();

            return TokenAccountTransactionStatus.Success;
        }

        public async Task<Decimal> GetBalance(string accountId)
        {
            var account = await _dbContext.TokenAccounts.FindAsync(accountId);

            if (account == null)
                throw new ArgumentException(string.Format("No account found for acountId: '{0}'", accountId));

            var lastSnapshot = await GetLastSnapshot(account.Id);

            if (lastSnapshot == null)
            {
                var sumOfAmounts= await _dbContext.TokenAccountTransactions
                    .Where(tr => tr.AccountId.Equals(account.Id))
                    .Select(tr => tr.Amount)
                    .DefaultIfEmpty(0.0M)
                    .SumAsync();

                return sumOfAmounts;
            }
            else
            {
                var sumOfAmountsAfterSnapshot = await _dbContext.TokenAccountTransactions
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
            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var account = await _dbContext.TokenAccounts.FindAsync(accountId);

                if (account == null)
                    throw new ArgumentException(string.Format("No account found for acountId: '{0}'", accountId));

                var lastSnapshot = await GetLastSnapshot(account.Id);

                var newSnapshot = new TokenAccountSnapshot
                {
                    AccountId = accountId,
                    IsFinalised = false
                };
                _dbContext.TokenAccountSnapshots.Add(newSnapshot);
                await _dbContext.SaveChangesAsync();

                decimal newSnapshotBalance;
                if (lastSnapshot == null)
                {
                    newSnapshotBalance = await _dbContext.TokenAccountTransactions
                        .Where(tr => tr.AccountId.Equals(account.Id))
                        .Where(tr => tr.MadeOn < newSnapshot.MadeOn)
                        .Select(tr => tr.Amount)
                        .DefaultIfEmpty(0.0M)
                        .SumAsync();
                }
                else
                {
                    var newTransactionsSum = await _dbContext.TokenAccountTransactions
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

                transactionScope.Complete();
            }
        }

        public async Task<MyTokenTransactionsPageResponse> GetMyTokenTransactionsNextPage(
            string accountId, MyTokenTransactionsPageRequest request, int pageSize)
        {
            // TODO_TEST_PANOS: the whole thing
            var query = _dbContext.TokenAccountTransactions
                .OrderByDescending(x => x.MadeOn)
                .Where(x => x.AccountId.Equals(accountId));
                
            if (request.madeBefore.HasValue)
                query = query.Where(x => x.MadeOn < request.madeBefore.Value);

            var transactions = await query.Take(pageSize + 1).ToListAsync();

            bool moreItemsExist = false;
            if (transactions.Count > pageSize)
            {
                moreItemsExist = true;
                transactions = transactions.Take(pageSize).ToList();
            }

            return new MyTokenTransactionsPageResponse
            {
                items = transactions.Select(x => x.ToItem()).ToList(),
                moreItemsExist = moreItemsExist,
                earliestMadeOn = transactions.Any() ? transactions.Last().MadeOn : request.madeBefore
            };
        }

        private async Task<TokenAccountSnapshot> GetLastSnapshot(string accountId)
        {
            var lastSnapshot = await _dbContext.TokenAccountSnapshots
                .Where(x => x.IsFinalised)
                .Where(x => x.AccountId.Equals(accountId))
                .OrderByDescending(x => x.MadeOn)
                .FirstOrDefaultAsync();
            return lastSnapshot;
        }

        private async Task<bool> IsTimeToMakeSnapshot(TokenAccount account)
        {
            var timeElapsedSinceLastSnapshot = _clock.OffsetNow - account.LastSnapshotOn;
            var snoozePeriod = _tokenAccountServiceConfig.SnapshotSnoozePeriod;
            if (timeElapsedSinceLastSnapshot < snoozePeriod)
                return false;

            var transactionsThreshold = _tokenAccountServiceConfig.SnapshotNumberOfTransactionsThreshold;

            var lastSnapshot = await GetLastSnapshot(account.Id);

            int numberOfTransactionsSinceLastSnapshot;
            if (lastSnapshot == null)
            {
                numberOfTransactionsSinceLastSnapshot = await _dbContext.TokenAccountTransactions
                    .Where(tr => tr.AccountId.Equals(account.Id))
                    .CountAsync();
            }
            else
            {
                numberOfTransactionsSinceLastSnapshot = await _dbContext.TokenAccountTransactions
                    .Where(tr => tr.AccountId.Equals(account.Id))
                    .Where(tr => lastSnapshot.MadeOn <= tr.MadeOn)
                    .CountAsync();
            }
            return numberOfTransactionsSinceLastSnapshot >= transactionsThreshold;
        }
    }
}
