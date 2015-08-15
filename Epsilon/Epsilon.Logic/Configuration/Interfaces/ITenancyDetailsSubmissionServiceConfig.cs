using Epsilon.Logic.Infrastructure.Primitives;
using System;

namespace Epsilon.Logic.Configuration.Interfaces
{
    public interface ITenancyDetailsSubmissionServiceConfig
    {
        bool Create_DisableFrequencyPerAddressCheck { get; }
        Frequency Create_MaxFrequencyPerAddress { get; }
        bool GlobalSwitch_DisableCreateTenancyDetailsSubmission { get; }
        TimeSpan MySubmissionsSummary_CachingPeriod { get; }
        int MySubmissionsSummary_ItemsLimit { get; }
    }
}
