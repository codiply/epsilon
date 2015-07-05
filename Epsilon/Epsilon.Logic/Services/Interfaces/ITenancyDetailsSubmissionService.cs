using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    public class CreateTenancyDetailsSubmissionOutcome
    {
        public bool IsRejected { get; set; }
        public string RejectionReason { get; set; }
        public Guid? TenancyDetailsSubmissionId { get; set; }
    }

    public interface ITenancyDetailsSubmissionService
    {
        Task<CreateTenancyDetailsSubmissionOutcome> Create(
            string userId,
            string userIpAddress,
            Guid addressId);
    }
}
