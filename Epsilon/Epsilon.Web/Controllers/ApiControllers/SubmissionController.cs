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
    public class SubmissionController : BaseApiController
    {
        private readonly ITenancyDetailsSubmissionService _tenancyDetailsSubmissionService;

        public SubmissionController(
            ITenancyDetailsSubmissionService tenancyDetailsSubmissionService)
        {
            _tenancyDetailsSubmissionService = tenancyDetailsSubmissionService;
        }

        [HttpGet]
        public async Task<UserTenancyDetailsSubmissionInfo> MySubmissionInfo()
        {
            return await _tenancyDetailsSubmissionService.GetUserSubmissionInfo(GetUserId());
        }
    }
}
