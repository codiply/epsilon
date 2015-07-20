using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Constants.Enums
{
    // NOTE 1: Values should be all UPPERCASE.
    // NOTE 2: These codes should match the Id's of all TokenAccountTransactionType's in the database (see reference data in database project).

    public enum TokenAccountTransactionTypeId
    {
        CREDIT,
        DEBIT
    }
}
