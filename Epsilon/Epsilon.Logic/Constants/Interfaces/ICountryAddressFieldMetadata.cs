using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Constants.Interfaces.CountryAddressFieldMetadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Constants.Interfaces
{
    namespace CountryAddressFieldMetadata
    {
        public class FieldMetadata
        {
            public bool IsUsed { get; set; }
            public bool IsRequired { get; set; }
        }

        public class CountryMetadata
        {
            public FieldMetadata Line1 { get; set; }
            public FieldMetadata Line2 { get; set; }
            public FieldMetadata Line3 { get; set; }
            public FieldMetadata Line4 { get; set; }
            public FieldMetadata Locality { get; set; }
            public FieldMetadata Region { get; set; }
            public FieldMetadata Postcode { get; set; }
        }
    }

    public interface ICountryAddressFieldMetadata
    {
        CountryMetadata GetForCountry(CountryId countryId);
    }
}
