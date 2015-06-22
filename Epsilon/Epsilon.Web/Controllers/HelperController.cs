using Epsilon.Logic.Constants;
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

namespace Epsilon.Web.Controllers
{
    [AllowIfConfigSettingTrue(AppSettingsKeys.EnableHelperController)]
    public class HelperController : BaseController
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

        public async Task<ActionResult> Index()
        {
            return View();
        }

        public async Task<ActionResult> PopulateAddresses()
        { 
            await _testDataPopulator.Populate();

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
    }
}