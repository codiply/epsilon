using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Helpers.Interfaces
{
    public interface ITextResourceHelper
    {
        Dictionary<string, Dictionary<string, string>> AllResources();
    }
}
