using Epsilon.Logic.Helpers.Interfaces;
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
        private readonly ITokenRewardMetadataHelper _tokenRewardMetadataHelper;

        public TokenController(
            IUserTokenService userTokenService,
            ITokenRewardMetadataHelper tokenRewardMetadataHelper)
        {
            _userTokenService = userTokenService;
            _tokenRewardMetadataHelper = tokenRewardMetadataHelper;
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

        public TokenRewardMetadata GetAllTokenRewardMetadata()
        {
            return _tokenRewardMetadataHelper.GetAll();
        }
    }
}
