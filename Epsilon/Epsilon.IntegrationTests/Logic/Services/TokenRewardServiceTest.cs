﻿using Epsilon.IntegrationTests.BaseFixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using NUnit.Framework;

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
                if (reward.TypeKey.StartsWith("Earn"))
                {
                    Assert.IsTrue(reward.Value >= 0M,
                        String.Format("TokenReward with SchemeId '{0}' and TypeKey '{1}' should have non-negative value because it starts with 'Earn'.",
                            reward.SchemeId, reward.TypeKey));
                }
                else if (reward.TypeKey.StartsWith("Spend"))
                {
                    Assert.IsTrue(reward.Value <= 0M,
                        String.Format("TokenReward with SchemeId '{0}' and TypeKey '{1}' should have non-positive value because it starts with 'Spend'.",
                            reward.SchemeId, reward.TypeKey));
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
