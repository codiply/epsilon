using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Entities.Interfaces;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.JsonModels;
using Epsilon.Logic.Models;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.Services.Interfaces.OutgoingVerification;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using Epsilon.Resources.Common;
using Epsilon.Resources.Logic.OutgoingVerification;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using static Epsilon.Logic.Helpers.RandomStringHelper;

namespace Epsilon.Logic.Services
{
    public class OutgoingVerificationService : IOutgoingVerificationService
    {
        private readonly IClock _clock;
        private readonly IAppCache _appCache;
        private readonly IAppCacheHelper _appCacheHelper;
        private readonly IEpsilonContext _dbContext;
        private readonly IAntiAbuseService _antiAbuseService;
        private readonly IOutgoingVerificationServiceConfig _outgoingVerificationServiceConfig;
        private readonly IRandomFactory _randomFactory;
        private readonly IUserResidenceService _userResidenceService;

        public OutgoingVerificationService(
            IClock clock,
            IAppCache appCache,
            IAppCacheHelper appCacheHelper,
            IEpsilonContext dbContext,
            IAntiAbuseService antiAbuseService,
            IOutgoingVerificationServiceConfig outgoingVerificationServiceConfig,
            IRandomFactory randomFactory,
            IUserResidenceService userResidenceService)
        {
            _clock = clock;
            _appCache = appCache;
            _appCacheHelper = appCacheHelper;
            _dbContext = dbContext;
            _antiAbuseService = antiAbuseService;
            _outgoingVerificationServiceConfig = outgoingVerificationServiceConfig;
            _randomFactory = randomFactory;
            _userResidenceService = userResidenceService;
        }

        public async Task<bool> VerificationIsAssignedToUser(string userId, Guid verificationUniqueId)
        {
            var verification = await _dbContext.TenantVerifications
                .SingleOrDefaultAsync(v => v.AssignedToId.Equals(userId) && v.UniqueId.Equals(verificationUniqueId));
            return verification != null;
        }

        public async Task<MyOutgoingVerificationsSummaryResponse> GetUserOutgoingVerificationsSummaryWithCaching(
            string userId, bool limitItemsReturned)
        {
            return await _appCache.GetAsync(
                AppCacheKey.GetUserOutgoingVerificationsSummary(userId, limitItemsReturned), 
                () => GetUserOutgoingVerificationsSummary(userId, limitItemsReturned), 
                _outgoingVerificationServiceConfig.MyOutgoingVerificationsSummary_CachingPeriod,
                WithLock.No);
        }

        public async Task<MyOutgoingVerificationsSummaryResponse> GetUserOutgoingVerificationsSummary(
            string userId, bool limitItemsReturned)
        {
            var now = _clock.OffsetNow;
            var expiryPeriod = TimeSpan.FromDays(_outgoingVerificationServiceConfig.Instructions_ExpiryPeriodInDays);

            var query = _dbContext.TenantVerifications
                .Include(x => x.TenancyDetailsSubmission.Address)
                .Where(x => x.AssignedToId.Equals(userId))
                .OrderByDescending(x => x.CreatedOn);

            List<TenantVerification> verifications;
            var moreItemsExist = false;
            if (limitItemsReturned)
            {
                var limit = _outgoingVerificationServiceConfig.MyOutgoingVerificationsSummary_ItemsLimit;
                verifications = await query.Take(limit + 1).ToListAsync();
                if (verifications.Count > limit)
                {
                    moreItemsExist = true;
                    verifications = verifications.Take(limit).ToList();
                }
            }
            else
            {
                verifications = await query.ToListAsync();
            }

            return new MyOutgoingVerificationsSummaryResponse
            {
                moreItemsExist = moreItemsExist,
                tenantVerifications = verifications.Select(x => x.ToInfo(now, expiryPeriod)).ToList()
            };
        }

