using Epsilon.Logic.Constants;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Resources.Common;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace Epsilon.Logic.Helpers
{
    public class TextResourceHelper : ITextResourceHelper
    {
        private readonly IAppCache _appCache;
        private readonly IAppSettingsHelper _appSettingsHelper;
        private readonly ICsvHelper _csvHelper;

        private readonly string _defaultLanguageId;
 
        private static Assembly _resourcesAssembly = typeof(CommonResources).Assembly;

        private ConcurrentDictionary<string, ResourceManager> _resourceManagers =
            new ConcurrentDictionary<string, ResourceManager>();

        public TextResourceHelper(
            IAppCache appCache,
            IAppSettingsHelper appSettingsHelper,
            ICsvHelper csvHelper)
        {
            _appCache = appCache;
            _appSettingsHelper = appSettingsHelper;
            _csvHelper = csvHelper;

            _defaultLanguageId = _appSettingsHelper.GetString(AppSettingsKey.DefaultLanguageId);
        }

        public void AllResourcesCsv(string cultureCode, TextWriter stream)
        {
            var header = new List<string> { "Resource Type", "Resource Name", "Default Value", "Localized Value" };
            var allResources = AllResources(cultureCode)
                .SelectMany(x => x.Entries.Select(y =>
                    new List<string> { x.Type, y.Name, y.DefaultValue, y.LocalizedValue }));

            _csvHelper.Write(stream, allResources, header);
        }

        public IList<LocalizedResource> AllResources(string cultureCode)
        {
            return _appCache.Get(AppCacheKey.TextResourceHelperAllResources(cultureCode),
                () => CompileAllResources(cultureCode), WithLock.No);
        }

        public IList<LocalizedResource> CompileAllResources(string cultureCode)
        {
            var answer = new List<LocalizedResource>();

            var allDefaultResources = GetAllResourcesForLanguage(_defaultLanguageId);

            var allLocalizedResources = cultureCode != null ? GetAllResourcesForLanguage(cultureCode) : null;

            foreach (var file in allDefaultResources.Keys)
            {
                var answerEntry = new LocalizedResource
                {
                    Type = file,
                    Entries = new List<LocalizedResourceEntry>()
                };
                var defaultResources = allDefaultResources[file];
                var localizedResources = 
                    (allLocalizedResources != null && allLocalizedResources.ContainsKey(file)) 
                    ? allLocalizedResources[file] 
                    : null;
                foreach (var name in defaultResources.Keys)
                {
                    var entry = new LocalizedResourceEntry
                    {
                        Name = name,
                        DefaultValue = defaultResources[name],
                        LocalizedValue =
                            (localizedResources != null && localizedResources.ContainsKey(name))
                            ? localizedResources[name]
                            : string.Empty
                    };
                    answerEntry.Entries.Add(entry);
                }
                answer.Add(answerEntry);
            }

            return answer;
        }

        private Dictionary<string, Dictionary<string, string>> GetAllResourcesForLanguage(string cultureCode)
        {
            var answer = new Dictionary<string, Dictionary<string, string>>();

            foreach (var file in AllResourceFiles())
            {
                answer.Add(file, GetResourcesFromFile(file, cultureCode));
            }

            return answer;
        }

        private List<string> AllResourceFiles()
        {
            return _resourcesAssembly.GetTypes()
                .OrderBy(x => x.FullName)
                .Select(x => x.FullName)
                .ToList();
        }

        private ResourceManager GetResourceManager(string resourceFullName)
        {
            return _resourceManagers.GetOrAdd(resourceFullName, id =>
                new ResourceManager(resourceFullName, _resourcesAssembly));
        }

        private Dictionary<string, string> GetResourcesFromFile(string resourceFullName, string cultureCode)
        {
            var cultureInfo = CultureInfo.GetCultureInfo(cultureCode);

            var resourceManager = GetResourceManager(resourceFullName);

            var resourceSet = resourceManager.GetResourceSet(cultureInfo, createIfNotExists: true, tryParents: true);

            var keys = new List<string>();

            foreach (DictionaryEntry resource in resourceSet)
            {
                keys.Add((string)resource.Key);
            }

            var answer = new Dictionary<string, string>();

            foreach (var key in keys.OrderBy(x => x))
            {
                answer.Add(key, resourceManager.GetString(key, cultureInfo));    
            }

            return answer;
        }
    }
}
