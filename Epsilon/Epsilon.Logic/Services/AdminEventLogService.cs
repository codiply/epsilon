using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services
{
    public class AdminEventLogService : IAdminEventLogService
    {
        private readonly IEpsilonContext _dbContext;

        public AdminEventLogService(
            IEpsilonContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Log(AdminEventLogKey key, string extraInfo)
        {
            var entity = new AdminEventLog
            {
                Key = EnumsHelper.AdminEventLogKey.ToString(key),
                ExtraInfo = extraInfo
            };
            _dbContext.AdminEventLogs.Add(entity);
            await _dbContext.SaveChangesAsync();
        }
    }
}
