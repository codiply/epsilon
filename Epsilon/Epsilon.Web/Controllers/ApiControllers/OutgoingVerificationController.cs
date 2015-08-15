using Epsilon.Logic.JsonModels;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Web.Controllers.BaseControllers;
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
        public async Task<MyOutgoingVerificationsSummaryResponse> MyOutgoingVerificationsSummary(
            MyOutgoingVerificationsSummaryRequest request)
        {
            if (request.allowCaching)
                return await _outgoingVerificationService.GetUserOutgoingVerificationsSummaryWithCaching(GetUserId(), request.limitItemsReturned);

            return await _outgoingVerificationService.GetUserOutgoingVerificationsSummary(GetUserId(), request.limitItemsReturned);
        }
    }
}
