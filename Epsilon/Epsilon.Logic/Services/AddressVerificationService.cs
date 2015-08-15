using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Entities.Interfaces;
using Epsilon.Logic.Forms.Submission;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Resources.Logic.AddressVerification;
using System;
using System.Threading.Tasks;

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

            var countryId = cleanAddress.CountryId;
            var postcode = cleanAddress.Postcode;
            var geocodePostcodeStatus =
                await _geocodeService.GeocodePostcode(postcode, countryId);

            // EnumSwitch:GeocodePostcodeStatus
            switch (geocodePostcodeStatus)
            {
                case GeocodePostcodeStatus.Success:
                    // Do nothing, all OK, move on.
                    break;
                // This group below are the cases where the verification truly failed because the address was not correct.
                case GeocodePostcodeStatus.MultipleMatches:
                case GeocodePostcodeStatus.NoMatches:
                case GeocodePostcodeStatus.ResultInWrongCountry:
                case GeocodePostcodeStatus.ResultWithWrongType:
                    await SaveGeocodeFailure(userId, userIpAddress, postcode, countryId,
                        EnumsHelper.GeocodePostcodeStatus.ToString(geocodePostcodeStatus), AppConstant.GEOCODE_QUERY_TYPE_POSTCODE);
                    return new AddressVerificationResponse
                    {
                        IsRejected = true,
                        AskUserToModify = true,
                        RejectionReason = AddressVerificationResources.GeocodePostcodeVerificationFailureRejectionMessage
                    };
                // This group is for failure due to technical reasons. Do not log a GeocodeFailure.
                case GeocodePostcodeStatus.OverQueryLimitTriedMaxTimes:
                case GeocodePostcodeStatus.ServiceUnavailable:
                    return new AddressVerificationResponse
                    {
                        IsRejected = true,
                        AskUserToModify = false,
                        RejectionReason = AddressVerificationResources.GeocodeTehnicalFailureRejectionMessage
                    };
                default:
                    throw new NotImplementedException(string.Format("Unexpected GeocodePostcodeStatus: '{0}'",
                        EnumsHelper.GeocodePostcodeStatus.ToString(geocodePostcodeStatus)));
            }


            // Move on to geocode the full address
            var fullAddressWithoutCountry = cleanAddress.FullAddressWithoutCountry();
            var geocodeServiceResponse = 
                await _geocodeService.GeocodeAddress(fullAddressWithoutCountry, countryId);

            // EnumSwitch:GeocodeAddressStatus
            switch (geocodeServiceResponse.Status)
            {
                case GeocodeAddressStatus.Success:
                    return new AddressVerificationResponse
                    {
                        IsRejected = false,
                        AskUserToModify = false,
                        AddressGeometry = geocodeServiceResponse.Geometry
                    };
                // This group below are the cases where the verification truly failed because the address was not correct.
                case GeocodeAddressStatus.MultipleMatches:
                case GeocodeAddressStatus.NoMatches:
                case GeocodeAddressStatus.ResultInWrongCountry:
                    await SaveGeocodeFailure(userId, userIpAddress, fullAddressWithoutCountry, countryId,
                        EnumsHelper.GeocodeAddressStatus.ToString(geocodeServiceResponse.Status), AppConstant.GEOCODE_QUERY_TYPE_ADDRESS);
                    return new AddressVerificationResponse
                    {
                        IsRejected = true,
                        AskUserToModify = true,
                        RejectionReason = AddressVerificationResources.GeocodeAddressVerificationFailureRejectionMessage
                    };
                // This group is for failure due to technical reasons. Do not log a GeocodeFailure.
                case GeocodeAddressStatus.OverQueryLimitTriedMaxTimes:
                case GeocodeAddressStatus.ServiceUnavailable:
                    return new AddressVerificationResponse
                    {
                        IsRejected = true,
                        AskUserToModify = false,
                        RejectionReason = AddressVerificationResources.GeocodeTehnicalFailureRejectionMessage
                    };
                default:
                    throw new NotImplementedException(string.Format("Unexpected GeocodeAddressStatus: '{0}'", 
                        EnumsHelper.GeocodeAddressStatus.ToString(geocodeServiceResponse.Status)));
            }
        }

        private async Task SaveGeocodeFailure(
            string userId, string userIpAddress, 
            string address, string countryId,
            string failureType, string queryType)
        {
            var geocodeFailure = new GeocodeFailure
            {
                CreatedById = userId,
                CreatedByIpAddress = userIpAddress,
                Address = address,
                CountryId = countryId,
                FailureType = failureType,
                QueryType = queryType
            };
            _dbContext.GeocodeFailures.Add(geocodeFailure);
            await _dbContext.SaveChangesAsync();
        }
    }
}
