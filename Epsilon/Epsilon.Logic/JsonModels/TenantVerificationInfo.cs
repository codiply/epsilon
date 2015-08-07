﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using T4TS;

namespace Epsilon.Logic.JsonModels
{
    [TypeScriptInterface]
    public class TenantVerificationInfo
    {
        public Guid uniqueId { get; set; }

        public string addressArea { get; set; }

        public bool stepVerificationSentOutDone { get; set; }
        public bool stepVerificationReceivedDone { get; set; }

        public bool canMarkAsSent { get; set; }
        public bool canViewInstructions { get; set; }
    }
}
