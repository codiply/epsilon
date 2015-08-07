﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using T4TS;

namespace Epsilon.Logic.JsonModels
{
    [TypeScriptInterface]
    public class PropertySearchResponse
    {
        public IList<PropertySearchResult> results { get; set; }

        public int resultsLimit { get; set; }
        
        public bool isResultsLimitExceeded { get; set; }
    }
}