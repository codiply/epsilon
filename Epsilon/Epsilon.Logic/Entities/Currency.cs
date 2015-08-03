using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Entities
{
    public class Currency
    {
        public virtual string Id { get; set; }
        public virtual string EnglishName { get; set; }
        public virtual string LocalName { get; set; }
        public virtual string Symbol { get; set; }

        [Timestamp]
        public virtual Byte[] Timestamp { get; set; }
    }
}
