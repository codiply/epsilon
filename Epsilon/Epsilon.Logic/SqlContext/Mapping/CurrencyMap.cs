using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.SqlContext.Mapping
{
    public class CurrencyMap : EntityTypeConfiguration<Currency>
    {
        public const int ID_MAX_LENGTH = 3;
        public const int ENGLISH_NAME_MAX_LENGTH = 64;
        public const int LOCAL_NAME_MAX_LENGTH = 64;
        public const int SYMBOL_MAX_LENGTH = 4;

        public CurrencyMap()
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
            this.Property(x => x.Symbol)
                .HasMaxLength(SYMBOL_MAX_LENGTH);
        }
    }
}
