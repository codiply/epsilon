﻿using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Entities.Interfaces;
using Epsilon.Logic.Forms.Submission;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.JsonModels;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Resources.Logic.Address;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;

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

        public async Task<AddressSearchResponse> SearchAddress(AddressSearchRequest request)
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
                .Where(x => !x.IsHidden)
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

        public async Task<PropertySearchResponse> SearchProperty(PropertySearchRequest request)
        {
            var resultsLimit = _addressServiceConfig.SearchPropertyResultsLimit;

            var countryIdOption = EnumsHelper.CountryId.Parse(request.countryId);
            if (string.IsNullOrEmpty(request.countryId)
                || string.IsNullOrEmpty(request.postcode)
                || !countryIdOption.HasValue)
            {
                return new PropertySearchResponse { resultsLimit = resultsLimit, isResultsLimitExceeded = false };
            }
            var countryId = countryIdOption.Value;

            var cleanPostcode = _addressCleansingHelper.CleansePostcode(countryId, request.postcode);

            var query = _dbContext.Addresses
                .Include(x => x.Country)
                .Include(x => x.TenancyDetailsSubmissions)
                .Where(x => !x.IsHidden)
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

            var results = addresses.Select(a => {
                var completeSubmissions = a.TenancyDetailsSubmissions
                    .Where(s => s.SubmittedOn.HasValue);
                var numberOfCompleteSubmissions = completeSubmissions.Count();
                DateTimeOffset? lastSubmissionOn = null;
                if (numberOfCompleteSubmissions > 0)
                    lastSubmissionOn = completeSubmissions.OrderByDescending(s => s.SubmittedOn).First().SubmittedOn;
                return new PropertySearchResult
                {
                    addressUniqueId = a.UniqueId,
                    fullAddress = a.FullAddress(),
                    numberOfCompletedSubmissions = numberOfCompleteSubmissions,
                    lastSubmissionOn = lastSubmissionOn
                };
            });

            if (exceededLimit)
            {
                results = results.Take(resultsLimit);
            }

            var response = new PropertySearchResponse
            {
                results = results.ToList(),
                resultsLimit = resultsLimit,
                isResultsLimitExceeded = exceededLimit
            };

            return response;
        }

        public async Task<Address> GetAddress(Guid addressUniqueId)
        {
            return await _dbContext.Addresses.SingleOrDefaultAsync(a => a.UniqueId.Equals(addressUniqueId));
        }

        public async Task<bool> AddressHasCompleteSubmissions(Guid addressUniqueId)
        {
            return await _dbContext.Addresses
                .Include(x => x.TenancyDetailsSubmissions)
                .Where(a => a.UniqueId.Equals(addressUniqueId))
                .SelectMany(s => s.TenancyDetailsSubmissions)
                .AnyAsync(s => s.SubmittedOn.HasValue);
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
            if (addressWithGeometry == null)
                return null;
            return addressWithGeometry.Geometry.ToAddressGeometryResponse();
        }

        public async Task<AddAddressOutcome> AddAddress(string userId, string userIpAddress, AddressForm form)
        {
            if (_addressServiceConfig.GlobalSwitch_DisableAddAddress)
                return new AddAddressOutcome
                {
                    IsRejected = true,
                    ReturnToForm = false,
                    RejectionReason = AddressResources.GlobalSwitch_AddAddressDisabled_Message,
                    AddressUniqueId = null
                };

            var formCountryId = form.CountryIdAsEnum();

            var antiAbuseServiceResponse = await _antiAbuseService.CanAddAddress(userId, userIpAddress, formCountryId);
            if (antiAbuseServiceResponse.IsRejected)
                return new AddAddressOutcome
                {
                    IsRejected = true,
                    ReturnToForm = false,
                    RejectionReason = antiAbuseServiceResponse.RejectionReason,
                    AddressUniqueId = null
                };

            var cleansedForm = _addressCleansingHelper.Cleanse(form);

            var verificationResponse = await _addressVerificationService.Verify(userId, userIpAddress, cleansedForm);
            if (verificationResponse.IsRejected)
                return new AddAddressOutcome
                {
                    IsRejected = true,
                    ReturnToForm = verificationResponse.AskUserToModify,
                    RejectionReason = verificationResponse.RejectionReason,
                    AddressUniqueId = null
                };
            
            var entity = cleansedForm.ToEntity();
            entity.CreatedById = userId;
            entity.CreatedByIpAddress = userIpAddress;

            entity.DistinctAddressCode = CalculateDistinctAddressCode(cleansedForm);

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

        public async Task<IList<long>> GetDuplicateAddressIds(Address address)
        {
            if (string.IsNullOrWhiteSpace(address.DistinctAddressCode))
            {
                return new List<long>();
            }

            var duplicateAddressIds = await _dbContext.Addresses
                .Where(x => !x.IsHidden)
                .Where(x => x.DistinctAddressCode.Equals(address.DistinctAddressCode) && !x.Id.Equals(address.Id))
                .Select(x => x.Id)
                .ToListAsync();

            return duplicateAddressIds;
        }

        public string CalculateDistinctAddressCode(AddressForm form)
        {
            var countryId = form.CountryIdAsEnum();
            // EnumSwitch:CountryId
            switch (countryId)
            {
                case CountryId.GB:
                    return CalculateDistinctAddressCodeGB(form);
                case CountryId.GR:
                    return null;
                default:
                    throw new NotImplementedException(string.Format("Unexpected CountryId: '{0}'",
                            EnumsHelper.CountryId.ToString()));
            }
        }

        private string CalculateDistinctAddressCodeGB(AddressForm form)
        {
            if (string.IsNullOrWhiteSpace(form.Line1) || string.IsNullOrWhiteSpace(form.Postcode))
                return null;

            var houseNumberRegex = new Regex(@"([0-9]{1,}[a-zA-z]*)");
            var matches = houseNumberRegex.Matches(form.Line1);
            if (matches.Count != 1)
                return null;

            var houseNumber = matches[0].Value;

            var distinctAddressCode = string.Format("{0}{1}{2}", form.CountryId, form.Postcode, houseNumber).ToUpperInvariant();
            return distinctAddressCode;
        }
    }
}
