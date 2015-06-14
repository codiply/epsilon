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
        [MaxLength(AddressMap.LINE_MAX_LENGTH)]
        public virtual string Line1 { get; set; }

        [MaxLength(AddressMap.LINE_MAX_LENGTH)]
        public virtual string Line2 { get; set; }

        [MaxLength(AddressMap.LINE_MAX_LENGTH)]
        public virtual string Line3 { get; set; }

        [MaxLength(AddressMap.CITY_MAX_LENGTH)]
        public virtual string CityTown { get; set; }

        [MaxLength(AddressMap.COUNTY_MAX_LENGTH)]
        public virtual string CountyStateProvince { get; set; }

        [MaxLength(AddressMap.POSTCODE_MAX_LENGTH)]
        public virtual string PostcodeOrZip { get; set; }

        [MaxLength(CountryMap.ID_MAX_LENGTH)]
        public virtual string CountryId { get; set; }

        public Address ToEntity()
        {
            return new Address
            {
                Line1 = this.Line1,
                Line2 = this.Line2,
                Line3 = this.Line3,
                CityTown = this.CityTown,
                CountyStateProvince = this.CountyStateProvince,
                PostcodeOrZip = this.PostcodeOrZip,
                CountryId = this.CountryId
            };
        }
    }
}
