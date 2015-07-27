using Epsilon.Logic.Infrastructure.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Constants.Interfaces
{
    public interface IDbAppSettingDefaultValue
    {
        double AdminAlertSnoozePeriodInHours { get; }

        Frequency AntiAbuse_AddAddress_MaxFrequencyPerIpAddress { get; }
        Frequency AntiAbuse_AddAddress_MaxFrequencyPerUser { get; }
        Frequency AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerIpAddress { get; }
        Frequency AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerUser { get; }
        Frequency AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerIpAddress { get; }
        Frequency AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerUser { get; }
        Frequency AntiAbuse_Register_GlobalMaxFrequency { get; }
        Frequency AntiAbuse_Register_MaxFrequencyPerIpAddress { get; }

        double GeocodeService_OverQueryLimitDelayBetweenRetriesInSeconds { get; }
        int GeocodeService_OverQueryLimitMaxRetries { get; }

        int SearchAddressResultsLimit { get; }

        Frequency TenancyDetailsSubmission_Create_MaxFrequencyPerAddress { get; }
        int TenancyDetailsSubmission_MySubmissionsSummary_ItemsLimit { get; }
    }
}
