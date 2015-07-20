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
        public string Id { get; set; }
        public string EnglishName { get; set; }
        public string Symbol { get; set; }

        [Timestamp]
        public virtual Byte[] Timestamp { get; set; }
    }
}
