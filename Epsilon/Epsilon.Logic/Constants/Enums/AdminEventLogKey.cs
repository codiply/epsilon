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
        GeoipApiMaxRetriesReached,
        GeoipApiSuccessAfterRetrying,
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
