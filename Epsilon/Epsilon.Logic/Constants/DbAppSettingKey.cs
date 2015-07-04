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

        public const string AntiAbuse_AddAddress_MaxFrequencyPerIpAddress = "AntiAbuse_AddAddress_MaxFrequencyPerIpAddress";
        public const string AntiAbuse_AddAddress_MaxFrequencyPerUser = "AntiAbuse_AddAddress_MaxFrequencyPerUser";
        public const string AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerIpAddress = 
            "AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerIpAddress";
        public const string AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerUser =
            "AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerUser";
        public const string AntiAbuse_AddAddress_DisableIpAddressFrequencyCheck =
            "AntiAbuse_AddAddress_DisableIpAddressFrequencyCheck";
        public const string AntiAbuse_AddAddress_DisableUserFrequencyCheck =
            "AntiAbuse_AddAddress_DisableUserFrequencyCheck";
        public const string AntiAbuse_CreateTenancyDetailsSubmission_DisableIpAddressFrequencyCheck =
            "AntiAbuse_CreateTenancyDetailsSubmission_DisableIpAddressFrequencyCheck";
        public const string AntiAbuse_CreateTenancyDetailsSubmission_DisableUserFrequencyCheck =
            "AntiAbuse_CreateTenancyDetailsSubmission_DisableUserFrequencyCheck";
        public const string AntiAbuse_Register_GlobalMaxFrequency = "AntiAbuse_Register_GlobalMaxFrequency";
        public const string AntiAbuse_Register_DisableGlobalFrequencyCheck = "AntiAbuse_Register_DisableGlobalFrequencyCheck";
        public const string AntiAbuse_Register_MaxFrequencyPerIpAddress = "AntiAbuse_Register_MaxFrequencyPerIpAddress";
        public const string AntiAbuse_Register_DisableIpAddressFrequencyCheck = "AntiAbuse_Register_DisableIpAddressFrequencyCheck";

        public const string SearchAddressResultsLimit = "SearchAddressResultsLimit";
    }
}
