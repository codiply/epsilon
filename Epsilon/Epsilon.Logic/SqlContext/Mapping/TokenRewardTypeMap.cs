using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.SqlContext.Mapping
{
    public class TokenRewardTypeMap : EntityTypeConfiguration<TokenRewardType>
    {
        public const int KEY_MAX_LENGTH = 128;
        public const int DESCRIPTION_MAX_LENGTH = 256;

        public TokenRewardTypeMap()
        {
            // Primary Key
            this.HasKey(x => x.Key);

            // Properties
            this.Property(x => x.Key)
                .HasMaxLength(KEY_MAX_LENGTH);
            this.Property(x => x.Description)
                .HasMaxLength(DESCRIPTION_MAX_LENGTH);
        }
    }
}
