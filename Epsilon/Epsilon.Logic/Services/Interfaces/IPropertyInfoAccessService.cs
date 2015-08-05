using Epsilon.Logic.Dtos;
using Epsilon.Logic.JsonModels;
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
        Task<MyExploredPropertiesSummaryResponse> GetUserExploredPropertiesSummaryWithCaching(string userId, bool limitItemsReturned);

        Task<MyExploredPropertiesSummaryResponse> GetUserExploredPropertiesSummary(string userId, bool limitItemsReturned);

        Task<CreatePropertyInfoAccessOutcome> Create(
            string userId,
            string userIpAddress,
            Guid accessUniqueId,
            Guid addressUniqueId);
    }
}
