using Epsilon.Web.Controllers.BaseControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers
{
    public class AdminController : AuthorizeBaseController
    {
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }
    }
}