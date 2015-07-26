using Epsilon.Logic.Constants;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Web.Controllers.BaseControllers;
using Epsilon.Web.Models.ViewModels.OutgoingVerification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers
{
    public class OutgoingVerificationController : BaseMvcController
    {
        private readonly IOutgoingVerificationService _outgoingVerificationService;

        public OutgoingVerificationController(
            IOutgoingVerificationService outgoingVerificationService)
        {
            _outgoingVerificationService = outgoingVerificationService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Pick(Guid verificationUniqueId)
        {
            throw new NotImplementedException();
        }

        public async Task<ActionResult> Instructions(Guid id)
        {
            var verificationUniqueId = id;
            var tenantVerifications = 
                await _outgoingVerificationService.GetVerificationForUser(GetUserId(), verificationUniqueId);

            if (tenantVerifications == null)
            {
                Danger("Sorry something went wrong.", true);
                return RedirectToAction(
                    AppConstant.AUTHENTICATED_USER_HOME_ACTION,
                    AppConstant.AUTHENTICATED_USER_HOME_CONTROLLER);
            }

            var model = OutgoingVerificationInstructionsViewModel.FromEntity(tenantVerifications);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MarkAsSent(Guid verificationUniqueId)
        {
            throw new NotImplementedException();
        }

        public ActionResult MyOutgoingVerificationsSummary()
        {
            return View();
        }
    }
}