using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Forms;
using Epsilon.Logic.FSharp;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Epsilon.Logic.Entities.Interfaces;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers;

namespace Epsilon.Logic.Services
{
    public class AddressVerificationService : IAddressVerificationService
    {
        private readonly IEpsilonContext _dbContext;
        private readonly IAddressCleansingHelper _addressCleansingHelper;
        private readonly IGeocodeService _geocodeService;

        public AddressVerificationService(
            IEpsilonContext dbContext,
            IAddressCleansingHelper addressCleansingHelper,
            IGeocodeService geocodeService)
        {
            _dbContext = dbContext;
            _addressCleansingHelper = addressCleansingHelper;
            _geocodeService = geocodeService;
        }

        public async Task<AddressVerificationResponse> Verify(string userId, string userIpAddress, AddressForm address)
        {
            var cleanAddress = _addressCleansingHelper.Cleanse(address);

            var geocodePostcodeStatus =
                await _geocodeService.GeocodePostcode(cleanAddress.Postcode, cleanAddress.CountryId);

            // EnumSwitch:GeocodePostcodeStatus
            switch (geocodePostcodeStatus)
            {
                case GeocodePostcodeStatus.Success:
                    // Don nothing, all OK, move on.
                    break;
                // This group below are the cases where the verification truly failed because the address was not correct.
                case GeocodePostcodeStatus.MultipleMatches:
                case GeocodePostcodeStatus.NoMatches:
                    // PANOS_TODO: Save a GeocodeFailure
                    return new AddressVerificationResponse
                    {
                        IsRejected = true,
                        // TODO_PANOS: put in a resource
                        RejectionReason = "We couldn't verify your postcode. Please check it is correct and if not submit again."
                    };
                // This group is for failure due to technical reasons. Do not log a GeocodeFailure.
                case GeocodePostcodeStatus.OverQueryLimitTriedMaxTimes:
                case GeocodePostcodeStatus.ServiceUnavailable:
                    return new AddressVerificationResponse
                    {
                        IsRejected = true,
                        // TODO_PANOS: put in a resource, same message as below
                        RejectionReason = "Due to technical issues we couldn't verify your address. Please try again in a few moments."
                    };
                default:
                    throw new NotImplementedException(string.Format("Unexpected GeocodePostcodeStatus: '{0}'",
                        EnumsHelper.GeocodePostcodeStatus.ToString(geocodePostcodeStatus)));
            }

            var geocodeServiceResponse = 
                await _geocodeService.GeocodeAddress(cleanAddress.FullAddressWithoutCountry(), cleanAddress.CountryId);

            // EnumSwitch:GeocodeAddressStatus
            switch (geocodeServiceResponse.Status)
            {
                case GeocodeAddressStatus.Success:
                    return new AddressVerificationResponse
                    {
                        IsRejected = false,
                        AddressGeometry = geocodeServiceResponse.Geometry
                    };
                // This group below are the cases where the verification truly failed because the address was not correct.
                case GeocodeAddressStatus.MultipleMatches:
                case GeocodeAddressStatus.NoMatches:
                    // PANOS_TODO: Save a GeocodeFailure
                    return new AddressVerificationResponse
                    {
                        IsRejected = true,
                        // TODO_PANOS: put in a resource
                        RejectionReason = "We couldn't verify your address. Please check the details provided are correct and if not submit again."
                    };
                // This group is for failure due to technical reasons. Do not log a GeocodeFailure.
                case GeocodeAddressStatus.OverQueryLimitTriedMaxTimes:
                case GeocodeAddressStatus.ServiceUnavailable:
                    return new AddressVerificationResponse
                    {
                        IsRejected = true,
                        // TODO_PANOS: put in a resource
                        RejectionReason = "Due to technical issues we couldn't verify your address. Please try again in a few moments."
                    };
                default:
                    throw new NotImplementedException(string.Format("Unexpected GeocodeAddressStatus: '{0}'", 
                        EnumsHelper.GeocodeAddressStatus.ToString(geocodeServiceResponse.Status)));
            }

            if (geocodeServiceResponse.Status == GeocodeAddressStatus.Success)
            {

            }

            if (geocodeServiceResponse.Status == GeocodeAddressStatus.MultipleMatches
                || geocodeServiceResponse.Status == GeocodeAddressStatus.NoMatches)
            {

            }

            if (geocodeServiceResponse.Status == GeocodeAddressStatus.OverQueryLimitTriedMaxTimes
                || geocodeServiceResponse.Status == GeocodeAddressStatus.ServiceUnavailable)
            {
                
            }
        }
    }
}
