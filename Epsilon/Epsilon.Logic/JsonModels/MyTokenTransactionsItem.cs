using System;
using T4TS;

namespace Epsilon.Logic.JsonModels
{
    [TypeScriptInterface]
    public class MyTokenTransactionsItem
    {
        public Guid uniqueId { get; set; }
        public string rewardTypeKey { get; set; }
        public Decimal amount { get; set; }
        public int quantity { get; set; }
        public DateTimeOffset madeOn { get; set; }
    }
}
