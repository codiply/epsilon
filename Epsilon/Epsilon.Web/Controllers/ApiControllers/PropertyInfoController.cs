using Epsilon.Logic.JsonModels;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Web.Controllers.BaseControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Epsilon.Web.Controllers.ApiControllers
{
    public class PropertyInfoController : BaseApiController
    {
        private readonly IPropertyInfoAccessService _propertyInfoAccessService;

        public PropertyInfoController(
            IPropertyInfoAccessService propertyInfoAccessService)
        {
            _propertyInfoAccessService = propertyInfoAccessService;
        }

        [HttpPost]
        public async Task<MyExploredPropertiesSummaryResponse> MyExploredPropertiesSummary(MyExploredPropertiesSummaryRequest request)
        {
            if (request.allowCaching)
                return await _propertyInfoAccessService.GetUserExploredPropertiesSummaryWithCaching(GetUserId(), request.limitItemsReturned);

            return await _propertyInfoAccessService.GetUserExploredPropertiesSummary(GetUserId(), request.limitItemsReturned);
        }
    }
}
