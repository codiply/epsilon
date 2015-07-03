using Epsilon.Web.Controllers.Filters;
using Epsilon.Web.Controllers.Filters.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Epsilon.Web.Controllers.BaseControllers
{
    [RequireSecureConnection]
    [Internationalization]
    [Authorize]
    public class BaseApiController : ApiController
    {
    }
}
