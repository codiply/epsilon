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
using Epsilon.Logic.Entities.Interfaces;
using Epsilon.Logic.Forms.Submission;
using Epsilon.Resources.Logic.Address;

namespace Epsilon.Logic.Services
{
    public class AddressService : IAddressService
    {
        private readonly IAppCache _appCache;
        private readonly IEpsilonContext _dbContext;
        private readonly IAddressServiceConfig _addressServiceConfig;
        private readonly IAddressCleansingHelper _addressCleansingHelper;
        private readonly IAddressVerificationService _addressVerificationService;
        private readonly IAntiAbuseService _antiAbuseService;

        public AddressService(
            IAppCache appCache,
            IEpsilonContext dbContext,
            IAddressServiceConfig addressServiceConfig,
            IAddressCleansingHelper addresCleansingHelper,
            IAddressVerificationService addressVerificationService,
            IAntiAbuseService antiAbuseService)
        {
            _appCache = appCache;
            _dbContext = dbContext;
            _addressServiceConfig = addressServiceConfig;
            _addressCleansingHelper = addresCleansingHelper;
            _addressVerificationService = addressVerificationService;
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
                return new AddressSearchResponse { resultsLimit = resultsLimit, isResultsLimitExceeded = false };
            }
            var countryId = countryIdOption.Value;

            var cleanPostcode = _addressCleansingHelper.CleansePostcode(countryId, request.postcode);

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
            
            var exceededLimit = addresses.Count > resultsLimit;
            
            var results = addresses.Select(x => new AddressSearchResult
            {
                addressUniqueId = x.UniqueId,
                fullAddress = x.FullAddress()
            });

            if (exceededLimit)
            {
                results = results.Take(resultsLimit);
            }

            var response = new AddressSearchResponse
            {
                results = results.ToList(),
                resultsLimit = resultsLimit,
                isResultsLimitExceeded = exceededLimit
            };

            return response; 
        }

        public async Task<Address> GetAddress(Guid addressUniqueId)
        {
            return await _dbContext.Addresses
                .SingleOrDefaultAsync(a => a.UniqueId.Equals(addressUniqueId));
        }

        public async Task<Address> GetAddressWithGeometries(Guid addressUniqueId)
        {
            return await _dbContext.Addresses
                .Include(a => a.Geometry)
                .Include(a => a.PostcodeGeometry)
                .SingleOrDefaultAsync(a => a.UniqueId.Equals(addressUniqueId));
        }

        public async Task<AddressGeometryResponse> GetGeometry(Guid addressUniqueId)
        {
            var addressWithGeometry = await _dbContext.Addresses
                .Include(a => a.Geometry)
                .SingleOrDefaultAsync(a => a.UniqueId.Equals(addressUniqueId));
            return addressWithGeometry.Geometry.ToAddressGeometryResponse();
        }

        public async Task<AddAddressOutcome> AddAddress(string userId, string userIpAddress, AddressForm dto)
        {
            // TODO_PANOS_TEST
            if (_addressServiceConfig.GlobalSwitch_DisableAddAddress)
                return new AddAddressOutcome
                {
                    IsRejected = true,
                    ReturnToForm = false,
                    RejectionReason = AddressResources.GlobalSwitch_AddAddressDisabled_Message,
                    AddressUniqueId = null
                };

            var antiAbuseServiceResponse = await _antiAbuseService.CanAddAddress(userId, userIpAddress);
            if (antiAbuseServiceResponse.IsRejected)
                return new AddAddressOutcome
                {
                    IsRejected = true,
                    ReturnToForm = false,
                    RejectionReason = antiAbuseServiceResponse.RejectionReason,
                    AddressUniqueId = null
                };

            var verificationResponse = await _addressVerificationService.Verify(userId, userIpAddress, dto);
            if (verificationResponse.IsRejected)
                return new AddAddressOutcome
                {
                    IsRejected = true,
                    ReturnToForm = verificationResponse.AskUserToModify,
                    RejectionReason = verificationResponse.RejectionReason,
                    AddressUniqueId = null
                };

            var cleansedDto = _addressCleansingHelper.Cleanse(dto);
            var entity = cleansedDto.ToEntity();
            entity.CreatedById = userId;
            entity.CreatedByIpAddress = userIpAddress;
            entity.DistinctAddressCode = CalculateDistinctAddressCode(dto);
            if (verificationResponse.AddressGeometry != null)
            {
                _dbContext.AddressGeometries.Add(verificationResponse.AddressGeometry);
                entity.Geometry = verificationResponse.AddressGeometry;
            }
            _dbContext.Addresses.Add(entity);

            await _dbContext.SaveChangesAsync();
            return new AddAddressOutcome
            {
                IsRejected = false,
                AddressUniqueId = entity.UniqueId
            };
        }

        public string CalculateDistinctAddressCode(AddressForm dto)
        {
            // TODO_PANOS: Find a mapping from address to a unique id.
            // For UK for example it could be something like
            // GB<POSTCODE><HOUSENUMBER>
            return dto.Postcode;
        }
    }
}
