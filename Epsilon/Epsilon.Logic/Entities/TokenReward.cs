using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Entities
{
    public class TokenReward
    {
        public virtual int SchemeId { get; set; }
        public virtual string Key { get; set; }
        public virtual decimal Value { get; set; }

        public virtual TokenRewardScheme Scheme { get; set; }
    }
}
