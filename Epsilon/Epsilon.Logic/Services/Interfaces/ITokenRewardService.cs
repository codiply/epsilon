using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using Epsilon.Logic.JsonModels;

namespace Epsilon.Logic.Services.Interfaces
{
    public interface ITokenRewardService
    {
        TokenRewardsSummaryResponse GetTokenRewardsSummary();

        TokenRewardMetadata GetAllTokenRewardMetadata();

        TokenRewardScheme GetCurrentScheme();

        TokenReward GetCurrentReward(TokenRewardKey key);
    }
}
