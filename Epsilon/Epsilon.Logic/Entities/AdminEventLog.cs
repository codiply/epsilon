using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Entities
{
    public class AdminEventLog
    {
        public virtual long Id { get; set; }
        public virtual string Type { get; set; }
        public virtual string ExtraInfo { get; set; }
        public virtual DateTimeOffset RecordedOn { get; set; }
    }
}
