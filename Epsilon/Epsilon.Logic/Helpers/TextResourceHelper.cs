using Epsilon.Logic.Constants;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Resources.Common;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Helpers
{
    public class TextResourceHelper : ITextResourceHelper
    {
        private readonly IAppSettingsHelper _appSettingsHelper;
        private readonly ICsvHelper _csvHelper;

        private readonly string _defaultLanguageId;
 
        private static Assembly _resourcesAssembly = typeof(CommonResources).Assembly;

        private ConcurrentDictionary<string, ResourceManager> _resourceManagers =
            new ConcurrentDictionary<string, ResourceManager>();

        public TextResourceHelper(
            IAppSettingsHelper appSettingsHelper,
            ICsvHelper csvHelper)
        {
            _appSettingsHelper = appSettingsHelper;
            _csvHelper = csvHelper;

            _defaultLanguageId = _appSettingsHelper.GetString(AppSettingsKey.DefaultLanguageId);
        }

        public void AllResourcesCsv(string languageId, TextWriter stream)
        {
            var allResources = AllResources(languageId)
                .Select(x => new List<string> { x.Type, x.Name, x.DefaultValue, x.LocalizedValue });

            _csvHelper.Write(allResources, stream);
        }

        public IList<LocalizedResourceEntry> AllResources(string languageId)
        {
            var answer = new List<LocalizedResourceEntry>();

            var allDefaultResources = GetAllResourcesForLanguage(_defaultLanguageId);

            var allLocalizedResources = languageId != null ? GetAllResourcesForLanguage(languageId) : null;

            foreach (var file in allDefaultResources.Keys)
            {
                var defaultResources = allDefaultResources[file];
                var localizedResources = 
                    (allLocalizedResources != null && allLocalizedResources.ContainsKey(file)) 
                    ? allLocalizedResources[file] 
                    : null;
                foreach (var name in defaultResources.Keys)
                {
                    var entry = new LocalizedResourceEntry
                    {
                        Type = file,
                        Name = name,
                        DefaultValue = defaultResources[name],
                        LocalizedValue =
                            (localizedResources != null && localizedResources.ContainsKey(name))
                            ? localizedResources[name]
                            : string.Empty
                    };
                    answer.Add(entry);
                }

            }

            return answer;
        }

        private Dictionary<string, Dictionary<string, string>> GetAllResourcesForLanguage(string languageId)
        {
            var answer = new Dictionary<string, Dictionary<string, string>>();

            foreach (var file in AllResourceFiles())
            {
                answer.Add(file, GetResourcesFromFile(file, languageId));
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

        private Dictionary<string, string> GetResourcesFromFile(string resourceFullName, string languageId)
        {
            var cultureInfo = CultureInfo.GetCultureInfo(languageId);

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
