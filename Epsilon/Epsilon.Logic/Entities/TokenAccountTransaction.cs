using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Entities
{
    public class TokenAccountTransaction : BaseEntity
    {
        public virtual long Id { get; set; }
        public virtual string AccountId { get; set; }
        public virtual string TypeId { get; set; }
        public virtual Decimal Amount { get; set; }
        public virtual DateTimeOffset MadeOn { get; set; }
        public virtual string Reference { get; set; }

        public virtual TokenAccount Account { get; set; }
        public virtual TokenAccountTransactionType Type { get; set; }
    }
}
