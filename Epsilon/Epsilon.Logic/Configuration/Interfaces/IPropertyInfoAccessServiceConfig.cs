using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Configuration.Interfaces
{
    public interface IPropertyInfoAccessServiceConfig
    {
        double ExpiryPeriodInDays { get; }
        bool GlobalSwitch_DisableCreatePropertyInfoAccess { get; }
        TimeSpan MyExploredPropertiesSummary_CachingPeriod { get; }
        int MyExploredPropertiesSummary_ItemsLimit { get; }
    }
}
