using Epsilon.Logic.Constants.Interfaces;
using System;

namespace Epsilon.Logic.Constants
{
    public class AppSettingsDefaultValue : IAppSettingsDefaultValue
    {
        public TimeSpan DefaultAppCacheSlidingExpiration { get { return TimeSpan.FromMinutes(15); } }
        public double TokenAccountSnapshotSnoozePeriodInHours { get { return 24; } }
        public int TokenAccountSnapshotNumberOfTransactionsThreshold { get { return 16; } }
    }
}
