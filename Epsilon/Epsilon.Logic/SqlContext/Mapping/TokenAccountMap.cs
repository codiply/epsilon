using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
