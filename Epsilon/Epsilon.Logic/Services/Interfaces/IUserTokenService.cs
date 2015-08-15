using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.JsonModels;
using System;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    public interface IUserTokenService
    {
        Task CreateAccount(string userId);

        Task<TokenBalanceResponse> GetBalance(string userId);

        Task<bool> SufficientFundsExistForTransaction(string userId, TokenRewardKey tokenRewardKey, int quantity = 1);

        Task<TokenAccountTransactionStatus> MakeTransaction(
            string userId, TokenRewardKey tokenRewardKey, Guid? internalReference = null,
            string externalReference = null, int quantity = 1);

        Task<MyTokenTransactionsPageResponse> GetMyTokenTransactionsNextPage(string userId, MyTokenTransactionsPageRequest request);
    }
}
