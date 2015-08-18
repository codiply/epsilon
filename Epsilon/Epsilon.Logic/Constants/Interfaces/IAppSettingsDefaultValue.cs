using System;

namespace Epsilon.Logic.Constants.Interfaces
{
    public interface IAppSettingsDefaultValue
    {
        TimeSpan DefaultAppCacheSlidingExpiration { get; }
        bool SmtpServiceEnableSsl { get; }
        int SmtpServicePort { get; }
        int SmtpServiceTimeoutMilliseconds { get; }
        double TokenAccountSnapshotSnoozePeriodInHours { get; }
        int TokenAccountSnapshotNumberOfTransactionsThreshold { get; }
    }
}
