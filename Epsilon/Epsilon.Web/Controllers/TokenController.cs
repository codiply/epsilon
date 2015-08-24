using Epsilon.Web.Controllers.BaseControllers;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers
{
    public class TokenController : BaseMvcController
    {
        public ActionResult Transactions()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Rewards()
        {
            return View();
        }
    }
}