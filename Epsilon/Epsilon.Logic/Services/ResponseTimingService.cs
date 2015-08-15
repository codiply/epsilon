using Epsilon.Logic.Entities;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services
{
    public class ResponseTimingService : IResponseTimingService
    {
        private IEpsilonContext _dbContext;

        public ResponseTimingService(
            IEpsilonContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Record(string languageId, string controllerName, string actionName, string httpVerb, bool isApi, double timeInMilliseconds)
        {
            var responseTiming = new ResponseTiming
            {
                LanguageId = languageId,
                ControllerName = controllerName,
                ActionName = actionName,
                HttpVerb = httpVerb,
                IsApi = isApi,
                TimeInMilliseconds = timeInMilliseconds
            };
            _dbContext.ResponseTimings.Add(responseTiming);
            _dbContext.SaveChanges();
        }

        public async Task RecordAsync(string languageId, string controllerName, string actionName, string httpVerb, bool isApi, double timeInMilliseconds)
        {
            var responseTiming = new ResponseTiming
            {
                LanguageId = languageId,
                ControllerName = controllerName,
                ActionName = actionName,
                HttpVerb = httpVerb,
                IsApi = isApi,
                TimeInMilliseconds = timeInMilliseconds
            };
            _dbContext.ResponseTimings.Add(responseTiming);
            await _dbContext.SaveChangesAsync();
        }
    }
}
