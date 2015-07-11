using Epsilon.Logic.Constants.Interfaces;
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

        public Frequency AntiAbuse_AddAddress_MaxFrequencyPerIpAddress { get { return new Frequency(2, TimeSpan.FromDays(1)); } }
        public Frequency AntiAbuse_AddAddress_MaxFrequencyPerUser { get { return new Frequency(2, TimeSpan.FromDays(30)); } }
        public Frequency AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerIpAddress { get { return new Frequency(2, TimeSpan.FromDays(1)); } }
        public Frequency AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerUser { get { return new Frequency(1, TimeSpan.FromDays(30)); } }
        public Frequency AntiAbuse_Register_GlobalMaxFrequency { get { return new Frequency(300, TimeSpan.FromDays(1)); } }
        public Frequency AntiAbuse_Register_MaxFrequencyPerIpAddress { get { return new Frequency(3, TimeSpan.FromDays(7)); } }

        public Frequency TenancyDetailsSubmission_Create_MaxFrequencyPerAddress { get { return new Frequency(1, TimeSpan.FromDays(30)); } }
        
        public int SearchAddressResultsLimit { get { return 30; } }
    }
}
