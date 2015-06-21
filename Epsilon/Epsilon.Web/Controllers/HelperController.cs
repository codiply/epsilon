﻿using Epsilon.Logic.Constants;
using Epsilon.Logic.SqlContext;
using Epsilon.Logic.TestDataPopulation.Interfaces;
using Epsilon.Web.Controllers.BaseControllers;
using Epsilon.Web.Controllers.Filters.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers
{
    [AllowIfConfigSettingTrue(AppSettingsKeys.EnableHelperController)]
    public class HelperController : AuthorizeBaseController
    {
        private readonly IEpsilonContext _dbContext;
        private readonly ITestDataPopulator _testDataPopulator;

        public HelperController(
            IEpsilonContext dbContext,
            ITestDataPopulator testDataPopulator)
        {
            _dbContext = dbContext;
            _testDataPopulator = testDataPopulator;
        }

        public async Task<ActionResult> PopulateAddresses()
        { 
            await _testDataPopulator.Populate();
            return Content("Success!");
        }

        public ActionResult SuccessAlert()
        {
            Success("This is a <b>Success</b> alert.", true);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult WarningAlert()
        {
            Warning("This is a <b>Warning</b> alert.", true);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult InformationAlert()
        {
            Warning("This is an <b>Information</b> alert.", true);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult DangerAlert()
        {
            Danger("This is a <b>Danger</b> alert.", true);
            return RedirectToAction("Index", "Home");
        }

    }
}