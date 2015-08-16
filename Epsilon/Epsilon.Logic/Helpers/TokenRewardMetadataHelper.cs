using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.JsonModels;
using Epsilon.Resources.Logic.TokenRewardKey;
using System.Collections.Generic;

namespace Epsilon.Logic.Helpers
{
    public class TokenRewardMetadataHelper : ITokenRewardMetadataHelper
    {
        public IList<TokenRewardTypeMetadata> GetAll()
        {
            var resourceManager = TokenRewardKeyResources.ResourceManager;

            var typeMetadata = new List<TokenRewardTypeMetadata>();

            foreach (var key in EnumsHelper.TokenRewardKey.GetValues())
            {
                typeMetadata.Add(new TokenRewardTypeMetadata
                {
                    key = EnumsHelper.TokenRewardKey.ToString(key),
                    displayName = resourceManager.GetString(ResourceNameForDisplayName(key)),
                    description = resourceManager.GetString(ResourceNameForDescription(key))
                });
            }

            return typeMetadata;
        }

        public static string ResourceNameForDisplayName(TokenRewardKey key)
        {
            return string.Format("{0}_DisplayName", EnumsHelper.TokenRewardKey.ToString(key));
        }

        public static string ResourceNameForDescription(TokenRewardKey key)
        {
            return string.Format("{0}_Description", EnumsHelper.TokenRewardKey.ToString(key));
        }
    }
}
