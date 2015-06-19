﻿using Epsilon.Logic.Forms;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Web.Controllers.BaseControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers
{ 
    public class SubmissionController : AuthorizeBaseController
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

        public async Task<ActionResult> Start()
        {
            var countries = _countryService.GetAvailableCountries();
            ViewBag.CountryId = new SelectList(countries, "Id", "EnglishName");
            return View();
        }

        public async Task<ActionResult> AddAddress(string id)
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
            [Bind(Include = "Line1,Line2,Line3,CityTown,CountyStateProvince,PostcodeOrZip,CountryId")] AddressForm address)
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