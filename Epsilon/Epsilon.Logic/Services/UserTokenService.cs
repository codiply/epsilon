﻿using Epsilon.Logic.Configuration.Interfaces;
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
        private readonly IUserTokenServiceConfig _userTokenServiceConfig;
        private readonly ITokenAccountService _tokenAccountService;
        private readonly ITokenRewardService _tokenRewardService;

        public UserTokenService(
            IAppCache appCache,
            IEpsilonContext dbContext,
            IUserTokenServiceConfig userTokenServiceConfig,
            ITokenAccountService tokenAccountService,
            ITokenRewardService tokenRewardService)
        {
            _appCache = appCache;
            _dbContext = dbContext;
            _userTokenServiceConfig = userTokenServiceConfig;
            _tokenAccountService = tokenAccountService;
            _tokenRewardService = tokenRewardService;
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
            string userId, TokenRewardKey tokenRewardKey, Guid? internalReference,
            string externalReference = null, int quantity = 1)
        {
            // TODO_PANOS_TEST
            if (quantity < 1)
                return TokenAccountTransactionStatus.WrongQuantity;

            var reward = _tokenRewardService.GetCurrentReward(tokenRewardKey);
            var totalAmount = quantity * reward.Value;

            return await MakeTransaction(userId, totalAmount, tokenRewardKey, internalReference, externalReference, quantity);
        }

        public async Task<MyTokenTransactionsPageResponse> GetMyTokenTransactionsNextPage(string userId, MyTokenTransactionsPageRequest request)
        {
            var accountId = userId;
            return await _tokenAccountService.GetMyTokenTransactionsNextPage(accountId, request, _userTokenServiceConfig.MyTokenTransactions_PageSize);
        }

        private async Task<TokenAccountTransactionStatus> MakeTransaction(
            string userId, decimal totalAmount, TokenRewardKey tokenRewardKey, Guid? internalReference, 
            string externalReference = null, int quantity = 1)
        {
            // TODO_PANOS_TEST
            switch (tokenRewardKey.AmountSign())
            {
                case TokenRewardKeyAmountSign.Positive:
                    if (totalAmount < 0.0M)
                        return TokenAccountTransactionStatus.WrongAmount;
                    break;
                case TokenRewardKeyAmountSign.Negative:
                    if (totalAmount > 0.0M)
                        return TokenAccountTransactionStatus.WrongAmount;
                    break;
            }

            var transactionStatus = await _tokenAccountService
                .MakeTransaction(userId, totalAmount, tokenRewardKey, internalReference, externalReference, quantity);
            if (transactionStatus == TokenAccountTransactionStatus.Success)
                _appCache.Remove(AppCacheKey.UserTokenBalance(userId));
            return transactionStatus;
        }
    }
}
