using Epsilon.Logic.Constants;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using Epsilon.Web.Controllers.BaseControllers;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers
{
    public class TextResourceController : BaseMvcController
    {
        private readonly IClock _clock;
        private readonly ITextResourceHelper _textResourceHelper;

        public TextResourceController(
            IClock clock,
            ITextResourceHelper textResourceHelper)
        {
            _clock = clock;
            _textResourceHelper = textResourceHelper;
        }

        public ActionResult List(string id)
        {
            var languageId = id;
            var allResources = _textResourceHelper.AllResources(languageId);
            return View(allResources);
        }

        public ActionResult Download(string id)
        {
            var languageId = id;
            using (var stream = new StringWriter())
            {
                _textResourceHelper.AllResourcesCsv(languageId, stream);

                var contentType = AppConstant.CONTENT_TYPE_CSV;
                var bytes = Encoding.UTF8.GetBytes(stream.ToString());
                var result = new FileContentResult(bytes, contentType);
                // TODO_PANOS: place date format in a constant.
                result.FileDownloadName = 
                    string.Format("Resources_{0}_{1}.csv", languageId, _clock.OffsetNow.ToString("yyyyMMddHHmmss"));
                return result;
             }
        }
    }
}
