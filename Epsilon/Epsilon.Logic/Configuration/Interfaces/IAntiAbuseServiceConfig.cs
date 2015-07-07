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
        bool Register_DisableGlobalFrequencyCheck { get; }
        Frequency Register_GlobalMaxFrequency { get; }
        bool Register_DisableIpAddressFrequencyCheck { get; }
        Frequency Register_MaxFrequencyPerIpAddress { get; }
        bool AddAddress_DisableIpAddressFrequencyCheck { get; }
        Frequency AddAddress_MaxFrequencyPerIpAddress { get; }
        bool AddAddress_DisableUserFrequencyCheck { get; }
        Frequency AddAddress_MaxFrequencyPerUser { get; }
        bool CreateTenancyDetailsSubmission_DisableIpAddressFrequencyCheck { get; }
        Frequency CreateTenancyDetailsSubmission_MaxFrequencyPerIpAddress { get; }
        bool CreateTenancyDetailsSubmission_DisableUserFrequencyCheck { get; }
        Frequency CreateTenancyDetailsSubmission_MaxFrequencyPerUser { get; }
    }
}
