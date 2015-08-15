using Epsilon.Web.Models.ViewModels.Shared;
using System;

namespace Epsilon.Web.Models.ViewModels.Submission
{
    public class UseAddressViewModel
    {
        public Guid SubmissionUniqueId { get; set; }
        public AddressDetailsViewModel AddressDetails { get; set; }
    }
}