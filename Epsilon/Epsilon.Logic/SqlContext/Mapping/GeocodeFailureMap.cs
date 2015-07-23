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
    class GeocodeFailureMap : EntityTypeConfiguration<GeocodeFailure>
    {
        public const int ADDRESS_MAX_LENGTH = 675;

        public GeocodeFailureMap()
        {
            // Primary Key
            this.HasKey(x => x.Id);

            // Properties
            this.Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            this.Property(x => x.Address)
                .HasMaxLength(ADDRESS_MAX_LENGTH);
            this.Property(x => x.CreatedOn)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
            this.Property(a => a.CreatedByIpAddress)
                .HasMaxLength(AppConstant.IP_ADDRESS_MAX_LENGTH);

            // Relationships
            this.HasRequired(x => x.CreatedBy)
                .WithMany()
                .HasForeignKey(x => x.CreatedById)
                .WillCascadeOnDelete(false);
            this.HasRequired(x => x.Country)
                .WithMany()
                .HasForeignKey(x => x.CountryId)
                .WillCascadeOnDelete(false);

            // Indexes
            this.Property(x => x.CreatedOn)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("IX_GeocodeFailure_CreatedOn_CreatedById", 1)));
            this.Property(x => x.CreatedById)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("IX_GeocodeFailure_CreatedOn_CreatedById", 2)));

            this.Property(x => x.CreatedOn)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("IX_GeocodeFailure_CreatedOn_CreatedByIpAddress", 1)));
            this.Property(x => x.CreatedByIpAddress)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("IX_GeocodeFailure_CreatedOn_CreatedByIpAddress", 2)));
        }
    }
}
