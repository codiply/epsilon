using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Entities
{
    public class CoinAccountTransactionType : BaseEntity
    {
        public virtual string Id { get; set; }
        public virtual string Description { get; set; }
    }
}
