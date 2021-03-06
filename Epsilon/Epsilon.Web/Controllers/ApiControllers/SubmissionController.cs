﻿using Epsilon.Logic.JsonModels;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Web.Controllers.BaseControllers;
using System.Threading.Tasks;
using System.Web.Http;

namespace Epsilon.Web.Controllers.ApiControllers
{
    public class SubmissionController : BaseApiController
    {
        private readonly ITenancyDetailsSubmissionService _tenancyDetailsSubmissionService;

        public SubmissionController(
            ITenancyDetailsSubmissionService tenancyDetailsSubmissionService)
        {
            _tenancyDetailsSubmissionService = tenancyDetailsSubmissionService;
        }

        [HttpPost]
        public async Task<MySubmissionsSummaryResponse> MySubmissionsSummary(MySubmissionsSummaryRequest request)
        {
            if (request.allowCaching)
                return await _tenancyDetailsSubmissionService.GetUserSubmissionsSummaryWithCaching(GetUserId(), request.limitItemsReturned);

            return await _tenancyDetailsSubmissionService.GetUserSubmissionsSummary(GetUserId(), request.limitItemsReturned);
        }
    }
}
