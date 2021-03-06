﻿using Epsilon.Logic.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Epsilon.Logic.SqlContext.Mapping
{
    public class PostcodeGeometryMap : EntityTypeConfiguration<PostcodeGeometry>
    {
        public PostcodeGeometryMap()
        {
            // Primary Key
            this.HasKey(x => new { x.CountryId, x.Postcode });

            // Properties
            this.Property(x => x.GeocodedOn)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
            this.Property(x => x.Postcode)
                .HasMaxLength(AddressMap.POSTCODE_MAX_LENGTH)
                .IsRequired();

            // Relationship
            this.HasRequired(x => x.Country)
                .WithMany()
                .HasForeignKey(x => x.CountryId)
                .WillCascadeOnDelete(false);
        }
    }
}
