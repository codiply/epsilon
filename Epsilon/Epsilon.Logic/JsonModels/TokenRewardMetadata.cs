using System.Collections.Generic;
using T4TS;

namespace Epsilon.Logic.JsonModels
{
    [TypeScriptInterface]
    public class TokenRewardMetadata
    {
        public IList<TokenRewardTypeMetadata> typeMetadata { get; set; }
    }
}
