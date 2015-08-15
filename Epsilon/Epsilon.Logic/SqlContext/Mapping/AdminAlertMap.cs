using Epsilon.Logic.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace Epsilon.Logic.SqlContext.Mapping
{
    public class AdminAlertMap : EntityTypeConfiguration<AdminAlert>
    {
        public const int KEY_MAX_LENGTH = 128;

        public AdminAlertMap()
        {
            // Primary Key
            this.HasKey(x => x.Id);

            // Properties
            this.Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            this.Property(x => x.Key)
                .HasMaxLength(KEY_MAX_LENGTH)
                .IsRequired();
            this.Property(x => x.SentOn)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            // Indexes
            this.Property(x => x.SentOn)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new IndexAttribute("IX_AdminAlert_SentOn_Key", 1)));
            this.Property(x => x.Key)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, 
                    new IndexAnnotation(new IndexAttribute("IX_AdminAlert_SentOn_Key", 2)));
        }
    }
}
