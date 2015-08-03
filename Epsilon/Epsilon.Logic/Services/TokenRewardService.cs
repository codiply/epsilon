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

namespace Epsilon.Logic.Services
{
    public class TokenRewardService : ITokenRewardService
    {
        private readonly IClock _clock;
        private readonly ICommonConfig _commonConfig;
        private readonly IAppCache _appCache;
        private readonly IEpsilonContext _dbContext;

        public TokenRewardService(
            IClock clock,
            ICommonConfig commonConfig,
            IAppCache appCache,
            IEpsilonContext dbContext)
        {
            _clock = clock;
            _commonConfig = commonConfig;
            _appCache = appCache;
            _dbContext = dbContext;
        }

        public TokenRewardScheme GetCurrentScheme()
        {
            var now = _clock.OffsetNow;
            return _appCache.Get(AppCacheKey.CURRENT_TOKEN_REWARD_SCHEME, 
                () => {
                    var currentScheme = _dbContext.TokenRewardSchemes
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
            var currentScheme = GetCurrentScheme();
            var keyToString = EnumsHelper.TokenRewardKey.ToString(rewardKey);
            var currentReward = currentScheme.Rewards
                .SingleOrDefault(x => x.TypeKey.Equals(keyToString));
            return currentReward;
        }
    }
}
