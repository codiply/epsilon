using Epsilon.Web.Controllers.BaseControllers;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers
{
    public class UserHomeController : BaseMvcController
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}