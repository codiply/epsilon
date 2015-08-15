using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Epsilon.Logic.Entities
{
    public class TokenAccount
    {
        [Key, ForeignKey("User")]
        public virtual string Id { get; set; }
        public virtual DateTimeOffset CreatedOn { get; set; }
        public virtual DateTimeOffset LastSnapshotOn { get; set; }

        [Timestamp]
        public virtual Byte[] Timestamp { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<TokenAccountSnapshot> Snapshots { get; set; }
        public virtual ICollection<TokenAccountTransaction> Transactions { get; set; }
    }
}
