using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Constants.Enums
{
    // !!! IMPORTANT !!!
    // All keys should either start with Earn or Spend.

    // NOTE: These keys should match the keys of TokenRewardType's in the database (see reference data in database project).

    public enum TokenRewardKey
    {
        // Earn
        EarnPerTenancyDetailsSubmission,
        EarnPerVerificationCodeEntered,
        EarnPerVerificationMailSent,
        
        // Spend
        SpendPerPropertyDetailsAccess
    }
}
