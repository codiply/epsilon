using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.Logic.Constants;
using NUnit.Framework;
using System;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class TokenRewardServiceTest : BaseIntegrationTestWithRollback
    {
        [Test]
        public async Task RewardsHaveSignDependingOnTheirType()
        {
            var allRewards = await DbProbe.TokenRewards.ToListAsync();

            foreach (var reward in allRewards)
            {
                if (reward.TypeKey.StartsWith(AppConstant.TOKEN_REWARD_KEY_EARN))
                {
                    Assert.IsTrue(reward.Value >= 0M,
                        String.Format("TokenReward with SchemeId '{0}' and TypeKey '{1}' should have non-negative value because it starts with '{1}'.",
                            reward.SchemeId, reward.TypeKey, AppConstant.TOKEN_REWARD_KEY_EARN));
                }
                else if (reward.TypeKey.StartsWith(AppConstant.TOKEN_REWARD_KEY_SPEND))
                {
                    Assert.IsTrue(reward.Value <= 0M,
                        String.Format("TokenReward with SchemeId '{0}' and TypeKey '{1}' should have non-positive value because it starts with '{2}'.",
                            reward.SchemeId, reward.TypeKey, AppConstant.TOKEN_REWARD_KEY_SPEND));
                } else
                {
                    Assert.IsTrue(false,
                        String.Format("TokenReward with SchemeId '{0}' and TypeKey '{1}' does not start with 'Earn' or 'Spend'.",
                            reward.SchemeId, reward.TypeKey));
                }
            }
        }
    }
}
