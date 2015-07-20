using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.SqlContext.Mapping
{
    public class TokenRewardMap : EntityTypeConfiguration<TokenReward>
    {
        public const int KEY_MAX_LENGTH = 128;

        public TokenRewardMap()
        {
            // Primary Key
            this.HasKey(x => new { x.SchemeId, x.Key });

            // Properties
            this.Property(x => x.Key)
                .HasMaxLength(KEY_MAX_LENGTH)
                .IsRequired();

            // Relationships
            this.HasRequired(x => x.Scheme)
                .WithMany(y => y.Rewards)
                .HasForeignKey(x => x.SchemeId);
        }
    }
}
