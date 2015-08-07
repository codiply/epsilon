using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Configuration.Interfaces
{
    public interface IOutgoingVerificationServiceConfig
    {
        bool GlobalSwitch_DisablePickOutgoingVerification { get; }
        double Instructions_ExpiryPeriodInDays { get; }
        TimeSpan MyOutgoingVerificationsSummary_CachingPeriod { get; }
        int MyOutgoingVerificationsSummary_ItemsLimit { get; }
        int VerificationsPerTenancyDetailsSubmission { get; }
    }
}
