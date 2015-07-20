using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    public interface ITokenAccountService
    {
        Task CreateAccount(string accountId);

        /// <summary>
        /// It makes a transaction for the specific account.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="amount">
        /// Positive (credits the account) or negative (debits the account). The amount will be simply added to the balance.
        /// Zero is also possible in order to record a transaction that does not change the balance.</param>
        /// <param name="transactionTypeId">
        /// The transaction type Id. This is purely for recording it on the transaction and it not used in any other way.
        /// This means it is not used to double check the amount has the right sign in agreement with the transaction type.
        /// </param>
        /// <param name="reference">
        /// A reference that might have different meanings depending on the transaction type.
        /// Usually it will be the Id of an Entity related to the specific transaction type.
        /// </param>
        /// <returns></returns>
        Task<TokenAccountTransactionStatus> MakeTransaction(
            string accountId, 
            Decimal amount,
            TokenAccountTransactionTypeId transactionTypeId, 
            string reference);

        Task<Decimal> GetBalance(string accountId);

        Task MakeSnapshot(string acountId);
    }
}
