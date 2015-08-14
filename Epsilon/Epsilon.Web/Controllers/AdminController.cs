using Epsilon.Logic.Constants;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Forms;
using Epsilon.Logic.Forms.Admin;
using Epsilon.Logic.FSharp;
using Epsilon.Logic.FSharp.GoogleGeocode;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Web.Controllers.BaseControllers;
using Epsilon.Web.Models.ViewModels.Admin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Epsilon.Logic.Wrappers.Interfaces;
using Epsilon.Logic.Helpers;

namespace Epsilon.Web.Controllers
{
    [Authorize(Roles = AspNetRole.Admin)]
    public class AdminController : BaseMvcController
    {
        private readonly IAppCache _appCache;
        private readonly IAppSettingsHelper _appSettingsHelper;
        private readonly IDbAppSettingsHelper _dbAppSettingsHelper;
        private readonly ISmtpService _smtpService;
        private readonly IGeoipClientFactory _geoipClientFactory;

        public AdminController(
            IAppCache appCache,
            IAppSettingsHelper appSettingsHelper,
            IDbAppSettingsHelper dbAppSettingsHelper,
            ISmtpService smtpService,
            IGeoipClientFactory geoipClientFactory)
        {
            _appCache = appCache;
            _appSettingsHelper = appSettingsHelper;
            _dbAppSettingsHelper = dbAppSettingsHelper;
            _smtpService = smtpService;
            _geoipClientFactory = geoipClientFactory;
        }

        public ActionResult Index()
        {
            return View();
        }

        #region AppCache

        public ActionResult AppCacheKeys()
        {
            var model = _appCache.AllKeys().OrderBy(x => x).ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ClearAppCache()
        {
            _appCache.Clear();

            Success("App Cache has been cleared!");

            return RedirectToAction("AppCacheKeys");
        }

        #endregion

        #region DbAppSettings

        public async Task<ActionResult> DbAppSettingList(string id)
        {
            var label = id;
            var allLabels = await _dbAppSettingsHelper.GetAllLabels();
            var model = new DbAppSettingListViewModel() { AllLabels = allLabels, SelectedLabel = label };
            if (string.IsNullOrWhiteSpace(label))
            {
                model.Settings = await _dbAppSettingsHelper.GetAllAppSettingEntities();
            }
            else
            {
                model.Settings = await _dbAppSettingsHelper.GetAllAppSettingEntitiesForLabel(label);
            };

            return View(model);
        }

        public async Task<ActionResult> DbAppSettingDetails(string id)
        {
            var entity = await _dbAppSettingsHelper.GetAppSettingEntity(id);
            var model = DbAppSettingForm.FromEntity(entity);
            return View(model);
        }

        public async Task<ActionResult> DbAppSettingEdit(string id)
        {
            var entity = await _dbAppSettingsHelper.GetAppSettingEntity(id);
            var model = DbAppSettingForm.FromEntity(entity);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DbAppSettingEdit(DbAppSettingForm form)
        {
            if (ModelState.IsValid)
            {
                await _dbAppSettingsHelper.Update(form, GetUserId());
                return RedirectToAction("DbAppSettingDetails", new { id = form.Id });
            }
            return View(form);
        }

        #endregion

        #region TestGoogleGeocodeApi

        public ActionResult TestGoogleGeocodeApi()
        {
            var model = new TestGoogleGeocodeApiViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> TestGoogleGeocodeApi(TestGoogleGeocodeApiViewModel model)
        {
            if (ModelState.IsValid)
            {
                var googleApiKey = _appSettingsHelper.GetString(AppSettingsKey.GoogleApiServerKey);
                var response = await Geocoder.getResponse(model.Address, model.Region, googleApiKey);
                model.Response = response;
                model.Geometries = Geocoder.parseGeometries(response).ToList();
                return View(model);
            }
            return View(model);
        }

        #endregion

        #region TestGeoipApi

        public ActionResult TestGeoipApi()
        {
            var model = new TestGeoipClientViewModel();

            var providerNames = EnumsHelper.GeoipProviderName.GetNames();
            ViewBag.GeoipProviderName = new SelectList(providerNames);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> TestGeoipApi(TestGeoipClientViewModel model)
        {
            var providerNames = EnumsHelper.GeoipProviderName.GetNames();
            ViewBag.GeoipProviderName = new SelectList(providerNames);

            if (ModelState.IsValid)
            {
                var client = _geoipClientFactory.Create();
                var providerName = EnumsHelper.GeoipProviderName.Parse(model.GeoipProviderName).Value;
                model.ClientResponse = await client.Geoip(providerName, model.IpAddress);
                return View(model);
            }
            return View(model);
        }

        #endregion

        #region TestEmail

        public ActionResult TestEmail()
        {
            var model = new TestEmailViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TestEmail(TestEmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var message = new MailMessage
                {
                    Subject = model.Subject,
                    Body = model.Body,
                    IsBodyHtml = true
                };
                message.To.Add(new MailAddress(model.ToEmailAddress, model.ToDisplayName));
                _smtpService.Send(message, allowThrowException: true);

                Success(String.Format("Message sent to <strong>{0}<strong>.", model.ToEmailAddress), true);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        #endregion
    }
}