        public async Task<PickVerificationOutcome> Pick(
            string userId,
            string userIpAddress,
            Guid verificationUniqueId)
        {
            var uiAlerts = new List<UiAlert>();

            if (_outgoingVerificationServiceConfig.GlobalSwitch_DisablePickOutgoingVerification)
                return new PickVerificationOutcome
                {
                    IsRejected = true,
                    RejectionReason = OutgoingVerificationResources.GlobalSwitch_PickOutgoingVerificationDisabled_Message,
                    VerificationUniqueId = null
                };

            var userResidenceResponse = await _userResidenceService.GetResidence(userId);
            if (userResidenceResponse.HasNoSubmissions)
            {
                return new PickVerificationOutcome
                {
                    IsRejected = true,
                    RejectionReason = OutgoingVerificationResources.Pick_CannotDetermineUserResidenceErrorMessage,
                    VerificationUniqueId = null
                };
            }

            var countryId = userResidenceResponse.Address.CountryIdAsEnum();

            var antiAbuseServiceResponse = await _antiAbuseService.CanPickOutgoingVerification(userId, userIpAddress, userResidenceResponse.Address.CountryIdAsEnum());
            if (antiAbuseServiceResponse.IsRejected)
                return new PickVerificationOutcome
                {
                    IsRejected = true,
                    RejectionReason = antiAbuseServiceResponse.RejectionReason,
                    VerificationUniqueId = null
                };

            var verificationsPerTenancyDetailsSubmission = _outgoingVerificationServiceConfig.VerificationsPerTenancyDetailsSubmission;

            var submissionIdsToAvoid = await _dbContext.TenantVerifications
                .Where(v => v.AssignedToId.Equals(userId) || v.AssignedByIpAddress.Equals(userIpAddress))
                .Select(v => v.TenancyDetailsSubmissionId)
                .Distinct()
                .ToListAsync();

            // I create a box around the user residence and avoid addresses in that box.
            var minDegreesDistance = _outgoingVerificationServiceConfig.Pick_MinDegreesDistanceInAnyDirection;
            var latitudeLowerBound = userResidenceResponse.Address.Geometry.Latitude - minDegreesDistance;
            var latitudeUpperBound = userResidenceResponse.Address.Geometry.Latitude + minDegreesDistance;
            var longitudeLowerBound = userResidenceResponse.Address.Geometry.Longitude - minDegreesDistance;
            var longitudeUpperBound = userResidenceResponse.Address.Geometry.Longitude + minDegreesDistance;

            var query = _dbContext.TenancyDetailsSubmissions
                .Include(s => s.Address)
                .Include(s => s.Address.Geometry)
                .Include(s => s.TenantVerifications)
                .Where(s => s.Address.CountryId.Equals(userResidenceResponse.Address.CountryId))
                .Where(s => !s.IsHidden)
                .Where(s => s.UserId != userId)
                .Where(s => s.CreatedByIpAddress != userIpAddress)
                .Where(s => s.Address.CreatedById != userId)
                .Where(s => s.Address.CreatedByIpAddress != userIpAddress)
                .Where(s => s.TenantVerifications.Count() < verificationsPerTenancyDetailsSubmission)
                .Where(s => !submissionIdsToAvoid.Contains(s.Id))
                .Where(s => s.Address.Geometry.Latitude < latitudeLowerBound || latitudeUpperBound < s.Address.Geometry.Latitude ||
                            s.Address.Geometry.Longitude < longitudeLowerBound || longitudeUpperBound < s.Address.Geometry.Longitude)
                .OrderBy(s => s.TenantVerifications.Count());
                
            var pickedSubmission = await query.FirstOrDefaultAsync();

            if (pickedSubmission == null)
            {
                return new PickVerificationOutcome
                {
                    IsRejected = true,
                    RejectionReason = OutgoingVerificationResources.Pick_NoVerificationAssignableToUser_RejectionMessage,
                    VerificationUniqueId = null
                };
            }

            var random = _randomFactory.Create(_clock.OffsetNow.Millisecond);

            var tenantVerification = new TenantVerification()
            {
                UniqueId = verificationUniqueId,
                TenancyDetailsSubmissionId = pickedSubmission.Id,
                AssignedToId = userId,
                AssignedByIpAddress = userIpAddress,
                SecretCode = RandomStringHelper.GetString(random, AppConstant.SECRET_CODE_MAX_LENGTH, CharacterCase.Upper)
            };

            _dbContext.TenantVerifications.Add(tenantVerification);
            await _dbContext.SaveChangesAsync();

            uiAlerts.Add(new UiAlert
            {
                Type = UiAlertType.Success,
                Message = string.Format(
                    OutgoingVerificationResources.Pick_SuccessMessage,
                    _outgoingVerificationServiceConfig.Instructions_ExpiryPeriodInDays)
            });

            _appCacheHelper.RemoveCachedUserOutgoingVerificationsSummary(userId);
            _appCacheHelper.RemoveCachedUserSubmissionsSummary(pickedSubmission.UserId);

            return new PickVerificationOutcome
            {
                IsRejected = false,
                VerificationUniqueId = tenantVerification.UniqueId,
                UiAlerts = uiAlerts
            };
        }

