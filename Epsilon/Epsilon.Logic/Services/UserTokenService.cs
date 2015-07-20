using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Infrastructure.Interfaces;
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
        private readonly IEpsilonContext _dbContext;
        private readonly ITokenAccountService _tokenAccountService;

        public UserTokenService(
            IEpsilonContext dbContext,
            ITokenAccountService tokenAccountService)
        {
            _dbContext = dbContext;
            _tokenAccountService = tokenAccountService;
        }

        public async Task CreateAccount(string userId)
        {
            await _tokenAccountService.CreateAccount(userId);
        }

        public async Task<decimal> GetBalance(string userId)
        {
            return await _tokenAccountService.GetBalance(userId);
        }

        public async Task<TokenAccountTransactionStatus> Credit(string userId, Decimal amount)
        {
            if (amount < 0)
                return TokenAccountTransactionStatus.WrongAmount;
            return await _tokenAccountService.MakeTransaction(userId, amount, TokenAccountTransactionTypeId.CREDIT, "");
        }

        public async Task<TokenAccountTransactionStatus> Debit(string userId, Decimal amount)
        {
            if (amount < 0)
                return TokenAccountTransactionStatus.WrongAmount;
            return await _tokenAccountService.MakeTransaction(userId, -amount, TokenAccountTransactionTypeId.DEBIT, "");
        }
    }
}
