using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Entities
{
    public class AppSettingLabel
    {
        public string AppSettingId { get; set; }
        public string Label { get; set; }

        public virtual AppSetting AppSetting { get; set; }
    }
}
