using Epsilon.Logic.Constants;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Web.Controllers.BaseControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers
{
    [Authorize(Roles = AspNetRole.Admin)]
    public class AdminController : BaseMvcController
    {
        private IAppCache _appCache;
        private IDbAppSettingsHelper _dbAppSettingsHelper;

        public AdminController(
            IAppCache appCache,
            IDbAppSettingsHelper dbAppSettingsHelper)
        {
            _appCache = appCache;
            _dbAppSettingsHelper = dbAppSettingsHelper;
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

        public async Task<ActionResult> DbAppSettingList()
        {
            var model = await _dbAppSettingsHelper.GetAllAppSettingEntities();
            return View(model);
        }

        public async Task<ActionResult> DbAppSettingDetails(string id)
        {
            var model = await _dbAppSettingsHelper.GetAppSettingEntity(id);
            return View(model);
        }

        public async Task<ActionResult> DbAppSettingEdit(string id)
        {
            var model = await _dbAppSettingsHelper.GetAppSettingEntity(id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DbAppSettingEdit(AppSetting model)
        {
            return View(model);
        }

        #endregion
    }
}