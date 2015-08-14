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
            var timer = _timerFactory.Create();
            timer.IntervalMilliseconds = timeoutMilliseconds;
            timer.AutoReset = false;
            timer.Start();
            timer.Elapsed += new ElapsedEventHandler((source, args) => CancelAsync());

            var answer = await _client.DownloadStringTaskAsync(url);
            return answer;
        }

        public void CancelAsync()
        {
            _client.CancelAsync();
        }
    }
}
