using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Resources.Common;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Epsilon.Logic.Infrastructure.Extensions;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Constants;
using Microsoft.AspNet.Identity;

namespace Epsilon.Web.Controllers.Filters.Mvc
{
    // TODO_TEST_PANOS
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class UserInterfaceCustomisationAttribute : ActionFilterAttribute
    {
        [Inject]
        public IUserInterfaceCustomisationService UserInterfaceCustomisationService { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                var userId = filterContext.HttpContext.User.Identity.GetUserId();
                var userInterfaceCustomisation = UserInterfaceCustomisationService.GetForUser(userId);
                filterContext.Controller.ViewBag.UserInterfaceCustomisation = userInterfaceCustomisation;
            }    
        }
    }
}
