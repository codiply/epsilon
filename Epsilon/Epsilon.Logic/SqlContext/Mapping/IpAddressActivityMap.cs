﻿using Epsilon.Logic.Constants;
using Epsilon.Logic.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace Epsilon.Logic.SqlContext.Mapping
{
    public class IpAddressActivityMap : EntityTypeConfiguration<IpAddressActivity>
    {
        public const int TYPE_MAX_LENGTH = 32;

        public IpAddressActivityMap()
        {
            // Primary Key
            this.HasKey(x => x.Id);

            // Properties
            this.Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            this.Property(x => x.ActivityType)
                .HasMaxLength(TYPE_MAX_LENGTH);
            this.Property(x => x.IpAddress)
                .HasMaxLength(AppConstant.IP_ADDRESS_MAX_LENGTH);
            this.Property(x => x.RecordedOn)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            // Indexes
            this.Property(x => x.RecordedOn)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, 
                    new IndexAnnotation(new IndexAttribute("IX_IpAddressActivity_RecordedOn_ActivityType_IpAddress", 1)));
            this.Property(x => x.ActivityType)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, 
                    new IndexAnnotation(new IndexAttribute("IX_IpAddressActivity_RecordedOn_ActivityType_IpAddress", 2)));
            this.Property(x => x.IpAddress)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, 
                    new IndexAnnotation(new IndexAttribute("IX_IpAddressActivity_RecordedOn_ActivityType_IpAddress", 3)));
        }
    }
}
