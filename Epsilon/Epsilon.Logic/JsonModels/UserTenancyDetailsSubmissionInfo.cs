﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using T4TS;

namespace Epsilon.Logic.JsonModels
{
    [TypeScriptInterface]
    public class UserTenancyDetailsSubmissionInfo
    {
        public IList<TenancyDetailsSubmissionInfo> submissions { get; set; }
    }
}
