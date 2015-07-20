using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Entities
{
    public class TokenAccountSnapshot : BaseEntity
    {
        public virtual long Id { get; set; }
        public virtual string AccountId { get; set; }
        public virtual Decimal Balance { get; set; }
        public virtual DateTimeOffset MadeOn { get; set; }
        public virtual bool IsFinalised { get; set; }

        public virtual TokenAccount Account { get; set; }
    }
}
