using Epsilon.Web.Controllers.Filters.Mvc;
using Epsilon.Web.Models.ViewAlerts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Epsilon.Logic.Infrastructure.Extensions;
using Microsoft.AspNet.Identity;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Dtos;

namespace Epsilon.Web.Controllers.BaseControllers
{
    [RequireSecureConnection(Order = 100)]
    [SanitizeIpAddress(Order = 200)]
    [Internationalization(Order = 300)]
    [DisableWholeWebsiteForMaintenance(Order = 400)]
    [Authorize(Order = 500)]
    [ResponseTiming(Order = 600)]
    public class BaseMvcController : Controller
    {
        internal string GetUserIpAddress()
        {
            var context = new HttpContextWrapper(HttpContext.ApplicationInstance.Context);
            return context.GetSanitizedIpAddress();
        }

        internal string GetUserId()
        {
            return User.Identity.GetUserId();
        }

        internal string GetLanguageId()
        {
            return (string)RouteData.Values["languageId"];
        }

        internal void AddUiAlert(UiAlert alert, bool dismissable = false)
        {
            switch (alert.Type)
            {
                case UiAlertType.Danger:
                    Danger(alert.Message, dismissable);
                    break;
                case UiAlertType.Information:
                    Information(alert.Message, dismissable);
                    break;
                case UiAlertType.Success:
                    Success(alert.Message, dismissable);
                    break;
                case UiAlertType.Warning:
                    Warning(alert.Message, dismissable);
                    break;
            }
        }

        internal void PresentUiAlerts(IEnumerable<UiAlert> alerts, bool dismissible = false)
        {
            if (alerts == null)
                return;
            foreach (var alert in alerts)
            {
                AddUiAlert(alert, dismissible);
            }
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