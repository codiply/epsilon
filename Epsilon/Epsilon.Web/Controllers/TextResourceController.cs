using Epsilon.Logic.Constants;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using Epsilon.Web.Controllers.BaseControllers;
using Epsilon.Web.Models.ViewModels.TextResource;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers
{
    [Authorize(Roles = AspNetRole.AdminOrTranslator)]
    public class TextResourceController : BaseMvcController
    {
        private readonly IClock _clock;
        private readonly ITextResourceHelper _textResourceHelper;
        private readonly ILanguageService _languageService;

        public TextResourceController(
            IClock clock,
            ITextResourceHelper textResourceHelper,
            ILanguageService languageService)
        {
            _clock = clock;
            _textResourceHelper = textResourceHelper;
            _languageService = languageService;
        }

        public ActionResult List(string id)
        {
            var cultureCode = id;

            var model = new ListViewModel
            {
                Resources = _textResourceHelper.AllResources(cultureCode),
                CultureCode = cultureCode,
                Languages = _languageService.GetAvailableAndUnavailableLanguages()
                
            };
            return View(model);
        }

        public ActionResult Download(string id)
        {
            var cultureCode = id;
            using (var stream = new StringWriter())
            {
                _textResourceHelper.AllResourcesCsv(cultureCode, stream);

                var contentType = AppConstant.CONTENT_TYPE_CSV;
                var bytes = Encoding.UTF8.GetBytes(stream.ToString());
                var result = new FileContentResult(bytes, contentType);
                result.FileDownloadName = 
                    string.Format("Resources_{0}_{1}.csv", 
                    _clock.OffsetNow.ToString(AppConstant.DATE_TIME_FORMAT_FOR_FILENAME), 
                    cultureCode ?? "default");
                return result;
             }
        }
    }
}
