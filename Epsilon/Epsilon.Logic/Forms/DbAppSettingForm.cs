using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Forms
{
    public class DbAppSettingForm
    {
        [Display(Name = "Key")]
        public string Id { get; set; }
        public string Value { get; set; }
        [Display(Name = "Value Type")]
        public string ValueType { get; set; }
        public string Description { get; set; }
        [Display(Name ="Updated On")]
        public DateTimeOffset? UpdatedOn { get; set; }


        public static DbAppSettingForm FromEntity(AppSetting entity)
        {
            return new DbAppSettingForm
            {
                Id = entity.Id,
                Value = entity.Value,
                ValueType = entity.ValueType,
                Description = entity.Description,
                UpdatedOn = entity.UpdatedOn
            };
        }
    }
}
