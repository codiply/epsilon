using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Entities
{
    public class Currency : BaseEntity
    {
        public string Id { get; set; }
        public string EnglishName { get; set; }
        public string Symbol { get; set; }
    }
}
