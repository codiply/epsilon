using Epsilon.Logic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Epsilon.Web.Models.ViewModels.OutgoingVerification
{
    public class InstructionsViewModel
    {
        public OutgoingVerificationInstructionsModel Instructions { get; set; }
        public bool ReturnToSummary { get; set; }
    }
}