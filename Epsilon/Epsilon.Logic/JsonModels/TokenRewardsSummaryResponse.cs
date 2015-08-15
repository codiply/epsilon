using System.Collections.Generic;
using T4TS;

namespace Epsilon.Logic.JsonModels
{
    [TypeScriptInterface]
    public class TokenRewardsSummaryResponse
    {
        public IList<TokenRewardTypeMetadata> typeMetadata { get; set; }

        public IList<TokenRewardTypeValue> earnTypeValues { get; set; }

        public IList<TokenRewardTypeValue> spendTypeValues { get; set; }
    }
}
