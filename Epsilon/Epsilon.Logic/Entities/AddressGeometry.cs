﻿using Epsilon.Logic.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Entities
{
    public class AddressGeometry : BaseEntity, IGeometry
    {
        [Key, ForeignKey("Address")]
        public virtual long Id { get; set; }
        public virtual double Latitude { get; set; }
        public virtual double Longitude { get; set; }
        public virtual double ViewportNortheastLatitude { get; set; }
        public virtual double ViewportNortheastLongitude { get; set; }
        public virtual double ViewportSouthwestLatitude { get; set; }
        public virtual double ViewportSouthwestLongitude { get; set; }
        public virtual DateTimeOffset GeocodedOn { get; set; }
        
        public virtual Address Address { get; set; }
    }
}