using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Models;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Resources.Logic.PropertyInfoAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Epsilon.Logic.JsonModels;
using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using Epsilon.Resources.Common;

namespace Epsilon.Logic.Services
{
    public class PropertyInfoAccessService : IPropertyInfoAccessService
    {
        private readonly IPropertyInfoAccessServiceConfig _propertyInfoAccessServiceConfig;
        private readonly IClock _clock;
        private readonly IAppCache _appCache;
        private readonly IEpsilonContext _dbContext;
        private readonly IAddressService _addressService;
        private readonly IUserTokenService _userTokenService;

        public PropertyInfoAccessService(
            IPropertyInfoAccessServiceConfig propertInfoAccessServiceConfig,
            IClock clock,
            IAppCache appCache,
            IEpsilonContext dbContext,
            IAddressService addressService,
            IUserTokenService userTokenService)
        {
            _propertyInfoAccessServiceConfig = propertInfoAccessServiceConfig;
            _clock = clock;
            _appCache = appCache;
            _dbContext = dbContext;
            _addressService = addressService;
            _userTokenService = userTokenService;
        }

        public async Task<MyExploredPropertiesSummaryResponse> GetUserExploredPropertiesSummaryWithCaching(
            string userId, bool limitItemsReturned)
        {
            // TODO_PANOS_TEST: unit test
            return await _appCache.GetAsync(
                AppCacheKey.GetUserExploredPropertiesSummary(userId, limitItemsReturned),
                () => GetUserExploredPropertiesSummary(userId, limitItemsReturned),
                _propertyInfoAccessServiceConfig.MyExploredPropertiesSummary_CachingPeriod,
                WithLock.No);
        }

        public async Task<MyExploredPropertiesSummaryResponse> GetUserExploredPropertiesSummary(string userId, bool limitItemsReturned)
        {
            var expiryPeriod = ExpiryPeriod();
            var now = _clock.OffsetNow;
            // TODO_PANOS_TEST
            var cutoff = now - expiryPeriod;

            // TODO_PANOS_TEST: test the whole thing
            var query = _dbContext.PropertyInfoAccesses
                .Include(x => x.Address)
                .Include(x => x.Address.Country)
                .Where(x => x.UserId.Equals(userId))
                .Where(x => x.CreatedOn > cutoff)
                .OrderByDescending(x => x.CreatedOn);

            List<PropertyInfoAccess> accesses;
            var moreItemsExist = false;
            if (limitItemsReturned)
            {
                var limit = _propertyInfoAccessServiceConfig.MyExploredPropertiesSummary_ItemsLimit;
                accesses = await query.Take(limit + 1).ToListAsync();
                if (accesses.Count > limit)
                {
                    moreItemsExist = true;
                    accesses = accesses.Take(limit).ToList();
                }
            }
            else
            {
                accesses = await query.ToListAsync();
            }

            return new MyExploredPropertiesSummaryResponse
            {
                moreItemsExist = moreItemsExist,
                exploredProperties = accesses.Select(x => x.ToExploredPropertyInfo(now, expiryPeriod)).ToList()
            };
        }

        public async Task<CreatePropertyInfoAccessOutcome> Create(
            string userId,
            string userIpAddress,
            Guid accessUniqueId,
            Guid addressUniqueId)
        {
            if (_propertyInfoAccessServiceConfig.GlobalSwitch_DisableCreatePropertyInfoAccess)
                return new CreatePropertyInfoAccessOutcome
                {
                    IsRejected = true,
                    RejectionReason = PropertyInfoAccessResources.GlobalSwitch_CreatePropertyInfoAccessDisabled_Message,
                    PropertyInfoAccessUniqueId = null
                };

            // TODO_PANOS_TEST: the whole thing

            var uiAlerts = new List<UiAlert>();

            var address = await _addressService.GetAddress(addressUniqueId);
            if (address == null)
            {
                // TODO_PANOS_TEST
                return new CreatePropertyInfoAccessOutcome
                {
                    IsRejected = true,
                    RejectionReason =  PropertyInfoAccessResources.Create_AddressNotFoundMessage
                };
            }

            var existingPropertyInfoAccess = await GetExistingUnexpiredAccess(userId, addressUniqueId);
            if (existingPropertyInfoAccess != null)
            {
                // TODO_PANOS_TEST
                return new CreatePropertyInfoAccessOutcome
                {
                    IsRejected = true,
                    RejectionReason = CommonResources.GenericInvalidActionMessage
                };
            }

            var tokenRewardKey = TokenRewardKey.SpendPerPropertyInfoAccess;
            var sufficientFundsExist = await _userTokenService.SufficientFundsExistForTransaction(userId, tokenRewardKey);

            if (!sufficientFundsExist)
            {
                return new CreatePropertyInfoAccessOutcome
                {
                    IsRejected = true,
                    RejectionReason = "Insufficient funds." // TODO_PANOS: put in a resource common with the status messages below.
                };
            }

            // TODO_PANOS: check if there is already an active property access

            var propertyInfoAccess = await DoCreate(userId, userIpAddress, accessUniqueId, address.Id);

            var tokenTransactionStatus = await _userTokenService
                .MakeTransaction(userId, tokenRewardKey, internalReference: propertyInfoAccess.UniqueId);

            if (tokenTransactionStatus == TokenAccountTransactionStatus.Success)
            {
                // TODO_PANOS: maybe add UiAlert for spending tokens.
            }
            else
            {
                return new CreatePropertyInfoAccessOutcome
                {
                    IsRejected = true,
                    RejectionReason = "Oops, something went wrong." // TODO_PANOS: translate status to message.
                };
            }

            // TODO_PANOS: commit the transaction here.

            uiAlerts.Add(new UiAlert
            {
                Type = UiAlertType.Success,
                Message = string.Format(
                    PropertyInfoAccessResources.Create_SuccessMessage, _propertyInfoAccessServiceConfig.ExpiryPeriodInDays)
            });

            RemoveCachedUserExploredPropertiesSummary(userId);

            return new CreatePropertyInfoAccessOutcome
            {
                IsRejected = false,
                PropertyInfoAccessUniqueId = propertyInfoAccess.UniqueId,
                // TODO_PANOS_TEST
                UiAlerts = uiAlerts
            };
        }