        // TODO_TEST_PANOS
        public async Task<GetVerificationMessageOutcome> GetVerificationMessage(string userId, Guid verificationUniqueId)
        {
            // TODO_TEST_PANOS

            var verification = await GetVerificationForUser(userId, verificationUniqueId,
                includeTenancyDetailsSubmission: true, includeAddress: true, includeOtherVerifications: false);
            if (verification == null)
            {
                // TODO_TEST_PANOS
                return new GetVerificationMessageOutcome
                {
                    IsRejected = true,
                    RejectionReason = CommonResources.GenericInvalidRequestMessage
                };
            }

            var now = _clock.OffsetNow;
            var expiryPeriod = TimeSpan.FromDays(_outgoingVerificationServiceConfig.Instructions_ExpiryPeriodInDays);

            if (!verification.CanViewInstructions(now, expiryPeriod))
            {
                // TODO_TEST_PANOS
                return new GetVerificationMessageOutcome
                {
                    IsRejected = true,
                    RejectionReason = CommonResources.GenericInvalidActionMessage
                };
            }

            var reciepientAddress = AddressModel.FromEntity(verification.TenancyDetailsSubmission.Address);

            var messageArguments = new VerificationMessageArgumentsModel
            {
                CountryId = reciepientAddress.CountryIdAsEnum,
                SecretCode = verification.SecretCode
            };

            return new GetVerificationMessageOutcome
            {
                IsRejected = false,
                MessageArguments = messageArguments
            };
        }

        // TODO_TEST_PANOS
        public async Task<GetInstructionsOutcome> GetInstructions(string userId, Guid verificationUniqueId)
        {
            // TODO_TEST_PANOS

            var verification = await GetVerificationForUser(userId, verificationUniqueId,
                includeTenancyDetailsSubmission: true, includeAddress: true, includeOtherVerifications: true);
            if (verification == null)
            {
                // TODO_TEST_PANOS
                return new GetInstructionsOutcome
                {
                    IsRejected = true,
                    RejectionReason = CommonResources.GenericInvalidRequestMessage
                };
            }

            var now = _clock.OffsetNow;
            var expiryPeriod = TimeSpan.FromDays(_outgoingVerificationServiceConfig.Instructions_ExpiryPeriodInDays);

            if (!verification.CanViewInstructions(now, expiryPeriod))
            {
                // TODO_TEST_PANOS
                return new GetInstructionsOutcome
                {
                    IsRejected = true,
                    RejectionReason = CommonResources.GenericInvalidActionMessage
                };
            }

            var otherUserHasMarkedAddressAsInvalid = verification.TenancyDetailsSubmission
                .TenantVerifications.Any(v => !v.AssignedToId.Equals(userId) && v.MarkedAddressAsInvalidOn.HasValue);

            var reciepientAddress = AddressModel.FromEntity(verification.TenancyDetailsSubmission.Address);

            var messageArguments = new VerificationMessageArgumentsModel
            {
                CountryId = reciepientAddress.CountryIdAsEnum,
                SecretCode = verification.SecretCode
            };

            var instructions = new OutgoingVerificationInstructionsModel
            {
                RecipientAddress = reciepientAddress,
                MessageArguments = messageArguments,
                VerificationUniqueId = verificationUniqueId,
                OtherUserHasMarkedAddressAsInvalid = otherUserHasMarkedAddressAsInvalid,
                CanMarkAddressAsInvalid = verification.CanMarkAddressAsInvalid(),
                CanMarkAsSent = verification.CanMarkAsSent()
            };

            return new GetInstructionsOutcome
            {
                IsRejected = false,
                Instructions = instructions,
            };
        }

