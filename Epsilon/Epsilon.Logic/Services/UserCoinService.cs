using Epsilon.Logic.Entities;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services
{
    public class UserCoinService : IUserCoinService
    {
        private readonly IEpsilonContext _dbContext;
        private readonly ICoinAccountService _coinAccountService;

        public UserCoinService(
            IEpsilonContext dbContext,
            ICoinAccountService coinAccountService)
        {
            _dbContext = dbContext;
            _coinAccountService = coinAccountService;
        }

        public async Task<decimal> GetBalance(User user)
        {
            return await _coinAccountService.GetBalance(user.Id);
        }
    }
}
