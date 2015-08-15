using Epsilon.Logic.Helpers.Interfaces;
using System.Collections.Specialized;

namespace Epsilon.Logic.Helpers
{
    public class AppSettingsHelper : BaseAppSettingsHelper, IAppSettingsHelper
    {
        public AppSettingsHelper(
            NameValueCollection appSettings,
            IParseHelper parseHelper)
            : base(appSettings, parseHelper)
        {
        }
    }
}
