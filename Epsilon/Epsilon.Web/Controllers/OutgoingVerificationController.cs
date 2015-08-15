using Epsilon.Logic.Constants;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Resources.Web.OutgoingVerification;
using Epsilon.Web.Controllers.BaseControllers;
using Epsilon.Web.Models.ViewModels.OutgoingVerification;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers
{
    public class OutgoingVerificationController : BaseMvcController
    {
        public const string MY_OUTGOING_VERIFICATIONS_SUMMARY_ACTION = "MyOutgoingVerificationsSummary";

        private readonly IOutgoingVerificationService _outgoingVerificationService;
        private readonly IUserResidenceService _userResidenceService;

        public OutgoingVerificationController(
            IOutgoingVerificationService outgoingVerificationService,
            IUserResidenceService userResidenceService)
        {
            _outgoingVerificationService = outgoingVerificationService;
            _userResidenceService = userResidenceService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Pick(PickOutgoingVerificationViewModel model)
        {
            var userResidenceServiceResponse = await _userResidenceService.GetResidence(GetUserId());
            if (userResidenceServiceResponse.HasNoSubmissions)
            {
                Danger(OutgoingVerificationResources.Pick_CannotDetermineUserResidenceBecauseOfNoSubmissions_ErrorMessage, true);
                return RedirectHome(false);
            }

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
                return RedirectToAction("Instructions", new { id = outcome.VerificationUniqueId, returnToSummary = returnToSummary });
            }
        }

        [HttpGet]
        public async Task<ActionResult> Instructions(Guid id, bool returnToSummary)
        {
            var verificationUniqueId = id;
            var getInstructionsOutcome =
                await _outgoingVerificationService.GetInstructions(GetUserId(), verificationUniqueId);

            if (getInstructionsOutcome.IsRejected)
            {
                Danger(getInstructionsOutcome.RejectionReason, true);
                return RedirectHome(returnToSummary);
            }

            var model = new InstructionsViewModel
            {
                Instructions = getInstructionsOutcome.Instructions,
                ReturnToSummary = returnToSummary
            };
            return View(model);
        }

        public async Task<ActionResult> PrintableVerificationMessage(Guid id)
        {
            var verificationUniqueId = id;
            var getVerificationMessageOutcome =
                await _outgoingVerificationService.GetVerificationMessage(GetUserId(), verificationUniqueId);

            if (getVerificationMessageOutcome.IsRejected)
            {
                Danger(getVerificationMessageOutcome.RejectionReason, true);
                return RedirectHome(false);
            }

            var model = getVerificationMessageOutcome.MessageArguments;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MarkAddressAsInvalid(Guid verificationUniqueId, bool returnToSummary)
        {
            var outcome = await _outgoingVerificationService.MarkAddressAsInvalid(GetUserId(), verificationUniqueId);
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