using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.FSharp.GeoipProvider;

namespace Epsilon.Logic.Models
{
    public class GeoipClientResponse
    {
        public WebClientResponseStatus Status { get; set; }
        public string ErrorMessage { get; set; }

        public string RawResponse { get; set; }

        public string CountryCode { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public GeoipProviderName GeoipProviderName { get; set; }

        static public GeoipClientResponse FromProviderClientResponse(GeoipProviderClientResponse providerClientResponse)
        {
            return new GeoipClientResponse
            {
                CountryCode = providerClientResponse.CountryCode,
                Latitude = providerClientResponse.Latitude,
                Longitude = providerClientResponse.Longitude
            };
        }
    }
}
