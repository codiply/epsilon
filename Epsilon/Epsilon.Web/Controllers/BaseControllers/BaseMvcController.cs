using Epsilon.Web.Controllers.Filters.Mvc;
using Epsilon.Web.Models.ViewAlerts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Epsilon.Logic.Infrastructure.Extensions;

namespace Epsilon.Web.Controllers.BaseControllers
{
    [RequireSecureConnection(Order = 100)]
    [SanitizeIpAddress(Order = 200)]
    [Internationalization(Order = 300)]
    [Authorize(Order = 400)]
    public class BaseMvcController : Controller
    {
        internal string UserIpAddress()
        {
            var context = new HttpContextWrapper(HttpContext.ApplicationInstance.Context);
            return context.GetSanitizedIpAddress();
        }

        internal void Success(string message, bool dismissable = false)
        {
            AddAlert(ViewAlertStyles.Success, message, dismissable);
        }

        internal void Information(string message, bool dismissable = false)
        {
            AddAlert(ViewAlertStyles.Information, message, dismissable);
        }

        internal void Warning(string message, bool dismissable = false)
        {
            AddAlert(ViewAlertStyles.Warning, message, dismissable);
        }

        internal void Danger(string message, bool dismissable = false)
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