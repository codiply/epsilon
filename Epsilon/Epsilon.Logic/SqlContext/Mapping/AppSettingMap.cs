using Epsilon.Logic.Entities;
using System.Data.Entity.ModelConfiguration;

namespace Epsilon.Logic.SqlContext.Mapping
{
    public class AppSettingMap : EntityTypeConfiguration<AppSetting>
    {
        public const int ID_MAX_LENGTH = 128;
        public const int VALUE_MAX_LENGTH = 256;
        public const int VALUE_TYPE_MAX_LENGTH = 16;

        public AppSettingMap()
        {
            // Primary Key
            this.HasKey(x => x.Id);

            // Properties
            this.Property(x => x.Id)
                .HasMaxLength(ID_MAX_LENGTH);
            this.Property(x => x.Value)
                .IsRequired()
                .HasMaxLength(VALUE_MAX_LENGTH);
            this.Property(x => x.ValueType)
                .IsRequired()
                .HasMaxLength(VALUE_TYPE_MAX_LENGTH);
            this.HasOptional(x => x.UpdatedBy)
                .WithMany()
                .HasForeignKey(x => x.UpdatedById)
                .WillCascadeOnDelete(false);
            this.Property(x => x.UpdatedOn)
                .IsOptional();
        }
    }
}
