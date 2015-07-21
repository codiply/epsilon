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
    public class AdminEventLogMap : EntityTypeConfiguration<AdminEventLog>
    {
        public const int EXTRA_INFO_MAX_LENGTH = 256;
        public const int KEY_MAX_LENGTH = 128;

        public AdminEventLogMap()
        {
            // Primary Key
            this.HasKey(x => x.Id);

            // Properties
            this.Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            this.Property(x => x.Key)
                .HasMaxLength(KEY_MAX_LENGTH)
                .IsRequired();
            this.Property(x => x.ExtraInfo)
                .HasMaxLength(EXTRA_INFO_MAX_LENGTH)
                .IsOptional();
            this.Property(x => x.RecordedOn)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            // Indexes
            this.Property(x => x.Key)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("IX_AdminEventLog_Key_RecordedOn", 1)));
            this.Property(x => x.RecordedOn)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("IX_AdminEventLog_Key_RecordedOn", 2)));
        }
    }
}
