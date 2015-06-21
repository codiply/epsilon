using Epsilon.Web.Controllers.Filters.Mvc;
using Epsilon.Web.Models.ViewAlerts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers.BaseControllers
{
    [Internationalization]
    public class BaseController : Controller
    {
        public void Success(string message, bool dismissable = false)
        {
            AddAlert(ViewAlertStyles.Success, message, dismissable);
        }

        public void Information(string message, bool dismissable = false)
        {
            AddAlert(ViewAlertStyles.Information, message, dismissable);
        }

        public void Warning(string message, bool dismissable = false)
        {
            AddAlert(ViewAlertStyles.Warning, message, dismissable);
        }

        public void Danger(string message, bool dismissable = false)
        {
            AddAlert(ViewAlertStyles.Danger, message, dismissable);
        }

        private void AddAlert(string alertStyle, string message, bool dismissable)
        {
            var alerts = TempData.ContainsKey(ViewAlert.TempDataKey)
                ? (List<ViewAlert>)TempData[ViewAlert.TempDataKey]
                : new List<ViewAlert>();

            alerts.Add(new ViewAlert
            {
                AlertStyle = alertStyle,
                Message = message,
                Dismissable = dismissable
            });

            TempData[ViewAlert.TempDataKey] = alerts;
        }
    }
}