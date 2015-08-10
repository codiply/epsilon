using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using Epsilon.Logic.FSharp.TelizeGeoip;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services
{
    // TODO_PANOS_TEST: whole thing
    public class GeoipInfoService : IGeoipInfoService
    {
        private IClock _clock;
        private IAppCache _appCache;
        private IEpsilonContext _dbContext;
        private IGeoipInfoServiceConfig _geoipInfoServiceConfig;
        private readonly IAdminAlertService _adminAlertService;
        private readonly IAdminEventLogService _adminEventLogService;

        public GeoipInfoService(
            IClock clock,
            IAppCache appCache,
            IEpsilonContext dbContext,
            IGeoipInfoServiceConfig geoipInfoServiceConfig,
            IAdminAlertService adminAlertService,
            IAdminEventLogService adminEventLogService)
        {
            _clock = clock;
            _appCache = appCache;
            _dbContext = dbContext;
            _geoipInfoServiceConfig = geoipInfoServiceConfig;
            _adminAlertService = adminAlertService;
            _adminEventLogService = adminEventLogService;
        }

        public async Task<GeoipInfo> GetInfo(string ipAddress)
        {
            return await _appCache.GetAsync(AppCacheKey.GetGeoipInfoForIpAddress(ipAddress),
                async () => await DoGetInfo(ipAddress), WithLock.No);
        }

        private async Task<GeoipInfo> DoGetInfo(string ipAddress)
        {
            var existingGeoipInfo = await _dbContext.GeoipInfos.FindAsync(ipAddress);
            if (existingGeoipInfo != null)
            {
                var isExpired = (_clock.OffsetNow - existingGeoipInfo.RecordedOn) > _geoipInfoServiceConfig.ExpiryPeriod;
                if (!isExpired)
                    return existingGeoipInfo;  
            }

            var geoipInfo = existingGeoipInfo == null ? new GeoipInfo() { IpAddress = ipAddress } : existingGeoipInfo;

            var geoip = await Geoip(ipAddress);
            Copy(geoip, geoipInfo);
            geoipInfo.RecordedOn = _clock.OffsetNow;

            if(existingGeoipInfo == null)
            {
                _dbContext.GeoipInfos.Add(geoipInfo);
            }
            else
            {
                _dbContext.Entry(geoipInfo).State = EntityState.Modified;
            }

            await _dbContext.SaveChangesAsync();
            return geoipInfo;
        }

        // TODO_PANOS: move this to a client class
        private async Task<TalizeGeoipInfo> Geoip(string ipAddress, int retryNo = 0)
        {
            try {
                // Terminate the recursion if needed.
                if (retryNo > _geoipInfoServiceConfig.MaxRetries)
                {
                    await RaiseMaxRetriesReached();
                    return null;
                }
                 
                var response = await GeoipClient.getResponse(ipAddress);
                var info = GeoipClient.parseResponse(response);

                // If this was a retry but you succeeded, log it.
                if (retryNo > 0)
                {
                    await LogOverQueryLimitSuccessAfterRetrying(retryNo);
                }

                return info;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return await Geoip(ipAddress, retryNo + 1);
            }
        }

        private void Copy(TalizeGeoipInfo from, GeoipInfo to)
        {
            to.CountryCode = from.CountryCode;
            to.ContinentCode = from.ContinentCode;
            to.Latitude = (double)from.Latitude;
            to.Longitude = (double)from.Longitude;
        }

        private async Task RaiseMaxRetriesReached()
        {
            _adminAlertService.SendAlert(AdminAlertKey.GoogleGeocodeApiStatusOverQueryLimitMaxRetriesReached);
            var extraInfo = new Dictionary<string, object>
            {
                { "MaximumRetries", _geoipInfoServiceConfig.MaxRetries }
            };
            await _adminEventLogService.Log(AdminEventLogKey.GeoipApiMaxRetriesReached, extraInfo);
        }

        private async Task LogOverQueryLimitSuccessAfterRetrying(int retryNo)
        {
            var extraInfo = new Dictionary<string, object>
            {
                { "RetriesUntilSuccess", retryNo }
            };
            await _adminEventLogService.Log(AdminEventLogKey.GeoipApiSuccessAfterRetrying, extraInfo);
        }
    }
}
