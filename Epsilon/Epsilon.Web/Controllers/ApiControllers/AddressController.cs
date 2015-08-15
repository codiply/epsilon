using Epsilon.Logic.JsonModels;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Web.Controllers.BaseControllers;
using System.Threading.Tasks;
using System.Web.Http;

namespace Epsilon.Web.Controllers.ApiControllers
{
    public class AddressController : BaseApiController
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        [HttpPost]
        public async Task<AddressSearchResponse> SearchAddress(AddressSearchRequest request)
        {
            return await _addressService.SearchAddress(request);
        }

        [HttpPost]
        public async Task<PropertySearchResponse> SearchProperty(PropertySearchRequest request)
        {
            return await _addressService.SearchProperty(request);
        }

        [HttpPost]
        public async Task<AddressGeometryResponse> Geometry(AddressGeometryRequest request)
        {
            return await _addressService.GetGeometry(request.uniqueId);
        }
    }
}
