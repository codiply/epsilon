using Epsilon.Logic.Constants;
using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
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
        public const int LOCALITY_MAX_LENGTH = 64;
        public const int REGION_MAX_LENGTH = 64;
        public const int POSTCODE_MAX_LENGTH = 16;
        public const byte LATITUDE_LONGITUDE_PRECISION = 18;
        public const byte LATITUDE_LONGITUDE_SCALE = 9;

        public AddressMap()
        {
            // Primary Key
            this.HasKey(x => x.Id);

            // Properties
            this.Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            this.Property(x => x.UniqueAddressCode)
                .HasMaxLength(UNIQUE_ADDRESS_CODE_MAX_LENGTH);
            this.Property(x => x.Line1)
                .HasMaxLength(LINE_MAX_LENGTH)
                .IsRequired();
            this.Property(x => x.Line2)
                .HasMaxLength(LINE_MAX_LENGTH);
            this.Property(x => x.Line3)
                .HasMaxLength(LINE_MAX_LENGTH);
            this.Property(x => x.Line4)
                .HasMaxLength(LINE_MAX_LENGTH);
            this.Property(x => x.Locality)
                .HasMaxLength(LOCALITY_MAX_LENGTH)
                .IsRequired();
            this.Property(x => x.Region)
                .HasMaxLength(REGION_MAX_LENGTH);
            this.Property(x => x.Postcode)
                .HasMaxLength(POSTCODE_MAX_LENGTH)
                .IsRequired();
            this.Property(x => x.CreatedOn)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
            this.Property(a => a.Latitude)
                .HasPrecision(LATITUDE_LONGITUDE_PRECISION, LATITUDE_LONGITUDE_SCALE);
            this.Property(a => a.Longitude)
                .HasPrecision(LATITUDE_LONGITUDE_PRECISION, LATITUDE_LONGITUDE_SCALE);
            this.Property(a => a.CreatedByIpAddress)
                .HasMaxLength(AppConstant.IP_ADDRESS_MAX_LENGTH);

            // Relationships
            this.HasRequired(x => x.Country)
                .WithMany()
                .HasForeignKey(x => x.CountryId)
                .WillCascadeOnDelete(false);
            this.HasRequired(x => x.CreatedBy)
                .WithMany()
                .HasForeignKey(x => x.CreatedById)
                .WillCascadeOnDelete(false);

            // Indexes
            this.Property(x => x.Postcode)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute()));
        }
    }
}
