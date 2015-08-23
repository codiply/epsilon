using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Web.Controllers.BaseControllers;
using System.Linq;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers
{
    public class TextResourceController : BaseMvcController
    {
        private readonly ITextResourceHelper _textResourceHelper;

        public TextResourceController(
            ITextResourceHelper textResourceHelper)
        {
            _textResourceHelper = textResourceHelper;
        }

        public ActionResult List()
        {
            var allResources = _textResourceHelper.AllResources();
            return View(allResources);
        }
    }
}
