using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    public interface IIpAddressActivityService
    {
        Task RecordRegistration(string email, string ipAddress);

        Task RecordLogin(string email, string ipAddress);
    }
}
