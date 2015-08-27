using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Entities.Interfaces;
using Epsilon.Logic.Forms.Submission;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.JsonModels;
using Epsilon.Logic.Models;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.Services.Interfaces.TenancyDetailsSubmission;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using Epsilon.Resources.Common;
using Epsilon.Resources.Logic.TenancyDetailsSubmission;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace Epsilon.Logic.Services
{
    public class TenancyDetailsSubmissionService : ITenancyDetailsSubmissionService
    {
        private readonly IClock _clock;
        private readonly IAppCache _appCache;
        private readonly IAppCacheHelper _appCacheHelper;
        private readonly ITenancyDetailsSubmissionServiceConfig _tenancyDetailsSubmissionServiceConfig;
        private readonly IEpsilonContext _dbContext;
        private readonly IAddressService _addressService;
        private readonly IAntiAbuseService _antiAbuseService;
        private readonly IUserTokenService _userTokenService;
        private readonly IUserInterfaceCustomisationService _userInterfaceCustomisationService;

        public TenancyDetailsSubmissionService(
            IClock clock,
            IAppCache appCache,
            IAppCacheHelper appCacheHelper,
            ITenancyDetailsSubmissionServiceConfig tenancyDetailsSubmissionServiceConfig,
            IEpsilonContext dbContext,
            IAddressService addressService,
            IAntiAbuseService antiAbuseService,
            IUserTokenService userTokenService,
            IUserInterfaceCustomisationService userInterfaceCustomisationService)
        {
            _clock = clock;
            _appCache = appCache;
            _appCacheHelper = appCacheHelper;
            _tenancyDetailsSubmissionServiceConfig = tenancyDetailsSubmissionServiceConfig;
            _dbContext = dbContext;
            _addressService = addressService;
            _antiAbuseService = antiAbuseService;
            _userTokenService = userTokenService;
            _userInterfaceCustomisationService = userInterfaceCustomisationService;
        }

        public async Task<bool> SubmissionBelongsToUser(string userId, Guid submissionUniqueId)
        {
            var result = await _dbContext.TenancyDetailsSubmissions
                .SingleOrDefaultAsync(s => s.UserId.Equals(userId) && s.UniqueId.Equals(submissionUniqueId));
            return result != null;
        }

        public async Task<MySubmissionsSummaryResponse> GetUserSubmissionsSummaryWithCaching(
            string userId, bool limitItemsReturned)
        {
            // TODO_TEST_PANOS: unit test
            return await _appCache.GetAsync(
                AppCacheKey.GetUserSubmissionsSummary(userId, limitItemsReturned),
                () => GetUserSubmissionsSummary(userId, limitItemsReturned), 
                _tenancyDetailsSubmissionServiceConfig.MySubmissionsSummary_CachingPeriod,
                WithLock.No);
        }

        public async Task<MySubmissionsSummaryResponse> GetUserSubmissionsSummary(string userId, bool limitItemsReturned)
        {
            var query = _dbContext.TenancyDetailsSubmissions
                .Include(x => x.TenantVerifications)
                .Include(x => x.Address)
                .Include(x => x.Address.Country)
                .Where(x => x.UserId.Equals(userId))
                .OrderByDescending(x => x.CreatedOn);

            List<TenancyDetailsSubmission> submissions;
            var moreItemsExist = false;
            if (limitItemsReturned)
            {
                var limit = _tenancyDetailsSubmissionServiceConfig.MySubmissionsSummary_ItemsLimit;
                submissions = await query.Take(limit + 1).ToListAsync();
                if (submissions.Count > limit)
                {
                    moreItemsExist = true;
                    submissions = submissions.Take(limit).ToList();
                }
            }
            else
            {
                submissions = await query.ToListAsync();
            }        

            return new MySubmissionsSummaryResponse
            {
                moreItemsExist = moreItemsExist,
                tenancyDetailsSubmissions = submissions.Select(x => x.ToInfo()).ToList()
            };
        }

        public async Task<CreateTenancyDetailsSubmissionOutcome> Create(
            string userId, 
            string userIpAddress, 
            Guid submissionUniqueId,
            Guid addressUniqueId)
        {
            var uiAlerts = new List<UiAlert>();

            if (_tenancyDetailsSubmissionServiceConfig.GlobalSwitch_DisableCreateTenancyDetailsSubmission)
                return new CreateTenancyDetailsSubmissionOutcome
                {
                    IsRejected = true,
                    RejectionReason = TenancyDetailsSubmissionResources.GlobalSwitch_CreateTenancyDetailsSubmissionDisabled_Message,
                    ReturnToForm = false,
                    TenancyDetailsSubmissionUniqueId = null
                };

            var address = await _addressService.GetAddress(addressUniqueId);
            if (address == null)
            {
                return new CreateTenancyDetailsSubmissionOutcome
                {
                    IsRejected = true,
                    RejectionReason = TenancyDetailsSubmissionResources.Create_AddressNotFoundMessage
                };
            }

            var antiAbuseCheck = await _antiAbuseService.CanCreateTenancyDetailsSubmission(userId, userIpAddress, address.CountryIdAsEnum());
            if (antiAbuseCheck.IsRejected)
            {
                return new CreateTenancyDetailsSubmissionOutcome
                {
                    IsRejected = true,
                    RejectionReason = antiAbuseCheck.RejectionReason
                };
            }

            if (!_tenancyDetailsSubmissionServiceConfig.Create_DisableFrequencyPerAddressCheck)
            {
                var tooManyRecentSubmissionsExist = await TooManyRecentSubmissionsExist(address.Id);
                if (tooManyRecentSubmissionsExist)
                    return new CreateTenancyDetailsSubmissionOutcome
                    {
                        IsRejected = true,
                        RejectionReason = TenancyDetailsSubmissionResources.Create_MaxFrequencyPerAddressCheck_RejectionMessage
                    };
            }

            var tenancyDetailsSubmission = await DoCreate(userId, userIpAddress, submissionUniqueId, address.Id);

            uiAlerts.Add(new UiAlert
            {
                Type = UiAlertType.Success,
                Message = TenancyDetailsSubmissionResources.Create_SuccessMessage
            });

            _appCacheHelper.RemoveCachedUserSubmissionsSummary(userId);
            // TODO_TEST_PANOS
            _userInterfaceCustomisationService.ClearCachedCustomisationForUser(userId);

            return new CreateTenancyDetailsSubmissionOutcome
            {
                IsRejected = false,
                TenancyDetailsSubmissionUniqueId = tenancyDetailsSubmission.UniqueId,
                // TODO_TEST_PANOS
                UiAlerts = uiAlerts
            };
        }

        public async Task<EnterVerificationCodeOutcome> EnterVerificationCode(string userId, VerificationCodeForm form)
        {
            var uiAlerts = new List<UiAlert>();

            var submission = await GetSubmissionForUser(userId, form.TenancyDetailsSubmissionUniqueId);
            if (submission == null)
            {
                return new EnterVerificationCodeOutcome
                {
                    IsRejected = true,
                    RejectionReason = CommonResources.GenericInvalidRequestMessage
                };
            }

            if (!submission.CanEnterVerificationCode())
            {
                return new EnterVerificationCodeOutcome
                {
                    IsRejected = true,
                    RejectionReason = CommonResources.GenericInvalidActionMessage
                };
            }

            var trimmedVerificationCodeToLower = form.VerificationCode.Trim().ToLowerInvariant();

            var verification =
                submission.TenantVerifications.SingleOrDefault(v => v.SecretCode.ToLowerInvariant().Equals(trimmedVerificationCodeToLower));

            if (verification == null)
            {
                return new EnterVerificationCodeOutcome
                {
                    IsRejected = true,
                    ReturnToForm = true,
                    RejectionReason = TenancyDetailsSubmissionResources.EnterVerification_InvalidVerificationCode_RejectionMessage
                };
            }

            if (verification.StepVerificationReceivedDone())
            {
                return new EnterVerificationCodeOutcome
                {
                    IsRejected = true,
                    ReturnToForm = false,
                    RejectionReason = TenancyDetailsSubmissionResources.EnterVerification_VerificationAlreadyUsed_RejectionMessage
                };
            }

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {

                var now = _clock.OffsetNow;

                var hasSenderBeenRewarded = verification.IsSenderRewarded();
                if (!hasSenderBeenRewarded)
                    verification.SenderRewardedOn = now; // TODO_TEST_PANOS

                verification.VerifiedOn = now;
                _dbContext.Entry(verification).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();

                uiAlerts.Add(new UiAlert
                {
                    Type = UiAlertType.Success,
                    Message = TenancyDetailsSubmissionResources.EnterVerificationCode_SuccessMessage
                });

                // TODO_TEST_PANOS: also test the correct internal reference is used.
                var recipientRewardStatus = await _userTokenService.MakeTransaction(userId, TokenRewardKey.EarnPerVerificationCodeEntered, submission.UniqueId);
                if (recipientRewardStatus == TokenAccountTransactionStatus.Success)
                {
                    uiAlerts.Add(new UiAlert
                    {
                        Message = CommonResources.TokenAccountCreditedForThisAction,
                        Type = UiAlertType.Success
                    });
                }
                else
                {
                    // This shouldn't fail, but I return failure before committing the transaction.
                    return new EnterVerificationCodeOutcome
                    {
                        IsRejected = true,
                        ReturnToForm = false,
                        RejectionReason = CommonResources.GenericErrorMessage
                    };
                }

                if (!hasSenderBeenRewarded)
                {
                    // TODO_TEST_PANOS: also test the correct internal reference is used.
                    var senderRewardStatus = await _userTokenService
                        .MakeTransaction(verification.AssignedToId, TokenRewardKey.EarnPerVerificationMailSent, verification.UniqueId);
                    if (senderRewardStatus != TokenAccountTransactionStatus.Success)
                    {
                        // TODO_TEST_PANOS
                        // This shouldn't fail, but I return failure before committing the transaction.
                        return new EnterVerificationCodeOutcome
                        {
                            IsRejected = true,
                            ReturnToForm = false,
                            RejectionReason = CommonResources.GenericErrorMessage
                        };
                    }
                }

                // Lucky Sender Logic
                if (now.Millisecond % 100 == 0)
                {
                    // TODO_TEST_PANOS: also test the correct internal reference is used.
                    var luckySenderRewardStatus = await _userTokenService
                        .MakeTransaction(verification.AssignedToId, TokenRewardKey.EarnPerVerificationLuckySender, verification.UniqueId);
                    if (luckySenderRewardStatus != TokenAccountTransactionStatus.Success)
                    {
                        // TODO_TEST_PANOS
                        // This shouldn't fail, but I return failure before committing the transaction.
                        return new EnterVerificationCodeOutcome
                        {
                            IsRejected = true,
                            ReturnToForm = false,
                            RejectionReason = CommonResources.GenericErrorMessage
                        };
                    }
                }

                // TODO_TEST_PANOS
                transactionScope.Complete();

                _appCacheHelper.RemoveCachedUserSubmissionsSummary(userId);
                _appCacheHelper.RemoveCachedUserOutgoingVerificationsSummary(verification.AssignedToId);
                // TODO_TEST_PANOS
                _userInterfaceCustomisationService.ClearCachedCustomisationForUser(userId);

                return new EnterVerificationCodeOutcome
                {
                    IsRejected = false,
                    ReturnToForm = false,
                    // TODO_TEST_PANOS
                    UiAlerts = uiAlerts
                };
            }
        }

        public async Task<SubmitTenancyDetailsOutcome> SubmitTenancyDetails(string userId, TenancyDetailsForm form)
        {
            var uiAlerts = new List<UiAlert>();

            var submission = await GetSubmissionForUser(userId, form.TenancyDetailsSubmissionUniqueId);
            if (submission == null)
            {
                return new SubmitTenancyDetailsOutcome
                {
                    IsRejected = true,
                    RejectionReason = CommonResources.GenericInvalidRequestMessage
                };
            }

            if (!submission.CanSubmitTenancyDetails())
            {
                return new SubmitTenancyDetailsOutcome
                {
                    IsRejected = true,
                    RejectionReason = CommonResources.GenericInvalidActionMessage
                };
            }

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                form.ApplyOnEntity(submission);
                submission.SubmittedOn = _clock.OffsetNow;
                // TODO_TEST_PANOS
                submission.CurrencyId = submission.Address.Country.CurrencyId;

                _dbContext.Entry(submission).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();

                uiAlerts.Add(new UiAlert
                {
                    Type = UiAlertType.Success,
                    // TODO_TEST_PANOS
                    Message = TenancyDetailsSubmissionResources.SubmitTenancyDetails_SuccessMessage
                });

                // TODO_TEST_PANOS: also test the correct internal reference is used.
                var rewardStatus = await _userTokenService.MakeTransaction(userId, TokenRewardKey.EarnPerTenancyDetailsSubmission, submission.UniqueId);
                if (rewardStatus == TokenAccountTransactionStatus.Success)
                {
                    uiAlerts.Add(new UiAlert
                    {
                        Message = CommonResources.TokenAccountCreditedForThisAction,
                        Type = UiAlertType.Success
                    });
                }
                else
                {
                    // This shouldn't fail, but I return failure before committing the transaction.
                    return new SubmitTenancyDetailsOutcome
                    {
                        IsRejected = true,
                        ReturnToForm = false,
                        RejectionReason = CommonResources.GenericErrorMessage
                    };
                }

                transactionScope.Complete();

                _appCacheHelper.RemoveCachedUserSubmissionsSummary(userId);

                return new SubmitTenancyDetailsOutcome
                {
                    IsRejected = false,
                    UiAlerts = uiAlerts
                };
            }
        }

        // TODO_TEST_PANOS
        public async Task<GetSubmissionAddressOutcome> GetSubmissionAddress(string userId, Guid submissionUniqueId)
        {
            var submission = await _dbContext.TenancyDetailsSubmissions
                .Where(s => s.UniqueId.Equals(submissionUniqueId))
                .Where(s => s.UserId.Equals(userId))
                .Include(s => s.Address)
                .Include(s => s.Address.Country)
                .SingleOrDefaultAsync();

            if (submission == null)
            {
                return new GetSubmissionAddressOutcome { SubmissionNotFound = true };
            }

            return new GetSubmissionAddressOutcome
            {
                SubmissionNotFound = false,
                Address = submission.Address
            };
        }

        private async Task<TenancyDetailsSubmission> GetSubmissionForUser(string userId, Guid uniqueId)
        {
            var submission = await  _dbContext.TenancyDetailsSubmissions
                .Include(s => s.Address)
                .Include(s => s.Address.Country)
                .Include(s => s.TenantVerifications)
                .Where(s => s.UniqueId.Equals(uniqueId))
                .Where(s => s.UserId.Equals(userId))
                .SingleOrDefaultAsync();

            return submission;
        }

        private async Task<bool> TooManyRecentSubmissionsExist(long addressId)
        {
            var maxFrequency = _tenancyDetailsSubmissionServiceConfig.Create_MaxFrequencyPerAddress;

            var windowStart = _clock.OffsetNow - maxFrequency.Period;


            int actualTimes;

            var address = await _dbContext.Addresses.FindAsync(addressId);

            if (string.IsNullOrWhiteSpace(address.DistinctAddressCode))
            {
                actualTimes = await _dbContext.TenancyDetailsSubmissions
                    .Where(s => s.AddressId.Equals(addressId))
                    .Where(a => a.CreatedOn > windowStart)
                    .CountAsync();
            }
            else
            {
                actualTimes = await _dbContext.TenancyDetailsSubmissions
                    .Where(s => s.Address.DistinctAddressCode.Equals(address.DistinctAddressCode))
                    .Where(a => a.CreatedOn > windowStart)
                    .CountAsync();
            }

            return actualTimes >= maxFrequency.Times;
        }

        private async Task<TenancyDetailsSubmission> DoCreate(
            string userId,
            string userIpAddress,
            Guid submissionUniqueId,
            long addressId)
        {
            var entity = new TenancyDetailsSubmission
            {
                UniqueId = submissionUniqueId,
                UserId = userId,
                CreatedByIpAddress = userIpAddress,
                AddressId = addressId
            };
            _dbContext.TenancyDetailsSubmissions.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }
    }
}
