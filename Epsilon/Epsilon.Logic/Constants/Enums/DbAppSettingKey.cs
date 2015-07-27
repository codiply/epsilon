﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Constants.Enums
{
    public enum DbAppSettingKey
    {
        AdminAlertSnoozePeriodInHours,
        AntiAbuse_AddAddress_DisableGeocodeFailureIpAddressFrequencyCheck,
        AntiAbuse_AddAddress_DisableGeocodeFailureUserFrequencyCheck,
        AntiAbuse_AddAddress_DisableGlobalFrequencyCheck,
        AntiAbuse_AddAddress_DisableIpAddressFrequencyCheck,
        AntiAbuse_AddAddress_DisableUserFrequencyCheck,
        AntiAbuse_AddAddress_GlobalMaxFrequency,
        AntiAbuse_AddAddress_MaxFrequencyPerIpAddress,
        AntiAbuse_AddAddress_MaxFrequencyPerUser,
        AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerIpAddress,
        AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerUser,
        AntiAbuse_CreateTenancyDetailsSubmission_DisableGlobalFrequencyCheck,
        AntiAbuse_CreateTenancyDetailsSubmission_DisableIpAddressFrequencyCheck,
        AntiAbuse_CreateTenancyDetailsSubmission_DisableUserFrequencyCheck,
        AntiAbuse_CreateTenancyDetailsSubmission_GlobalMaxFrequency,
        AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerIpAddress,
        AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerUser,
        AntiAbuse_PickOutgoingVerification_DisableGlobalFrequencyCheck,
        AntiAbuse_PickOutgoingVerification_GlobalMaxFrequency,
        AntiAbuse_Register_DisableGlobalFrequencyCheck,
        AntiAbuse_Register_DisableIpAddressFrequencyCheck,
        AntiAbuse_Register_GlobalMaxFrequency,
        AntiAbuse_Register_MaxFrequencyPerIpAddress,
        EnableResponseTiming,
        GeocodeService_OverQueryLimitDelayBetweenRetriesInSeconds,
        GeocodeService_OverQueryLimitMaxRetries,
        SearchAddressResultsLimit,
        TenancyDetailsSubmission_Create_DisableFrequencyPerAddressCheck,
        TenancyDetailsSubmission_Create_MaxFrequencyPerAddress,
        TenancyDetailsSubmission_MySubmissionsSummary_ItemsLimit,
    }
}