using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using GeocodeSharp.Google;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services
{
    public class GeocodeService : IGeocodeService
    {
        public const string TYPE_COUNTRY = "country";
        public const string TYPE_POSTAL_CODE = "postal_code";
        public const string TYPE_POSTAL_CODE_PREFIX = "postal_code_prefix";

        private readonly IClock _clock;
        private readonly IRandomFactory _randomFactory;
        private readonly IEpsilonContext _dbContext;
        private readonly IGeocodeServiceConfig _geocodeServiceConfig;
        private readonly IGeocodeClientFactory _geocodeClientFactory;
        private readonly IAdminAlertService _adminAlertService;
        private readonly IAdminEventLogService _adminEventLogService;
        private readonly IElmahHelper _elmahHelper;

        public GeocodeService(
            IClock clock,
            IRandomFactory randomFactory,
            IEpsilonContext dbContext,
            IGeocodeServiceConfig geocodeServiceConfig,
            IGeocodeClientFactory geocodeClientFactory,
            IAdminAlertService adminAlertService,
            IAdminEventLogService adminEventLogService,
            IElmahHelper elmahHelper)
        {
            _clock = clock;
            _randomFactory = randomFactory;
            _dbContext = dbContext;
            _geocodeServiceConfig = geocodeServiceConfig;
            _geocodeClientFactory = geocodeClientFactory;
            _adminAlertService = adminAlertService;
            _adminEventLogService = adminEventLogService;
            _elmahHelper = elmahHelper;
        }

        public async Task<GeocodeAddressResponse> GeocodeAddress(string address, string countryId)
        {
            return await GeocodeAddress(address, countryId, 0);
        }

        public async Task<GeocodePostcodeStatus> GeocodePostcode(string postcode, string countryId)
        {
            return await GeocodePostcode(postcode, countryId, 0);
        }

        private async Task<GeocodeAddressResponse> GeocodeAddress(string address, string countryId, int retryNo)
        {
            if (retryNo > _geocodeServiceConfig.OverQueryLimitMaxRetries)
            {
                await RaiseOverQueryLimitMaxRetriesReached(AppConstant.GEOCODE_QUERY_TYPE_ADDRESS);
                return new GeocodeAddressResponse { Status = GeocodeAddressStatus.OverQueryLimitTriedMaxTimes };
            }

            var response = await Geocode(address, countryId);

            if (response == null ||
                response.Status == GeocodeStatus.InvalidRequest ||
                response.Status == GeocodeStatus.RequestDenied ||
                response.Status == GeocodeStatus.Unexpected ||
                response.Status == GeocodeStatus.UnknownError)
                return new GeocodeAddressResponse { Status = GeocodeAddressStatus.ServiceUnavailable };

            if (response.Status == GeocodeStatus.OverQueryLimit)
            {
                await Task.Delay(GetDelayWithRandomNoise());
                return await GeocodeAddress(address, countryId, retryNo + 1);
            }

            if (response.Status == GeocodeStatus.ZeroResults || response.Results.Count() == 0)
                return new GeocodeAddressResponse { Status = GeocodeAddressStatus.NoMatches };

            if (response.Results.Count() > 1)
                return new GeocodeAddressResponse { Status = GeocodeAddressStatus.MultipleMatches };

            var result = response.Results.Single();

            var inspectionOutcome = InspectGeocodeAddressResult(result, countryId);
            if (inspectionOutcome != GeocodeAddressStatus.Success)
                return new GeocodeAddressResponse { Status = inspectionOutcome };

            var addressGeometry = new AddressGeometry
            {
                Latitude = result.Geometry.Location.Latitude,
                Longitude = result.Geometry.Location.Longitude,
                ViewportNortheastLatitude = result.Geometry.Viewport.Northeast.Latitude,
                ViewportNortheastLongitude = result.Geometry.Viewport.Northeast.Longitude,
                ViewportSouthwestLatitude = result.Geometry.Viewport.Southwest.Latitude,
                ViewportSouthwestLongitude = result.Geometry.Viewport.Southwest.Longitude,
            };

            // If this was a retry but you succeeded, log it.
            if (retryNo > 0)
            {
                await LogOverQueryLimitSuccessAfterRetrying(retryNo, AppConstant.GEOCODE_QUERY_TYPE_ADDRESS);
            }

            return new GeocodeAddressResponse
            {
                Status = GeocodeAddressStatus.Success,
                Geometry = addressGeometry
            };
        }

        private async Task<GeocodePostcodeStatus> GeocodePostcode(string postcode, string countryId, int retryNo)
        {
            // Terminate the recursion if needed.
            if (retryNo > _geocodeServiceConfig.OverQueryLimitMaxRetries)
            {
                await RaiseOverQueryLimitMaxRetriesReached(AppConstant.GEOCODE_QUERY_TYPE_POSTCODE);
                return GeocodePostcodeStatus.OverQueryLimitTriedMaxTimes;
            }

            // Check if we have already geocoded the postcode.
            var existingPostcodeGeometry = await _dbContext.PostcodeGeometries
                .FindAsync(countryId, postcode);
            if (existingPostcodeGeometry != null)
                return GeocodePostcodeStatus.Success;

            // This is a new postcode, geocode it!
            var response = await Geocode(postcode, countryId);

            if (response == null ||
                response.Status == GeocodeStatus.InvalidRequest ||
                response.Status == GeocodeStatus.RequestDenied ||
                response.Status == GeocodeStatus.Unexpected ||
                response.Status == GeocodeStatus.UnknownError)
                return GeocodePostcodeStatus.ServiceUnavailable;

            if (response.Status == GeocodeStatus.OverQueryLimit)
            {
                await Task.Delay(GetDelayWithRandomNoise());
                return await GeocodePostcode(postcode, countryId, retryNo + 1);
            }

            if (response.Status == GeocodeStatus.ZeroResults || response.Results.Count() == 0)
                return GeocodePostcodeStatus.NoMatches;

            if (response.Results.Count() > 1)
                return GeocodePostcodeStatus.MultipleMatches;

            var result = response.Results.Single();

            var inspectionOutcome = InspectGeocodePostcodeResult(result, countryId);
            if (inspectionOutcome != GeocodePostcodeStatus.Success)
                return inspectionOutcome;

            var postcodeGeometry = new PostcodeGeometry
            {
                CountryId = countryId,
                Postcode = postcode,
                Latitude = result.Geometry.Location.Latitude,
                Longitude = result.Geometry.Location.Longitude,
                ViewportNortheastLatitude = result.Geometry.Viewport.Northeast.Latitude,
                ViewportNortheastLongitude = result.Geometry.Viewport.Northeast.Longitude,
                ViewportSouthwestLatitude = result.Geometry.Viewport.Southwest.Latitude,
                ViewportSouthwestLongitude = result.Geometry.Viewport.Southwest.Longitude,
            };
            _dbContext.PostcodeGeometries.Add(postcodeGeometry);
            await _dbContext.SaveChangesAsync();

            // If this was a retry but you succeeded, log it.
            if (retryNo > 0)
            {
                await LogOverQueryLimitSuccessAfterRetrying(retryNo, AppConstant.GEOCODE_QUERY_TYPE_POSTCODE);
            }

            return GeocodePostcodeStatus.Success;
        }
        
        private async Task RaiseOverQueryLimitMaxRetriesReached(string queryType)
        {
            _adminAlertService.SendAlert(AdminAlertKey.GoogleGeocodeApiStatusOverQueryLimitMaxRetriesReached);
            var extraInfo = new Dictionary<string, object>
            {
                { "QueryType", queryType },
                { "MaximumRetries", _geocodeServiceConfig.OverQueryLimitMaxRetries }
            };
            await _adminEventLogService.Log(AdminEventLogKey.GoogleGeocodeApiStatusOverQueryLimitMaxRetriesReached, extraInfo);
        }

        private async Task LogOverQueryLimitSuccessAfterRetrying(int retryNo, string queryType)
        {
            var extraInfo = new Dictionary<string, object>
            {
                { "QueryType", queryType },
                { "RetriesUntilSuccess", retryNo }
            };
            await _adminEventLogService.Log(AdminEventLogKey.GoogleGeocodeApiStatusOverQueryLimitSuccessAfterRetrying, extraInfo);
        }

        private GeocodeAddressStatus InspectGeocodeAddressResult(GeocodeResult result, string countryId)
        {
            var countryComponent = result.AddressComponents.SingleOrDefault(x =>
                x.Types.Any(t => t.ToLowerInvariant().Equals(TYPE_COUNTRY)));
            if (countryComponent.ShortName.ToLowerInvariant() != countryId.ToLowerInvariant())
            {
                return GeocodeAddressStatus.ResultInWrongCountry;
            }

            return GeocodeAddressStatus.Success;
        }

        private GeocodePostcodeStatus InspectGeocodePostcodeResult(GeocodeResult result, string countryId)
        {
            var countryComponent = result.AddressComponents.SingleOrDefault(x => 
                x.Types.Any(t => t.ToLowerInvariant().Equals(TYPE_COUNTRY)));
            if (countryComponent.ShortName.ToLowerInvariant() != countryId.ToLowerInvariant())
            {
                return GeocodePostcodeStatus.ResultInWrongCountry;
            }
            
            var isPostalCodeType = result.Types.Any(t => t.ToLowerInvariant().Equals(TYPE_POSTAL_CODE));
            var isPostalCodePrefixType = result.Types.Any(t => t.ToLowerInvariant().Equals(TYPE_POSTAL_CODE_PREFIX));

            if (!isPostalCodeType || isPostalCodePrefixType)
            {
                return GeocodePostcodeStatus.ResultWithWrongType;
            }

            return GeocodePostcodeStatus.Success;
        }

        private async Task<GeocodeResponse> Geocode(string address, string countryId)
        {
            try {
                var geocodeClient = _geocodeClientFactory.Create(_geocodeServiceConfig.GoogleApiServerKey);
                var response = await geocodeClient.GeocodeAddress(address, countryId);

                if (response == null)
                    return null;

                switch (response.Status)
                {
                    case GeocodeStatus.InvalidRequest:
                        _adminAlertService.SendAlert(AdminAlertKey.GoogleGeocodeApiStatusInvalidRequest);
                        var extraInfo = new Dictionary<string, object>
                        {
                            { "Address", address }, { "Region", countryId }
                        };
                        await _adminEventLogService.Log(AdminEventLogKey.GoogleGeocodeApiStatusInvalidRequest, extraInfo);
                        return response;
                    case GeocodeStatus.RequestDenied:
                        _adminAlertService.SendAlert(AdminAlertKey.GoogleGeocodeApiStatusRequestDenied);
                        await _adminEventLogService.Log(AdminEventLogKey.GoogleGeocodeApiStatusRequestDenied, null);
                        return response;
                    case GeocodeStatus.Unexpected:
                        _adminAlertService.SendAlert(AdminAlertKey.GoogleGeocodeApiStatusUnexpected);
                        await _adminEventLogService.Log(AdminEventLogKey.GoogleGeocodeApiStatusUnexpected, null);
                        return response;
                    case GeocodeStatus.UnknownError:
                        _adminAlertService.SendAlert(AdminAlertKey.GoogleGeocodeApiStatusUknownError);
                        await _adminEventLogService.Log(AdminEventLogKey.GoogleGeocodeApiStatusUknownError, null);
                        return response;
                    default:
                        return response;
                }
            }
            catch(Exception ex)
            {
                _elmahHelper.Raise(ex);
                _adminAlertService.SendAlert(AdminAlertKey.GoogleGeocodeApiClientException);
            }

            return null;
        }

        private TimeSpan GetDelayWithRandomNoise()
        {
            var random = _randomFactory.Create(_clock.OffsetNow.Millisecond);
            var delayMilliseconds = _geocodeServiceConfig.OverQueryLimitDelayBetweenRetries.Milliseconds;
            var randomMilliseconds = (0.75 + 0.5 * random.NextDouble()) * delayMilliseconds;
            return TimeSpan.FromMilliseconds(randomMilliseconds);
        }
    }
}
