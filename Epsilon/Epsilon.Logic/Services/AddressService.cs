﻿using Epsilon.Logic.Entities;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext;
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

namespace Epsilon.Logic.Services
{
    public class AddressService : IAddressService
    {
        private readonly IAppCache _appCache;
        private readonly IEpsilonContext _dbContext;

        public AddressService(
            IAppCache appCache,
            IEpsilonContext dbContext)
        {
            _appCache = appCache;
            _dbContext = dbContext;
        }

        public async Task<IList<TypeaheadAddressResult>> TypeaheadSearch(TypeaheadAddressRequest request)
        {
            var addresses = await _dbContext.Addresses
                .Include(x => x.Country)
                .Where(x => x.CountryId.Equals(request.countryId)
                            && (x.PostcodeOrZip.Equals(request.searchTerm) 
                                || x.Line1.Equals(request.searchTerm))).ToListAsync();

            return addresses.Select(x => new TypeaheadAddressResult
            {
                addressId = x.Id,
                fullAddress = x.FullAddress()
            }).ToList();
        }

        public async Task<Address> AddAddress(AddressForm dto)
        {
            var entity = dto.ToEntity();
            entity.UniqueAddressCode = CalculateUniqueAddressCode(dto);
            _dbContext.Addresses.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public string CalculateUniqueAddressCode(AddressForm dto)
        {
            // TODO: Find a mapping from address to a unique id.
            // For UK for example it could be something like
            // GB<POSTCODE><HOUSENUMBER>
            return dto.PostcodeOrZip;
        }
    }
}
