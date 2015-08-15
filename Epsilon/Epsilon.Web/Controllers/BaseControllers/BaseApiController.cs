using Epsilon.Web.Controllers.Filters.WebApi;
using Microsoft.AspNet.Identity;
using System.Web.Http;

namespace Epsilon.Web.Controllers.BaseControllers
{
    [DisableWholeWebsiteForMaintenance]
    [RequireSecureConnection]
    [Internationalization]
    [Authorize]
    [ResponseTiming]
    public class BaseApiController : ApiController
    {
        internal string GetUserId()
        {
            return User.Identity.GetUserId();
        }
    }
}
