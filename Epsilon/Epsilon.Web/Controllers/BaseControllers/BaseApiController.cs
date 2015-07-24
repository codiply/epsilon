using Epsilon.Web.Controllers.Filters;
using Epsilon.Web.Controllers.Filters.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;

namespace Epsilon.Web.Controllers.BaseControllers
{
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
