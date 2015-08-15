using T4TS;

namespace Epsilon.Logic.JsonModels
{
    [TypeScriptInterface]
    public class MySubmissionsSummaryRequest
    {
        public bool limitItemsReturned { get; set; }
        public bool allowCaching { get; set; }
    }
}
