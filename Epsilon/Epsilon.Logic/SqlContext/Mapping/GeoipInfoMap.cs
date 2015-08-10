using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.SqlContext.Mapping
{
    public class GeoipInfoMap : EntityTypeConfiguration<GeoipInfo>
    {
        public const int CONTINENT_CODE_MAX_LENGTH = 2;

        public GeoipInfoMap()
        {
            // Primary Key
            this.HasKey(x => x.IpAddress);

            this.Property(x => x.CountryCode)
                .IsRequired()
                .HasMaxLength(CountryMap.ID_MAX_LENGTH);
            this.Property(x => x.ContinentCode)
                .HasMaxLength(CONTINENT_CODE_MAX_LENGTH);
            this.Property(x => x.RecordedOn)
                .IsRequired();
        }
    }
}
