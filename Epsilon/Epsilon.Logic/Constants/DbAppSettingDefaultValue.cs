﻿using Epsilon.Logic.Constants.Interfaces;
using Epsilon.Logic.Infrastructure.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public int AntiAbuse_PickOutgoingVerification_MaxOutstandingPerUser { get { return 8; } }
        public int AntiAbuse_PickOutgoingVerification_MaxOutstandingPerUserForNewUser { get { return 4; } }
        public Frequency AntiAbuse_PickOutgoingVerification_MaxFrequencyPerIpAddress { get { return new Frequency(8, TimeSpan.FromDays(3)); } }
        
        public Frequency AntiAbuse_Register_GlobalMaxFrequency { get { return new Frequency(300, TimeSpan.FromDays(1)); } }
        public Frequency AntiAbuse_Register_MaxFrequencyPerIpAddress { get { return new Frequency(3, TimeSpan.FromDays(7)); } }

        public double GeocodeService_OverQueryLimitDelayBetweenRetriesInSeconds { get { return 1.0; } }
        public int GeocodeService_OverQueryLimitMaxRetries { get { return 3; } }

        public int OutgoingVerification_MyOutgoingVerificationsSummary_ItemsLimit { get { return 10; } }
        public int OutgoingVerification_VerificationsPerTenancyDetailsSubmission { get { return 2; } }

        public int SearchAddressResultsLimit { get { return 30; } }

        public Frequency TenancyDetailsSubmission_Create_MaxFrequencyPerAddress { get { return new Frequency(1, TimeSpan.FromDays(30)); } }
        public int TenancyDetailsSubmission_MySubmissionsSummary_ItemsLimit { get { return 10; } }
    }
}
