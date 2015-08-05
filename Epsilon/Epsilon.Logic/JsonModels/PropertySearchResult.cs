using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using T4TS;

namespace Epsilon.Logic.JsonModels
{
    [TypeScriptInterface]
    public class PropertySearchResult
    {
        public Guid addressUniqueId { get; set; }

        public string fullAddress { get; set; }

        public int numberOfCompletedSubmissions { get; set; }

        public DateTimeOffset? lastSubmissionOn { get; set; }
    }
}
