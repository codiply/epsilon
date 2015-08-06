using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.JsonModels
{
    public class ExploredPropertyInfo
    {
        public Guid uniqueId { get; set; }
        public string displayAddress { get; set; }
        public DateTimeOffset expiresOn { get; set; }
    }
}
