using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.SqlContext.Mapping
{
    public class AppSettingMap : EntityTypeConfiguration<AppSetting>
    {
        public const int VALUE_TYPE_MAX_LENGTH = 16;

        public AppSettingMap()
        {
            // Primary Key
            this.HasKey(x => x.Id);

            // Properties
            this.Property(x => x.Value)
                .IsRequired();
            this.Property(x => x.ValueType)
                .IsRequired()
                .HasMaxLength(VALUE_TYPE_MAX_LENGTH);
        }
    }
}
