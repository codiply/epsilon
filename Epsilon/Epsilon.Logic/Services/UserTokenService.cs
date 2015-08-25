using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.JsonModels;
using Epsilon.Logic.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services
{
    public class UserTokenService : IUserTokenService
    {
        private readonly IAppCache _appCache;
        private readonly IUserTokenServiceConfig _userTokenServiceConfig;
        private readonly ITokenAccountService _tokenAccountService;
        private readonly ITokenRewardService _tokenRewardService;

        public UserTokenService(
            IAppCache appCache,
            IUserTokenServiceConfig userTokenServiceConfig,
            ITokenAccountService tokenAccountService,
            ITokenRewardService tokenRewardService)
        {
            _appCache = appCache;
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

        public async Task<bool> SufficientFundsExistForTransaction(string userId, TokenRewardKey tokenRewardKey, int quantity = 1)
        {
            if (quantity < 1)
                throw new ArgumentException(string.Format("Quantity has value '{0}' which is less than 1.", quantity));

            var totalAmount = CalculateTotalAmount(tokenRewardKey, quantity);

            var accountId = userId;

            return await _tokenAccountService.SufficientFundsExistForTransaction(accountId, totalAmount);
        }

        public async Task<TokenAccountTransactionStatus> MakeTransaction(
            string userId, TokenRewardKey tokenRewardKey, Guid? internalReference,
            string externalReference = null, int quantity = 1)
        {
            if (quantity < 1)
                return TokenAccountTransactionStatus.WrongQuantity;

            var totalAmount = CalculateTotalAmount(tokenRewardKey, quantity);

            return await MakeTransaction(userId, totalAmount, tokenRewardKey, internalReference, externalReference, quantity);
        }

        public async Task<MyTokenTransactionsPageResponse> GetMyTokenTransactionsNextPage(string userId, MyTokenTransactionsPageRequest request)
        {
            var accountId = userId;
            return await _tokenAccountService.GetMyTokenTransactionsNextPage(accountId, request, _userTokenServiceConfig.MyTokenTransactions_PageSize);
        }

        private decimal CalculateTotalAmount(TokenRewardKey tokenRewardKey, int quantity)
        {
            var reward = _tokenRewardService.GetCurrentReward(tokenRewardKey);
            var totalAmount = quantity * reward.Value;

            return totalAmount;
        }

        private async Task<TokenAccountTransactionStatus> MakeTransaction(
            string userId, decimal totalAmount, TokenRewardKey tokenRewardKey, Guid? internalReference, 
            string externalReference = null, int quantity = 1)
        {
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
