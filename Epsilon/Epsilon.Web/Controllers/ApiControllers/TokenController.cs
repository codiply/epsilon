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
    public class TokenController : BaseApiController
    {
        private readonly IUserTokenService _userTokenService;

        public TokenController(
            IUserTokenService userTokenService)
        {
            _userTokenService = userTokenService;
        }

        [HttpGet]
        public async Task<TokenBalanceResponse> Balance()
        {
            return await _userTokenService.GetBalance(GetUserId());
        }

        [HttpPost]
        public async Task<MyTokenTransactionsPageResponse> MyTokenTransactionsNextPage(MyTokenTransactionsPageRequest request)
        {
            return await _userTokenService.GetMyTokenTransactionsNextPage(GetUserId(), request);
        }
    }
}
