using Epsilon.Logic.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    public class BaseOutcome
    {
        public bool IsRejected { get; set; }
        public string RejectionReason { get; set; }
        public IList<UiAlert> UiAlerts { get; set; }
    }

    public class CreatePropertyInfoAccessOutcome : BaseOutcome
    {
        public Guid? PropertyInfoAccessUniqueId { get; set; }
    }

    public interface IPropertyInfoAccessService
    {
        Task<CreatePropertyInfoAccessOutcome> Create(
            string userId,
            string userIpAddress,
            Guid accessUniqueId,
            Guid addressUniqueId);
    }
}
