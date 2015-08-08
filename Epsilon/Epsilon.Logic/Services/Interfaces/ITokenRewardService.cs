using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using Epsilon.Logic.JsonModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
