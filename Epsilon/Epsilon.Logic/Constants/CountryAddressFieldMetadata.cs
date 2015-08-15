using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Constants.Interfaces;
using Epsilon.Logic.Constants.Interfaces.CountryAddressFieldMetadata;
using Epsilon.Logic.Helpers;
using System;

namespace Epsilon.Logic.Constants
{
    public class CountryAddressFieldMetadata : ICountryAddressFieldMetadata
    {
        public CountryMetadata GetForCountry(CountryId countryId)
        {
            // EnumSwitch:CountryId
            switch (countryId)
            {
                case CountryId.GB:
                    return new CountryMetadata
                    {
                        Line1 = new FieldMetadata { IsUsed = true, IsRequired = true },
                        Line2 = new FieldMetadata { IsUsed = true, IsRequired = true },
                        Line3 = new FieldMetadata { IsUsed = false, IsRequired = false },
                        Line4 = new FieldMetadata { IsUsed = false, IsRequired = false },
                        Locality = new FieldMetadata { IsUsed = true, IsRequired = true },
                        Region = new FieldMetadata { IsUsed = true, IsRequired = false },
                        Postcode = new FieldMetadata { IsUsed = true, IsRequired = true }
                    };
                case CountryId.GR:
                    return new CountryMetadata
                    {
                        Line1 = new FieldMetadata { IsUsed = true, IsRequired = true },
                        Line2 = new FieldMetadata { IsUsed = false, IsRequired = false },
                        Line3 = new FieldMetadata { IsUsed = false, IsRequired = false },
                        Line4 = new FieldMetadata { IsUsed = false, IsRequired = false },
                        Locality = new FieldMetadata { IsUsed = true, IsRequired = true },
                        Region = new FieldMetadata { IsUsed = true, IsRequired = false },
                        Postcode = new FieldMetadata { IsUsed = true, IsRequired = true }
                    };
                default:
                    throw new NotImplementedException(string.Format("Unexpected CountryId: '{0}'",
                        EnumsHelper.CountryId.ToString(countryId)));
            }
        }
    }
}
