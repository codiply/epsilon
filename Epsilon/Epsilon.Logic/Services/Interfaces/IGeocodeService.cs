using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
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
        Task<GeocodeAddressResponse> GeocodeAddress(string address, string countryId);

        Task<GeocodePostcodeStatus> GeocodePostcode(string postcode, string countryId);
    }
}
