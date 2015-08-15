using Epsilon.Logic.Models;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    public interface IUserInterfaceCustomisationService
    {

        UserInterfaceCustomisationModel GetForUser(string userId);

        Task<UserInterfaceCustomisationModel> GetForUserAsync(string userId);

        void ClearCachedCustomisationForUser(string userId);
    }
}
