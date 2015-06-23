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
                var dictionary = languages.ToImmutableDictionary(x => x.Id.ToLower());
                return dictionary;
            }, WithLock.Yes);
        }
    }
}
