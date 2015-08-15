using System;
using T4TS;

namespace Epsilon.Logic.JsonModels
{
    [TypeScriptInterface]
    public class AddressSearchResult
    {
        public Guid addressUniqueId { get; set; }

        public string fullAddress { get; set; }
    }
}
