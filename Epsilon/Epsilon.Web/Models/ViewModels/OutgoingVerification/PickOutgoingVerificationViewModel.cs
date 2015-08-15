using System;

namespace Epsilon.Web.Models.ViewModels.OutgoingVerification
{
    public class PickOutgoingVerificationViewModel
    {
        public Guid VerificationUniqueId { get; set; }
        public bool ReturnToSummary { get; set; }
    }
}