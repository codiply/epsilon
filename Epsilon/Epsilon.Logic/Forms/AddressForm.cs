using Epsilon.Logic.Entities;
using Epsilon.Logic.SqlContext.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Forms
{
    public class AddressForm
    {        
        // !!! IMPORTANT !!!
        // If you add new fields, make sure you update methods ToEntity() and Clone() at the bottom

        public virtual Guid Id { get; set; }

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
                Id = this.Id,
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

        public AddressForm Clone()
        {
            return new AddressForm
            {
                Id = this.Id,
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
    }
}
