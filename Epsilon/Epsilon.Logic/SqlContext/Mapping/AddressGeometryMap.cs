using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.SqlContext.Mapping
{
    public class AddressGeometryMap : EntityTypeConfiguration<AddressGeometry>
    {
        public AddressGeometryMap()
        {
            // Primary Key
            this.HasKey(x => x.Id);

            // Properties
            this.Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            // Relationships
            this.HasRequired(x => x.Address)
                .WithOptional(x => x.Geometry)
                .Map(x => x.MapKey("AddressId"));
        }
    }
}
