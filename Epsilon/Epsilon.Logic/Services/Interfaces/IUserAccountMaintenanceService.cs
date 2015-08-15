using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    public interface IUserAccountMaintenanceService
    {
        Task DoMaintenance(string email);
    }
}
