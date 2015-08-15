using System;
using T4TS;

namespace Epsilon.Logic.JsonModels
{
    [TypeScriptInterface]
    public class MyTokenTransactionsPageRequest
    {
        public DateTimeOffset? madeBefore { get; set; }
    }
}
