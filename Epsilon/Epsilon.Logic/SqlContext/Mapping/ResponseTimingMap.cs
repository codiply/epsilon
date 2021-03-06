﻿using Epsilon.Logic.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Epsilon.Logic.SqlContext.Mapping
{
    public class ResponseTimingMap : EntityTypeConfiguration<ResponseTiming>
    {
        public const int ACTION_MAX_LENGTH = 128;
        public const int CONTROLLER_MAX_LENGTH = 128;
        public const int HTTP_VERB_MAX_LENGTH = 8;

        public ResponseTimingMap()
        {
            // Primary Key
            this.HasKey(x => x.Id);

            // Properties
            this.Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            this.Property(x => x.MeasuredOn)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
            this.Property(x => x.ActionName)
                .HasMaxLength(ACTION_MAX_LENGTH);
            this.Property(x => x.ControllerName)
                .IsRequired()
                .HasMaxLength(CONTROLLER_MAX_LENGTH);
            this.Property(x => x.HttpVerb)
                .IsRequired()
                .HasMaxLength(HTTP_VERB_MAX_LENGTH);

            // Relationships
            this.HasRequired(x => x.Language)
                .WithMany()
                .HasForeignKey(x => x.LanguageId);
        }
    }
}
