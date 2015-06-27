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
    public class UserCoinService : IUserCoinService
    {
        private readonly IEpsilonContext _dbContext;
        private readonly ICoinAccountService _coinAccountService;

        public UserCoinService(
            IEpsilonContext dbContext,
            ICoinAccountService coinAccountService)
        {
            _dbContext = dbContext;
            _coinAccountService = coinAccountService;
        }

        public async Task CreateAccount(string userId)
        {
            await _coinAccountService.CreateAccount(userId);
        }

        public async Task<decimal> GetBalance(string userId)
        {
            return await _coinAccountService.GetBalance(userId);
        }

        public async Task<CoinAccountTransactionStatus> Credit(string userId, Decimal amount)
        {
            if (amount < 0)
                return CoinAccountTransactionStatus.WrongAmount;
            return await _coinAccountService.MakeTransaction(userId, amount, CoinAccountTransactionTypeId.CREDIT, "");
        }

        public async Task<CoinAccountTransactionStatus> Debit(string userId, Decimal amount)
        {
            if (amount < 0)
                return CoinAccountTransactionStatus.WrongAmount;
            return await _coinAccountService.MakeTransaction(userId, -amount, CoinAccountTransactionTypeId.DEBIT, "");
        }
    }
}
