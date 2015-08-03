using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using Epsilon.Logic.JsonModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    public interface IUserTokenService
    {
        Task CreateAccount(string userId);

        Task<TokenBalanceResponse> GetBalance(string userId);

        Task<TokenAccountTransactionStatus> MakeTransaction(
            string userId, decimal amount, TokenRewardKey tokenRewardKey, Guid? internalReference = null,
            string externalReference = null, int quantity = 1);
    }
}
