using Epsilon.Logic.Infrastructure.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Configuration.Interfaces
{
    public interface IAntiAbuseServiceConfig
    {
        bool AddAddress_DisableGlobalFrequencyCheck { get; }
        Frequency AddAddress_GlobalMaxFrequency { get; }
        bool AddAddress_DisableIpAddressFrequencyCheck { get; }
        Frequency AddAddress_MaxFrequencyPerIpAddress { get; }
        bool AddAddress_DisableUserFrequencyCheck { get; }
        Frequency AddAddress_MaxFrequencyPerUser { get; }
        bool AddAddress_DisableGeocodeFailureIpAddressFrequencyCheck { get; }
        Frequency AddAddress_MaxGeocodeFailureFrequencyPerIpAddress { get; }
        bool AddAddress_DisableGeocodeFailureUserFrequencyCheck { get; }
        Frequency AddAddress_MaxGeocodeFailureFrequencyPerUser { get; }

        bool CreateTenancyDetailsSubmission_DisableGlobalFrequencyCheck { get; }
        Frequency CreateTenancyDetailsSubmission_GlobalMaxFrequency { get; }
        bool CreateTenancyDetailsSubmission_DisableIpAddressFrequencyCheck { get; }
        Frequency CreateTenancyDetailsSubmission_MaxFrequencyPerIpAddress { get; }
        bool CreateTenancyDetailsSubmission_DisableUserFrequencyCheck { get; }
        Frequency CreateTenancyDetailsSubmission_MaxFrequencyPerUser { get; }

        bool GlobalSwitch_DisableRegister { get; }

        bool PickOutgoingVerification_DisableGlobalFrequencyCheck { get; }
        bool PickOutgoingVerification_DisableIpAddressFrequencyCheck { get; }
        bool PickOutgoingVerification_DisableMaxOutstandingPerUserCheck { get; }
        Frequency PickOutgoingVerification_GlobalMaxFrequency { get; }
        int PickOutgoingVerification_MaxOutstandingPerUserConstant { get; }
        Frequency PickOutgoingVerification_MaxFrequencyPerIpAddress { get; }

        bool Register_DisableGlobalFrequencyCheck { get; }
        Frequency Register_GlobalMaxFrequency { get; }
        bool Register_DisableIpAddressFrequencyCheck { get; }
        Frequency Register_MaxFrequencyPerIpAddress { get; }

    }
}
