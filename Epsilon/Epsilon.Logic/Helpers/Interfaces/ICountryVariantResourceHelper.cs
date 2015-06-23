using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Helpers.Interfaces
{
    public interface ICountryVariantResourceHelper
    {
        Dictionary<string, string> GetVariants(string resourceName);

        Dictionary<string, Dictionary<string, string>> GetVariants(IList<string> resourceNames);

        Dictionary<string, string> GetVariantsForCountry(string countryId, IList<string> resourceNames);
    }
}
