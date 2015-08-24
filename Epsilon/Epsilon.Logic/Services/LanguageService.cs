using Epsilon.Logic.Constants;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using System;
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
                GetLanguagesFromDictionary(x => x.IsAvailable), WithLock.Yes);
            return availableLanguages;
        }

        public IList<Language> GetAvailableAndUnavailableLanguages()
        {
            var languages = _appCache.Get(AppCacheKey.AVAILABLE_AND_UNAVAILABLE_LANGUAGES, () =>
                GetLanguagesFromDictionary(x => true), WithLock.Yes);
            return languages;
        }

        public Language GetLanguage(string languageId)
        {
            var dictionary = GetLanguageDictionary();
            Language answer;
            if (dictionary.TryGetValue(languageId.ToLower(), out answer))
                return answer;
            return null;
        }

        private IList<Language> GetLanguagesFromDictionary(Func<Language, bool> filter)
        {
            var dictionary = GetLanguageDictionary();
            return dictionary.Select(x => x.Value).Where(filter).ToList();
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
