using System.Collections.Generic;
using T4TS;

namespace Epsilon.Logic.JsonModels
{
    [TypeScriptInterface]
    public class MySubmissionsSummaryResponse
    {
        public IList<TenancyDetailsSubmissionInfo> tenancyDetailsSubmissions { get; set; }
        public bool moreItemsExist { get; set; }
    }
}
