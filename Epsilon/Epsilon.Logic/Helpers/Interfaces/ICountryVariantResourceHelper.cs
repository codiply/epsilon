using Epsilon.Logic.Constants.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Helpers.Interfaces
{
    public interface ICountryVariantResourceHelper
    {
        Dictionary<string, string> GetVariants(CountryVariantResourceName resourceName);

        Dictionary<string, Dictionary<string, string>> GetVariants(IList<CountryVariantResourceName> resourceNames);

        Dictionary<string, string> GetVariantsForCountry(string countryId, IList<CountryVariantResourceName> resourceNames);
    }
}
