using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    public interface IIpAddressActivityService
    {
        Task RecordRegistration(string userId, string ipAddress);

        Task RecordLogin(string userId, string ipAddress);
    }
}
