using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Helpers.Interfaces
{
    public class LocalizedResourceEntry
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string DefaultValue { get; set; }
        public string LocalizedValue { get; set; }
    }

    public interface ITextResourceHelper
    {
        IList<LocalizedResourceEntry> AllResources(string languageId);

        void AllResourcesCsv(string languageId, TextWriter stream);
    }
}
