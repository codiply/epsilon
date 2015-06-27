using Epsilon.Logic.Constants;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.TestDataPopulation.Interfaces;
using Epsilon.Web.Controllers.BaseControllers;
using Epsilon.Web.Controllers.Filters.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace Epsilon.Web.Controllers
{
    [AllowIfConfigSettingTrue(AppSettingsKey.EnableHelperController)]
    public class HelperController : BaseController
    {
        private readonly IEpsilonContext _dbContext;
        private readonly ITestDataPopulator _testDataPopulator;
        private readonly IAdminAlertService _adminAlertService;
        private readonly IUserCoinService _userCointService;

        public HelperController(
            IEpsilonContext dbContext,
            ITestDataPopulator testDataPopulator,
            IAdminAlertService adminAlertService,
            IUserCoinService userCoinService)
        {
            _dbContext = dbContext;
            _testDataPopulator = testDataPopulator;
            _adminAlertService = adminAlertService;
            _userCointService = userCoinService;
        }

        public async Task<ActionResult> Index()
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
            Success(String.Format("AdminAlert for key '{0}' sent.", adminAlertKey), true);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> Coins()
        {
            var balance = await _userCointService.GetBalance(User.Identity.GetUserId());

            return View(balance);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CoinsCredit(Decimal amount)
        {
            var status = await _userCointService.Credit(User.Identity.GetUserId(), amount);

            return RedirectToAction("Coins");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CoinsDebit(Decimal amount)
        {
            var status = await _userCointService.Debit(User.Identity.GetUserId(), amount);

            return RedirectToAction("Coins");
        }
    }
}