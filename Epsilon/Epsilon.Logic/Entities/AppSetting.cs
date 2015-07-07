using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Entities
{
    public class AppSetting : BaseEntity
    {
        [Display(Name = "Key")]
        public virtual string Id { get; set; }

        public virtual string Value { get; set; }

        [Display(Name = "Value Type")]
        public virtual string ValueType { get; set; }

        public virtual string Description { get; set; }

        public virtual string UpdatedById { get; set; }

        [Display(Name = "Updated On")]
        public virtual DateTimeOffset? UpdatedOn { get; set; }
        
        public virtual User UpdatedBy { get; set; }

        [NotMapped]
        public virtual DbAppSettingValueType? ValueTypeAsEnum
        {
            get
            {
                return EnumsHelper.DbAppSettingValueType.Parse(ValueType);
            }
        }
    }
}
