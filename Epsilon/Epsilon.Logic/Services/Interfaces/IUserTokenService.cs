﻿using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
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

        Task<decimal> GetBalance(string userId);

        Task<TokenAccountTransactionStatus> Credit(string userId, Decimal amount);

        Task<TokenAccountTransactionStatus> Debit(string userId, Decimal amount);
    }
}