using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Services.Interfaces;
using GeocodeSharp.Google;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services
{
    public class GeocodingService : IGeocodingService
    {
        private readonly IGeocodingServiceConfig _geocodingServiceConfig;
        private readonly IGeocodeClientFactory _geocodeClientFactory;
        private readonly IAdminAlertService _adminAlertService;

        public GeocodingService(
            IGeocodingServiceConfig geocodingServiceConfig,
            IGeocodeClientFactory geocodeClientFactory,
            IAdminAlertService adminAlertService)
        {
            _geocodingServiceConfig = geocodingServiceConfig;
            _geocodeClientFactory = geocodeClientFactory;
            _adminAlertService = adminAlertService;
        }

        public async Task<AddressGeometry> GeocodeAddress(string address, string region)
        {
            var response = await Geocode(address, region);

            return null;
        }

        public async Task<PostcodeGeometry> GeocodePostcode(string postcode, string region)
        {
            var response = await Geocode(postcode, region);
            return null;
        }

        private async Task<GeocodeResponse> Geocode(string address, string region)
        {
            try {
                var geocodeClient = _geocodeClientFactory.Create(_geocodingServiceConfig.GoogleApiServerKey);
                var response = await geocodeClient.GeocodeAddress(address, region);
                return response;
            }
            catch(Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                _adminAlertService.SendAlert(AdminAlertKey.GoogleGeocodeApiError);
            }
            return null;
        }
    }
}
