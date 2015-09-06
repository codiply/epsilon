using Epsilon.Logic.Constants.Enums;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    public interface IIpAddressActivityService
    {
        Task RecordWithUserId(string userId, IpAddressActivityType activityType, string ipAddress);
        Task RecordWithUserEmail(string email, IpAddressActivityType activityType, string ipAddress);
    }
}
