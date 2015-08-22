using System;

namespace Epsilon.Logic.Configuration.Interfaces
{
    public interface IUserAccountMaintenanceServiceConfig
    {
        bool DisableRewardOutgoingVerificationSendersIfNoneUsedAfterCertainPeriod { get; }
        TimeSpan OutgoingVerification_RewardSendersIfNoneUsed_AfterPeriod { get; }
    }
}
