using System.Collections.Generic;
using System.IO;

namespace Epsilon.Logic.Helpers.Interfaces
{
    public interface ICsvHelper
    {
        void Write(TextWriter stream, IEnumerable<IEnumerable<string>> values, IEnumerable<string> header = null);
    }
}
