using Epsilon.Logic.JsonModels;
using Epsilon.Logic.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    public class BaseOutcome
    {
        public bool IsRejected { get; set; }
        public string RejectionReason { get; set; }
    }

    public class BaseOutcomeWithAlerts : BaseOutcome
    {
        public IList<UiAlert> UiAlerts { get; set; }
    }

    public class CreatePropertyInfoAccessOutcome : BaseOutcomeWithAlerts
    {
        public Guid? PropertyInfoAccessUniqueId { get; set; }
    }

    public class GetInfoOutcome : BaseOutcome
    {
        public ViewPropertyInfoModel PropertyInfo { get; set; }
    }

    public interface IPropertyInfoAccessService
    {
        Task<MyExploredPropertiesSummaryResponse> GetUserExploredPropertiesSummaryWithCaching(string userId, bool limitItemsReturned);

        Task<MyExploredPropertiesSummaryResponse> GetUserExploredPropertiesSummary(string userId, bool limitItemsReturned);

        Task<GetInfoOutcome> GetInfo(string userId, Guid accessUniqueId);

        Task<Guid?> GetExistingUnexpiredAccessUniqueId(string userId, Guid addressUniqueId);

        Task<CreatePropertyInfoAccessOutcome> Create(
            string userId,
            string userIpAddress,
            Guid accessUniqueId,
            Guid addressUniqueId);
    }
}
