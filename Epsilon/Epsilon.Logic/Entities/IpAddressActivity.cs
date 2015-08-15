using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Epsilon.Logic.Entities
{
    public class IpAddressActivity
    {
        public virtual long Id { get; set; }
        [ForeignKey("User")]
        public virtual string UserId { get; set; }
        public virtual string ActivityType { get; set; }
        public virtual string IpAddress { get; set; }
        public virtual DateTimeOffset RecordedOn { get; set; }

        public virtual User User { get; set; }

        [NotMapped]
        public virtual IpAddressActivityType? ActivityTypeAsEnum
        {
            get
            {
                return EnumsHelper.IpAddressActivityType.Parse(ActivityType);
            }
        }
    }
}
