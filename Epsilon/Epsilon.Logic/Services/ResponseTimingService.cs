using Epsilon.Logic.Entities;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public void Record(string controllerName, string actionName, bool isApi, double timeInMilliseconds)
        {
            var responseTiming = new ResponseTiming
            {
                ControllerName = controllerName,
                ActionName = actionName,
                IsApi = isApi,
                TimeInMilliseconds = timeInMilliseconds
            };
            _dbContext.ResponseTimings.Add(responseTiming);
            _dbContext.SaveChanges();
        }

    }
}
