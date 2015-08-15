using System;
using System.Collections.Generic;
using T4TS;

namespace Epsilon.Logic.JsonModels
{
    [TypeScriptInterface]
    public class MyTokenTransactionsPageResponse
    {
        public IList<MyTokenTransactionsItem> items { get; set; }
        public bool moreItemsExist { get; set; }
        public DateTimeOffset? earliestMadeOn { get; set; }
    }
}
