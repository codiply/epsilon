using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.JsonModels;
using Epsilon.Logic.Models;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using Epsilon.Resources.Common;
using Epsilon.Resources.Logic.PropertyInfoAccess;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace Epsilon.Logic.Services
{
    public class PropertyInfoAccessService : IPropertyInfoAccessService
    {
        private readonly IPropertyInfoAccessServiceConfig _propertyInfoAccessServiceConfig;
        private readonly IClock _clock;
        private readonly IAppCache _appCache;
        private readonly IAppCacheHelper _appCacheHelper;
        private readonly IEpsilonContext _dbContext;
        private readonly IAddressService _addressService;
        private readonly IUserTokenService _userTokenService;
        private readonly ICurrencyService _currencyService;

        public PropertyInfoAccessService(
            IPropertyInfoAccessServiceConfig propertInfoAccessServiceConfig,
            IClock clock,
            IAppCache appCache,
            IAppCacheHelper appCacheHelper,
            IEpsilonContext dbContext,
            IAddressService addressService,
            IUserTokenService userTokenService,
            ICurrencyService currencyService)
        {
            _propertyInfoAccessServiceConfig = propertInfoAccessServiceConfig;
            _clock = clock;
            _appCache = appCache;
            _appCacheHelper = appCacheHelper;
            _dbContext = dbContext;
            _addressService = addressService;
            _userTokenService = userTokenService;
            _currencyService = currencyService;
        }

        public async Task<MyExploredPropertiesSummaryResponse> GetUserExploredPropertiesSummaryWithCaching(
            string userId, bool limitItemsReturned)
        {
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
            var cutoff = now - expiryPeriod;

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

                var uiAlerts = new List<UiAlert>();

                var address = await _addressService.GetAddress(addressUniqueId);
                if (address == null)
                {
                    // TODO_TEST_PANOS
                    return new CreatePropertyInfoAccessOutcome
                    {
                        IsRejected = true,
                        RejectionReason = PropertyInfoAccessResources.Create_AddressNotFoundMessage
                    };
                }

                var existingPropertyInfoAccess = await GetExistingUnexpiredAccess(userId, addressUniqueId);
                if (existingPropertyInfoAccess != null)
                {
                    // TODO_TEST_PANOS
                    return new CreatePropertyInfoAccessOutcome
                    {
                        IsRejected = true,
                        RejectionReason = CommonResources.GenericInvalidActionMessage
                    };
                }

                var completeSubmissionsExist = await _addressService.AddressHasCompleteSubmissions(addressUniqueId);

                if (!completeSubmissionsExist)
                {
                    // TODO_TEST_PANOS
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
                    // TODO_TEST_PANOS
                    return new CreatePropertyInfoAccessOutcome
                    {
                        IsRejected = true,
                        RejectionReason = CommonResources.InsufficientTokensErrorMessage
                    };
                }

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var propertyInfoAccess = await DoCreate(userId, userIpAddress, accessUniqueId, address.Id);

                var tokenTransactionStatus = await _userTokenService
                    .MakeTransaction(userId, tokenRewardKey, internalReference: propertyInfoAccess.UniqueId);

                if (tokenTransactionStatus != TokenAccountTransactionStatus.Success)
                {
                    return new CreatePropertyInfoAccessOutcome
                    {
                        IsRejected = true,
                        RejectionReason = CommonResources.GenericErrorMessage
                    };
                }

                transactionScope.Complete();

                uiAlerts.Add(new UiAlert
                {
                    Type = UiAlertType.Success,
                    Message = string.Format(
                        PropertyInfoAccessResources.Create_SuccessMessage, _propertyInfoAccessServiceConfig.ExpiryPeriodInDays)
                });

                _appCacheHelper.RemoveCachedUserExploredPropertiesSummary(userId);

                // TODO_TEST_PANOS
                return new CreatePropertyInfoAccessOutcome
                {
                    IsRejected = false,
                    PropertyInfoAccessUniqueId = propertyInfoAccess.UniqueId,
                    UiAlerts = uiAlerts
                };
            }
        }

        // TODO_TEST_PANOS
        public async Task<GetInfoOutcome> GetInfo(string userId, Guid accessUniqueId)
        {
            // TODO_TEST_PANOS
            var access = await GetPropertyInfoAccessForUser(userId, accessUniqueId, includeAddress: true, includeSubmissions: true);
            if (access == null)
            {
                // TODO_TEST_PANOS
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
                // TODO_TEST_PANOS
                return new GetInfoOutcome
                {
                    IsRejected = true,
                    RejectionReason = CommonResources.GenericInvalidActionMessage
                };
            }

            var mainProperty = access.Address;
            var duplicateAddressIds = await _addressService.GetDuplicateAddressIds(mainProperty);

            var duplicateProperties = await GetDuplicateProperties(duplicateAddressIds.ToList());

            var propertyInfo = ViewPropertyInfoModel.Construct(mainProperty, duplicateProperties, _currencyService);

            return new GetInfoOutcome
            {
                IsRejected = false,
                PropertyInfo = propertyInfo
            };
        }
        
        public async Task<Guid?> GetExistingUnexpiredAccessUniqueId(string userId, Guid addressUniqueId)
        {
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

        private async Task<PropertyInfoAccess> GetPropertyInfoAccessForUser(
            string userId, Guid uniqueId, bool includeAddress, bool includeSubmissions)
        {
            IQueryable<PropertyInfoAccess> query = _dbContext.PropertyInfoAccesses;

            if (includeAddress)
            {
                query = query
                    .Include(a => a.Address)
                    .Include(a => a.Address.Country);
            }

            if (includeSubmissions)
            {
                query = query
                    .Include(a => a.Address.TenancyDetailsSubmissions);
            }

            var propertyInfoAccess = await query
                .Where(a => a.UniqueId.Equals(uniqueId))
                .Where(a => a.UserId.Equals(userId))
                .SingleOrDefaultAsync();

            return propertyInfoAccess;
        }

        private async Task<IList<Address>> GetDuplicateProperties(List<long> duplicateAddressIds)
        {
            var duplicateProperties = await _dbContext.Addresses
                .Include(a => a.Country)
                .Include(a => a.TenancyDetailsSubmissions)
                .Where(a => duplicateAddressIds.Contains(a.Id))
                .Where(a => a.TenancyDetailsSubmissions.Any(s => s.SubmittedOn.HasValue))
                .ToListAsync();
            return duplicateProperties;
        }

        private async Task<PropertyInfoAccess> GetExistingUnexpiredAccess(string userId, Guid addressUniqueId)
        {
            // TODO_TEST_PANOS
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

        private TimeSpan ExpiryPeriod()
        {
            return TimeSpan.FromDays(_propertyInfoAccessServiceConfig.ExpiryPeriodInDays);
        }
    }
}
