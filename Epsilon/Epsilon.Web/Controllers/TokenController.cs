using Epsilon.Web.Controllers.BaseControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers
{
    public class TokenController : BaseMvcController
    {
        public ActionResult Transactions()
        {
            return View();
        }
    }
}