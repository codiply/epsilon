using Epsilon.Logic.Entities;

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
