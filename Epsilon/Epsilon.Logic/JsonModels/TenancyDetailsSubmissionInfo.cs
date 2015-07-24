using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public bool stepDetailsSubmittedDone { get; set; }
        public bool stepMoveOutDateEnteredDone { get; set; }

        public bool canEnterVerificationCode { get; set; }
        public bool canSubmitDetails { get; set; }
        public bool canEnterMoveOutDate { get; set; }
    }
}
