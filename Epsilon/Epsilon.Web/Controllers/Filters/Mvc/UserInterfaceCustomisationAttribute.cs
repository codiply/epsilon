﻿using Epsilon.Logic.Services.Interfaces;
using Microsoft.AspNet.Identity;
using System;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers.Filters.Mvc
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class UserInterfaceCustomisationAttribute : BaseActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                var userInterfaceCustomisationService = CurrentDependencyResolver.GetService<IUserInterfaceCustomisationService>();
                var userId = filterContext.HttpContext.User.Identity.GetUserId();
                var userInterfaceCustomisation = userInterfaceCustomisationService.GetForUser(userId);
                filterContext.Controller.ViewBag.UserInterfaceCustomisation = userInterfaceCustomisation;
            }    
        }
    }
}
