using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Epsilon.Logic.Helpers;

namespace Epsilon.Logic.Services
{
    public class IpAddressActivityService : IIpAddressActivityService
    {
        private readonly IEpsilonContext _dbContext;

        public IpAddressActivityService(IEpsilonContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task RecordRegistration(string userId, string ipAddress)
        {
            // TODO_PANOS: wrap in try catch
            await RecordActivity(userId, IpAddressActivityType.Registration, ipAddress);
        }

        public async Task RecordLogin(string email, string ipAddress)
        { 
            // TODO_PANOS: wrap in try catch
            var user = await _dbContext.Users.SingleAsync(u => u.Email.Equals(email));
            await RecordActivity(user.Id, IpAddressActivityType.Login, ipAddress);
        }

        private async Task RecordActivity(string userId, IpAddressActivityType activityType, string ipAddress)
        {
            var activity = new IpAddressActivity
            {
                UserId = userId,
                ActivityType = EnumsHelper.IpAddressActivityType.ToString(activityType),
                IpAddress = ipAddress
            };
            _dbContext.IpAddressActivities.Add(activity);
            await _dbContext.SaveChangesAsync();
        }
    }
}
