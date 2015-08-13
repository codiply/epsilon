using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Constants.Enums
{
    // NOTE: If you add a GeoiopProviderName search for string 'EnumSwitch:GeoipProviderName' for 
    //       all places where you need to add a case in a switch statement.
    public enum GeoipProviderName
    {
        Telize,
        Freegeoip
    }
}
