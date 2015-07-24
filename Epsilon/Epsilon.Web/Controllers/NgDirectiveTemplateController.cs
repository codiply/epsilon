using Epsilon.Web.Controllers.BaseControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers
{
    public class NgDirectiveTemplateController : BaseMvcController
    {
        // I just use this action to get the base url of this controller in _Layout.cshtml
        public ActionResult Index()
        {
            return Content("");
        }

        public ActionResult MySubmissionsSummary()
        {
            return View();
        }
    }
}