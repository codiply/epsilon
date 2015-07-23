using Epsilon.Logic.JsonModels;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Web.Controllers.BaseControllers;
using Epsilon.Web.Controllers.Filters.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        public async Task<AddressSearchResponse> Search(AddressSearchRequest request)
        {
            return await _addressService.Search(request);
        }

        [HttpPost]
        public async Task<AddressGeometryResponse> Geometry(AddressGeometryRequest request)
        {
            return await _addressService.GetGeometryViaUniqueId(request.uniqueId);
        }
    }
}
