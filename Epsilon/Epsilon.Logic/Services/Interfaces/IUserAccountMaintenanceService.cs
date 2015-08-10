using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    public interface IUserAccountMaintenanceService
    {
        Task DoMaintenance(string userId);
    }
}
