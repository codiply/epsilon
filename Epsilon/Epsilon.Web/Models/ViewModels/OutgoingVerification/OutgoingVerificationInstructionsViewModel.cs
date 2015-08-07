using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Epsilon.Web.Models.ViewModels.OutgoingVerification
{
    public class OutgoingVerificationInstructionsViewModel
    {
        // TODO_PANOS


        /// <summary>
        /// Note: You will need to Include TenancyDetailsSubmission, TenancyDetailsSubmission.Address,
        ///       and TenancyDetailsSubmission.Address.Country in your entity for this to work.
        /// </summary>
        /// <returns></returns>
        public static OutgoingVerificationInstructionsViewModel FromEntity(TenantVerification entity)
        {
            // TODO_PANOS
            throw new NotImplementedException();
        }
    }
}