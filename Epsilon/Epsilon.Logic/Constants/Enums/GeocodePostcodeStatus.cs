using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Constants.Enums
{
    public enum GeocodePostcodeStatus
    {
        Success,
        NoMatches,
        MultipleMatches,
        OverQueryLimitTriedMaxTimes,
        ServiceUnavailable
    }
}
