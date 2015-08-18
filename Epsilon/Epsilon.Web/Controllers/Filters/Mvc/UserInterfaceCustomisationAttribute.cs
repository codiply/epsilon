using Epsilon.Logic.Services.Interfaces;
using Microsoft.AspNet.Identity;
using Ninject;
using System;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers.Filters.Mvc
{
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
