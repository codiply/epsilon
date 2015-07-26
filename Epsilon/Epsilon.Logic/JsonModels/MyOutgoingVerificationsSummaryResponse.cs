using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using T4TS;

namespace Epsilon.Logic.JsonModels
{
    [TypeScriptInterface]
    public class MyOutgoingVerificationsSummaryResponse
    {
        public IList<TenantVerificationInfo> tenantVerifications { get; set; }

        public bool moreItemsExist { get; set; }
    }
}
