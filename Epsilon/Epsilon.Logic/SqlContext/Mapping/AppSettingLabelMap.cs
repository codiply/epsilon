using Epsilon.Logic.Entities;
using System.Data.Entity.ModelConfiguration;

namespace Epsilon.Logic.SqlContext.Mapping
{
    public class AppSettingLabelMap : EntityTypeConfiguration<AppSettingLabel>
    {
        public const int LABEL_MAX_LENGTH = 128;

        public AppSettingLabelMap()
        {
            // Primary Key
            this.HasKey(x => new { x.AppSettingId, x.Label });

            // Properties
            this.Property(x => x.Label)
                .IsRequired()
                .HasMaxLength(LABEL_MAX_LENGTH);

            // Relationships
            this.HasRequired(x => x.AppSetting)
                .WithMany(y => y.Labels)
                .HasForeignKey(x => x.AppSettingId)
                .WillCascadeOnDelete(true);
        }
    }
}
