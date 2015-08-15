using Epsilon.Logic.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace Epsilon.Logic.SqlContext.Mapping
{
    public class AdminEventLogMap : EntityTypeConfiguration<AdminEventLog>
    {
        // NOTE: This needs to fit at least a full address (plus json field key etc)!
        public const int EXTRA_INFO_MAX_LENGTH = 1024;
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
            this.Property(x => x.RecordedOn)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("IX_AdminEventLog_RecordedOn_Key", 1)));
            this.Property(x => x.Key)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("IX_AdminEventLog_RecordedOn_Key", 2)));
        }
    }
}
