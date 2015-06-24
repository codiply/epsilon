using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Entities
{
    public class AdminAlert
    {
        public virtual Guid Id { get; set; }
        public virtual string Key { get; set; }
        public virtual DateTimeOffset SentOn { get; set; }
    }
}
