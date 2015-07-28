using Epsilon.Logic.Entities;
using Epsilon.Logic.Services.Interfaces.UserResidenceService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    namespace UserResidenceService
    {
        public class GetResidenceResponse
        {
            public bool HasNoSubmissions { get; set; }
            public Address Address { get; set; }
            public bool IsVerified { get; set; }
        }
    }

    public interface IUserResidenceService
    {
        Task<GetResidenceResponse> GetResidence(string userId);
    }
}
