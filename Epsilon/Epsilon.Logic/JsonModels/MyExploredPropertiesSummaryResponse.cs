using System.Collections.Generic;
using T4TS;

namespace Epsilon.Logic.JsonModels
{
    [TypeScriptInterface]
    public class MyExploredPropertiesSummaryResponse
    {
        public IList<ExploredPropertyInfo> exploredProperties { get; set; }

        public bool moreItemsExist { get; set; }
    }
}
