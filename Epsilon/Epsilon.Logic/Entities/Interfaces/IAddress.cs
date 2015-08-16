using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Epsilon.Logic.Entities.Interfaces
{
    public interface IAddress
    {
        string Line1 { get; }
        string Line2 { get; }
        string Line3 { get; }
        string Line4 { get; }
        string Locality { get; }
        string Region { get; }
        string Postcode { get; }
        string CountryId { get; }
    }

    public static class IAddressExtensions
    {
        public static string FullAddressWithoutCountry(this IAddress address)
        {
            var pieces = new List<string> {
                address.Line1, address.Line2, address.Line3, address.Line4,
                address.Locality, address.Region, address.Postcode };

            return string.Join(", ", pieces.Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        public static string LocalityRegionPostcode(this IAddress address)
        {
            var pieces = new List<string> { address.Locality, address.Region, address.Postcode };

            return string.Join(", ", pieces.Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        public static CountryId CountryIdAsEnum(this IAddress address)
        {
            return EnumsHelper.CountryId.Parse(address.CountryId).Value;
        }
    }
}
