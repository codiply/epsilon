using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using System;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services
{
    public class IpAddressActivityService : IIpAddressActivityService
    {
        private readonly IEpsilonContext _dbContext;
        private readonly IElmahHelper _elmahHelper;

        public IpAddressActivityService(
            IEpsilonContext dbContext,
            IElmahHelper elmahHelper)
        {
            _dbContext = dbContext;
            _elmahHelper = elmahHelper;
        }

        public async Task RecordWithUserId(string userId, IpAddressActivityType activityType, string ipAddress)
        {
            try {
                await RecordActivity(userId, activityType, ipAddress);
            }
            catch (Exception ex)
            {
                _elmahHelper.Raise(ex);
            }
        }

        public async Task RecordWithUserEmail(string email, IpAddressActivityType activityType, string ipAddress)
        {
            try
            {
                var user = await _dbContext.Users.SingleAsync(u => u.Email.Equals(email));
                await RecordActivity(user.Id, activityType, ipAddress);
            }
            catch (Exception ex)
            {
                _elmahHelper.Raise(ex);
            }
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
