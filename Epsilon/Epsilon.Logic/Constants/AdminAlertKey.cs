using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers;

namespace Epsilon.Logic.Constants
{
    public static class AdminAlertKey
    {
        public const string AddAddressGlobalMaxFrequencyReached = "AddAddressGlobalMaxFrequencyReached";
        public const string CreateTenancyDetailsSubmissionGlobalMaxFrequencyReached = "CreateTenancyDetailsSubmissionGlobalMaxFrequencyReached";
        public const string DbAppSettingsNotLoaded = "DbAppSettingsNotLoaded";
        public const string GeoipRotatingClientMaxRotationsReached = "GeoipRotatingClientMaxRotationsReached";

        public static string GeoipRotatingClientProviderFailed(GeoipProviderName providerName, WebClientResponseStatus responseStatus)
        {
            return string.Format("GeoipRotatingClientProviderFailed:{0}:{1}", 
                EnumsHelper.GeoipProviderName.ToString(providerName), EnumsHelper.WebClientResponseStatus.ToString(responseStatus));
        }

        public const string GoogleGeocodeApiClientException = "GoogleGeocodeApiClientException";
        public const string GoogleGeocodeApiStatusInvalidRequest = "GoogleGeocodeApiStatusInvalidRequest";
        public const string GoogleGeocodeApiStatusOverQueryLimitMaxRetriesReached = "GoogleGeocodeApiStatusOverQueryLimitMaxRetriesReached";
        public const string GoogleGeocodeApiStatusRequestDenied = "GoogleGeocodeApiStatusRequestDenied";
        public const string GoogleGeocodeApiStatusUnexpected = "GoogleGeocodeApiStatusUnexpected";
        public const string GoogleGeocodeApiStatusUknownError = "GoogleGeocodeApiStatusUknownError";
        public const string PickOutgoingVerificationGlobalMaxFrequencyReached = "PickOutgoingVerificationGlobalMaxFrequencyReached";
        public const string RegistrationGlobalMaxFrequencyReached = "RegistrationGlobalMaxFrequencyReached";
        public const string UserAccountMaintenanceCheckForUnrewardedOutgoingVerificationsTokenTransactionFailed =
            "UserAccountMaintenanceCheckForUnrewardedOutgoingVerificationsTokenTransactionFailed";
        public const string UserAccountMaintenanceThrewException = "UserAccountMaintenanceThrewException";
        
    }
}
