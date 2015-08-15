using Epsilon.Logic.Constants;
using Epsilon.Logic.Entities;
using System.Data.Entity.ModelConfiguration;

namespace Epsilon.Logic.SqlContext.Mapping
{
    public class TokenRewardMap : EntityTypeConfiguration<TokenReward>
    {
        public TokenRewardMap()
        {
            // Primary Key
            this.HasKey(x => new { x.SchemeId, x.TypeKey });

            // Properties
            this.Property(x => x.Value)
                .HasPrecision(AppConstant.TOKEN_AMOUNT_PRECISION, AppConstant.TOKEN_AMOUNT_SCALE);

            // Relationships
            this.HasRequired(x => x.Scheme)
                .WithMany(y => y.Rewards)
                .HasForeignKey(x => x.SchemeId);
            this.HasRequired(x => x.Type)
                .WithMany()
                .HasForeignKey(x => x.TypeKey);
        }
    }
}
