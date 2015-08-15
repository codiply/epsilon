using Epsilon.Logic.Constants.Interfaces;
using Epsilon.Logic.Infrastructure.Primitives;
using System;

namespace Epsilon.Logic.Constants
{
    public class DbAppSettingDefaultValue : IDbAppSettingDefaultValue
    {
        public double AdminAlertSnoozePeriodInHours { get { return 24.0; } }

        public Frequency AntiAbuse_AddAddress_GlobalMaxFrequency { get { return new Frequency(1000, TimeSpan.FromDays(1)); } }
        public Frequency AntiAbuse_AddAddress_MaxFrequencyPerIpAddress { get { return new Frequency(2, TimeSpan.FromDays(1)); } }
        public Frequency AntiAbuse_AddAddress_MaxFrequencyPerUser { get { return new Frequency(2, TimeSpan.FromDays(30)); } }
        public Frequency AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerIpAddress { get { return new Frequency(8, TimeSpan.FromHours(1)); } }
        public Frequency AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerUser { get { return new Frequency(4, TimeSpan.FromHours(2)); } }
 
        public Frequency AntiAbuse_CreateTenancyDetailsSubmission_GlobalMaxFrequency { get { return new Frequency(10000, TimeSpan.FromDays(1)); } }
        public Frequency AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerIpAddress { get { return new Frequency(2, TimeSpan.FromDays(1)); } }
        public Frequency AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerUser { get { return new Frequency(1, TimeSpan.FromDays(30)); } }

        public Frequency AntiAbuse_PickOutgoingVerification_GlobalMaxFrequency { get { return new Frequency(10000, TimeSpan.FromDays(1)); } }
        public Frequency AntiAbuse_PickOutgoingVerification_MaxOutstandingFrequencyPerUser { get { return new Frequency(8, TimeSpan.FromDays(60)); } }
        public Frequency AntiAbuse_PickOutgoingVerification_MaxOutstandingFrequencyPerUserForNewUser { get { return new Frequency(4, TimeSpan.FromDays(60)); } }
        public Frequency AntiAbuse_PickOutgoingVerification_MaxFrequencyPerIpAddress { get { return new Frequency(8, TimeSpan.FromDays(3)); } }
        
        public Frequency AntiAbuse_Register_GlobalMaxFrequency { get { return new Frequency(300, TimeSpan.FromDays(1)); } }
        public Frequency AntiAbuse_Register_MaxFrequencyPerIpAddress { get { return new Frequency(3, TimeSpan.FromDays(7)); } }

        public double GeocodeService_OverQueryLimitDelayBetweenRetriesInSeconds { get { return 1.0; } }
        public int GeocodeService_OverQueryLimitMaxRetries { get { return 3; } }

        public double GeoipClient_TimeoutInMilliseconds { get { return 8000.0; } }
        public double GeoipInfo_ExpiryPeriodInDays { get { return 30; } }
        public int GeoipRotatingClient_MaxRotations { get { return 2; } }

        public double OutgoingVerification_Instructions_ExpiryPeriodInDays { get { return 7.0; } }
        public double OutgoingVerification_MyOutgoingVerificationsSummary_CachingPeriodInMinutes { get { return 15.0; } }
        public int OutgoingVerification_MyOutgoingVerificationsSummary_ItemsLimit { get { return 10; } }
        public double OutgoingVerification_RewardSendersIfNoneUsed_AfterPeriodInDays { get { return 30; } }
        // 1 degree is roughly 100 km (it varies slightly depending on the location)
        public double OutgoingVerification_Pick_MinDegreesDistanceInAnyDirection { get { return 0.1; } }
        public int OutgoingVerification_VerificationsPerTenancyDetailsSubmission { get { return 2; } }

        public double PropertInfoAccess_ExpiryPeriodInDays { get { return 30; } }
        public double PropertInfoAccess_MyExploredPropertiesSummary_CachingPeriodInMinutes { get { return 15.0; } }
        public int PropertInfoAccess_MyExploredPropertiesSummary_ItemsLimit { get { return 10; } }

        public int Address_SearchAddressResultsLimit { get { return 50; } }
        public int Address_SearchPropertyResultsLimit { get { return 50; } }

        public Frequency TenancyDetailsSubmission_Create_MaxFrequencyPerAddress { get { return new Frequency(1, TimeSpan.FromDays(30)); } }
        public double TenancyDetailsSubmission_MySubmissionsSummary_CachingPeriodInMinutes { get { return 15.0; } }
        public int TenancyDetailsSubmission_MySubmissionsSummary_ItemsLimit { get { return 10; } }

        public int Token_MyTokenTransactions_PageSize { get { return 30; } }
    }
}
