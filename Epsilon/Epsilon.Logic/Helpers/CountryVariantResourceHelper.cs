using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Resources.CountryVariants;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Resources;

namespace Epsilon.Logic.Helpers
{
    public class CountryVariantResourceHelper : ICountryVariantResourceHelper
    {
        private ConcurrentDictionary<string, ResourceManager> _countryResourceManagers = 
            new ConcurrentDictionary<string, ResourceManager>();
        private List<string> _countryIds;

        public CountryVariantResourceHelper()
        {
            _countryIds = Enum.GetNames(typeof(CountryId)).OrderBy(x => x).ToList();
        }

        public Dictionary<string, string> GetVariants(CountryVariantResourceName resourceName)
        {
            var answer = new Dictionary<string, string>();

            foreach (var countryId in _countryIds)
            {
                var resourceManager = GetCountryResourceManager(countryId);
                var value = resourceManager.GetString(EnumsHelper.CountryVariantResourceName.ToString(resourceName));
                answer.Add(countryId, value);
            }

            return answer;
        }

        public Dictionary<string, Dictionary<string, string>> GetVariants(IList<CountryVariantResourceName> resourceNames)
        {
            var answer = _countryIds.ToDictionary(id => id, id => GetVariantsForCountry(id, resourceNames));
            return answer;
        }
        
        public Dictionary<string, string> GetVariantsForCountry(string countryId, 
            IList<CountryVariantResourceName> resourceNames)
        {
            var resourceManager = GetCountryResourceManager(countryId);
            var answer = resourceNames
                .Select(resourceName => EnumsHelper.CountryVariantResourceName.ToString(resourceName)).
                ToDictionary(name => name, name => resourceManager.GetString(name));
            return answer;
        }

        private ResourceManager GetCountryResourceManager(string countryId)
        {
            return _countryResourceManagers.GetOrAdd(countryId, id =>
                new ResourceManager(AppConstant.COUNTRY_VARIANT_RESOURCES_STEM + id, typeof(ResourcesGB).Assembly));
        }
    }
}
