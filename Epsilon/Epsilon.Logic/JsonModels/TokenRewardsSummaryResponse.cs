using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
