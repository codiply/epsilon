using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Constants.Interfaces;
using Epsilon.Logic.Constants.Interfaces.CountryAddressFieldMetadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Constants
{
    public class CountryAddressFieldMetadata : ICountryAddressFieldMetadata
    {
        public CountryMetadata GetForCountry(CountryId countryId)
        {
            switch(countryId)
            {
                case CountryId.GB:
                    return new CountryMetadata
                    {
                        Line1 = new FieldMetadata { IsUsed = true, IsRequired = true },
                        Line2 = new FieldMetadata { IsUsed = true, IsRequired = true },
                        Line3 = new FieldMetadata { IsUsed = true, IsRequired = true },
                        Line4 = new FieldMetadata { IsUsed = false, IsRequired = false },
                        Locality = new FieldMetadata { IsUsed = true, IsRequired = true },
                        Region = new FieldMetadata { IsUsed = true, IsRequired = false },
                        Postcode = new FieldMetadata { IsUsed = true, IsRequired = true }
                    };
                case CountryId.GR:
                    return new CountryMetadata
                    {
                        Line1 = new FieldMetadata { IsUsed = true, IsRequired = true },
                        Line2 = new FieldMetadata { IsUsed = true, IsRequired = false },
                        Line3 = new FieldMetadata { IsUsed = false, IsRequired = false },
                        Line4 = new FieldMetadata { IsUsed = false, IsRequired = false },
                        Locality = new FieldMetadata { IsUsed = true, IsRequired = true },
                        Region = new FieldMetadata { IsUsed = true, IsRequired = false },
                        Postcode = new FieldMetadata { IsUsed = true, IsRequired = true }
                    };
                default:
                    throw new NotImplementedException("Unexpected CountrId.");
            }
        }
    }
}
