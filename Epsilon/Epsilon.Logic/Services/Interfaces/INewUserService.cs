using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    public interface INewUserService
    {
        Task Setup(string userId, string userIpAddress, string languageId);
    }
}
