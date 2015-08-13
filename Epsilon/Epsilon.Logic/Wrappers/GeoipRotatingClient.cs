using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants.Enums;
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
    public class GeoipRotatingClient : IGeoipRotatingClient
    {
        private readonly IGeoipRotatingClientConfig _geoipRotatingClientConfig;
        private readonly IAdminAlertService _adminAlertService;
        private readonly IAdminEventLogService _adminEventLogService;
        private readonly IGeoipClient _geoipClient;

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
        }

        public async Task<GeoipClientResponse> Geoip(string ipAddress)
        {
            // TODO_PANOS_TEST
            return await _geoipClient.Geoip(GeoipProviderName.Freegeoip, ipAddress);
        }

        //private async Task<TalizeGeoipInfo> Geoip(string ipAddress, int retryNo = 0)
        //{
        //    try
        //    {
        //        // Terminate the recursion if needed.
        //        if (retryNo > _geoipInfoServiceConfig.MaxRetries)
        //        {
        //            await RaiseMaxRetriesReached();
        //            return null;
        //        }

        //        var response = await GeoipClient.getResponse(ipAddress);
        //        var info = GeoipClient.parseResponse(response);

        //        // If this was a retry but you succeeded, log it.
        //        if (retryNo > 0)
        //        {
        //            await LogOverQueryLimitSuccessAfterRetrying(retryNo);
        //        }

        //        return info;
        //    }
        //    catch (Exception ex)
        //    {
        //        Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
        //        return await Geoip(ipAddress, retryNo + 1);
        //    }
        //}

        //private async Task RaiseMaxRetriesReached()
        //{
        //    _adminAlertService.SendAlert(AdminAlertKey.GoogleGeocodeApiStatusOverQueryLimitMaxRetriesReached);
        //    var extraInfo = new Dictionary<string, object>
        //    {
        //        { "MaximumRetries", _geoipInfoServiceConfig.MaxRetries }
        //    };
        //    await _adminEventLogService.Log(AdminEventLogKey.GeoipApiMaxRetriesReached, extraInfo);
        //}

        //private async Task LogOverQueryLimitSuccessAfterRetrying(int retryNo)
        //{
        //    var extraInfo = new Dictionary<string, object>
        //    {
        //        { "RetriesUntilSuccess", retryNo }
        //    };
        //    await _adminEventLogService.Log(AdminEventLogKey.GeoipApiSuccessAfterRetrying, extraInfo);
        //}
    }
}
