using Epsilon.Logic.Entities;
using System.Data.Entity.ModelConfiguration;

namespace Epsilon.Logic.SqlContext.Mapping
{
    public class GeoipInfoMap : EntityTypeConfiguration<GeoipInfo>
    {
        public const int GEOIP_PROVIDER_NAME_MAX_LENGTH = 32;

        public GeoipInfoMap()
        {
            // Primary Key
            this.HasKey(x => x.IpAddress);

            this.Property(x => x.CountryCode)
                .IsRequired()
                .HasMaxLength(CountryMap.ID_MAX_LENGTH);
            this.Property(x => x.GeoipProviderName)
                .IsRequired()
                .HasMaxLength(GEOIP_PROVIDER_NAME_MAX_LENGTH);
            this.Property(x => x.RecordedOn)
                .IsRequired();
        }
    }
}
