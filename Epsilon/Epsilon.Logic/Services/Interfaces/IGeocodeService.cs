using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using GeocodeSharp.Google;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    public class GeocodeAddressResponse
    {
        public GeocodeAddressStatus Status { get; set; }
        public AddressGeometry Geometry { get; set; }
    }

    public interface IGeocodeService
    {
        Task<GeocodeAddressResponse> GeocodeAddress(string address, string region);

        Task<GeocodePostcodeStatus> GeocodePostcode(string postcode, string countryId);
    }
}
