using System.Collections.Generic;
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
