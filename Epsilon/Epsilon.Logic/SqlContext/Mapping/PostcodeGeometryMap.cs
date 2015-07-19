﻿using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.SqlContext.Mapping
{
    public class PostcodeGeometryMap : EntityTypeConfiguration<PostcodeGeometry>
    {
        public PostcodeGeometryMap()
        {
            // Primary Key
            this.HasKey(x => new { x.CountryId, x.Postcode });
            
            // Properties
            this.Property(x => x.GeocodedOn)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
        }
    }
}
