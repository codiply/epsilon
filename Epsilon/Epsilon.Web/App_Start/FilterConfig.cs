﻿using Epsilon.Web.Controllers.Filters.Mvc;
using System.Web.Mvc;

namespace Epsilon.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorWithElmahAttribute());
        }
    }
}
