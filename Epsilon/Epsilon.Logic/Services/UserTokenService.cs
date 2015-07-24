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

        public TokenBalanceResponse GetBalance(string userId)
        {
            var balance = (decimal)_appCache.Get(AppCacheKey.UserTokenBalance(userId), () =>
             {
                 return (object)Task.Run(() => _tokenAccountService.GetBalance(userId)).Result;
             }, WithLock.No);
            return new TokenBalanceResponse { balance = balance };
        }

        public async Task<TokenAccountTransactionStatus> Credit(string userId, Decimal amount)
        {
            if (amount < 0)
                return TokenAccountTransactionStatus.WrongAmount;
            return await MakeTransaction(userId, amount, TokenAccountTransactionTypeId.CREDIT, "");
        }

        public async Task<TokenAccountTransactionStatus> Debit(string userId, Decimal amount)
        {
            if (amount < 0)
                return TokenAccountTransactionStatus.WrongAmount;
            return await MakeTransaction(userId, -amount, TokenAccountTransactionTypeId.DEBIT, "");
        }

        private async Task<TokenAccountTransactionStatus> MakeTransaction(
            string userId, decimal amount, TokenAccountTransactionTypeId transactionTypeId, string reference)
        {
            var transactionStatus = await _tokenAccountService.MakeTransaction(userId, amount, transactionTypeId, reference);
            if (transactionStatus == TokenAccountTransactionStatus.Success)
                _appCache.Remove(AppCacheKey.UserTokenBalance(userId));
            return transactionStatus;
        }
    }
}
