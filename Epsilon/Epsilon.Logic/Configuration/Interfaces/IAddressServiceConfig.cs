using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Configuration.Interfaces
{
    public interface IAddressServiceConfig
    {
        bool GlobalSwitch_DisableAddAddress { get; }
        int SearchAddressResultsLimit { get; }
        int SearchPropertyResultsLimit { get; }
    }
}
