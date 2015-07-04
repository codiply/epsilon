using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services
{
    public class NewUserService : INewUserService
    {
        private readonly IEpsilonContext _dbContext;
        private readonly IUserCoinService _userCoinService;
        private readonly IUserPreferenceService _userPreferenceService;

        public NewUserService(
            IEpsilonContext dbContext,
            IUserCoinService userCoinService,
            IUserPreferenceService userPreferenceService)
        {
            _dbContext = dbContext;
            _userCoinService = userCoinService;
            _userPreferenceService = userPreferenceService;
        }

        public async Task Setup(string userId, string languageId)
        {
            await CreateCoinAccount(userId);
            await CreateUserPreference(userId, languageId);
        }

        private async Task CreateCoinAccount(string userId)
        {
            await _userCoinService.CreateAccount(userId);
        }

        private async Task CreateUserPreference(string userId, string languageId)
        {
            await _userPreferenceService.CreateUserPreference(userId, languageId);
        }
    }
}
