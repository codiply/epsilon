using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Resources.Common;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Helpers
{
    public class TextResourceHelper : ITextResourceHelper
    {
        private static Assembly _resourcesAssembly = typeof(CommonResources).Assembly;

        private ConcurrentDictionary<string, ResourceManager> _resourceManagers =
            new ConcurrentDictionary<string, ResourceManager>();

        public Dictionary<string, Dictionary<string, string>> AllResources()
        {
            var answer = new Dictionary<string, Dictionary<string, string>>();

            foreach (var file in AllResourceFiles())
            {
                answer.Add(file, GetResourcesFromFile(file));
            }

            return answer;
        }

        private List<string> AllResourceFiles()
        {
            return _resourcesAssembly.GetTypes()
                .Select(x => x.FullName)
                .ToList();
        }

        private ResourceManager GetResourceManager(string resourceFullName)
        {
            return _resourceManagers.GetOrAdd(resourceFullName, id =>
                new ResourceManager(resourceFullName, _resourcesAssembly));
        }

        private Dictionary<string, string> GetResourcesFromFile(string resourceFullName)
        {
            var culture = CultureInfo.GetCultureInfo("en");

            var resourceManager = GetResourceManager(resourceFullName);

            var resourceSet = resourceManager.GetResourceSet(culture, createIfNotExists: true, tryParents: true);

            var answer = new Dictionary<string, string>();

            foreach (DictionaryEntry resource in resourceSet)
            {
                answer.Add((string)resource.Key, (string)resource.Value);
            }

            return answer;
        }
    }
}
