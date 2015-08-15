using System;
using T4TS;

namespace Epsilon.Logic.JsonModels
{
    [TypeScriptInterface]
    public class TenantVerificationInfo
    {
        public Guid uniqueId { get; set; }

        public string addressArea { get; set; }

        public bool markedAddrressInvalid { get; set; }

        public bool stepVerificationSentOutDone { get; set; }
        public bool stepVerificationReceivedDone { get; set; }

        public bool canMarkAsSent { get; set; }
        public bool canViewInstructions { get; set; }
    }
}
