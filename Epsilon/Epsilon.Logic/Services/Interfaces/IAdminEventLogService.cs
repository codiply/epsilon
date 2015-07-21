using Epsilon.Logic.Constants.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    public interface IAdminEventLogService
    {
        Task Log(AdminEventLogKey key, string extraInfo);
    }
}
