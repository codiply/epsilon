using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using T4TS;

namespace Epsilon.Logic.JsonModels
{
    [TypeScriptInterface]
    public class AddressSearchResponse
    {
        public IList<AddressSearchResult> Results { get; set; }

        public int ResultsLimit { get; set; }
        
        public bool IsResultsLimitReached { get; set; }
    }
}
