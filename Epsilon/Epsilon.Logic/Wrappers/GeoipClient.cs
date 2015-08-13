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
    public class GeoipClient : IGeoipClient
    {
        public async Task<GeoipClientResponse> Geoip(GeoipProviderName providerName, string ipAddress)
        {
            try {
                string rawResponse = string.Empty;
                GeoipProviderClientResponse parsedResponse = null;

                // EnumSwitch:GeoipProviderName
                switch (providerName)
                {
                    case GeoipProviderName.Freegeoip:
                        rawResponse = await FreegeoipGeoipProviderClient.getResponse(ipAddress);
                        parsedResponse = FreegeoipGeoipProviderClient.parseResponse(rawResponse);
                        break;
                    case GeoipProviderName.Telize:
                        rawResponse = await TelizeGeoipProviderClient.getResponse(ipAddress);
                        parsedResponse = TelizeGeoipProviderClient.parseResponse(rawResponse);
                        break;
                    default:
                        throw new NotImplementedException(string.Format("Unexpected GeoipProviderName: '{0}'",
                            EnumsHelper.GeoipProviderName.ToString(providerName)));
                }

                var response = GeoipClientResponse.FromProviderClientResponse(parsedResponse);
                response.RawResponse = rawResponse;
                response.GeoipProviderName = providerName;
                response.Status = GeoipClientResponseStatus.Succcess;

                return response;
            }
            catch (Exception ex)
            {
                return new GeoipClientResponse
                {
                    Status = GeoipClientResponseStatus.Failure,
                    GeoipProviderName = providerName
                };
            }
        }
    }
}
