using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.JsonModels;
using Epsilon.Resources.Logic.TokenRewardKey;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Helpers
{
    public class TokenRewardMetadataHelper : ITokenRewardMetadataHelper
    {
        // TODO_PANOS_TEST: everything here

        public static string ResourceNameForDisplayName(TokenRewardKey key)
        {
            return string.Format("{0}_DisplayName", EnumsHelper.TokenRewardKey.ToString(key));
        }

        public static string ResourceNameForDescription(TokenRewardKey key)
        {
            return string.Format("{0}_Description", EnumsHelper.TokenRewardKey.ToString(key));
        }

        public TokenRewardMetadata GetAll()
        {
            var resourceManager = TokenRewardKeyResources.ResourceManager;

            var answer = new TokenRewardMetadata
            {
                keyMetadata = new List<TokenRewardKeyMetadata>()
            };

            foreach (var key in EnumsHelper.TokenRewardKey.GetValues())
            {
                answer.keyMetadata.Add(new TokenRewardKeyMetadata
                {
                    key = EnumsHelper.TokenRewardKey.ToString(key),
                    displayName = resourceManager.GetString(ResourceNameForDisplayName(key)),
                    description = resourceManager.GetString(ResourceNameForDescription(key))
                });
            }

            return answer;
        }
    }
}
