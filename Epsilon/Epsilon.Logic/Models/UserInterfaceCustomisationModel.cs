using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Models
{
    public class UserInterfaceCustomisationModel
    {
        public bool HasNoTenancyDetailsSubmissions { get; set; }
        public bool CanCreateTenancyDetailsSubmission { get; set; }
        public bool CanPickOutgoingVerification { get; set; }
        public bool IsUserResidenceVerified { get; set; }
        public Country UserResidenceCountry { get; set; }
    }
}
