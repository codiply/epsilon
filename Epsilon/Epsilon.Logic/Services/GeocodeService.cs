using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
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
        private const string TYPE_POSTCODE = "postcode";
        private const string TYPE_ADDRESS = "address";

        private readonly IEpsilonContext _dbContext;
        private readonly IGeocodeServiceConfig _geocodeServiceConfig;
        private readonly IGeocodeClientFactory _geocodeClientFactory;
        private readonly IAdminAlertService _adminAlertService;
        private readonly IAdminEventLogService _adminEventLogService;

        public GeocodeService(
            IEpsilonContext dbContext,
            IGeocodeServiceConfig geocodeServiceConfig,
            IGeocodeClientFactory geocodeClientFactory,
            IAdminAlertService adminAlertService,
            IAdminEventLogService adminEventLogService)
        {
            _dbContext = dbContext;
            _geocodeServiceConfig = geocodeServiceConfig;
            _geocodeClientFactory = geocodeClientFactory;
            _adminAlertService = adminAlertService;
            _adminEventLogService = adminEventLogService;
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
                await RaiseOverQueryLimitMaxRetriesReached(TYPE_ADDRESS);
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
                await Task.Delay(_geocodeServiceConfig.OverQueryLimitDelayBetweenRetries);
                return await GeocodeAddress(address, countryId, retryNo + 1);
            }

            if (response.Status == GeocodeStatus.ZeroResults || response.Results.Count() == 0)
                return new GeocodeAddressResponse { Status = GeocodeAddressStatus.NoMatches };

            if (response.Results.Count() > 1)
                return new GeocodeAddressResponse { Status = GeocodeAddressStatus.MultipleMatches };

            var result = response.Results.Single();
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
                await LogOverQueryLimitSuccessAfterRetrying(retryNo, TYPE_ADDRESS);
            }

            return new GeocodeAddressResponse
            {
                Status = GeocodeAddressStatus.Success,
                Geometry = addressGeometry
            };
        }

        private async Task<GeocodePostcodeStatus> GeocodePostcode(string postcode, string countryId, int retryNo)
        {
            if (retryNo > _geocodeServiceConfig.OverQueryLimitMaxRetries)
            {
                await RaiseOverQueryLimitMaxRetriesReached(TYPE_POSTCODE);
                return GeocodePostcodeStatus.OverQueryLimitTriedMaxTimes;
            }

            var response = await Geocode(postcode, countryId);

            if (response == null ||
                response.Status == GeocodeStatus.InvalidRequest ||
                response.Status == GeocodeStatus.RequestDenied ||
                response.Status == GeocodeStatus.Unexpected ||
                response.Status == GeocodeStatus.UnknownError)
                return GeocodePostcodeStatus.ServiceUnavailable;

            if (response.Status == GeocodeStatus.OverQueryLimit)
            {
                await Task.Delay(_geocodeServiceConfig.OverQueryLimitDelayBetweenRetries);
                return await GeocodePostcode(postcode, countryId, retryNo + 1);
            }

            if (response.Status == GeocodeStatus.ZeroResults || response.Results.Count() == 0)
                return GeocodePostcodeStatus.NoMatches;

            if (response.Results.Count() > 1)
                return GeocodePostcodeStatus.MultipleMatches;

            var result = response.Results.Single();
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
                await LogOverQueryLimitSuccessAfterRetrying(retryNo, TYPE_POSTCODE);
            }

            return GeocodePostcodeStatus.Success;
        }
        
        private async Task RaiseOverQueryLimitMaxRetriesReached(string type)
        {
            _adminAlertService.SendAlert(AdminAlertKey.GooglGeocodeApiStatusOverQueryLimitMaxRetriesReached);
            var extraInfo = string.Format("Maximum retries: {0} Type: {1}", _geocodeServiceConfig.OverQueryLimitMaxRetries, type);
            await _adminEventLogService.Log(AdminEventLogKey.GooglGeocodeApiStatusOverQueryLimitMaxRetriesReached, extraInfo);
        }

        private async Task LogOverQueryLimitSuccessAfterRetrying(int retryNo, string type)
        {
            var extraInfo = string.Format("Retries until success: {0} Type: {1}", retryNo, type);
            await _adminEventLogService.Log(AdminEventLogKey.GooglGeocodeApiStatusOverQueryLimitSuccessAfterRetrying, extraInfo);
        }

        private async Task<GeocodeResponse> Geocode(string address, string region)
        {
            try {
                var geocodeClient = _geocodeClientFactory.Create(_geocodeServiceConfig.GoogleApiServerKey);
                var response = await geocodeClient.GeocodeAddress(address, region);

                if (response == null)
                    return null;

                switch (response.Status)
                {
                    case GeocodeStatus.InvalidRequest:
                        _adminAlertService.SendAlert(AdminAlertKey.GoogleGeocodeApiStatusInvalidRequest);
                        var extraInfo = string.Format("Address:'{0}' Region: '{1}'", address, region);
                        await _adminEventLogService.Log(AdminEventLogKey.GoogleGeocodeApiStatusInvalidRequest, extraInfo);
                        return response;
                    case GeocodeStatus.RequestDenied:
                        _adminAlertService.SendAlert(AdminAlertKey.GoogleGeocodeApiStatusRequestDenied);
                        await _adminEventLogService.Log(AdminEventLogKey.GoogleGeocodeApiStatusRequestDenied,"");
                        return response;
                    case GeocodeStatus.Unexpected:
                        _adminAlertService.SendAlert(AdminAlertKey.GoogleGeocodeApiStatusUnexpected);
                        await _adminEventLogService.Log(AdminEventLogKey.GoogleGeocodeApiStatusUnexpected, "");
                        return response;
                    case GeocodeStatus.UnknownError:
                        _adminAlertService.SendAlert(AdminAlertKey.GoogleGeocodeApiStatusUknownError);
                        await _adminEventLogService.Log(AdminEventLogKey.GoogleGeocodeApiStatusUknownError, "");
                        return response;
                    default:
                        return response;
                }
            }
            catch(Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                _adminAlertService.SendAlert(AdminAlertKey.GoogleGeocodeApiClientException);
            }

            return null;
        }
    }
}
