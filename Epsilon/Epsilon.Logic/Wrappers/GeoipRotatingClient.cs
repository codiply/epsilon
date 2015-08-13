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
    // TODO_PANOS_TEST
    public class GeoipRotatingClient : IGeoipRotatingClient
    {
        private readonly IGeoipRotatingClientConfig _geoipRotatingClientConfig;
        private readonly IAdminAlertService _adminAlertService;
        private readonly IAdminEventLogService _adminEventLogService;
        private readonly IGeoipClient _geoipClient;

        private readonly IList<GeoipProviderName> _providers = new List<GeoipProviderName>();

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
                var provider = EnumsHelper.GeoipProviderName.Parse(p);
                if (!provider.HasValue)
                {
                    throw new Exception(string.Format("Cannot parse GeoipProviderName {0}.", p));
                }

                _providers.Add(provider.Value);
            }
        }

        public async Task<GeoipClientResponse> Geoip(string ipAddress, int rotationNo = 1)
        {
            // TODO_PANOS_TEST
            foreach (var provider in _providers)
            {
                var response = await _geoipClient.Geoip(provider, ipAddress);
                if (response.Status == GeoipClientResponseStatus.Succcess)
                {
                    return response;
                }
                else
                {
                    await LogGeoipClientFailure(provider, rotationNo);
                }
            }

            if (rotationNo < _geoipRotatingClientConfig.MaxRotations)
                return await Geoip(ipAddress, rotationNo + 1);

            await RaiseMaxRotationsReached();

            return new GeoipClientResponse
            {
                Status = GeoipClientResponseStatus.Failure
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

        private async Task LogGeoipClientFailure(GeoipProviderName providerName, int rotationNo)
        {
            var extraInfo = new Dictionary<string, object>
            {
                { "GeoipProviderName", EnumsHelper.GeoipProviderName.ToString(providerName) },
                { "RotationNo", rotationNo }
            };
            await _adminEventLogService.Log(AdminEventLogKey.GeoipClientFailure, extraInfo);
        }
    }
}
