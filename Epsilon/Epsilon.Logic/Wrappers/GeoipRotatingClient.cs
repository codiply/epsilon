using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Models;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Wrappers
{
    // TODO_TEST_PANOS
    public class GeoipRotatingClient : IGeoipRotatingClient
    {
        private readonly IGeoipRotatingClientConfig _geoipRotatingClientConfig;
        private readonly IAdminAlertService _adminAlertService;
        private readonly IAdminEventLogService _adminEventLogService;
        private readonly IGeoipClient _geoipClient;

        private readonly IList<GeoipProviderName> _providerNames = new List<GeoipProviderName>();

        public GeoipRotatingClient(
            IGeoipRotatingClientConfig geoipRotatingClientConfig,
            IAdminAlertService adminAlertService,
            IAdminEventLogService adminEventLogService,
            IGeoipClient geoipClient)
        {
            _geoipRotatingClientConfig = geoipRotatingClientConfig;
            _adminAlertService = adminAlertService;
            _adminEventLogService = adminEventLogService;
            _geoipClient = geoipClient;

            foreach (var p in _geoipRotatingClientConfig.ProviderRotation.Split(','))
            {
                var providerName = EnumsHelper.GeoipProviderName.Parse(p);
                if (!providerName.HasValue)
                {
                    throw new Exception(string.Format("Cannot parse GeoipProviderName {0}.", p));
                }

                _providerNames.Add(providerName.Value);
            }
        }

        public async Task<GeoipClientResponse> Geoip(string ipAddress, int rotationNo = 1)
        {
            bool hasFailedOnce = false;
            // TODO_TEST_PANOS
            foreach (var providerName in _providerNames)
            {
                var response = await _geoipClient.Geoip(providerName, ipAddress);
                if (response.Status == WebClientResponseStatus.Success)
                {
                    if (hasFailedOnce)
                    {
                        await LogSucccessAfterFailures(providerName, rotationNo);
                    }
                    return response;
                }
                else
                {
                    hasFailedOnce = true;
                    await RaiseGeoipClientFailed(providerName, response.Status, response.ErrorMessage);
                }
            }

            if (rotationNo < _geoipRotatingClientConfig.MaxRotations)
                return await Geoip(ipAddress, rotationNo + 1);

            await RaiseMaxRotationsReached();

            return new GeoipClientResponse
            {
                Status = WebClientResponseStatus.Error
            };
        }

        private async Task RaiseMaxRotationsReached()
        {
            _adminAlertService.SendAlert(AdminAlertKey.GeoipRotatingClientMaxRotationsReached);
            var extraInfo = new Dictionary<string, object>
            {
                { "MaxRotations", _geoipRotatingClientConfig.MaxRotations }
            };
            await _adminEventLogService.Log(AdminEventLogKey.GeoipRotatingClientMaxRotationsReached, extraInfo);
        }

        private async Task RaiseGeoipClientFailed(GeoipProviderName providerName, WebClientResponseStatus responseStatus, string errorMessage)
        {
            _adminAlertService.SendAlert(AdminAlertKey.GeoipRotatingClientProviderFailed(providerName, responseStatus));
            var extraInfo = new Dictionary<string, object>
            {
                { "GeoipProviderName", EnumsHelper.GeoipProviderName.ToString(providerName) },
                { "ResponseStatus", EnumsHelper.WebClientResponseStatus.ToString(responseStatus) },
                { "ErrorMessage", errorMessage }
            };
            await _adminEventLogService.Log(AdminEventLogKey.GeoipClientFailure, extraInfo);
        }

        private async Task LogSucccessAfterFailures(GeoipProviderName successfulProviderName, int rotationNo)
        {
            var extraInfo = new Dictionary<string, object>
            {
                { "SuccessfulGeoipProviderName", EnumsHelper.GeoipProviderName.ToString(successfulProviderName) },
                { "RotationNo", rotationNo }
            };
            await _adminEventLogService.Log(AdminEventLogKey.GeoipRotatingClientSuccessAfterFailures, extraInfo);
        }
    }
}
