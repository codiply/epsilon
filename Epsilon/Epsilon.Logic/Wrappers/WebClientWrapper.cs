using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Timers;

namespace Epsilon.Logic.Wrappers
{
    public class WebClientWrapper : IWebClientWrapper
    {
        private readonly ITimerFactory _timerFactory;
        private readonly IElmahHelper _elmahHelper;

        private readonly WebClient _client = new WebClient();
        private bool _cancelled = false;

        public WebClientWrapper(
            ITimerFactory timerFactory,
            IElmahHelper elmahHelper)
        {
            _timerFactory = timerFactory;
            _elmahHelper = elmahHelper;
        }

        public async Task<WebClientResponse> DownloadStringTaskAsync(string url, double timeoutMilliseconds)
        {
            try {
                var timer = _timerFactory.Create();
                timer.IntervalMilliseconds = timeoutMilliseconds;
                timer.AutoReset = false;
                timer.Start();
                timer.Elapsed += new ElapsedEventHandler((source, e) => CancelAsync());

                _client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(CompletedEventHandler);

                var response = await _client.DownloadStringTaskAsync(url);

                return new WebClientResponse
                {
                    Status = WebClientResponseStatus.Success,
                    Response = response
                };
            }
            catch (Exception ex)
            {
                _elmahHelper.Raise(ex);

                if (_cancelled)
                {
                    return new WebClientResponse
                    {
                        Status = WebClientResponseStatus.Timeout
                    };
                }
                else
                {
                    return new WebClientResponse
                    {
                        Status = WebClientResponseStatus.Error,
                        ErrorMessage = ex.Message
                    };
                }
            }
        }

        private void CompletedEventHandler(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Cancelled)
                _cancelled = true;
        }

        public void CancelAsync()
        {
            _client.CancelAsync();
        }
    }
}
