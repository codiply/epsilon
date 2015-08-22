using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    public interface IUserAccountMaintenanceService
    {
        Task<bool> DoMaintenance(string email);
    }
}
