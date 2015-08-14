using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Wrappers.Interfaces
{
    public interface IWebClientWrapper
    {
        Task<string> DownloadStringTaskAsync(string url);

        Task<string> DownloadStringTaskAsync(string url, double timeoutMilliseconds);

        void CancelAsync();
    }
}
