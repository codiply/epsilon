using Epsilon.Logic.Constants;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Web.Controllers.BaseControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers
{
    [Authorize(Roles = AspNetRoles.Admin)]
    public class AdminController : BaseController
    {
        private IAppCache _appCache;

        public AdminController(
            IAppCache appCache)
        {
            _appCache = appCache;
        }

        public ActionResult Index()
        {
            return View();
        }

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
    }
}