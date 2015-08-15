using System;

namespace Epsilon.Logic.Constants.Interfaces
{
    public interface IAppSettingsDefaultValue
    {
        TimeSpan DefaultAppCacheSlidingExpiration { get; }
        double TokenAccountSnapshotSnoozePeriodInHours { get; }
        int TokenAccountSnapshotNumberOfTransactionsThreshold { get; }
    }
}