        public async Task<GetInfoOutcome> GetInfo(string userId, Guid accessUniqueId)
        {
            // TODO_PANOS_TEST
            var access = await GetPropertyInfoAccessForUser(userId, accessUniqueId);
            if (access == null)
            {
                // TODO_PANOS_TEST
                return new GetInfoOutcome
                {
                    IsRejected = true,
                    RejectionReason = CommonResources.GenericInvalidRequestMessage
                };
            }

            var now = _clock.OffsetNow;
            var expiryPeriod = TimeSpan.FromDays(_propertyInfoAccessServiceConfig.ExpiryPeriodInDays);

            if (!access.CanViewInfo(now, expiryPeriod))
            {
                // TODO_PANOS_TEST
                return new GetInfoOutcome
                {
                    IsRejected = true,
                    RejectionReason = CommonResources.GenericInvalidActionMessage
                };
            }

            // TODO_PANOS
            var propertyInfo = new ViewPropertyInfoModel
            {

            };

            return new GetInfoOutcome
            {
                IsRejected = false,
                PropertyInfo = propertyInfo
            };
        }

        public async Task<Guid?> GetExistingUnexpiredAccessUniqueId(string userId, Guid addressUniqueId)
        {
            // TODO_PANOS_TEST
            var existingUnexpiredAccess = await GetExistingUnexpiredAccess(userId, addressUniqueId);
            if (existingUnexpiredAccess == null)
                return null;
            return existingUnexpiredAccess.UniqueId;
        }

        private async Task<PropertyInfoAccess> DoCreate(
            string userId,
            string userIpAddress,
            Guid accessUniqueId,
            long addressId)
        {
            var entity = new PropertyInfoAccess
            {
                UniqueId = accessUniqueId,
                UserId = userId,
                CreatedByIpAddress = userIpAddress,
                AddressId = addressId
            };
            _dbContext.PropertyInfoAccesses.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        private async Task<PropertyInfoAccess> GetPropertyInfoAccessForUser(string userId, Guid uniqueId)
        {
            var propertyInfoAccess = await _dbContext.PropertyInfoAccesses
                .Include(a => a.Address)
                .Include(a => a.Address.Country)
                .Where(a => a.UniqueId.Equals(uniqueId))
                .Where(a => a.UserId.Equals(userId))
                .SingleOrDefaultAsync();

            return propertyInfoAccess;
        }

        private async Task<PropertyInfoAccess> GetExistingUnexpiredAccess(string userId, Guid addressUniqueId)
        {
            // TODO_PANOS_TEST
            var cutoff = _clock.OffsetNow - ExpiryPeriod();

            var propertyInfoAccess = await _dbContext.PropertyInfoAccesses
                .Include(a => a.Address)
                .Where(a => a.Address.UniqueId.Equals(addressUniqueId))
                .Where(a => a.UserId.Equals(userId))
                .Where(a => a.CreatedOn > cutoff)
                .OrderByDescending(a => a.CreatedOn)
                .FirstOrDefaultAsync();

            return propertyInfoAccess;
        }

        private void RemoveCachedUserExploredPropertiesSummary(string userId)
        {
            _appCache.Remove(AppCacheKey.GetUserExploredPropertiesSummary(userId, true));
            _appCache.Remove(AppCacheKey.GetUserExploredPropertiesSummary(userId, false));
        }

        private TimeSpan ExpiryPeriod()
        {
            return TimeSpan.FromDays(_propertyInfoAccessServiceConfig.ExpiryPeriodInDays);
        }
    }
}
