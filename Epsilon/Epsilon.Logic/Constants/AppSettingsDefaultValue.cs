using Epsilon.Logic.Constants.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Constants
{
    public class AppSettingsDefaultValue : IAppSettingsDefaultValue
    {
        public TimeSpan DefaultAppCacheSlidingExpiration { get { return TimeSpan.FromMinutes(15); } }
        public double TokenAccountSnapshotSnoozePeriodInHours { get { return 24; } }
        public int TokenAccountSnapshotNumberOfTransactionsThreshold { get { return 16; } }
    }
}
