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
            // TODO_PANOS
            return address;
        }

        public AddressForm CleanseForStorage(AddressForm address)
        {
            // TODO_PANOS
            return address;
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
