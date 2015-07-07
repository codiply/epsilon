using Epsilon.Logic.Entities;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Epsilon.Logic.Forms;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.Infrastructure;
using Epsilon.Logic.Constants;
using System.Collections;
using Epsilon.Logic.JsonModels;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Constants.Interfaces;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Configuration.Interfaces;

namespace Epsilon.Logic.Services
{
    public class AddressService : IAddressService
    {
        private readonly IAppCache _appCache;
        private readonly IEpsilonContext _dbContext;
        private readonly IAddressServiceConfig _addressServiceConfig;
        private readonly IAddressCleansingHelper _addressCleansingHelper;
        private readonly IAntiAbuseService _antiAbuseService;

        public AddressService(
            IAppCache appCache,
            IEpsilonContext dbContext,
            IAddressServiceConfig addressServiceConfig,
            IAddressCleansingHelper addresCleansingHelper,
            IAntiAbuseService antiAbuseService)
        {
            _appCache = appCache;
            _dbContext = dbContext;
            _addressServiceConfig = addressServiceConfig;
            _addressCleansingHelper = addresCleansingHelper;
            _antiAbuseService = antiAbuseService;
        }

        public async Task<AddressSearchResponse> Search(AddressSearchRequest request)
        {
            var resultsLimit = _addressServiceConfig.SearchAddressResultsLimit;

            var countryIdOption = EnumsHelper.CountryId.Parse(request.countryId);
            if (string.IsNullOrEmpty(request.countryId)
                || string.IsNullOrEmpty(request.postcode)
                || !countryIdOption.HasValue)
            {
                return new AddressSearchResponse { ResultsLimit = resultsLimit, IsResultsLimitReached = false };
            }
            var countryId = countryIdOption.Value;

            var cleanPostcode = _addressCleansingHelper.CleanPostcode(countryId, request.postcode);

            var query = _dbContext.Addresses
                .Include(x => x.Country)
                .Where(x => x.CountryId.Equals(request.countryId)
                            && (x.Postcode.Equals(cleanPostcode)));

            if (!string.IsNullOrWhiteSpace(request.terms))
            {
                var terms = request.terms.Split(' ', ',').Where(t => !String.IsNullOrWhiteSpace(t)).Select(t => t.Trim());

                foreach (var term in terms)
                {
                    query = query.Where(a =>
                        a.Line1.Contains(term) ||
                        a.Line2.Contains(term) ||
                        a.Line3.Contains(term) ||
                        a.Line4.Contains(term) ||
                        a.Locality.Contains(term) ||
                        a.Region.Contains(term));
                }
            }

            query = query.OrderBy(x => x.Line1)
                .ThenBy(x => x.Line2)
                .ThenBy(x => x.Line3)
                .ThenBy(x => x.Line4)
                .ThenBy(x => x.Locality)
                .ThenBy(x => x.Region);

            var addresses = await query.Take(resultsLimit + 1).ToListAsync();
            
            var reachedLimit = addresses.Count > resultsLimit;
            
            var results = addresses.Select(x => new AddressSearchResult
            {
                addressId = x.Id,
                fullAddress = x.FullAddress()
            });

            if (reachedLimit)
            {
                results = results.Take(resultsLimit);
            }

            var response = new AddressSearchResponse
            {
                Results = results.ToList(),
                ResultsLimit = resultsLimit,
                IsResultsLimitReached = reachedLimit
            };

            return response; 
        }

        public async Task<Address> GetAddress(Guid addressId)
        {
            return await _dbContext.Addresses.FindAsync(addressId);
        }

        public async Task<AddAddressOutcome> AddAddress(string userId, string userIpAddress, AddressForm dto)
        {
            var antiAbuseServiceResponse = await _antiAbuseService.CanAddAddress(userId, userIpAddress);
            if (antiAbuseServiceResponse.IsRejected)
                return new AddAddressOutcome
                {
                    IsRejected = true,
                    RejectionReason = antiAbuseServiceResponse.RejectionReason,
                    AddressId = null
                };

            var entity = dto.ToEntity();
            entity.CreatedById = userId;
            entity.CreatedByIpAddress = userIpAddress;
            entity.UniqueAddressCode = CalculateUniqueAddressCode(dto);

            _dbContext.Addresses.Add(entity);

            await _dbContext.SaveChangesAsync();
            return new AddAddressOutcome
            {
                IsRejected = false,
                AddressId = entity.Id
            };
        }

        public string CalculateUniqueAddressCode(AddressForm dto)
        {
            // TODO_PANOS: Find a mapping from address to a unique id.
            // For UK for example it could be something like
            // GB<POSTCODE><HOUSENUMBER>
            return dto.Postcode;
        }
    }
}
