﻿using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Forms.Submission;
using Epsilon.Logic.Helpers.Interfaces;
using System;
using System.Text.RegularExpressions;

namespace Epsilon.Logic.Helpers
{
    public class AddressCleansingHelper : IAddressCleansingHelper
    {
        public AddressForm Cleanse(AddressForm address)
        {
            var clone = address.CloneAndTrim();
            var countryId = EnumsHelper.CountryId.Parse(clone.CountryId);
            clone.Postcode = CleansePostcode(countryId.Value, clone.Postcode);
            return clone;
        }

        public string CleansePostcode(CountryId countryId, string postcode)
        {
            // EnumSwitch:CountryId
            switch (countryId)
            {
                case CountryId.GB:
                    return Regex.Replace(postcode, @"\s+", "").ToUpperInvariant();
                case CountryId.GR:
                    return Regex.Replace(postcode, @"\s+", "");
                default:
                    throw new NotImplementedException(string.Format("Unexpected CountryId: '{0}'",
                        EnumsHelper.CountryId.ToString(countryId)));
            }
        }
    }
}
