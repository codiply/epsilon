using System;
using System.ComponentModel.DataAnnotations;

namespace Epsilon.Logic.Entities
{
    public class TokenAccountSnapshot
    {
        public virtual long Id { get; set; }
        public virtual string AccountId { get; set; }
        public virtual Decimal Balance { get; set; }
        public virtual DateTimeOffset MadeOn { get; set; }
        public virtual bool IsFinalised { get; set; }

        [Timestamp]
        public virtual Byte[] Timestamp { get; set; }

        public virtual TokenAccount Account { get; set; }
    }
}
