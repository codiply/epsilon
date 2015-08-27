using Epsilon.Logic.Entities;
using Epsilon.Logic.Forms.Submission;
using Epsilon.Logic.JsonModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    public class AddAddressOutcome
    {
        public bool IsRejected { get; set; }
        public string RejectionReason { get; set; }
        public bool ReturnToForm { get; set; }
        public Guid? AddressUniqueId { get; set; }
    }

    public interface IAddressService
    {
        Task<AddressSearchResponse> SearchAddress(AddressSearchRequest request);

        Task<PropertySearchResponse> SearchProperty(PropertySearchRequest request);

        Task<bool> AddressHasCompleteSubmissions(Guid addressUniqueId);

        Task<Address> GetAddress(Guid addressUniqueId);

        Task<Address> GetAddressWithGeometries(Guid addressUniqueId);

        Task<AddressGeometryResponse> GetGeometry(Guid addressUniqueId);

        Task<AddAddressOutcome> AddAddress(string userId, string userIpAddress, AddressForm form);

        Task<IList<long>> GetDuplicateAddressIds(Address address);
    }
}
