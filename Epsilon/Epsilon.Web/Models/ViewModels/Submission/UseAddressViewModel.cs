using Epsilon.Web.Models.ViewModels.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Epsilon.Web.Models.ViewModels.Submission
{
    public class UseAddressViewModel
    {
        public Guid SubmissionId { get; set; }
        public AddressDetailsViewModel AddressDetails { get; set; }
    }
}