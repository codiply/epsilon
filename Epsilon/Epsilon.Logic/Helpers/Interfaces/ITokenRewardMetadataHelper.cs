using Epsilon.Logic.JsonModels;
using System.Collections.Generic;

namespace Epsilon.Logic.Helpers.Interfaces
{
    public interface ITokenRewardMetadataHelper
    {
        IList<TokenRewardTypeMetadata> GetAll();
    }
}
