using Epsilon.Logic.Models;

namespace Epsilon.Web.Models.ViewModels.OutgoingVerification
{
    public class InstructionsViewModel
    {
        public OutgoingVerificationInstructionsModel Instructions { get; set; }
        public bool ReturnToSummary { get; set; }
    }
}