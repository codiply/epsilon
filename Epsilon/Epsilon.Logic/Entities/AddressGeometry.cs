using Epsilon.Logic.Entities.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Epsilon.Logic.Entities
{
    public class AddressGeometry : IGeometry
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

        [Timestamp]
        public virtual byte[] Timestamp { get; set; }

        public virtual Address Address { get; set; }
    }
}
