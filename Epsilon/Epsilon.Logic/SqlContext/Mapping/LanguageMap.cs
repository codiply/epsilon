using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.SqlContext.Mapping
{
    public class LanguageMap : EntityTypeConfiguration<Language>
    {
        public const int ID_MAX_LENGTH = 8;
        public const int ENGLISH_NAME_MAX_LENGTH = 64;
        public const int LOCAL_NAME_MAX_LENGTH = 64;
        public const int CULTURE_CODE_MAX_LENGTH = 10;

        public LanguageMap()
        {
            // Primary Key
            this.HasKey(x => x.Id);

            // Properties
            this.Property(x => x.Id)
                .HasMaxLength(ID_MAX_LENGTH);
            this.Property(x => x.EnglishName)
                .HasMaxLength(ENGLISH_NAME_MAX_LENGTH);
            this.Property(x => x.LocalName)
                .HasMaxLength(LOCAL_NAME_MAX_LENGTH);
            this.Property(x => x.CultureCode)
                .HasMaxLength(CULTURE_CODE_MAX_LENGTH);
        }
    }
}
