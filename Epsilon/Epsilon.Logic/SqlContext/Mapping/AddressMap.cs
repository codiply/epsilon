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
        public const int DISTINCT_ADDRESS_CODE_MAX_LENGTH = 32;
        public const int LINE_MAX_LENGTH = 128;
        public const int LOCALITY_MAX_LENGTH = 64;
        public const int REGION_MAX_LENGTH = 64;
        public const int POSTCODE_MAX_LENGTH = 16;

        public AddressMap()
        {
            // Primary Key
            this.HasKey(x => x.Id);

            // Properties
            this.Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            this.Property(x => x.UniqueId)
                .IsRequired();
            this.Property(x => x.DistinctAddressCode)
                .HasMaxLength(DISTINCT_ADDRESS_CODE_MAX_LENGTH);
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
            this.HasRequired(x => x.PostcodeGeometry)
                .WithMany(x => x.Addresses)
                .HasForeignKey(x => new { x.CountryId, x.Postcode });

            // Indexes
            this.Property(x => x.UniqueId)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("IX_Address_UniqueId") { IsUnique = true }));

            this.Property(x => x.Postcode)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, 
                    new IndexAnnotation(new IndexAttribute("IX_Address_Postcode")));

            this.Property(x => x.CreatedOn)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new[]
                {
                    new IndexAttribute("IX_Address_CreatedOn"),
                    new IndexAttribute("IX_Address_CreatedOn_CreatedByIpAddress", 1),
                    new IndexAttribute("IX_Address_CreatedOn_CreatedById", 1)
                }));

            this.Property(x => x.CreatedByIpAddress)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("IX_Address_CreatedOn_CreatedByIpAddress", 2)));

            this.Property(x => x.CreatedById)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("IX_Address_CreatedOn_CreatedById", 2)));

        }
    }
}
