using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    public interface IResponseTimingService
    {
        void Record(string controllerName, string actionName, bool isApi, double timeInMilliseconds);
    }
}
