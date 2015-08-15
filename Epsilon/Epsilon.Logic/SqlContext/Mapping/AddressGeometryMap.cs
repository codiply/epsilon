using Epsilon.Logic.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Epsilon.Logic.SqlContext.Mapping
{
    public class AddressGeometryMap : EntityTypeConfiguration<AddressGeometry>
    {
        public AddressGeometryMap()
        {
            // Primary Key
            this.HasKey(x => x.Id);

            // Properties
            this.Property(x => x.GeocodedOn)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            // Relationships
            this.HasRequired(x => x.Address)
                .WithOptional(y => y.Geometry)
                .WillCascadeOnDelete(true);
        }
    }
}
