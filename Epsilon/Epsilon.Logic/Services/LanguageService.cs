using Epsilon.Logic.Constants;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;

namespace Epsilon.Logic.Services
{
    public class LanguageService : ILanguageService
    {
        private readonly IEpsilonContext _dbContext;
        private readonly IAppCache _appCache;

        public LanguageService(
            IEpsilonContext dbContext,
            IAppCache appCache)
        {
            _dbContext = dbContext;
            _appCache = appCache;
        }

        public async Task<IList<Language>> GetAvailableLanguages()
        {
            var availableLanguages = await _appCache.GetAsync(AppCacheKeys.AVAILABLE_LANGUAGES, () =>
                GetAvailableLanguagesFromDictionary(), WithLock.Yes);
            return availableLanguages;
        }

        public async Task<Language> GetLanguage(string languageId)
        {
            var dictionary = await GetLanguageDictionary();
            return dictionary[languageId.ToLower()];
        }

        private async Task<IList<Language>> GetAvailableLanguagesFromDictionary()
        {
            var dictionary = await GetLanguageDictionary();
            return dictionary.Select(x => x.Value).Where(x => x.IsAvailable).ToList();
        }

        private async Task<ImmutableDictionary<string, Language>> GetLanguageDictionary()
        {
            return await _appCache.GetAsync(AppCacheKeys.LANGUAGES_DICTIONARY, () => FetchLanguageDictionary(), WithLock.Yes);
        }

        private async Task<ImmutableDictionary<string, Language>> FetchLanguageDictionary()
        {
            var languages = await _dbContext.Languages.ToListAsync();
            var dictionary = languages.ToImmutableDictionary(x => x.Id.ToLower());
            return dictionary;
        }
        
    }
}
