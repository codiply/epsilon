using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Forms;
using Epsilon.Logic.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Epsilon.Logic.Helpers
{
    public class AddressCleansingHelper : IAddressCleansingHelper
    {
        public AddressForm CleanseForVerification(AddressForm address)
        {
            return address.Clone();
        }

        public AddressForm CleanseForStorage(AddressForm address)
        {
            var clone = address.Clone();
            var countryId = EnumsHelper.CountryId.Parse(clone.CountryId);
            clone.Postcode = CleanPostcode(countryId.Value, clone.Postcode);
            return clone;
        }

        public string CleanPostcode(CountryId countryId, string postcode)
        {
            switch (countryId)
            {
                case CountryId.GB:
                    return Regex.Replace(postcode, @"\s+", "").ToUpperInvariant();
                default:
                    return postcode;
            }
        }
    }
}
