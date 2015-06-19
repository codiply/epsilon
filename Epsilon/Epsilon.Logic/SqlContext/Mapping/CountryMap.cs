using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.SqlContext.Mapping
{
    public class CountryMap : EntityTypeConfiguration<Country>
    {
        public const int ID_MAX_LENGTH = 2;
        public const int ENGLISH_NAME_MAX_LENGTH = 64;
        public const int LOCALIZED_NAME_MAX_LENGTH = 64;

        public CountryMap()
        {
            // Primary Key
            this.HasKey(x => x.Id);

            // Properties
            this.Property(x => x.Id)
                .IsFixedLength()
                .HasMaxLength(ID_MAX_LENGTH);
            this.Property(x => x.EnglishName)
                .HasMaxLength(ENGLISH_NAME_MAX_LENGTH);
            this.Property(x => x.LocalizedName)
                .HasMaxLength(LOCALIZED_NAME_MAX_LENGTH);

            // Relationships
            this.HasRequired(x => x.Currency)
                .WithMany()
                .HasForeignKey(x => x.CurrencyId);
        }
    }
}
