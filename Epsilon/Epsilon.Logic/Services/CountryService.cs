using Epsilon.Logic.Entities;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext;
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

        public async Task<IList<Country>> GetAvailableCountries()
        {
            var countries = await GetAvailableCountriesFromDictionary();
            return countries;
        }

        private async Task<IList<Country>> GetAvailableCountriesFromDictionary()
        {
            var dictionary = await GetCountryDictionary();
            return dictionary.Select(x => x.Value).Where(x => x.IsAvailable).ToList();
        }

        private async Task<ImmutableDictionary<string, Country>> GetCountryDictionary()
        {
            return await _appCache.GetAsync(AppCacheKeys.COUNTRIES_DICTIONARY, () => FetchCountryDictionary(), WithLock.Yes);
        }

        private async Task<ImmutableDictionary<string, Country>> FetchCountryDictionary()
        {
            var languages = await _dbContext.Countries.ToListAsync();
            var dictionary = languages.ToImmutableDictionary(x => x.Id.ToLower());
            return dictionary;
        }
    }
}
