using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Constants.Enums
{
    public enum GeocodeAddressStatus
    {
        Success,
        NoMatches,
        MultipleMatches,
        OverQueryLimitTriedMaxTimes,
        ServiceUnavailable
    }
}
