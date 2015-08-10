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

        Frequency AntiAbuse_AddAddress_GlobalMaxFrequency { get; }
        Frequency AntiAbuse_AddAddress_MaxFrequencyPerIpAddress { get; }
        Frequency AntiAbuse_AddAddress_MaxFrequencyPerUser { get; }
        Frequency AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerIpAddress { get; }
        Frequency AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerUser { get; }

        Frequency AntiAbuse_CreateTenancyDetailsSubmission_GlobalMaxFrequency { get; }
        Frequency AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerIpAddress { get; }
        Frequency AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerUser { get; }

        Frequency AntiAbuse_PickOutgoingVerification_GlobalMaxFrequency { get; }
        Frequency AntiAbuse_PickOutgoingVerification_MaxOutstandingFrequencyPerUser { get; }
        Frequency AntiAbuse_PickOutgoingVerification_MaxOutstandingFrequencyPerUserForNewUser { get; }
        Frequency AntiAbuse_PickOutgoingVerification_MaxFrequencyPerIpAddress { get; }

        Frequency AntiAbuse_Register_GlobalMaxFrequency { get; }
        Frequency AntiAbuse_Register_MaxFrequencyPerIpAddress { get; }

        double GeocodeService_OverQueryLimitDelayBetweenRetriesInSeconds { get; }
        int GeocodeService_OverQueryLimitMaxRetries { get; }

        double GeoipInfo_ExpiryPeriodInDays { get; }
        int GeoipInfoService_MaxRetries { get; }

        double OutgoingVerification_Instructions_ExpiryPeriodInDays { get; }
        double OutgoingVerification_MyOutgoingVerificationsSummary_CachingPeriodInMinutes { get; }
        int OutgoingVerification_MyOutgoingVerificationsSummary_ItemsLimit { get; }
        double OutgoingVerification_RewardSendersIfNoneUsed_AfterPeriodInDays { get; }
        int OutgoingVerification_VerificationsPerTenancyDetailsSubmission { get; }

        double PropertInfoAccess_ExpiryPeriodInDays { get; }
        double PropertInfoAccess_MyExploredPropertiesSummary_CachingPeriodInMinutes { get; }
        int PropertInfoAccess_MyExploredPropertiesSummary_ItemsLimit { get; }
 
        int Address_SearchAddressResultsLimit { get; }
        int Address_SearchPropertyResultsLimit { get; }

        Frequency TenancyDetailsSubmission_Create_MaxFrequencyPerAddress { get; }
        double TenancyDetailsSubmission_MySubmissionsSummary_CachingPeriodInMinutes { get; }
        int TenancyDetailsSubmission_MySubmissionsSummary_ItemsLimit { get; }

        int Token_MyTokenTransactions_PageSize { get; }
    }
}
