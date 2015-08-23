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
        void Write(IEnumerable<IEnumerable<string>> values, TextWriter stream, IEnumerable<string> header = null);
    }
}
