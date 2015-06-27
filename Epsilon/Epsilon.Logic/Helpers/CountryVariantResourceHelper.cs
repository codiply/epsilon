using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
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
        private List<string> _countryIds;

        public CountryVariantResourceHelper()
        {
            _countryIds = Enum.GetNames(typeof(CountryId)).OrderBy(x => x).ToList();
        }

        public Dictionary<string, string> GetVariants(string resourceName)
        {
            var answer = new Dictionary<string, string>();

            foreach (var countryId in _countryIds)
            {
                var resourceManager = GetCountryResourceManager(countryId);
                var value = resourceManager.GetString(resourceName);
                answer.Add(countryId, value);
            }

            return answer;
        }

        public Dictionary<string, Dictionary<string, string>> GetVariants(IList<string> resourceNames)
        {
            var answer = _countryIds.ToDictionary(id => id, id => GetVariantsForCountry(id, resourceNames));
            return answer;
        }
        
        public Dictionary<string, string> GetVariantsForCountry(string countryId, IList<string> resourceNames)
        {
            var resourceManager = GetCountryResourceManager(countryId);
            var answer = resourceNames.ToDictionary(name => name, name => resourceManager.GetString(name));
            return answer;
        }

        private ResourceManager GetCountryResourceManager(string countryId)
        {
            return _countryResourceManagers.GetOrAdd(countryId, id =>
                new ResourceManager(AppConstant.COUNTRY_VARIANT_RESOURCES_STEM + id, typeof(ResourcesGB).Assembly));
        }
    }
}
