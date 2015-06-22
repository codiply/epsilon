using Epsilon.Logic.Constants;
using Epsilon.Web.Controllers.BaseControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers
{
    [Authorize(Roles = AspNetRoles.Admin)]
    public class AdminController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}