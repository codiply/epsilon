using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Entities
{
    public class BaseEntity
    {
        [Timestamp]
        public virtual Byte[] Timestamp { get; set; }
    }
}
