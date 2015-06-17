using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Entities
{
    public class Language
    {
        public string Id { get; set; }
        public string EnglishName { get; set; }
        public string LocalizedName { get; set; }
        public bool IsAvailable { get; set; }
    }
}
