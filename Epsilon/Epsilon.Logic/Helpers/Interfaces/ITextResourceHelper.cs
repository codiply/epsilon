using System.Collections.Generic;
using System.IO;

namespace Epsilon.Logic.Helpers.Interfaces
{
    public class LocalizedResource
    {
        public string Type { get; set; }
        public IList<LocalizedResourceEntry> Entries { get; set; }
    }

    public class LocalizedResourceEntry
    {
        
        public string Name { get; set; }
        public string DefaultValue { get; set; }
        public string LocalizedValue { get; set; }
    }

    public interface ITextResourceHelper
    {
        IList<LocalizedResource> AllResources(string cultureCode);

        void AllResourcesCsv(string cultureCode, TextWriter stream);
    }
}
