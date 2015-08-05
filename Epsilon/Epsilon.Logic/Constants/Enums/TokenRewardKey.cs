using Epsilon.Logic.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Constants.Enums
{
    // !!! IMPORTANT !!!
    // All keys should either start with Earn or Spend.

    // NOTE 1: These keys should match the keys of TokenRewardType's in the database (see reference data in database project).
    // NOTE 2: Each new key needs to have a DisplayName and Description defined in TokenRewardKeyResources.

    // TODO_PANOS_TEST: unit test for Note 2 above.

    public enum TokenRewardKey
    {
        // Earn
        EarnPerTenancyDetailsSubmission,
        EarnPerVerificationCodeEntered,
        EarnPerVerificationMailSent,
        
        // Spend
        SpendPerPropertyInfoAccess
    }

    public enum TokenRewardKeyType
    {
        Earn,
        Spend
    }

    public enum TokenRewardKeyAmountSign
    {
        Positive,
        Negative
    }

    public static class TokenRewardKeyExtensions
    {
        public static TokenRewardKeyAmountSign AmountSign(this TokenRewardKey key)
        {
            switch (key.EarnOrSpend())
            {
                case TokenRewardKeyType.Earn:
                    return TokenRewardKeyAmountSign.Positive;
                case TokenRewardKeyType.Spend:
                    return TokenRewardKeyAmountSign.Negative;
                default:
                    throw new Exception(string.Format("Unexpected TokenRewardKey '{0} that doesn't start with either '{1}' or '{2}'.",
                    key, AppConstant.TOKEN_REWARD_KEY_EARN, AppConstant.TOKEN_REWARD_KEY_SPEND));
            }
        }

        public static TokenRewardKeyType EarnOrSpend(this TokenRewardKey key)
        {
            var name = EnumsHelper.TokenRewardKey.ToString(key);

            if (name.StartsWith(AppConstant.TOKEN_REWARD_KEY_EARN))
                return TokenRewardKeyType.Earn;
            if (name.StartsWith(AppConstant.TOKEN_REWARD_KEY_SPEND))
                return TokenRewardKeyType.Spend;

            throw new Exception(string.Format("Unexpected TokenRewardKey '{0} that doesn't start with either '{1}' or '{2}'.",
                name, AppConstant.TOKEN_REWARD_KEY_EARN, AppConstant.TOKEN_REWARD_KEY_SPEND));
        }
    }
}
