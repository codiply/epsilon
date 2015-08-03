using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Entities
{
    public class TokenRewardType
    {
        public virtual string Key { get; set; }
        public virtual string Description { get; set; }

        [NotMapped]
        public virtual TokenRewardKey? KeyAsEnum
        {
            get
            {
                return EnumsHelper.TokenRewardKey.Parse(Key);
            }
        }
    }
}
