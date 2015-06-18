using Epsilon.Logic.SqlContext;
using Epsilon.Web.Controllers.BaseControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers
{
    public class HelperController : AnonymousBaseController
    {
        private readonly IEpsilonContext _dbContext;

        public HelperController(IEpsilonContext dbContext)
        {
            _dbContext = dbContext;
        }

        public ActionResult CreateDatabase()
        {
            var x = _dbContext.Users.Any();
            return Content(x.ToString());
        }

        public ActionResult SuccessAlert()
        {
            Success("This is a <b>Success</b> alert.", true);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult WarningAlert()
        {
            Warning("This is a <b>Warning</b> alert.", true);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult InformationAlert()
        {
            Warning("This is an <b>Information</b> alert.", true);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult DangerAlert()
        {
            Danger("This is a <b>Danger</b> alert.", true);
            return RedirectToAction("Index", "Home");
        }

    }
}