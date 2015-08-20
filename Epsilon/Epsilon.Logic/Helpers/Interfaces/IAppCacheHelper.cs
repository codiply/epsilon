using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Helpers.Interfaces
{
    public interface IAppCacheHelper
    {
        void RemoveCachedUserSubmissionsSummary(string userId);

        void RemoveCachedUserOutgoingVerificationsSummary(string userId);

        void RemoveCachedUserExploredPropertiesSummary(string userId);
    }
}
