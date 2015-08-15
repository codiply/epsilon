using System;

namespace Epsilon.Logic.Configuration.Interfaces
{
    public interface ITokenAccountServiceConfig
    {
        TimeSpan SnapshotSnoozePeriod { get; }
        int SnapshotNumberOfTransactionsThreshold { get; }
    }
}
