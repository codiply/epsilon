using Epsilon.Web.Controllers.BaseControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers
{
    public class HomeController : AnonymousBaseController
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}