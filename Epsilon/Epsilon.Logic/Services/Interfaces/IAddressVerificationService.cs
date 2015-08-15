using Epsilon.Logic.Entities;
using Epsilon.Logic.Forms.Submission;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    public class AddressVerificationResponse
    {
        public bool IsRejected { get; set; }
        public string RejectionReason { get; set; }
        public bool AskUserToModify { get; set; }
        public AddressGeometry AddressGeometry { get; set; }
    }

    public interface IAddressVerificationService
    {
        Task<AddressVerificationResponse> Verify(string userId, string userIpAddress, AddressForm address);
    }
}
