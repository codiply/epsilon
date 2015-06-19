using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Entities
{
    public class Language
    {
        public virtual string Id { get; set; }
        public virtual string EnglishName { get; set; }
        public virtual string NativeName { get; set; }
        public virtual string CultureCode { get; set; }
        public virtual bool IsAvailable { get; set; }
    }
}