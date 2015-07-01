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

namespace Epsilon.Web.Controllers
{ 
    public class SubmissionController : BaseMvcController
    {
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
            ViewBag.CountryId = new SelectList(countries, "Id", "EnglishName", countryId);

            var model = new AddressForm
            {
                CountryId = countryId,
                Postcode = postcode
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveAddress(
            [Bind(Include = "Line1,Line2,Line3,Line4,Locality,Region,Postcode,CountryId")] AddressForm address)
        {
            if (ModelState.IsValid)
            {
                // TODO_PANOS: Get the ip address of the user.
                var ipAddress = "192.168.0.1";
                var outcome = await _addressService.AddAddress(User.Identity.GetUserId(), ipAddress, address);
                if (outcome.IsRejected)
                {
                    Danger(outcome.RejectionReason, true);
                    return RedirectToAction(
                        AppConstant.AUTHENTICATED_USER_HOME_ACTION,
                        AppConstant.AUTHENTICATED_USER_HOME_CONTROLLER);
                }
                return await UseAddress(outcome.AddressId.Value);
            }

            var countries = _countryService.GetAvailableCountries();
            ViewBag.CountryId = new SelectList(countries, "Id", "EnglishName", address.CountryId);
            return View("AddAddress", address);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UseAddress(Guid selectedAddressId)
        {
            Success(String.Format("Address id <strong>{0}</strong>.", selectedAddressId));

            return RedirectToAction(
                    AppConstant.AUTHENTICATED_USER_HOME_ACTION,
                    AppConstant.AUTHENTICATED_USER_HOME_CONTROLLER);
        }

        public async Task<ActionResult> VerifyAddress()
        {
            throw new NotImplementedException();
        }

        public async Task<ActionResult> SubmitTenancyDetails()
        {
            throw new NotImplementedException();
        }
    }
}