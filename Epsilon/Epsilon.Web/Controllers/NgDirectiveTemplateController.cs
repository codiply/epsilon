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
        public ActionResult MySubmissionsSummary()
        {
            return View();
        }
    }
}