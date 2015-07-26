using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Constants
{
    public class DbAppSettingKey
    {
        public const string AdminAlertSnoozePeriodInHours = "AdminAlertSnoozePeriodInHours";

        public const string AntiAbuse_AddAddress_DisableIpAddressFrequencyCheck =
            "AntiAbuse_AddAddress_DisableIpAddressFrequencyCheck";
        public const string AntiAbuse_AddAddress_DisableUserFrequencyCheck =
            "AntiAbuse_AddAddress_DisableUserFrequencyCheck";
        public const string AntiAbuse_AddAddress_MaxFrequencyPerIpAddress = 
            "AntiAbuse_AddAddress_MaxFrequencyPerIpAddress";
        public const string AntiAbuse_AddAddress_MaxFrequencyPerUser = 
            "AntiAbuse_AddAddress_MaxFrequencyPerUser";
        public const string AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerIpAddress = 
            "AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerIpAddress";
        public const string AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerUser =
            "AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerUser";
        public const string AntiAbuse_CreateTenancyDetailsSubmission_DisableIpAddressFrequencyCheck =
            "AntiAbuse_CreateTenancyDetailsSubmission_DisableIpAddressFrequencyCheck";
        public const string AntiAbuse_CreateTenancyDetailsSubmission_DisableUserFrequencyCheck =
            "AntiAbuse_CreateTenancyDetailsSubmission_DisableUserFrequencyCheck";
        public const string AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerIpAddress =
            "AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerIpAddress";
        public const string AntiAbuse_AddAddress_DisableGeocodeFailureIpAddressFrequencyCheck =
            "AntiAbuse_AddAddress_DisableGeocodeFailureIpAddressFrequencyCheck";
        public const string AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerUser =
            "AntiAbuse_AddAddress_MaxGeocodeFailureFrequencyPerUser";
        public const string AntiAbuse_AddAddress_DisableGeocodeFailureUserFrequencyCheck =
            "AntiAbuse_AddAddress_DisableGeocodeFailureUserFrequencyCheck";
        public const string AntiAbuse_Register_GlobalMaxFrequency =
            "AntiAbuse_AddAddress_DisableGeocodeFailureUserFrequencyCheck";
        public const string AntiAbuse_Register_DisableGlobalFrequencyCheck = 
            "AntiAbuse_Register_DisableGlobalFrequencyCheck";
        public const string AntiAbuse_Register_MaxFrequencyPerIpAddress = 
            "AntiAbuse_Register_MaxFrequencyPerIpAddress";
        public const string AntiAbuse_Register_DisableIpAddressFrequencyCheck = 
            "AntiAbuse_Register_DisableIpAddressFrequencyCheck";

        public const string TenancyDetailsSubmission_Create_MaxFrequencyPerAddress = 
            "TenancyDetailsSubmission_Create_MaxFrequencyPerAddress";
        public const string TenancyDetailsSubmission_Create_DisableFrequencyPerAddressCheck = 
            "TenancyDetailsSubmission_Create_DisableFrequencyPerAddressCheck";
        public const string TenancyDetailsSubmission_MySubmissionsSummary_ItemsLimit =
            "TenancyDetailsSubmission_MySubmissionsSummary_ItemsLimit";

        public const string GeocodeService_OverQueryLimitMaxRetries =
            "GeocodeService_OverQueryLimitMaxRetries";
        public const string GeocodeService_OverQueryLimitDelayBetweenRetriesInSeconds =
            "GeocodeService_OverQueryLimitDelayBetweenRetriesInSeconds";

        public const string SearchAddressResultsLimit = "SearchAddressResultsLimit";

        public const string EnableResponseTiming = "EnableResponseTiming";
    }
}
