using System;
using System.Collections.Generic;

namespace Epsilon.Logic.Entities
{
    public class TokenRewardScheme
    {
        public virtual int Id { get; set; }
        public virtual DateTimeOffset EffectiveFrom { get; set;}

        public virtual ICollection<TokenReward> Rewards { get; set; }
    }
}
