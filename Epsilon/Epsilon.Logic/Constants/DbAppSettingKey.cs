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

        public const string SearchAddressResultsLimit = "SearchAddressResultsLimit";
    }
}
