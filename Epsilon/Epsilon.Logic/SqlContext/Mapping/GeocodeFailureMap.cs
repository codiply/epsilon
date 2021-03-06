﻿using Epsilon.Logic.Constants;
using Epsilon.Logic.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace Epsilon.Logic.SqlContext.Mapping
{
    class GeocodeFailureMap : EntityTypeConfiguration<GeocodeFailure>
    {
        public const int ADDRESS_MAX_LENGTH = 675;
        public const int FAILURE_TYPE_MAX_LENGTH = 128;
        public const int QUERY_TYPE_MAX_LENGTH = 16;

        public GeocodeFailureMap()
        {
            // Primary Key
            this.HasKey(x => x.Id);

            // Properties
            this.Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            this.Property(x => x.Address)
                .HasMaxLength(ADDRESS_MAX_LENGTH);
            this.Property(x => x.FailureType)
                .HasMaxLength(FAILURE_TYPE_MAX_LENGTH);
            this.Property(x => x.QueryType)
                .HasMaxLength(QUERY_TYPE_MAX_LENGTH);
            this.Property(x => x.CreatedOn)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
            this.Property(a => a.CreatedByIpAddress)
                .HasMaxLength(AppConstant.IP_ADDRESS_MAX_LENGTH);

            // Relationships
            this.HasRequired(x => x.CreatedBy)
                .WithMany()
                .HasForeignKey(x => x.CreatedById)
                .WillCascadeOnDelete(true);
            this.HasRequired(x => x.Country)
                .WithMany()
                .HasForeignKey(x => x.CountryId)
                .WillCascadeOnDelete(true);

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
