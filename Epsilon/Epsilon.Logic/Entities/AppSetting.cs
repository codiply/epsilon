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
        public virtual string Id { get; set; }
        public virtual string Value { get; set; }
        public virtual string ValueType { get; set; }
        public virtual string Description { get; set; }
        public virtual string UpdatedById { get; set; }
        
        public virtual string UpdatedBy { get; set; }

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
