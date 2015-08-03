using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.JsonModels;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services
{
    public class UserTokenService : IUserTokenService
    {
        private readonly IAppCache _appCache;
        private readonly IEpsilonContext _dbContext;
        private readonly ITokenAccountService _tokenAccountService;

        public UserTokenService(
            IAppCache appCache,
            IEpsilonContext dbContext,
            ITokenAccountService tokenAccountService)
        {
            _appCache = appCache;
            _dbContext = dbContext;
            _tokenAccountService = tokenAccountService;
        }

        public async Task CreateAccount(string userId)
        {
            await _tokenAccountService.CreateAccount(userId);
        }

        public async Task<TokenBalanceResponse> GetBalance(string userId)
        {
            var balance = await _appCache.GetAsync(AppCacheKey.UserTokenBalance(userId), async () =>
             {
                 return (object) await _tokenAccountService.GetBalance(userId);
             }, WithLock.No);
            return new TokenBalanceResponse { balance = (decimal)balance };
        }

        public async Task<TokenAccountTransactionStatus> MakeTransaction(
            string userId, decimal amount, TokenRewardKey tokenRewardKey, Guid? internalReference, 
            string externalReference = null, int quantity = 1)
        {
            switch (tokenRewardKey.AmountSign())
            {
                // TODO_PANOS_TEST
                case TokenRewardKeyAmountSign.Positive:
                    if (amount < 0.0M)
                        return TokenAccountTransactionStatus.WrongAmount;
                    break;
                // TODO_PANOS_TEST
                case TokenRewardKeyAmountSign.Negative:
                    if (amount > 0.0M)
                        return TokenAccountTransactionStatus.WrongAmount;
                    break;
            }

            var transactionStatus = await _tokenAccountService
                .MakeTransaction(userId, amount, tokenRewardKey, internalReference, externalReference, quantity);
            if (transactionStatus == TokenAccountTransactionStatus.Success)
                _appCache.Remove(AppCacheKey.UserTokenBalance(userId));
            return transactionStatus;
        }
    }
}
