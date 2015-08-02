using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Constants.Interfaces;
using Epsilon.Logic.Forms;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Web.Controllers.BaseControllers;
using Epsilon.Web.Models.ViewModels.Submission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Epsilon.Web.Models.ViewModels.Shared;
using Epsilon.Resources.Web.Submission;
using Epsilon.Logic.Forms.Submission;
using Epsilon.Resources.Common;

namespace Epsilon.Web.Controllers
{ 
    public class SubmissionController : BaseMvcController
    {
        public const string MY_SUBMISSIONS_SUMMARY_ACTION = "MySubmissionSSummary";

        private readonly ICountryService _countryService;
        private readonly IAddressService _addressService;
        private readonly ITenancyDetailsSubmissionService _tenancyDetailsSubmissionService;

        public SubmissionController(
            ICountryService countryService,
            IAddressService addressService,
            ITenancyDetailsSubmissionService tenancyDetailsSubmissionService)
        {
            _countryService = countryService;
            _addressService = addressService;
            _tenancyDetailsSubmissionService = tenancyDetailsSubmissionService;
        }

        public ActionResult SearchAddress()
        {
            var availableCountries = _countryService.GetAvailableCountries();
            var model = new SearchAddressViewModel
            {
                AvailableCountries = availableCountries
            };
            return View(model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddAddress(string countryId, string postcode)
        {
            countryId = countryId.ToUpperInvariant();

            var countries = _countryService.GetAvailableCountries();
            ViewBag.CountryId = new SelectList(countries, "Id", AppConstant.COUNTRY_DISPLAY_FIELD, countryId);

            var model = new AddressForm
            {
                UniqueId = Guid.NewGuid(),
                CountryId = countryId,
                Postcode = postcode
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveAddress(AddressForm address)
        {
            if (ModelState.IsValid)
            {
                var outcome = await _addressService.AddAddress(User.Identity.GetUserId(), GetUserIpAddress(), address);
                if (outcome.IsRejected)
                {
                    Danger(outcome.RejectionReason, true);
                    if (outcome.ReturnToForm)
                    {
                        ViewBag.CountryId = new SelectList(_countryService.GetAvailableCountries(), "Id", AppConstant.COUNTRY_DISPLAY_FIELD, address.CountryId);
                        return View("AddAddress", address);
                    }
                    else
                    {
                        return RedirectToAction(
                            AppConstant.AUTHENTICATED_USER_HOME_ACTION,
                            AppConstant.AUTHENTICATED_USER_HOME_CONTROLLER);
                    }
                }
                
                return RedirectToAction("UseAddress", new { id = address.UniqueId });
            }

            ViewBag.CountryId = new SelectList(_countryService.GetAvailableCountries(), "Id", AppConstant.COUNTRY_DISPLAY_FIELD, address.CountryId);
            return View("AddAddress", address);
        }

        [HttpGet]
        public async Task<ActionResult> UseAddress(Guid id)
        {
            var addressUniqueId = id;
            var entity = await _addressService.GetAddress(addressUniqueId);
            var model = new UseAddressViewModel
            {
                SubmissionUniqueId = Guid.NewGuid(),
                AddressDetails = AddressDetailsViewModel.FromEntity(entity)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UseAddressConfirmed(Guid submissionUniqueId, Guid selectedAddressUniqueId)
        {
            var outcome = await _tenancyDetailsSubmissionService
                .Create(GetUserId(), GetUserIpAddress(), submissionUniqueId, selectedAddressUniqueId);
            if (outcome.IsRejected)
            {
                Danger(outcome.RejectionReason, true);
            }
            else
            {
                Success(SubmissionResources.UseAddressConfirmed_SuccessMessage, true);
            }

            return RedirectToAction(
                AppConstant.AUTHENTICATED_USER_HOME_ACTION,
                AppConstant.AUTHENTICATED_USER_HOME_CONTROLLER);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnterVerificationCode(Guid submissionUniqueId, bool returnToSummary)
        {
            var submissionBelongsToUser = await _tenancyDetailsSubmissionService.SubmissionBelongsToUser(GetUserId(), submissionUniqueId);
            if (!submissionBelongsToUser)
            {
                Danger(CommonResources.GenericInvalidRequestMessage, true);
                return RedirectHome(returnToSummary);
            }
            var model = new VerificationCodeForm
            {
                TenancyDetailsSubmissionUniqueId = submissionUniqueId,
                ReturnToSummary = returnToSummary
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnterVerificationCodeSubmit(VerificationCodeForm form)
        {
            if (ModelState.IsValid)
            {
                var outcome = await _tenancyDetailsSubmissionService.EnterVerificationCode(GetUserId(), form);
                if (outcome.IsRejected)
                {
                    Danger(outcome.RejectionReason, true);
                    if (outcome.ReturnToForm)
                    {
                        return View("EnterVerificationCode", form);
                    }
                    else
                    {
                        return RedirectHome(form.ReturnToSummary);
                    }
                }

                Success(SubmissionResources.EnterVerificationCode_SuccessMessage, true);
                return RedirectHome(form.ReturnToSummary);
            }

            return View("EnterVerificationCode", form);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitTenancyDetails(Guid submissionUniqueId, bool returnToSummary)
        {
            var submissionBelongsToUser = await _tenancyDetailsSubmissionService.SubmissionBelongsToUser(GetUserId(), submissionUniqueId);
            if (!submissionBelongsToUser)
            {
                Danger(CommonResources.GenericInvalidRequestMessage, true);
                return RedirectHome(returnToSummary);
            }
            var model = new TenancyDetailsForm
            {
                TenancyDetailsSubmissionUniqueId = submissionUniqueId,
                ReturnToSummary = returnToSummary
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitTenancyDetailsSave(TenancyDetailsForm form)
        {
            if (ModelState.IsValid)
            {
                var outcome = await _tenancyDetailsSubmissionService.SubmitTenancyDetails(GetUserId(), form);
                if (outcome.IsRejected)
                {
                    Danger(outcome.RejectionReason, true);
                    if (outcome.ReturnToForm)
                    {
                        return View("SubmitTenancyDetails", form);
                    }
                    else
                    {
                        return RedirectHome(form.ReturnToSummary);
                    }
                }

                // TODO_PANOS: add a Success message here.
                return RedirectHome(form.ReturnToSummary);
            }

            return View("SubmitTenancyDetails", form);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitMoveOutDetails(Guid submissionUniqueId, bool returnToSummary)
        {
            var submissionBelongsToUser = await _tenancyDetailsSubmissionService.SubmissionBelongsToUser(GetUserId(), submissionUniqueId);
            if (!submissionBelongsToUser)
            {
                Danger(CommonResources.GenericInvalidRequestMessage, true);
                return RedirectHome(returnToSummary);
            }
            var model = new MoveOutDetailsForm
            {
                TenancyDetailsSubmissionUniqueId = submissionUniqueId,
                ReturnToSummary = returnToSummary
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitMoveOutDetailsSave(MoveOutDetailsForm form)
        {
            if (ModelState.IsValid)
            {
                var outcome = await _tenancyDetailsSubmissionService.SubmitMoveOutDetails(GetUserId(), form);
                if (outcome.IsRejected)
                {
                    Danger(outcome.RejectionReason, true);
                    if (outcome.ReturnToForm)
                    {
                        return View("SubmitMoveOutDetails", form);
                    }
                    else
                    {
                        return RedirectHome(form.ReturnToSummary);
                    }
                }

                // TODO_PANOS: add a Success message here.
                return RedirectHome(form.ReturnToSummary);
            }

            return View("SubmitMoveOutDetails", form);
        }

        [HttpGet]
        public ActionResult MySubmissionsSummary()
        {
            return View();
        }

        private ActionResult RedirectHome(bool returnToSummary)
        {
            if (returnToSummary)
            {
                return RedirectToAction(MY_SUBMISSIONS_SUMMARY_ACTION);
            }

            return RedirectToAction(
                AppConstant.AUTHENTICATED_USER_HOME_ACTION,
                AppConstant.AUTHENTICATED_USER_HOME_CONTROLLER);
        }
    }
}