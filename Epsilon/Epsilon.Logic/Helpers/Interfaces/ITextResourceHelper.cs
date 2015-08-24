using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        IList<LocalizedResource> AllResources(string languageId);

        void AllResourcesCsv(string languageId, TextWriter stream);
    }
}
