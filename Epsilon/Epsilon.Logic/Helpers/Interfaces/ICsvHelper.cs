using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Helpers.Interfaces
{
    public interface ICsvHelper
    {
        void Write(TextWriter stream, IEnumerable<IEnumerable<string>> values, IEnumerable<string> header = null);
    }
}
