using Epsilon.Logic.Constants;
using Epsilon.Web.Controllers.BaseControllers;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers
{
    public class HomeController : BaseMvcController
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction(
                    AppConstant.AUTHENTICATED_USER_HOME_ACTION,
                    AppConstant.AUTHENTICATED_USER_HOME_CONTROLLER);
            }

            return View();
        }

        [AllowAnonymous]
        public ActionResult Faq()
        {
            return View();
        }
    }
}