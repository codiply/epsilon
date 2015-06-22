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

        public async Task<ActionResult> PopulateAddresses()
        { 
            await _testDataPopulator.Populate();
            return RedirectToAction("Index");
        }

        public ActionResult SuccessAlert()
        {
            Success("This is a <b>Success</b> alert.", true);
            return RedirectToAction(
                    AppConstants.AUTHENTICATED_USER_HOME_ACTION,
                    AppConstants.AUTHENTICATED_USER_HOME_CONTROLLER); ;
        }

        public ActionResult WarningAlert()
        {
            Warning("This is a <b>Warning</b> alert.", true);
            return RedirectToAction(
                    AppConstants.AUTHENTICATED_USER_HOME_ACTION,
                    AppConstants.AUTHENTICATED_USER_HOME_CONTROLLER); ;
        }

        public ActionResult InformationAlert()
        {
            Warning("This is an <b>Information</b> alert.", true);
            return RedirectToAction(
                    AppConstants.AUTHENTICATED_USER_HOME_ACTION,
                    AppConstants.AUTHENTICATED_USER_HOME_CONTROLLER); ;
        }

        public ActionResult DangerAlert()
        {
            Danger("This is a <b>Danger</b> alert.", true);
            return RedirectToAction(
                    AppConstants.AUTHENTICATED_USER_HOME_ACTION,
                    AppConstants.AUTHENTICATED_USER_HOME_CONTROLLER); ;
        }
    }
}