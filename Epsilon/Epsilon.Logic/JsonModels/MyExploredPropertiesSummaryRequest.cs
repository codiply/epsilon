using T4TS;

namespace Epsilon.Logic.JsonModels
{
    [TypeScriptInterface]
    public class MyExploredPropertiesSummaryRequest
    {
        public bool limitItemsReturned { get; set; }
        public bool allowCaching { get; set; }
    }
}