        // TODO_TEST_PANOS
        public async Task<MarkVerificationAsSentOutcome> MarkAsSent(string userId, Guid verificationUniqueId)
        {
            var uiAlerts = new List<UiAlert>();

            var verification = await GetVerificationForUser(
                userId, verificationUniqueId,
                includeTenancyDetailsSubmission: true, includeAddress: false, includeOtherVerifications: false);
            if (verification == null)
            {
                // TODO_TEST_PANOS
                return new MarkVerificationAsSentOutcome
                {
                    IsRejected = true,
                    RejectionReason = CommonResources.GenericInvalidRequestMessage
                };
            }

            if (!verification.CanMarkAsSent())
            {
                // TODO_TEST_PANOS
                return new MarkVerificationAsSentOutcome
                {
                    IsRejected = true,
                    RejectionReason = CommonResources.GenericInvalidActionMessage
                };
            }

            // TODO_TEST_PANOS
            verification.MarkedAsSentOn = _clock.OffsetNow;
            _dbContext.Entry(verification).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            uiAlerts.Add(new UiAlert
            {
                Type = UiAlertType.Success,
                // TODO_TEST_PANOS
                Message = OutgoingVerificationResources.MarkAsSent_SuccessMessage
            });

            _appCacheHelper.RemoveCachedUserOutgoingVerificationsSummary(userId);
            _appCacheHelper.RemoveCachedUserSubmissionsSummary(verification.TenancyDetailsSubmission.UserId);

            // TODO_TEST_PANOS
            return new MarkVerificationAsSentOutcome
            {
                IsRejected = false,
                UiAlerts = uiAlerts
            };
        }

        // TODO_TEST_PANOS
        public async Task<MarkAddressAsInvalidOutcome> MarkAddressAsInvalid(string userId, Guid verificationUniqueId)
        {
            var uiAlerts = new List<UiAlert>();

            var verification = await GetVerificationForUser(
                userId, verificationUniqueId,
                includeTenancyDetailsSubmission: false, includeAddress: false, includeOtherVerifications: false);
            if (verification == null)
            {
                // TODO_TEST_PANOS
                return new MarkAddressAsInvalidOutcome
                {
                    IsRejected = true,
                    RejectionReason = CommonResources.GenericInvalidRequestMessage
                };
            }

            if (!verification.CanMarkAddressAsInvalid())
            {
                // TODO_TEST_PANOS
                return new MarkAddressAsInvalidOutcome
                {
                    IsRejected = true,
                    RejectionReason = CommonResources.GenericInvalidActionMessage
                };
            }

            // TODO_TEST_PANOS
            verification.MarkedAddressAsInvalidOn = _clock.OffsetNow;
            _dbContext.Entry(verification).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            uiAlerts.Add(new UiAlert
            {
                Type = UiAlertType.Success,
                // TODO_TEST_PANOS
                Message = OutgoingVerificationResources.MarkAddressAsInvalid_SuccessMessage
            });

            _appCacheHelper.RemoveCachedUserOutgoingVerificationsSummary(userId);

            // TODO_TEST_PANOS
            return new MarkAddressAsInvalidOutcome
            {
                IsRejected = false,
                UiAlerts = uiAlerts
            };
        }

        private async Task<TenantVerification> GetVerificationForUser(
            string assignedUserId, Guid uniqueId, 
            bool includeTenancyDetailsSubmission, bool includeAddress, bool includeOtherVerifications)
        {
            var query = _dbContext.TenantVerifications
                .Include(x => x.TenancyDetailsSubmission);

            if (includeTenancyDetailsSubmission || includeAddress)
            {
                query = query.Include(s => s.TenancyDetailsSubmission);
            }

            if (includeAddress)
            {
                query = query
                    .Include(s => s.TenancyDetailsSubmission.Address)
                    .Include(s => s.TenancyDetailsSubmission.Address.Country);
            }

            if (includeOtherVerifications)
            {
                query = query
                    .Include(s => s.TenancyDetailsSubmission.TenantVerifications);
            }

            var submission = await query
                .Where(s => s.UniqueId.Equals(uniqueId))
                .Where(s => s.AssignedToId.Equals(assignedUserId))
                .SingleOrDefaultAsync();

            return submission;
        }
    }
}
