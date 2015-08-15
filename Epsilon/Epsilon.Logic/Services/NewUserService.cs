using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services
{
    public class NewUserService : INewUserService
    {
        private readonly IEpsilonContext _dbContext;
        private readonly IUserTokenService _userTokenService;
        private readonly IUserPreferenceService _userPreferenceService;
        private readonly IIpAddressActivityService _ipAddressActivityService;

        public NewUserService(
            IEpsilonContext dbContext,
            IUserTokenService userTokenService,
            IUserPreferenceService userPreferenceService,
            IIpAddressActivityService ipAddressActivityService)
        {
            _dbContext = dbContext;
            _userTokenService = userTokenService;
            _userPreferenceService = userPreferenceService;
            _ipAddressActivityService = ipAddressActivityService;
        }

        public async Task Setup(string userId, string userIpAddress, string languageId)
        {
            await CreateTokenAccount(userId);
            await CreateUserPreference(userId, languageId);
            await _ipAddressActivityService.RecordRegistration(userId, userIpAddress);
        }

        private async Task CreateTokenAccount(string userId)
        {
            await _userTokenService.CreateAccount(userId);
        }

        private async Task CreateUserPreference(string userId, string languageId)
        {
            await _userPreferenceService.Create(userId, languageId);
        }
    }
}
