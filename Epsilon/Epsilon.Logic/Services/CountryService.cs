using Epsilon.Logic.Entities;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Collections.Immutable;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Constants.Enums;

namespace Epsilon.Logic.Services
{
    public class CountryService : ICountryService
    {
        private readonly IEpsilonContext _dbContext;
        private readonly IAppCache _appCache;

        public CountryService(
            IEpsilonContext dbContext,
            IAppCache appCache)
        {
            _dbContext = dbContext;
            _appCache = appCache;
        }

        public IList<Country> GetAvailableCountries()
        {
            var countries = GetAvailableCountriesFromDictionary();
            return countries;
        }

        public string GetDisplayName(string countryId)
        {
            var dictionary = GetCountryDictionary();
            var country = dictionary[countryId.ToUpper()];
            return AppConstant.COUNTRY_DISPLAY_FIELD_SELECTOR(country);
        }

        // TODO_TEST_PANOS
        public Country GetCountry(string countryId)
        {
            var dictionary = GetCountryDictionary();
            var key = countryId.ToUpper();
            if (dictionary.ContainsKey(key))
            {
                var country = dictionary[countryId.ToUpper()];
                return country;
            }
            return null;
        }

        // TODO_TEST_PANOS
        public bool IsCountryAvailable(CountryId countryId)
        {
            var dictionary = GetCountryDictionary();
            var key = EnumsHelper.CountryId.ToString(countryId).ToUpperInvariant();
            if (dictionary.ContainsKey(key))
            {
                var country = dictionary[key];
                return country != null && country.IsAvailable;
            }
     
            return false;
        }

        private IList<Country> GetAvailableCountriesFromDictionary()
        {
            var dictionary = GetCountryDictionary();
            return dictionary.Select(x => x.Value).Where(x => x.IsAvailable).ToList();
        }

        private ImmutableDictionary<string, Country> GetCountryDictionary()
        {
            return _appCache.Get(AppCacheKey.COUNTRIES_DICTIONARY, () =>
            {
                var languages = _dbContext.Countries.ToList();
                var dictionary = languages.ToImmutableDictionary(x => x.Id.ToUpper());
                return dictionary;
            }, WithLock.Yes);
        }
    }
}
