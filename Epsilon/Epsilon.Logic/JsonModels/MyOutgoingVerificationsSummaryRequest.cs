﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using T4TS;

namespace Epsilon.Logic.JsonModels
{
    [TypeScriptInterface]
    public class MyOutgoingVerificationsSummaryRequest
    {
        public bool limitItemsReturned { get; set; }
    }
}