using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.JsonModels;

namespace Epsilon.Logic.Services
{
    public class TokenRewardService : ITokenRewardService
    {
        private readonly IClock _clock;
        private readonly ICommonConfig _commonConfig;
        private readonly IAppCache _appCache;
        private readonly IEpsilonContext _dbContext;
        private readonly ITokenRewardMetadataHelper _tokenRewardMetadataHelper;

        public TokenRewardService(
            IClock clock,
            ICommonConfig commonConfig,
            IAppCache appCache,
            IEpsilonContext dbContext,
            ITokenRewardMetadataHelper tokenRewardMetadataHelper)
        {
            _clock = clock;
            _commonConfig = commonConfig;
            _appCache = appCache;
            _dbContext = dbContext;
            _tokenRewardMetadataHelper = tokenRewardMetadataHelper;
        }

        public async Task<TokenRewardsSummaryResponse> GetTokenRewardsSummary()
        {
            // TODO_PANOS_TEST: all
            var currentScheme = GetCurrentScheme();
            var rewards = currentScheme.Rewards
                .Select(r => new { TypeKey = r.TypeKeyAsEnum, Reward = r })
                .Where(x => x.TypeKey.HasValue)
                .Select(x => new { EarnOrSpend = x.TypeKey.Value.EarnOrSpend(), Reward = x.Reward });

            var earnRewards = rewards.Where(x => x.EarnOrSpend == TokenRewardKeyType.Earn).Select(x => x.Reward);
            var spendRewards = rewards.Where(x => x.EarnOrSpend == TokenRewardKeyType.Spend).Select(x => x.Reward);

            return new TokenRewardsSummaryResponse
            {
                typeMetadata = _tokenRewardMetadataHelper.GetAll(),
                earnTypeValues = earnRewards.Select(x => x.ToTokeRewardTypeValue()).ToList(),
                spendTypeValues = spendRewards.Select(x => x.ToTokeRewardTypeValue()).ToList()
            };
        }

        public TokenRewardMetadata GetAllTokenRewardMetadata()
        {
            // TODO_PANOS_TEST:
            return new TokenRewardMetadata
            {
                typeMetadata = _tokenRewardMetadataHelper.GetAll()
            };
        }

        public TokenRewardScheme GetCurrentScheme()
        {
            // TODO_PANOS_TEST
            var now = _clock.OffsetNow;
            return _appCache.Get(AppCacheKey.CURRENT_TOKEN_REWARD_SCHEME, 
                () => {
                    var currentScheme = _dbContext.TokenRewardSchemes
                        .Include(x => x.Rewards)
                        .OrderByDescending(x => x.EffectiveFrom)
                        .First(x => x.EffectiveFrom < now);
                    return currentScheme;
                }, 
                ignoredCurrentScheme => {
                    var nextScheme = _dbContext.TokenRewardSchemes
                        .Include(x => x.Rewards)
                        .OrderBy(x => x.EffectiveFrom)
                        .FirstOrDefault(x => x.EffectiveFrom > now);
                    if (nextScheme == null)
                    {
                        return _commonConfig.DefaultAppCacheSlidingExpiration;
                    }
                    else
                    {
                        var slidingExpiration = nextScheme.EffectiveFrom - now;
                        return slidingExpiration;
                    }
                },
                _commonConfig.DefaultAppCacheSlidingExpiration, 
                WithLock.No);
        }

        public TokenReward GetCurrentReward(TokenRewardKey rewardKey)
        {
            // TODO_PANOS_TEST
            var currentScheme = GetCurrentScheme();
            var keyToString = EnumsHelper.TokenRewardKey.ToString(rewardKey);
            var currentReward = currentScheme.Rewards
                .SingleOrDefault(x => x.TypeKey.Equals(keyToString));
            return currentReward;
        }
    }
}
