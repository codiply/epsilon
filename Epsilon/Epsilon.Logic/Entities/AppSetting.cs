using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Entities
{
    public class AppSetting : BaseEntity
    {
        public virtual string Id { get; set; }
        public virtual string Value { get; set; }
    }
}
