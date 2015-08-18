using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Configuration
{
    public class AppCacheConfig : IAppCacheConfig
    {
        private readonly IAppSettingsHelper _appSettingsHelper;

        public AppCacheConfig(
            IAppSettingsHelper appSettingsHelper)
        {
            _appSettingsHelper = appSettingsHelper;
        }

        public bool DisableAppCache
        {
            get
            {
                return _appSettingsHelper.GetBool(AppSettingsKey.DisableAppCache) == true;
            }
        }

        public bool DisableAsynchronousLocking
        {
            get
            {
                return _appSettingsHelper.GetBool(AppSettingsKey.DisableAsynchronousLockingInAppCache) == true;
            }
        }

        public bool DisableSynchronousLocking
        {
            get
            {
                return _appSettingsHelper.GetBool(AppSettingsKey.DisableSynchronousLockingInAppCache) == true;
            }
        }
    }
}
