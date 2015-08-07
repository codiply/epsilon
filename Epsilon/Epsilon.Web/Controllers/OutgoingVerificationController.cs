using Epsilon.Logic.Constants;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Resources.Common;
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
        public const string MY_OUTGOING_VERIFICATIONS_SUMMARY_ACTION = "MyOutgoingVerificationsSummary";

        private readonly IOutgoingVerificationService _outgoingVerificationService;

        public OutgoingVerificationController(
            IOutgoingVerificationService outgoingVerificationService)
        {
            _outgoingVerificationService = outgoingVerificationService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Pick(PickOutgoingVerificationViewModel model)
        {
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PickConfirmed(Guid verificationUniqueId, bool returnToSummary)
        {
            var outcome = await _outgoingVerificationService.Pick(GetUserId(), GetUserIpAddress(), verificationUniqueId);
            if (outcome.IsRejected)
            {
                Danger(outcome.RejectionReason, true);
                return RedirectHome(returnToSummary);
            }
            else
            {
                PresentUiAlerts(outcome.UiAlerts, true);
                // TODO_PANOS: redirect to instructions instead
                return RedirectHome(returnToSummary);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Instructions(Guid verificationUniqueId, bool returnToSummary)
        {
            var getInstructionsOutcome =
                await _outgoingVerificationService.GetInstructions(GetUserId(), verificationUniqueId);

            if (getInstructionsOutcome.IsRejected)
            {
                Danger(getInstructionsOutcome.RejectionReason, true);
                return RedirectHome(returnToSummary);
            }

            var model = getInstructionsOutcome.Instructions;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MarkAsSent(Guid verificationUniqueId, bool returnToSummary)
        {
            var outcome = await _outgoingVerificationService.MarkAsSent(GetUserId(), verificationUniqueId);
            if (outcome.IsRejected)
            {
                Danger(outcome.RejectionReason, true);
            }
            else
            {
                PresentUiAlerts(outcome.UiAlerts, true);
            }
            return RedirectHome(returnToSummary);
        }

        public ActionResult MyOutgoingVerificationsSummary()
        {
            return View();
        }

        private ActionResult RedirectHome(bool returnToSummary)
        {
            if (returnToSummary)
            {
                return RedirectToAction(MY_OUTGOING_VERIFICATIONS_SUMMARY_ACTION);
            }

            return RedirectToAction(
                AppConstant.AUTHENTICATED_USER_HOME_ACTION,
                AppConstant.AUTHENTICATED_USER_HOME_CONTROLLER);
        }
    }
}