using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.SqlContext.Mapping
{
    public class TokenRewardSchemeMap : EntityTypeConfiguration<TokenRewardScheme>
    {
        public TokenRewardSchemeMap()
        {
            // Primary Key
            this.HasKey(x => x.Id);
        }
    }
}
