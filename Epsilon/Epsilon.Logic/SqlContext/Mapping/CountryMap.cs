﻿using Epsilon.Logic.Entities;
using System.Data.Entity.ModelConfiguration;

namespace Epsilon.Logic.SqlContext.Mapping
{
    public class CountryMap : EntityTypeConfiguration<Country>
    {
        public const int ID_MAX_LENGTH = 2;
        public const int ENGLISH_NAME_MAX_LENGTH = 64;
        public const int LOCAL_NAME_MAX_LENGTH = 64;

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
            this.Property(x => x.LocalName)
                .HasMaxLength(LOCAL_NAME_MAX_LENGTH);

            // Relationships
            this.HasRequired(x => x.Currency)
                .WithMany()
                .HasForeignKey(x => x.CurrencyId)
                .WillCascadeOnDelete(false);
            this.HasRequired(x => x.MainLanguage)
                .WithMany()
                .HasForeignKey(x => x.MainLanguageId)
                .WillCascadeOnDelete(false);
        }
    }
}
