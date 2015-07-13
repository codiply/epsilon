﻿using Epsilon.Logic.Forms;
using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Epsilon.Logic.JsonModels;

namespace Epsilon.Logic.Services.Interfaces
{
    public class AddAddressOutcome
    {
        public bool IsRejected { get; set; }
        public string RejectionReason { get; set; }
        public Guid? AddressUniqueId { get; set; }
    }

    public interface IAddressService
    {
        Task<AddressSearchResponse> Search(AddressSearchRequest request);

        Task<Address> GetAddress(long addressId);

        Task<Address> GetAddressViaUniqueId(Guid addressUniqueId);

        Task<AddAddressOutcome> AddAddress(string userId, string userIpAddress, AddressForm dto);
    }
}
