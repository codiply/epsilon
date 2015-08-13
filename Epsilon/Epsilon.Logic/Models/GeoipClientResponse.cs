using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.FSharp.GeoipProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Models
{
    public class GeoipClientResponse
    {
        public GeoipClientResponseStatus Status { get; set; }

        public string RawResponse { get; set; }

        public string CountryCode { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public GeoipProviderName GeoipProviderName { get; set; }

        static public GeoipClientResponse FromProviderClientResponse(GeoipProviderClientResponse providerClientResponse)
        {
            // TODO_PANOS_TEST
            return new GeoipClientResponse
            {
                CountryCode = providerClientResponse.CountryCode,
                Latitude = providerClientResponse.Latitude,
                Longitude = providerClientResponse.Longitude
            };
        }
    }
}
