using Epsilon.Logic.Constants;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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

        public IList<Language> GetAvailableLanguages()
        {
            var availableLanguages = _appCache.Get(AppCacheKey.AVAILABLE_LANGUAGES, () =>
                GetAvailableLanguagesFromDictionary(), WithLock.Yes);
            return availableLanguages;
        }

        public Language GetLanguage(string languageId)
        {
            var dictionary = GetLanguageDictionary();
            Language answer;
            if (dictionary.TryGetValue(languageId.ToLower(), out answer))
                return answer;
            return null;
        }

        private IList<Language> GetAvailableLanguagesFromDictionary()
        {
            var dictionary = GetLanguageDictionary();
            return dictionary.Select(x => x.Value).Where(x => x.IsAvailable).ToList();
        }

        private ImmutableDictionary<string, Language> GetLanguageDictionary()
        {
            return _appCache
                .Get(AppCacheKey.LANGUAGES_DICTIONARY, () => {
                    var languages = _dbContext.Languages.ToList();
                    var dictionary = languages.ToImmutableDictionary(x => x.Id.ToLower());
                    return dictionary;
                }, WithLock.Yes);
        }      
    }
}
