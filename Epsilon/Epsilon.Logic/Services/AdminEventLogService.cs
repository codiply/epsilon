using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services
{
    public class AdminEventLogService : IAdminEventLogService
    {
        private readonly IEpsilonContext _dbContext;
        private readonly IElmahHelper _elmahHelper;

        public AdminEventLogService(
            IEpsilonContext dbContext,
            IElmahHelper elmahHelper)
        {
            _dbContext = dbContext;
            _elmahHelper = elmahHelper;
        }

        public async Task Log(AdminEventLogKey key, Dictionary<string, object> extraInfo)
        {
            try {
                var entity = new AdminEventLog
                {
                    Key = EnumsHelper.AdminEventLogKey.ToString(key),
                };
                if (extraInfo != null)
                {
                    entity.ExtraInfo = JsonConvert.SerializeObject(extraInfo, Formatting.None);
                }
                _dbContext.AdminEventLogs.Add(entity);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _elmahHelper.Raise(ex);
            }
        }
    }
}
