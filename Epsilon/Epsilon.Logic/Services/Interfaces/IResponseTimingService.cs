using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    public interface IResponseTimingService
    {
        void Record(string languageId, string controllerName, string actionName, string httpVerb, bool isApi, double timeInMilliseconds);

        Task RecordAsync(string languageId, string controllerName, string actionName, string httpVerb, bool isApi, double timeInMilliseconds);
    }
}
