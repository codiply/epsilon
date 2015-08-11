﻿using Epsilon.Logic.Configuration.Interfaces;
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
using System.Transactions;
using Epsilon.Logic.Constants;

namespace Epsilon.Logic.Services
{
    // TODO_PANOS_TEST: whole thing
    public class UserAccountMaintenanceService : IUserAccountMaintenanceService
    {
        private readonly IClock _clock;
        private readonly IEpsilonContext _dbContext;
        private readonly IUserAccountMaintenanceServiceConfig _userAccountMaintenanceServiceConfig;
        private readonly IUserTokenService _userTokenService;
        private readonly IAdminAlertService _adminAlertService;
        private readonly IAdminEventLogService _adminEventLogService;

        public UserAccountMaintenanceService(
            IClock clock,
            IEpsilonContext dbContext,
            IUserAccountMaintenanceServiceConfig userAccountMaintenanceServiceConfig,
            IUserTokenService userTokenService,
            IAdminAlertService adminAlertService,
            IAdminEventLogService adminEventLogService)
        {
            _clock = clock;
            _dbContext = dbContext;
            _userAccountMaintenanceServiceConfig = userAccountMaintenanceServiceConfig;
            _userTokenService = userTokenService;
            _adminAlertService = adminAlertService;
            _adminEventLogService = adminEventLogService;
        }

        public async Task DoMaintenance(string userId)
        {
            try {
                await CheckForUnrewardedOutgoingVerifications(userId);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                await RaiseMaintenanceThrewException(userId);
            }
        }

        private async Task CheckForUnrewardedOutgoingVerifications(string userId)
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
                            return;
                        }
                    }

                    await _dbContext.SaveChangesAsync();
                }

                transactionScope.Complete();
            }
        }

        private async Task RaiseMaintenanceThrewException(string maintenanceUserId)
        {
            _adminAlertService.SendAlert(AdminAlertKey.UserAccountMaintenanceThrewException);
            var extraInfo = new Dictionary<string, object>
            {
                { "MaintenanceUserId", maintenanceUserId }
            };
            await _adminEventLogService.Log(AdminEventLogKey.UserAccountMaintenanceThrewException, extraInfo);
        }

        private async Task RaiseCheckForUnrewardedOutgoingVerificationsTokenTransactionFailed(string maintenanceUserId, string failedTransactionUserId)
        {
            _adminAlertService.SendAlert(AdminAlertKey.UserAccountMaintenanceCheckForUnrewardedOutgoingVerificationsTokenTransactionFailed);
            var extraInfo = new Dictionary<string, object>
            {
                { "MaintenanceUserId", maintenanceUserId },
                { "FailedTransactionUserId", failedTransactionUserId }
            };
            await _adminEventLogService.Log(AdminEventLogKey.UserAccountMaintenanceCheckForUnrewardedOutgoingVerificationsTokenTransactionFailed, extraInfo);
        }
    }
}
