using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
