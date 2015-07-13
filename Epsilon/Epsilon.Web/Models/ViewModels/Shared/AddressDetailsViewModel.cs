using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Epsilon.Web.Models.ViewModels.Shared
{
    public class AddressDetailsViewModel
    {
        public virtual long Id { get; set; }
        public virtual Guid UniqueId { get; set; }
        public virtual string Line1 { get; set; }
        public virtual string Line2 { get; set; }
        public virtual string Line3 { get; set; }
        public virtual string Line4 { get; set; }
        public virtual string Locality { get; set; }
        public virtual string Region { get; set; }
        public virtual string Postcode { get; set; }
        public virtual string CountryId { get; set; }

        public static AddressDetailsViewModel FromEntity(Address entity)
        {
            return new AddressDetailsViewModel
            {
                Id = entity.Id,
                UniqueId = entity.UniqueId,
                Line1 = entity.Line1,
                Line2 = entity.Line2,
                Line3 = entity.Line3,
                Line4 = entity.Line4,
                Locality = entity.Locality,
                Region = entity.Region,
                Postcode = entity.Postcode,
                CountryId = entity.CountryId
            };
        }
    }
}