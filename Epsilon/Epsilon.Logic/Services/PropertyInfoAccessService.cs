using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Dtos;
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

namespace Epsilon.Logic.Services
{
    public class PropertyInfoAccessService : IPropertyInfoAccessService
    {
        private readonly IAppCache _appCache;
        private readonly IEpsilonContext _dbContext;
        private readonly IAddressService _addressService;
        private readonly IUserTokenService _userTokenService;

        public PropertyInfoAccessService(
            IAppCache appCache,
            IEpsilonContext dbContext,
            IAddressService addressService,
            IUserTokenService userTokenService)
        {
            _dbContext = dbContext;
            _addressService = addressService;
            _userTokenService = userTokenService;
        }

        public async Task<CreatePropertyInfoAccessOutcome> Create(
            string userId,
            string userIpAddress,
            Guid accessUniqueId,
            Guid addressUniqueId)
        {
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
   
            var propertyInfoAccess = await DoCreate(userId, userIpAddress, accessUniqueId, address.Id);

            var tokenTransactionStatus = await _userTokenService
                .MakeTransaction(userId, TokenRewardKey.SpendPerPropertyInfoAccess, internalReference: propertyInfoAccess.UniqueId);

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
                Message = PropertyInfoAccessResources.Create_SuccessMessage
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

        public async Task<PropertyInfoAccess> DoCreate(
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

        
        private void RemoveCachedUserExploredPropertiesSummary(string userId)
        {
            _appCache.Remove(AppCacheKey.GetUserExploredPropertiesSummary(userId, true));
            _appCache.Remove(AppCacheKey.GetUserExploredPropertiesSummary(userId, false));
        }
    }
}
