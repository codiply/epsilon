using Epsilon.Logic.Constants;
using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    public interface ICoinAccountService
    {
        Task<CoinAccountTransaction> MakeTransaction(
            string accountId, 
            Decimal amount, 
            CoinAccountTransactionTypeId transactionTypeId, 
            string reference);

        Task<Decimal> GetBalance(string accountId);

        Task MakeSnapshot(string acountId);
    }
}
