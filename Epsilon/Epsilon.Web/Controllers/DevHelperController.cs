﻿using Epsilon.Logic.Constants;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.TestDataPopulation.Interfaces;
using Epsilon.Web.Controllers.BaseControllers;
using Epsilon.Web.Controllers.Filters.Mvc;
using Microsoft.AspNet.Identity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers
{
    [Authorize(Roles = AspNetRole.Admin)]
    [AllowIfConfigSettingTrue(AppSettingsKey.EnableDevHelperController)]
    public class DevHelperController : BaseMvcController
    {
        private readonly IEpsilonContext _dbContext;
        private readonly ITestDataPopulator _testDataPopulator;
        private readonly IAdminAlertService _adminAlertService;
        private readonly IUserTokenService _userTokenService;

        public DevHelperController(
            IEpsilonContext dbContext,
            ITestDataPopulator testDataPopulator,
            IAdminAlertService adminAlertService,
            IUserTokenService userTokenService)
        {
            _dbContext = dbContext;
            _testDataPopulator = testDataPopulator;
            _adminAlertService = adminAlertService;
            _userTokenService = userTokenService;
        }
        
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> PopulateAddresses()
        { 
            await _testDataPopulator.Populate(User.Identity.GetUserId());

            Success("Addresses table has been populated!");

            return RedirectToAction("Index");
        }

        public ActionResult SuccessAlert()
        {
            Success("This is a <b>Success</b> alert.", false);
            return RedirectToAction("Index");
        }

        public ActionResult SuccessAlertDismissable()
        {
            Success("This is a dismissable <b>Success</b> alert.", true);
            return RedirectToAction("Index");
        }

        public ActionResult WarningAlert()
        {
            Warning("This is a <b>Warning</b> alert.", false);
            return RedirectToAction("Index");
        }

        public ActionResult WarningAlertDismissable()
        {
            Warning("This is a dismissable <b>Warning</b> alert.", true);
            return RedirectToAction("Index");
        }

        public ActionResult InformationAlert()
        {
            Information("This is an <b>Information</b> alert.", false);
            return RedirectToAction("Index");
        }

        public ActionResult InformationAlertDismissable()
        {
            Information("This is a dismissable <b>Information</b> alert.", true);
            return RedirectToAction("Index");
        }

        public ActionResult DangerAlert()
        {
            Danger("This is a <b>Danger</b> alert.", false);
            return RedirectToAction("Index");
        }

        public ActionResult DangerAlertDismissable()
        {
            Danger("This is a dismissable <b>Danger</b> alert.", true);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendAdminAlert(string adminAlertKey)
        {
            _adminAlertService.SendAlert(adminAlertKey);
            Success(string.Format("AdminAlert for key '{0}' sent.", adminAlertKey), true);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> Tokens()
        {
            var balance = await _userTokenService.GetBalance(GetUserId());

            ViewBag.TokenRewardKey = 
                EnumsHelper.TokenRewardKey.GetNames().Select(x => new SelectListItem() { Text = x, Value = x }).ToList();

            return View(balance);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> TokensTransaction(
            string tokenRewardKey,
            int quantity)
        {
            var tokenRewardKeyEnum = EnumsHelper.TokenRewardKey.Parse(tokenRewardKey);
            if (tokenRewardKeyEnum.HasValue)
            {
                var status = await _userTokenService.MakeTransaction(GetUserId(), tokenRewardKeyEnum.Value, quantity: quantity);
            }

            return RedirectToAction("Tokens");
        }
    }
}