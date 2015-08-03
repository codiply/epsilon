﻿using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using Epsilon.Resources.Logic.TenancyDetailsSubmission;
using Epsilon.Resources.Web.Submission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Epsilon.Logic.JsonModels;
using Epsilon.Logic.Services.Interfaces.TenancyDetailsSubmission;
using Epsilon.Logic.Forms.Submission;
using Epsilon.Resources.Common;
using Epsilon.Logic.Dtos;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.Constants;

namespace Epsilon.Logic.Services
{
    public class TenancyDetailsSubmissionService : ITenancyDetailsSubmissionService
    {
        private readonly IClock _clock;
        private readonly IAppCache _appCache;
        private readonly ITenancyDetailsSubmissionServiceConfig _tenancyDetailsSubmissionServiceConfig;
        private readonly IEpsilonContext _dbContext;
        private readonly IAddressService _addressService;
        private readonly IAntiAbuseService _antiAbuseService;
        private readonly IUserTokenService _userTokenService;

        public TenancyDetailsSubmissionService(
            IClock clock,
            IAppCache appCache,
            ITenancyDetailsSubmissionServiceConfig tenancyDetailsSubmissionServiceConfig,
            IEpsilonContext dbContext,
            IAddressService addressService,
            IAntiAbuseService antiAbuseService,
            IUserTokenService userTokenService)
        {
            _clock = clock;
            _appCache = appCache;
            _tenancyDetailsSubmissionServiceConfig = tenancyDetailsSubmissionServiceConfig;
            _dbContext = dbContext;
            _addressService = addressService;
            _antiAbuseService = antiAbuseService;
            _userTokenService = userTokenService;
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
            // TODO_PANOS_TEST: unit test
            return await _appCache.GetAsync(
                AppCacheKey.GetUserSubmissionsSummary(userId, limitItemsReturned),
                () => GetUserSubmissionsSummary(userId, limitItemsReturned), 
                _tenancyDetailsSubmissionServiceConfig.TenancyDetailsSubmission_MySubmissionsSummary_CachingPeriod,
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
                    RejectionReason = SubmissionResources.UseAddressConfirmed_AddressNotFoundMessage
                };
            }

