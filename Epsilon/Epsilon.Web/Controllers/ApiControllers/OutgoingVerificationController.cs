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
    public class OutgoingVerificationController : BaseApiController
    {
        private readonly IOutgoingVerificationService _outgoingVerificationService;

        public OutgoingVerificationController(
            IOutgoingVerificationService outgoingVerificationService)
        {
            _outgoingVerificationService = outgoingVerificationService;
        }

        [HttpPost]
        public async Task<MyOutgoingVerificationsSummaryResponse> MyOutgoingVerificationsSummary(MyOutgoingVerificationsSummaryRequest request)
        {
            return await _outgoingVerificationService.GetUserOutgoingVerificationsSummary(GetUserId(), request);
        }
    }
}
