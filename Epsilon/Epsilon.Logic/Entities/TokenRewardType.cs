using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers;
using System.ComponentModel.DataAnnotations.Schema;

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
