using Epsilon.Logic.Wrappers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Epsilon.Logic.Wrappers
{
    public class WebClientWrapper : IWebClientWrapper
    {
        private readonly ITimerFactory _timerFactory;
        private readonly WebClient _client = new WebClient();
        private bool _cancelled = false;

        public WebClientWrapper(ITimerFactory timerFactory)
        {
            _timerFactory = timerFactory;
        }

        public async Task<string> DownloadStringTaskAsync(string url)
        {
            return await _client.DownloadStringTaskAsync(url);
        }

        public async Task<string> DownloadStringTaskAsync(string url, double timeoutMilliseconds)
        {
            try {
                var timer = _timerFactory.Create();
                timer.IntervalMilliseconds = timeoutMilliseconds;
                timer.AutoReset = false;
                timer.Start();
                timer.Elapsed += new ElapsedEventHandler((source, e) => CancelAsync());

                _client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(CompletedEventHandler);

                var answer = await _client.DownloadStringTaskAsync(url);

                return answer;
            }
            catch (WebException ex)
            {
                if (_cancelled)
                    throw new WebClientTimeoutException();
                else
                    throw ex;
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
