using Epsilon.Logic.Infrastructure.Primitives;

namespace Epsilon.Logic.Configuration.Interfaces
{
    public interface IAntiAbuseServiceConfig
    {
        bool AddAddress_DisableGeocodeFailureIpAddressFrequencyCheck { get; }
        bool AddAddress_DisableGeocodeFailureUserFrequencyCheck { get; }
        bool AddAddress_DisableGeoipCheck { get; }
        bool AddAddress_DisableGlobalFrequencyCheck { get; }
        bool AddAddress_DisableIpAddressFrequencyCheck { get; }
        bool AddAddress_DisableUserFrequencyCheck { get; }
        Frequency AddAddress_GlobalMaxFrequency { get; }
        Frequency AddAddress_MaxFrequencyPerIpAddress { get; }
        Frequency AddAddress_MaxFrequencyPerUser { get; }
        Frequency AddAddress_MaxGeocodeFailureFrequencyPerIpAddress { get; }
        Frequency AddAddress_MaxGeocodeFailureFrequencyPerUser { get; }

        bool CreateTenancyDetailsSubmission_DisableGeoipCheck { get; }
        bool CreateTenancyDetailsSubmission_DisableGlobalFrequencyCheck { get; }
        bool CreateTenancyDetailsSubmission_DisableIpAddressFrequencyCheck { get; }
        bool CreateTenancyDetailsSubmission_DisableUserFrequencyCheck { get; }
        Frequency CreateTenancyDetailsSubmission_GlobalMaxFrequency { get; }
        Frequency CreateTenancyDetailsSubmission_MaxFrequencyPerIpAddress { get; }
        Frequency CreateTenancyDetailsSubmission_MaxFrequencyPerUser { get; }

        bool GlobalSwitch_DisableRegister { get; }
        bool GlobalSwitch_DisableUseOfGeoipInformation { get; }

        bool PickOutgoingVerification_DisableGeoipCheck { get;}
        bool PickOutgoingVerification_DisableGlobalFrequencyCheck { get; }
        bool PickOutgoingVerification_DisableIpAddressFrequencyCheck { get; }
        bool PickOutgoingVerification_DisableMaxOutstandingFrequencyPerUserCheck { get; }
        Frequency PickOutgoingVerification_GlobalMaxFrequency { get; }
        Frequency PickOutgoingVerification_MaxOutstandingFrequencyPerUser { get; }
        Frequency PickOutgoingVerification_MaxOutstandingFrequencyPerUserForNewUser { get; }
        Frequency PickOutgoingVerification_MaxFrequencyPerIpAddress { get; }

        bool Register_DisableGlobalFrequencyCheck { get; }
        bool Register_DisableIpAddressFrequencyCheck { get; }
        Frequency Register_GlobalMaxFrequency { get; }
        Frequency Register_MaxFrequencyPerIpAddress { get; }

    }
}
