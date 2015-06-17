using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Entities
{
    public class Country : BaseEntity
    {
        public virtual string Id { get; set; }
        public virtual string EnglishName { get; set; }
        public virtual string LocalizedName { get; set; }
        public virtual string CurrencyId { get; set; }
        public virtual bool IsAvailable { get; set; }

        public virtual Currency Currency { get; set; }
    }
}
