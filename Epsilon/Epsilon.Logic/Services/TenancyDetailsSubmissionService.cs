using Epsilon.Logic.Entities;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using Epsilon.Resources.Web.Submission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services
{
    public class TenancyDetailsSubmissionService : ITenancyDetailsSubmissionService
    {
        private readonly IClock _clock;
        private readonly IEpsilonContext _dbContext;
        private readonly IAddressService _addressService;
        private readonly IAntiAbuseService _antiAbuseService;

        public TenancyDetailsSubmissionService(
            IClock clock,
            IEpsilonContext dbContext,
            IAddressService addressService,
            IAntiAbuseService antiAbuseService)
        {
            _clock = clock;
            _dbContext = dbContext;
            _addressService = addressService;
            _antiAbuseService = antiAbuseService;
        }

        public async Task<CreateTenancyDetailsSubmissionOutcome> Create(
            string userId, 
            string userIpAddress, 
            Guid submissionId,
            Guid addressId)
        {
            var address = await _addressService.GetAddress(addressId);
            if (address == null)
            {
                return new CreateTenancyDetailsSubmissionOutcome
                {
                    IsRejected = true,
                    RejectionReason = SubmissionResources.UseAddressConfirmed_AddressNotFoundMessage
                };
            }

            var antiAbuseCheck = await _antiAbuseService.CanCreateTenancyDetailsSubmission(userId, userIpAddress);
            if (antiAbuseCheck.IsRejected)
            {
                return new CreateTenancyDetailsSubmissionOutcome
                {
                    IsRejected = true,
                    RejectionReason = antiAbuseCheck.RejectionReason
                };
            }

            var tenancyDetailsSubmission = await DoCreate(userId, userIpAddress, submissionId, addressId);
            return new CreateTenancyDetailsSubmissionOutcome
            {
                IsRejected = false,
                TenancyDetailsSubmissionId = tenancyDetailsSubmission.Id
            };
        }

        private async Task<TenancyDetailsSubmission> DoCreate(
            string userId,
            string userIpAddress,
            Guid submissionId,
            Guid addressId)
        {
            var entity = new TenancyDetailsSubmission
            {
                Id = submissionId,
                UserId = userId,
                CreatedByIpAddress = userIpAddress,
                AddressId = addressId
            };
            _dbContext.TenancyDetailsSubmissions.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }
    }
}
