using System;

namespace Epsilon.Logic.Configuration.Interfaces
{
    public interface IUserAccountMaintenanceServiceConfig
    {
        TimeSpan OutgoingVerification_RewardSendersIfNoneUsed_AfterPeriod { get; }
    }
}
