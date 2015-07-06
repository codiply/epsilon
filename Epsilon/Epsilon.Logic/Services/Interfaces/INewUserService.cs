using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    public interface INewUserService
    {
        Task Setup(string userId, string userIpAddress, string languageId);
    }
}
