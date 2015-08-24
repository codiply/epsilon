using Epsilon.Web.Controllers.BaseControllers;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers
{
    public class NgDirectiveTemplateController : BaseMvcController
    {
        public ActionResult MyExploredPropertiesSummary()
        {
            return View();
        }

        public ActionResult MyOutgoingVerificationsSummary()
        {
            return View();
        }

        public ActionResult MySubmissionsSummary()
        {
            return View();
        }

        public ActionResult MyTokenTransactions()
        {
            return View();
        }

        public ActionResult StarRatingEditor()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult TokenRewardsSummary()
        {
            return View();
        }
    }
}