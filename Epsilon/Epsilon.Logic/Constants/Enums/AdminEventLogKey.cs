namespace Epsilon.Logic.Constants.Enums
{
    public enum AdminEventLogKey
    {
        AddAddressGlobalMaxFrequencyReached,
        CreateTenancyDetailsSubmissionGlobalMaxFrequencyReached,
        GeoipClientFailure,
        GeoipRotatingClientMaxRotationsReached,
        GeoipRotatingClientSuccessAfterFailures,
        GoogleGeocodeApiClientException,
        GoogleGeocodeApiClientReturnedNull,
        GoogleGeocodeApiStatusOverQueryLimitMaxRetriesReached,
        GoogleGeocodeApiStatusOverQueryLimitSuccessAfterRetrying,
        GoogleGeocodeApiStatusInvalidRequest,
        GoogleGeocodeApiStatusRequestDenied,
        GoogleGeocodeApiStatusUnexpected,
        GoogleGeocodeApiStatusUknownError,
        PickOutgoingVerificationGlobalMaxFrequencyReached,
        RegistrationGlobalMaxFrequencyReached,
        UserAccountMaintenanceCheckForUnrewardedOutgoingVerificationsTokenTransactionFailed,
        UserAccountMaintenanceThrewException
    }
}
