using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace Epsilon.Logic.Services
{
    public class UserAccountMaintenanceService : IUserAccountMaintenanceService
    {
        private readonly IClock _clock;
        private readonly IEpsilonContext _dbContext;
        private readonly IUserAccountMaintenanceServiceConfig _userAccountMaintenanceServiceConfig;
        private readonly IUserTokenService _userTokenService;
        private readonly IAdminAlertService _adminAlertService;
        private readonly IAdminEventLogService _adminEventLogService;
        private readonly IElmahHelper _elmahHelper;

        public UserAccountMaintenanceService(
            IClock clock,
            IEpsilonContext dbContext,
            IUserAccountMaintenanceServiceConfig userAccountMaintenanceServiceConfig,
            IUserTokenService userTokenService,
            IAdminAlertService adminAlertService,
            IAdminEventLogService adminEventLogService,
            IElmahHelper elmahHelper)
        {
            _clock = clock;
            _dbContext = dbContext;
            _userAccountMaintenanceServiceConfig = userAccountMaintenanceServiceConfig;
            _userTokenService = userTokenService;
            _adminAlertService = adminAlertService;
            _adminEventLogService = adminEventLogService;
            _elmahHelper = elmahHelper;
        }

        public async Task<bool> DoMaintenance(string email)
        {
            try {
                var user = await _dbContext.Users.SingleAsync(u => u.Email.Equals(email));
                var success = await CheckForUnrewardedOutgoingVerifications(user.Id);
                return success;
            }
            catch (Exception ex)
            {
                _elmahHelper.Raise(ex);
                await RaiseMaintenanceThrewException(email);
                return false;
            }
        }

        private async Task<bool> CheckForUnrewardedOutgoingVerifications(string userId)
        {
            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var cutoff = _clock.OffsetNow - _userAccountMaintenanceServiceConfig.OutgoingVerification_RewardSendersIfNoneUsed_AfterPeriod;

                var unrewardedOutgoingVerifications = await _dbContext.TenantVerifications
                    .Include(x => x.TenancyDetailsSubmission)
                    .Include(x => x.TenancyDetailsSubmission.TenantVerifications)
                    .Where(x => x.CreatedOn < cutoff)
                    .Where(x => x.AssignedToId.Equals(userId))
                    .Where(x => !x.SenderRewardedOn.HasValue)
                    .ToListAsync();

                var outgoingVerificationsToReward = unrewardedOutgoingVerifications
                    // There should be at least on more verification
                    .Where(x => x.TenancyDetailsSubmission.TenantVerifications.Count > 1)
                    .Where(x => x.TenancyDetailsSubmission.TenantVerifications.All(y => !y.SenderRewardedOn.HasValue && y.CreatedOn < cutoff))
                    .SelectMany(x => x.TenancyDetailsSubmission.TenantVerifications).ToList();

                if (outgoingVerificationsToReward.Any())
                {

                    foreach (var verificationToReward in outgoingVerificationsToReward)
                    {
                        verificationToReward.SenderRewardedOn = _clock.OffsetNow;
                        _dbContext.Entry(verificationToReward).State = EntityState.Modified;
                        var status = await _userTokenService.MakeTransaction(
                            verificationToReward.AssignedToId, TokenRewardKey.EarnPerVerificationMailSent, verificationToReward.UniqueId);
                        if (status != TokenAccountTransactionStatus.Success)
                        {
                            await RaiseCheckForUnrewardedOutgoingVerificationsTokenTransactionFailed(userId, verificationToReward.AssignedToId);
                            // This should always be Success because I am crediting tokens, however if this
                            // is not the case I return here without saving or committing the transaction.
                            return false;
                        }
                    }

                    await _dbContext.SaveChangesAsync();
                }

                transactionScope.Complete();

                return true;
            }
        }

        private async Task RaiseMaintenanceThrewException(string maintenanceUserEmail)
        {
            _adminAlertService.SendAlert(AdminAlertKey.UserAccountMaintenanceThrewException);
            var extraInfo = new Dictionary<string, object>
            {
                { AdminEventLogExtraInfoKey.MaintenanceUserEmail, maintenanceUserEmail }
            };
            await _adminEventLogService.Log(AdminEventLogKey.UserAccountMaintenanceThrewException, extraInfo);
        }

        private async Task RaiseCheckForUnrewardedOutgoingVerificationsTokenTransactionFailed(string maintenanceUserId, string failedTransactionUserId)
        {
            _adminAlertService.SendAlert(AdminAlertKey.UserAccountMaintenanceCheckForUnrewardedOutgoingVerificationsTokenTransactionFailed);
            var extraInfo = new Dictionary<string, object>
            {
                { AdminEventLogExtraInfoKey.MaintenanceUserId, maintenanceUserId },
                { AdminEventLogExtraInfoKey.FailedTransactionUserId, failedTransactionUserId }
            };
            await _adminEventLogService.Log(AdminEventLogKey.UserAccountMaintenanceCheckForUnrewardedOutgoingVerificationsTokenTransactionFailed, extraInfo);
        }
    }
}
