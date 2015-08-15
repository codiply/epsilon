using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Forms.Submission;

namespace Epsilon.Logic.Helpers.Interfaces
{
    public interface IAddressCleansingHelper
    {
        AddressForm Cleanse(AddressForm address);

        string CleansePostcode(CountryId countryId, string postcode);
    }
}
