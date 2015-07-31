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
        public const string MY_OUTGOING_VERIFICATION_SUMMARY = "MyOutgoingVerificationsSummary";

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
                // TODO_PANOS: put in a resource.
                Success("A new outgoing verification has been assigned to you.", true);
                // TODO_PANOS: redirect to instructions instead
                return RedirectHome(returnToSummary);
            }
        }

        public async Task<ActionResult> Instructions(Guid id)
        {
            var verificationUniqueId = id;
            var tenantVerifications = 
                await _outgoingVerificationService.GetVerificationForUser(GetUserId(), verificationUniqueId);

            if (tenantVerifications == null)
            {
                Danger(CommonResources.GenericInvalidRequestMessage, true);
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
            var outcome = await _outgoingVerificationService.MarkAsSent(GetUserId(), verificationUniqueId);
            if (outcome.IsRejected)
            {
                Danger(outcome.RejectionReason, true);
            }
            return RedirectToAction(
                AppConstant.AUTHENTICATED_USER_HOME_ACTION,
                AppConstant.AUTHENTICATED_USER_HOME_CONTROLLER);
        }

        public ActionResult MyOutgoingVerificationsSummary()
        {
            return View();
        }

        private ActionResult RedirectHome(bool returnToSummary)
        {
            if (returnToSummary)
            {
                return RedirectToAction(MY_OUTGOING_VERIFICATION_SUMMARY);
            }
            else
            {
                return RedirectToAction(
                    AppConstant.AUTHENTICATED_USER_HOME_ACTION,
                    AppConstant.AUTHENTICATED_USER_HOME_CONTROLLER);
            }
        }
    }
}