﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using T4TS;

namespace Epsilon.Logic.JsonModels
{
    [TypeScriptInterface]
    public class AddressSearchRequest
    {
        public string countryId { get; set; }

        public string postcode { get; set; }
    }
}