using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    public interface IAdminEventLogService
    {
        Task Log(AdminEventLogKey key, Dictionary<string, object> extraInfo);
    }
}
