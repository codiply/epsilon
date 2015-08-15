using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Epsilon.Logic.Entities
{
    public class AppSetting
    {
        public virtual string Id { get; set; }

        public virtual string Value { get; set; }

        public virtual string ValueType { get; set; }

        public virtual string Description { get; set; }

        public virtual string UpdatedById { get; set; }

        public virtual DateTimeOffset? UpdatedOn { get; set; }

        [Timestamp]
        public virtual Byte[] Timestamp { get; set; }

        public virtual User UpdatedBy { get; set; }

        public virtual ICollection<AppSettingLabel> Labels { get; set; }

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
