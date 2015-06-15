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
    public class AddressMap : EntityTypeConfiguration<Address>
    {
        public const int UNIQUE_ADDRESS_CODE_MAX_LENGTH = 32;
        public const int LINE_MAX_LENGTH = 256;
        public const int CITY_MAX_LENGTH = 64;
        public const int COUNTY_MAX_LENGTH = 64;
        public const int POSTCODE_MAX_LENGTH = 16;

        public AddressMap()
        {
            // Primary Key
            this.HasKey(x => x.Id);

            // Properties
            this.Property(x => x.UniqueAddressCode)
                .HasMaxLength(UNIQUE_ADDRESS_CODE_MAX_LENGTH);
            this.Property(x => x.Line1)
                .HasMaxLength(LINE_MAX_LENGTH)
                .IsRequired();
            this.Property(x => x.Line2)
                .HasMaxLength(LINE_MAX_LENGTH);
            this.Property(x => x.Line3)
                .HasMaxLength(LINE_MAX_LENGTH);
            this.Property(x => x.CityTown)
                .HasMaxLength(CITY_MAX_LENGTH)
                .IsRequired();
            this.Property(x => x.CountyStateProvince)
                .HasMaxLength(COUNTY_MAX_LENGTH);
            this.Property(x => x.PostcodeOrZip)
                .HasMaxLength(POSTCODE_MAX_LENGTH)
                .IsRequired();

            // Relationships
            this.HasRequired(x => x.Country)
                .WithMany()
                .HasForeignKey(x => x.CountryId);
        }
    }
}
