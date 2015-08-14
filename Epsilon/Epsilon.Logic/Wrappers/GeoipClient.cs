using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Models;
using Epsilon.Logic.Wrappers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Epsilon.Logic.FSharp.GeoipProvider;
using Epsilon.Logic.Helpers;

namespace Epsilon.Logic.Wrappers
{
    // TODO_PANOS_TEST
    public class GeoipClient : IGeoipClient
    {
        private readonly IWebClientFactory _webClientFactory;

        public GeoipClient(
            IWebClientFactory webClientFactory)
        {
            _webClientFactory = webClientFactory;
        }

        public async Task<GeoipClientResponse> Geoip(GeoipProviderName providerName, string ipAddress)
        {
            try {
                var rawResponse = await GetRawResponse(providerName, ipAddress);

                if (rawResponse.Status == WebClientResponseStatus.Success)
                {

                    var parsedResponse = ParseResponse(providerName, rawResponse.Response);

                    var thisResponse = GeoipClientResponse.FromProviderClientResponse(parsedResponse);
                    thisResponse.RawResponse = rawResponse.Response;
                    thisResponse.GeoipProviderName = providerName;
                    thisResponse.Status = WebClientResponseStatus.Success;

                    return thisResponse;
                }
                else
                {
                    return new GeoipClientResponse
                    {
                        Status = rawResponse.Status,
                        ErrorMessage = rawResponse.ErrorMessage,
                        GeoipProviderName = providerName
                    };
                }
            }
            catch (Exception ex)
            {
                ElmahHelper.Raise(ex);
                return new GeoipClientResponse
                {
                    Status = WebClientResponseStatus.Error,
                    ErrorMessage = ex.Message,
                    GeoipProviderName = providerName
                };
            }
        }

        private async Task<WebClientResponse> GetRawResponse(GeoipProviderName providerName, string ipAddress)
        {
            string url = string.Empty;
            // EnumSwitch:GeoipProviderName
            switch (providerName)
            {
                case GeoipProviderName.Freegeoip:
                    url = string.Format(@"https://freegeoip.net/json/{0}", ipAddress);
                    break;
                case GeoipProviderName.Telize:
                    url = string.Format(@"https://www.telize.com/geoip/{0}", ipAddress);
                    break;
                default:
                    throw new NotImplementedException(string.Format("Unexpected GeoipProviderName: '{0}'",
                        EnumsHelper.GeoipProviderName.ToString(providerName)));
            }

            var timeoutInMilliseconds = 2000.0; // TODO_PANOS: put in config

            var webClient = _webClientFactory.Create();
            var response = await webClient.DownloadStringTaskAsync(url, timeoutInMilliseconds);

            return response;
        }

        private GeoipProviderClientResponse ParseResponse(GeoipProviderName providerName, string rawResponse)
        {
            // EnumSwitch:GeoipProviderName
            switch (providerName)
            {
                case GeoipProviderName.Freegeoip:
                    return FreegeoipGeoipProviderClient.ParseResponse(rawResponse);
                case GeoipProviderName.Telize:
                    return TelizeGeoipProviderClient.ParseResponse(rawResponse);
                default:
                    throw new NotImplementedException(string.Format("Unexpected GeoipProviderName: '{0}'",
                        EnumsHelper.GeoipProviderName.ToString(providerName)));
            }
        }
    }
}
