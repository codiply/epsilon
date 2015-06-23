using Epsilon.Logic.Constants;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Resources.CountryVariants;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Helpers
{
    public class CountryVariantResourceHelper : ICountryVariantResourceHelper
    {
        private ConcurrentDictionary<string, ResourceManager> _countryResourceManagers = 
            new ConcurrentDictionary<string, ResourceManager>();

        public Dictionary<string, string> GetVariants(string resourceName)
        {
            var countryCodes = Enum.GetNames(typeof(CountryId)).ToList();
            var answer = new Dictionary<string, string>();

            foreach (var code in countryCodes)
            {
                var resourceManager = GetCountryResourceManager(code);
                var value = resourceManager.GetString(resourceName);
                answer.Add(code, value);
            }

            return answer;
        }

        private ResourceManager GetCountryResourceManager(string countryCode)
        {
            return _countryResourceManagers.GetOrAdd(countryCode, code =>
                new ResourceManager(AppConstant.COUNTRY_VARIANT_RESOURCES_STEM + code, typeof(ResourcesGB).Assembly));
        }
    }
}
