using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Entities
{
    public class TokenRewardScheme
    {
        public virtual int Id { get; set; }
        public virtual DateTimeOffset EffectiveFrom { get; set;}

        public virtual ICollection<TokenReward> Rewards { get; set; }
    }
}
