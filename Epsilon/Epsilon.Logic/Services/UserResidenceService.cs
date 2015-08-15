using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.Services.Interfaces.UserResidenceService;
using Epsilon.Logic.SqlContext.Interfaces;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services
{
    public class UserResidenceService : IUserResidenceService
    {
        private readonly IEpsilonContext _dbContext;

        public UserResidenceService(
            IEpsilonContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetResidenceResponse> GetResidence(string userId)
        {
            var lastVerifiedSubmission = await _dbContext.TenancyDetailsSubmissions
                .Include(s => s.TenantVerifications)
                .Include(s => s.Address)
                .Include(s => s.Address.Country)
                .Include(s => s.Address.Geometry)
                .Where(s => s.UserId.Equals(userId))
                .Where(s => s.TenantVerifications.Any(v => v.VerifiedOn.HasValue))
                .OrderByDescending(s => s.CreatedOn)
                .FirstOrDefaultAsync();

            if (lastVerifiedSubmission != null)
            {
                return new GetResidenceResponse
                {
                    HasNoSubmissions = false,
                    Address = lastVerifiedSubmission.Address,
                    IsVerified = true
                };
            }

            var lastUnverifiedSubmission = await _dbContext.TenancyDetailsSubmissions
                .Include(s => s.Address)
                .Include(s => s.Address.Country)
                .Include(s => s.Address.Geometry)
                .Where(s => s.UserId.Equals(userId))
                .OrderByDescending(s => s.CreatedOn)
                .FirstOrDefaultAsync();

            if (lastUnverifiedSubmission != null)
            {
                return new GetResidenceResponse
                {
                    HasNoSubmissions = false,
                    Address = lastUnverifiedSubmission.Address,
                    IsVerified = false
                };
            }

            return new GetResidenceResponse
            {
                HasNoSubmissions = true,
                Address = null,
                IsVerified = false
            };
        }
    }
}
