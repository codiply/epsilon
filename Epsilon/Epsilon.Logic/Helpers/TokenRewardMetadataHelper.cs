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
        // TODO_TEST_PANOS: everything here

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

        private static string ResourceNameForDisplayName(TokenRewardKey key)
        {
            return string.Format("{0}_DisplayName", EnumsHelper.TokenRewardKey.ToString(key));
        }

        private static string ResourceNameForDescription(TokenRewardKey key)
        {
            return string.Format("{0}_Description", EnumsHelper.TokenRewardKey.ToString(key));
        }
    }
}
