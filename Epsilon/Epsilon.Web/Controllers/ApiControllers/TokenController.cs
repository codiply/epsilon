using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.JsonModels;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Web.Controllers.BaseControllers;
using System.Threading.Tasks;
using System.Web.Http;

namespace Epsilon.Web.Controllers.ApiControllers
{
    public class TokenController : BaseApiController
    {
        private readonly IUserTokenService _userTokenService;
        private readonly ITokenRewardService _tokenRewardService;

        public TokenController(
            IUserTokenService userTokenService,
            ITokenRewardMetadataHelper tokenRewardMetadataHelper,
            ITokenRewardService tokenRewardService)
        {
            _userTokenService = userTokenService;
            _tokenRewardService = tokenRewardService;
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

        [HttpGet]
        public TokenRewardMetadata TokenRewardMetadata()
        {
            return _tokenRewardService.GetAllTokenRewardMetadata();
        }

        [HttpGet]
        public TokenRewardsSummaryResponse TokenRewardsSummary()
        {
            return _tokenRewardService.GetTokenRewardsSummary();
        }
    }
}
