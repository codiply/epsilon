using System;
using T4TS;

namespace Epsilon.Logic.JsonModels
{
    [TypeScriptInterface]
    public class TenancyDetailsSubmissionInfo
    {
        public Guid uniqueId { get; set; }
        public string displayAddress { get; set; }

        public bool stepVerificationCodeSentOutDone { get; set; }
        public bool stepVerificationCodeEnteredDone { get; set; }
        public bool stepTenancyDetailsSubmittedDone { get; set; }

        public bool canEnterVerificationCode { get; set; }
        public bool canSubmitTenancyDetails { get; set; }
    }
}
