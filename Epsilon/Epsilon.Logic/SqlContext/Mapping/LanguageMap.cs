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
        public const int ID_MAX_LENGTH = 10;
        public const int ENGLISH_NAME_MAX_LENGTH = 64;
        public const int NATIVE_NAME_MAX_LENGTH = 64;

        public LanguageMap()
        {
            // Primary Key
            this.HasKey(x => x.Id);

            // Properties
            this.Property(x => x.Id)
                .HasMaxLength(ID_MAX_LENGTH);
            this.Property(x => x.EnglishName)
                .HasMaxLength(ENGLISH_NAME_MAX_LENGTH);
            this.Property(x => x.NativeName)
                .HasMaxLength(NATIVE_NAME_MAX_LENGTH);

            // Relationships
            this.HasOptional(x => x.UseLanguage)
                .WithMany()
                .HasForeignKey(x => x.UseLanguageId);
        }
    }
}
