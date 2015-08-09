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
    public class CurrencyService : ICurrencyService
    {
        private readonly IEpsilonContext _dbContext;
        private readonly IAppCache _appCache;

        public CurrencyService(
            IEpsilonContext dbContext,
            IAppCache appCache)
        {
            _dbContext = dbContext;
            _appCache = appCache;
        }

        public IList<Currency> GetAll()
        {
            var dictionary = GetCurrencyDictionary();
            return dictionary.Select(x => x.Value).ToList();
        }

        public Currency Get(string currencyId)
        {
            var dictionary = GetCurrencyDictionary();
            var currency = dictionary[currencyId.ToUpper()];
            return currency;
        }

        public string GetDisplayName(string currencyId)
        {
            var dictionary = GetCurrencyDictionary();
            var currency = dictionary[currencyId.ToUpper()];
            return AppConstant.CURRENCY_DISPLAY_FIELD_SELECTOR(currency);
        }

        public string GetSymbol(string currencyId)
        {
            var dictionary = GetCurrencyDictionary();
            var currency = dictionary[currencyId.ToUpper()];
            return currency.Symbol;
        }

        private ImmutableDictionary<string, Currency> GetCurrencyDictionary()
        {
            return _appCache.Get(AppCacheKey.CURRENCIES_DICTIONARY, () =>
            {
                var currencies = _dbContext.Currencies.ToList();
                var dictionary = currencies.ToImmutableDictionary(x => x.Id.ToUpper());
                return dictionary;
            }, WithLock.Yes);
        }
    }
}