            var antiAbuseCheck = await _antiAbuseService.CanCreateTenancyDetailsSubmission(userId, userIpAddress);
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
                Message = TenancyDetailsSubmissionResources.UseAddress_SuccessMessage
            });

            RemoveCachedUserSubmissionsSummary(userId);

            return new CreateTenancyDetailsSubmissionOutcome
            {
                IsRejected = false,
                TenancyDetailsSubmissionUniqueId = tenancyDetailsSubmission.UniqueId,
                // TODO_PANOS_TEST
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

            if(!submission.CanEnterVerificationCode())
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

            verification.VerifiedOn = _clock.OffsetNow;
            _dbContext.Entry(verification).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            uiAlerts.Add(new UiAlert
            {
                Type = UiAlertType.Success,
                Message = TenancyDetailsSubmissionResources.EnterVerificationCode_SuccessMessage
            });

            // TODO_PANOS_TEST: also test the correct internal reference is used.
            var rewardStatus1 = await _userTokenService.MakeTransaction(userId, TokenRewardKey.EarnPerVerificationCodeEntered, submission.UniqueId);
            if (rewardStatus1 == TokenAccountTransactionStatus.Success)
            {
                uiAlerts.Add(new UiAlert
                {
                    Message = "Your account has been credited with tokens for this action.", // TODO_PANOS: put in resource
                    Type = UiAlertType.Success
                });
            }
            else
            {
                // TODO_PANOS: return failure before committing the transaction later on.
            }

            // TODO_PANOS_TEST: also test the correct internal reference is used.
            var rewardStatus2 = await _userTokenService
                .MakeTransaction(verification.AssignedToId, TokenRewardKey.EarnPerVerificationMailSent, verification.UniqueId);
            if (rewardStatus2 != TokenAccountTransactionStatus.Success)
            {
                // TODO_PANOS: return failure before committing the transaction later on.
            }

            // TODO_PANOS: commit the transaction down here

            RemoveCachedUserSubmissionsSummary(userId);

            return new EnterVerificationCodeOutcome
            {
                IsRejected = false,
                ReturnToForm = false,
                // TODO_PANOS_TEST
                UiAlerts = uiAlerts
            };
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

            if(!submission.CanSubmitTenancyDetails())
            {
                return new SubmitTenancyDetailsOutcome
                {
                    IsRejected = true,
                    RejectionReason = CommonResources.GenericInvalidActionMessage
                };
            }

            submission.RentPerMonth = form.RentPerMonth;
            submission.NumberOfBedrooms = form.NumberOfBedrooms;
            submission.IsPartOfProperty = form.IsPartOfProperty;
            submission.MoveInDate = form.MoveInDate;
            submission.SubmittedOn = _clock.OffsetNow;
            // TODO_PANOS_TEST
            submission.CurrencyId = submission.Address.Country.CurrencyId;

            _dbContext.Entry(submission).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            uiAlerts.Add(new UiAlert
            {
                Type = UiAlertType.Success,
                // TODO_PANOS_TEST
                Message = TenancyDetailsSubmissionResources.SubmitTenancyDetails_SuccessMessage
            });

            // TODO_PANOS_TEST: also test the correct internal reference is used.
            var rewardStatus = await _userTokenService.MakeTransaction(userId, TokenRewardKey.EarnPerTenancyDetailsSubmission, submission.UniqueId);
            if (rewardStatus == TokenAccountTransactionStatus.Success)
            {
                uiAlerts.Add(new UiAlert
                {
                    Message = "Your account has been credited with tokens for this action.", // TODO_PANOS: put in resource
                    Type = UiAlertType.Success
                });
            }
            else
            {
                // TODO_PANOS: return failure before committing the transaction later on.
            }

            // TODO_PANOS: commit the transaction down here

            RemoveCachedUserSubmissionsSummary(userId);

            return new SubmitTenancyDetailsOutcome
            {
                IsRejected = false,
                UiAlerts = uiAlerts
            };
        }

        public async Task<SubmitMoveOutDetailsOutcome> SubmitMoveOutDetails(string userId, MoveOutDetailsForm form)
        {
            var uiAlerts = new List<UiAlert>();

            var submission = await GetSubmissionForUser(userId, form.TenancyDetailsSubmissionUniqueId);
            if (submission == null)
            {
                return new SubmitMoveOutDetailsOutcome
                {
                    IsRejected = true,
                    RejectionReason = CommonResources.GenericInvalidRequestMessage
                };
            }

            if (!submission.CanSubmitMoveOutDetails())
            {
                return new SubmitMoveOutDetailsOutcome
                {
                    IsRejected = true,
                    RejectionReason = CommonResources.GenericInvalidActionMessage
                };
            }

            submission.MoveOutDate = form.MoveOutDate;
            submission.MoveOutDateSubmittedOn = _clock.OffsetNow;

            _dbContext.Entry(submission).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            uiAlerts.Add(new UiAlert
            {
                Type = UiAlertType.Success,
                Message = TenancyDetailsSubmissionResources.SubmitMoveOutDetails_SuccessMessage
            });

            RemoveCachedUserSubmissionsSummary(userId);

            return new SubmitMoveOutDetailsOutcome
            {
                IsRejected = false,
                // TODO_PANOS_TEST
                UiAlerts = uiAlerts
            };
        }

        public async Task<GetSubmissionCountryOutcome> GetSubmissionCountry(string userId, Guid submissionUniqueId)
        {
            // TODO_PANOS_TEST
            var submission = await _dbContext.TenancyDetailsSubmissions
                .Where(s => s.UniqueId.Equals(submissionUniqueId))
                .Where(s => s.UserId.Equals(userId))
                .Include(s => s.Address.Country)
                .SingleOrDefaultAsync();

            if (submission == null)
            {
                return new GetSubmissionCountryOutcome { SubmissionNotFound = true };
            }

            return new GetSubmissionCountryOutcome
            {
                SubmissionNotFound = false,
                Country = submission.Address.Country
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
            var actualTimes = await _dbContext.TenancyDetailsSubmissions
                .Where(s => s.AddressId.Equals(addressId))
                .Where(a => a.CreatedOn > windowStart)
                .CountAsync();

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

        private void RemoveCachedUserSubmissionsSummary(string userId)
        {
            _appCache.Remove(AppCacheKey.GetUserSubmissionsSummary(userId, true));
            _appCache.Remove(AppCacheKey.GetUserSubmissionsSummary(userId, false));
        }
    }
}
