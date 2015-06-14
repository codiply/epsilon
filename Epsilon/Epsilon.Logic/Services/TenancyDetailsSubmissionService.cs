using Epsilon.Logic.Entities;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext;
using Epsilon.Logic.Wrappers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services
{
    public class TenancyDetailsSubmissionService : ITenancyDetailsSubmissionService
    {
        public readonly IClock _clock;
        public readonly IEpsilonContext _dbContext;

        public TenancyDetailsSubmissionService(
            IClock clock,
            IEpsilonContext dbContext)
        {
            _clock = clock;
            _dbContext = dbContext;
        }

        public void Create(string userId, Address address)
        {
        }
    }
}
