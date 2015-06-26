using Epsilon.Logic.Constants;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Helpers
{
    public class DbAppSettingsHelper : AppSettingsHelper, IDbAppSettingsHelper
    {
        public DbAppSettingsHelper(
            IEpsilonContext dbContext,
            IParseHelper parseHelper,
            IAppCache appCache)
            : base(null, parseHelper)
        {
            Collection = appCache.Get<NameValueCollection>(AppCacheKey.DB_APP_SETTINGS, () => 
            {
                var allValues = dbContext.AppSettings.OrderBy(x => x.Id).ToList();
                var collection = new NameValueCollection(allValues.Count);
                allValues.ForEach(x => collection.Add(x.Id, x.Value));
                return collection;
            }, WithLock.Yes);
        }
    }
}
