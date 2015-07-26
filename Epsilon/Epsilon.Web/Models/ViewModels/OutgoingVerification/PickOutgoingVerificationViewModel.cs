using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Epsilon.Web.Models.ViewModels.OutgoingVerification
{
    public class PickOutgoingVerificationViewModel
    {
        public Guid VerificationUniqueId { get; set; }
        public bool ReturnToSummary { get; set; }
    }
}