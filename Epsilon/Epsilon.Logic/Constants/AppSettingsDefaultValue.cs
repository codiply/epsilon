using Epsilon.Logic.Constants.Interfaces;
using System;

namespace Epsilon.Logic.Constants
{
    public class AppSettingsDefaultValue : IAppSettingsDefaultValue
    {
        public TimeSpan DefaultAppCacheSlidingExpiration { get { return TimeSpan.FromMinutes(15); } }
        public bool SmtpServiceEnableSsl { get { return true; } }
        public int SmtpServicePort { get { return 587; } }
        public int SmtpServiceTimeoutMilliseconds { get { return 10000; } }
        public double TokenAccountSnapshotSnoozePeriodInHours { get { return 24; } }
        public int TokenAccountSnapshotNumberOfTransactionsThreshold { get { return 16; } }
    }
}
