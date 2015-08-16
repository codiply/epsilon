using Epsilon.Logic.Constants.Enums;
using System;
using System.Threading.Tasks;

namespace Epsilon.Logic.Wrappers.Interfaces
{
    public class WebClientTimeoutException : Exception { }

    public class WebClientResponse
    {
        public string Response { get; set; }
        public WebClientResponseStatus Status { get; set; }
        public string ErrorMessage { get; set; }
    }

    public interface IWebClientWrapper
    {
        Task<WebClientResponse> DownloadStringTaskAsync(string url, double timeoutMilliseconds);

        void CancelAsync();
    }
}
