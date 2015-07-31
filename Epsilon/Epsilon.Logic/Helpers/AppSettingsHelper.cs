using Epsilon.Logic.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
