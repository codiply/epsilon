using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    public class AntiAbuserServiceResponse
    {
        public bool IsRejected { get; set; }
        public string RejectionReason { get; set; }
    }

    public interface IAntiAbuseService
    {
        Task<AntiAbuserServiceResponse> CanAddAddress(string userId);
        Task<AntiAbuserServiceResponse> CanCreateTenancyDetailsSubmission(string userId);
    }
}
