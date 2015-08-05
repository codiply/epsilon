using Epsilon.Logic.Constants;
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
    public interface ITokenAccountService
    {
        Task CreateAccount(string accountId);

        Task<bool> SufficientFundsExistForTransaction(string accountId, Decimal amount);

        /// <summary>
        /// It makes a transaction for the specific account.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="amount">
        /// Positive (credits the account) or negative (debits the account). 
        /// This is the total amount for the whole quantity of the transaction.
        /// The amount will be simply added to the balance.
        /// Zero is also possible in order to record a transaction that does not change the balance.</param>
        /// <param name="tokenRewardKey">
        /// The token reward key. This is purely for recording it on the transaction and it is not used in any other way.
        /// This means it is not used to double check the amount has the right sign in agreement with the token reward type.
        /// </param>
        /// <param name="internalReference">
        /// A reference that might have different meanings depending on the transaction type.
        /// Usually it will be the Id of an Entity related to the specific transaction type.
        /// </param>
        /// <param name="externalReference">
        /// Freetext field for recording external reference information.
        /// </param>
        /// <param name="quantity">
        /// Positive number with default value 1. This is just for recording it on the transaction. The amount should be the total amount.
        /// </param>
        /// <returns></returns>
        Task<TokenAccountTransactionStatus> MakeTransaction(
            string accountId, 
            Decimal amount,
            TokenRewardKey tokenRewardKey,
            Guid? internalReference = null,
            string externalReference = null,
            int quantity = 1);

        Task<Decimal> GetBalance(string accountId);

        Task MakeSnapshot(string acountId);

        Task<MyTokenTransactionsPageResponse> GetMyTokenTransactionsNextPage(
            string accountId, MyTokenTransactionsPageRequest request, int pageSize);
    }
}
