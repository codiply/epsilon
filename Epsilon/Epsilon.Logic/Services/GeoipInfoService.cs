using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.Models;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using System;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services
{
    public class GeoipInfoService : IGeoipInfoService
    {
        private readonly IClock _clock;
        private readonly IAppCache _appCache;
        private readonly IEpsilonContext _dbContext;
        private readonly IGeoipInfoServiceConfig _geoipInfoServiceConfig;
        private readonly IGeoipRotatingClient _geoipRotatingClient;

        public GeoipInfoService(
            IClock clock,
            IAppCache appCache,
            IEpsilonContext dbContext,
            IGeoipInfoServiceConfig geoipInfoServiceConfig,
            IGeoipRotatingClient geoipRotatingClient)
        {
            _clock = clock;
            _appCache = appCache;
            _dbContext = dbContext;
            _geoipInfoServiceConfig = geoipInfoServiceConfig;
            _geoipRotatingClient = geoipRotatingClient;
        }
        
        public GeoipInfo GetInfo(string ipAddress)
        {
            return Task.Run(async () => await GetInfoAsync(ipAddress)).Result;
        }

        public async Task<GeoipInfo> GetInfoAsync(string ipAddress)
        {
            return await _appCache.GetAsync(AppCacheKey.GetGeoipInfoForIpAddress(ipAddress),
                async () => await DoGetInfo(ipAddress), x =>
                {
                    var timeToExpiry = (x.RecordedOn + _geoipInfoServiceConfig.ExpiryPeriod) - _clock.OffsetNow;
                    return timeToExpiry;
                },
                _geoipInfoServiceConfig.ExpiryPeriod, 
                WithLock.Yes);
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

            var geoipClientResponse = await _geoipRotatingClient.Geoip(ipAddress);

            if (geoipClientResponse.Status != WebClientResponseStatus.Success)
            {
                return null;
            }

            Copy(geoipClientResponse, geoipInfo);
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

        private void Copy(GeoipClientResponse from, GeoipInfo to)
        {
            to.CountryCode = from.CountryCode;
            to.Latitude = from.Latitude;
            to.Longitude = from.Longitude;
            to.GeoipProviderName = EnumsHelper.GeoipProviderName.ToString(from.GeoipProviderName);
        }
    }
}
