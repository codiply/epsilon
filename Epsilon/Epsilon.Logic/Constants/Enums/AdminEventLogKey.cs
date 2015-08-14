using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Constants.Enums
{
    public enum AdminEventLogKey
    {
        AddAddressGlobalMaxFrequencyReached,
        CreateTenancyDetailsSubmissionGlobalMaxFrequencyReached,
        GeoipClientFailure,
        GeoipRotatingClientMaxRotationsReached,
        GeoipRotatingClientSuccessAfterFailures,
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
