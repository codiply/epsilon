using Epsilon.Logic.SqlContext;
using Epsilon.Web.Controllers.BaseControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers
{
    public class DebugController : AnonymousBaseController
    {
        private readonly IEpsilonContext _dbContext;

        public DebugController(IEpsilonContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: Debug
        public ActionResult Index()
        {
            var user = _dbContext.Users.First();
            return Content(user.Email);
        }
    }
}