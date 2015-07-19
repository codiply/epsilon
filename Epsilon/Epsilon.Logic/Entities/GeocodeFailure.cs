using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Entities
{
    public class GeocodeFailure
    {
        public virtual long Id { get; set; }
        public virtual string Address { get; set; }
        public virtual string Region { get; set; }
        public virtual string QueryType { get; set; }
        public virtual string FailureType { get; set; }
        public virtual DateTimeOffset MadeOn { get; set; }
        public virtual string MadeById { get; set; }
        public virtual string MadeByIpAddress { get; set; }

        public virtual User MadeBy { get; set; }
    }
}
