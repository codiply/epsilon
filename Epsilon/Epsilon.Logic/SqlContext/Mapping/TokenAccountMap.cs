using Epsilon.Logic.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Epsilon.Logic.SqlContext.Mapping
{
    public class TokenAccountMap : EntityTypeConfiguration<TokenAccount>
    {
        public TokenAccountMap()
        {
            // Properties
            this.Property(x => x.CreatedOn)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
        }
    }
}
