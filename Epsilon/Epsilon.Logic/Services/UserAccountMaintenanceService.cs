using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Epsilon.Logic.Constants.Enums;

namespace Epsilon.Logic.Services
{
    // TODO_PANOS_TEST: whole thing
    public class UserAccountMaintenanceService : IUserAccountMaintenanceService
    {
        private readonly IClock _clock;
        private readonly IEpsilonContext _dbContext;
        private readonly IUserAccountMaintenanceServiceConfig _userAccountMaintenanceServiceConfig;
        private readonly IUserTokenService _userTokenService;

        public UserAccountMaintenanceService(
            IClock clock,
            IEpsilonContext dbContext,
            IUserAccountMaintenanceServiceConfig userAccountMaintenanceServiceConfig,
            IUserTokenService userTokenService)
        {
            _clock = clock;
            _dbContext = dbContext;
            _userAccountMaintenanceServiceConfig = userAccountMaintenanceServiceConfig;
            _userTokenService = userTokenService;
        }

        public async Task DoMaintenance(string userId)
        {
            await CheckForUnrewardedOutgoingVerifications(userId);
        }

        private async Task CheckForUnrewardedOutgoingVerifications(string userId)
        {
            // TODO_PANOS: wrap in transaction

            var cutoff = _clock.OffsetNow - _userAccountMaintenanceServiceConfig.OutgoingVerification_RewardSendersIfNoneUsed_AfterPeriod;

            var unrewardedOutgoingVerifications = await _dbContext.TenantVerifications
                .Include(x => x.TenancyDetailsSubmission)
                .Include(x => x.TenancyDetailsSubmission.TenantVerifications)
                .Where(x => x.CreatedOn < cutoff)
                .Where(x => x.AssignedToId.Equals(userId))
                .Where(x => !x.SenderRewardedOn.HasValue)
                .ToListAsync();

            var outgoingVerificationsToReward = unrewardedOutgoingVerifications
                .Where(x => x.TenancyDetailsSubmission.TenantVerifications.All(y => !y.SenderRewardedOn.HasValue && y.CreatedOn < cutoff))
                .SelectMany(x => x.TenancyDetailsSubmission.TenantVerifications).ToList();

            foreach (var verificationToReward in outgoingVerificationsToReward)
            {
                verificationToReward.SenderRewardedOn = _clock.OffsetNow;
                _dbContext.Entry(verificationToReward).State = EntityState.Modified;
                var status = await _userTokenService.MakeTransaction(
                    verificationToReward.AssignedToId, TokenRewardKey.EarnPerVerificationMailSent, verificationToReward.UniqueId);
                if (status != TokenAccountTransactionStatus.Success)
                    // I return here without committing the transaction
                    return;
            }

            await _dbContext.SaveChangesAsync();

            // TODO_PANOS commit transaction here
        }
    }
}
