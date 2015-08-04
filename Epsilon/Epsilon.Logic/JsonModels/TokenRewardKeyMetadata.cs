using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using T4TS;

namespace Epsilon.Logic.JsonModels
{
    [TypeScriptInterface]
    public class TokenRewardKeyMetadata
    {
        public string key { get; set; }
        public string displayName { get; set; }
        public string description { get; set; }
    }
}
