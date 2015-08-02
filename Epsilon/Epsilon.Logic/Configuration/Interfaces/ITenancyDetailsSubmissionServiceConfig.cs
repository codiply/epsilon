using Epsilon.Logic.Infrastructure.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Configuration.Interfaces
{
    public interface ITenancyDetailsSubmissionServiceConfig
    {
        bool Create_DisableFrequencyPerAddressCheck { get; }
        Frequency Create_MaxFrequencyPerAddress { get; }
        bool GlobalSwitch_DisableCreateTenancyDetailsSubmission { get; }
        TimeSpan TenancyDetailsSubmission_MySubmissionsSummary_CachingPeriod { get; }
        int MySubmissionsSummary_ItemsLimit { get; }
    }
}
