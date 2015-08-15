using System;
using T4TS;

namespace Epsilon.Logic.JsonModels
{
    [TypeScriptInterface]
    public class ExploredPropertyInfo
    {
        public Guid accessUniqueId { get; set; }
        public string displayAddress { get; set; }
        public DateTimeOffset expiresOn { get; set; }

        public bool canViewInfo { get; set; }
    }
}
