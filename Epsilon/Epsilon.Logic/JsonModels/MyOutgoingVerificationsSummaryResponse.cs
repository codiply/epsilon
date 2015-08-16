using System.Collections.Generic;
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
