using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Entities
{
    public class UserPreference : BaseEntity
    {
        public virtual Guid Id { get; set; }
        public virtual string LanguageId { get; set; }

        public virtual User User { get; set; }
        public virtual Language Language { get; set; } 
    }
}
