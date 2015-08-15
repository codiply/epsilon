using Epsilon.Logic.Constants.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    public interface IAdminEventLogService
    {
        Task Log(AdminEventLogKey key, Dictionary<string, object> extraInfo);
    }
}
