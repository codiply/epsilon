﻿using Epsilon.Logic.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Epsilon.Logic.Entities
{
    public class PostcodeGeometry : IGeometry
    {
        public virtual string Postcode { get; set; }
        public virtual string CountryId { get; set; }

        public virtual double Latitude { get; set; }
        public virtual double Longitude { get; set; }
        public virtual double ViewportNortheastLatitude { get; set; }
        public virtual double ViewportNortheastLongitude { get; set; }
        public virtual double ViewportSouthwestLatitude { get; set; }
        public virtual double ViewportSouthwestLongitude { get; set; }

        public virtual DateTimeOffset GeocodedOn { get; set; }

        [Timestamp]
        public virtual byte[] Timestamp { get; set; }

        public virtual ICollection<Address> Addresses { get; set; }
        public virtual Country Country { get; set; }
    }
}
