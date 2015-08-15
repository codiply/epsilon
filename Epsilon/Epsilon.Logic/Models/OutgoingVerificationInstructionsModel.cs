using System;

namespace Epsilon.Logic.Models
{
    public class OutgoingVerificationInstructionsModel
    {
        public AddressModel RecipientAddress { get; set; }

        public VerificationMessageArgumentsModel MessageArguments { get; set; }

        public Guid VerificationUniqueId { get; set; }

        public bool OtherUserHasMarkedAddressAsInvalid { get; set; }

        public bool CanMarkAddressAsInvalid { get; set; }

        public bool CanMarkAsSent { get; set; }
    }
}
