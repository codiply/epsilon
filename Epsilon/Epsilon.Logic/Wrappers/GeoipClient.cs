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
                string rawResponse = await GetRawResponse(providerName, ipAddress);

                var parsedResponse = ParseResponse(providerName, rawResponse);

                var thisResponse = GeoipClientResponse.FromProviderClientResponse(parsedResponse);
                thisResponse.RawResponse = rawResponse;
                thisResponse.GeoipProviderName = providerName;
                thisResponse.Status = GeoipClientResponseStatus.Succcess;

                return thisResponse;
            }
            catch (WebClientTimeoutException ex)
            {
                //Elmah.ErrorSignal.FromCurrentContext().Raise(ex);

                return new GeoipClientResponse
                {
                    Status = GeoipClientResponseStatus.Timeout,
                    GeoipProviderName = providerName
                };
            }
            catch (Exception ex)
            {
                //Elmah.ErrorSignal.FromCurrentContext().Raise(ex);

                return new GeoipClientResponse
                {
                    Status = GeoipClientResponseStatus.Failure,
                    GeoipProviderName = providerName
                };
            }
        }

        private async Task<string> GetRawResponse(GeoipProviderName providerName, string ipAddress)
        {
            string url = string.Empty;
            // EnumSwitch:GeoipProviderName
            switch (providerName)
            {
                case GeoipProviderName.Freegeoip:
                    url = string.Format(@"https://freegeoip.netSS/json/%s", ipAddress);
                    break;
                case GeoipProviderName.Telize:
                    url = string.Format(@"https://www.telize.com/geoip/%s", ipAddress);
                    break;
                default:
                    throw new NotImplementedException(string.Format("Unexpected GeoipProviderName: '{0}'",
                        EnumsHelper.GeoipProviderName.ToString(providerName)));
            }

            var timeoutInMilliseconds = 1000.0; // TODO_PANOS: from config

            var webClient = _webClientFactory.Create();
            var rawResponse = await webClient.DownloadStringTaskAsync(url, timeoutInMilliseconds);

            return rawResponse;
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
