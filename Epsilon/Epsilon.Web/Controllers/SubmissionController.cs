using Epsilon.Logic.Constants;
using Epsilon.Logic.Forms;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Web.Controllers.BaseControllers;
using Epsilon.Web.Models.ViewModels.Submission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers
{ 
    public class SubmissionController : BaseController
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

        public ActionResult Start()
        {
            var availableCountries = _countryService.GetAvailableCountries();
            var model = new StartViewModel
            {
                AvailableCountries = availableCountries
            };
            return View(model);
        }
        
        public ActionResult AddAddress(string id)
        {
            var countryId = id.ToUpper();

            var countries = _countryService.GetAvailableCountries();
            ViewBag.CountryId = new SelectList(countries, "Id", "EnglishName", countryId);

            var model = new AddressForm
            {
                CountryId = countryId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddAddress(
            [Bind(Include = "Line1,Line2,Line3,Line4,Locality,Region,Postcode,CountryId")] AddressForm address)
        {
            if (ModelState.IsValid)
            {
                await _addressService.AddAddress(address);
                return RedirectToAction("Index");
            }

            var countries = _countryService.GetAvailableCountries();
            ViewBag.CountryId = new SelectList(countries, "Id", "EnglishName", address.CountryId);
            return View(address);
        }

        [HttpPost]
        public async Task<ActionResult> UseAddress(string selectedAddressId)
        {
            Success(String.Format("Address id <strong>{0}</strong>.", selectedAddressId));

            return RedirectToAction(
                    AppConstant.AUTHENTICATED_USER_HOME_ACTION,
                    AppConstant.AUTHENTICATED_USER_HOME_CONTROLLER);
        }

        public async Task<ActionResult> Verify()
        {
            throw new NotImplementedException();
        }

        public async Task<ActionResult> Submit()
        {
            throw new NotImplementedException();
        }
    }
}