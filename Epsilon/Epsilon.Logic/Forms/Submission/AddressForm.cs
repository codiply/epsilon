using Epsilon.Logic.Entities;
using Epsilon.Logic.Entities.Interfaces;
using Epsilon.Logic.SqlContext.Mapping;
using System;
using System.ComponentModel.DataAnnotations;

namespace Epsilon.Logic.Forms.Submission
{
    public class AddressForm : IAddress
    {        
        // !!! IMPORTANT !!!
        // If you add new fields, make sure you update methods ToEntity() and CloneAndTrim() at the bottom

        public virtual Guid UniqueId { get; set; }

        [MaxLength(AddressMap.LINE_MAX_LENGTH)]
        public virtual string Line1 { get; set; }

        [MaxLength(AddressMap.LINE_MAX_LENGTH)]
        public virtual string Line2 { get; set; }

        [MaxLength(AddressMap.LINE_MAX_LENGTH)]
        public virtual string Line3 { get; set; }

        [MaxLength(AddressMap.LINE_MAX_LENGTH)]
        public virtual string Line4 { get; set; }

        [MaxLength(AddressMap.LOCALITY_MAX_LENGTH)]
        public virtual string Locality { get; set; }

        [MaxLength(AddressMap.REGION_MAX_LENGTH)]
        public virtual string Region { get; set; }

        [MaxLength(AddressMap.POSTCODE_MAX_LENGTH)]
        public virtual string Postcode { get; set; }

        [MaxLength(CountryMap.ID_MAX_LENGTH)]
        public virtual string CountryId { get; set; }

        public Address ToEntity()
        {
            return new Address
            {
                UniqueId = this.UniqueId,
                Line1 = this.Line1,
                Line2 = this.Line2,
                Line3 = this.Line3,
                Line4 = this.Line4,
                Locality = this.Locality,
                Region = this.Region,
                Postcode = this.Postcode,
                CountryId = this.CountryId
            };
        }

        public AddressForm CloneAndTrim()
        {
            return new AddressForm
            {
                UniqueId = this.UniqueId,
                Line1 = this.Line1 == null ? null : this.Line1.Trim(),
                Line2 = this.Line2 == null ? null : this.Line2.Trim(),
                Line3 = this.Line3 == null ? null : this.Line3.Trim(),
                Line4 = this.Line4 == null ? null : this.Line4.Trim(),
                Locality = this.Locality == null ? null : this.Locality.Trim(),
                Region = this.Region == null ? null : this.Region.Trim(),
                Postcode = this.Postcode == null ? null : this.Postcode.Trim(),
                CountryId = this.CountryId
            };
        }
    }
}
