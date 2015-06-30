using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    public class AntiAbuseServiceResponse
    {
        public bool IsRejected { get; set; }
        public string RejectionReason { get; set; }
    }

    public interface IAntiAbuseService
    {
        Task<AntiAbuseServiceResponse> CanAddAddress(string userId);
        Task<AntiAbuseServiceResponse> CanCreateTenancyDetailsSubmission(string userId);
    }
}
