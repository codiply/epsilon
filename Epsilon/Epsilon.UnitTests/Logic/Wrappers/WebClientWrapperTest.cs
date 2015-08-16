using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Wrappers;
using Epsilon.Logic.Wrappers.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.UnitTests.Logic.Wrappers
{
    [TestFixture]
    public class WebClientWrapperTest
    {
        [Test]
        public async Task DownloadStringTaskAsync_WorksForSingleRequest()
        {
            var client = CreateWebClientWrapper();

            var timeoutInMilliseconds = 10 * 1000;
            var url = "http://www.google.com";

            var firstResponse = await client.DownloadStringTaskAsync(url, timeoutInMilliseconds);

            Assert.AreEqual(WebClientResponseStatus.Success, firstResponse.Status,
                "Status on first response should be Success.");
            Assert.IsNotNullOrEmpty(firstResponse.Response, "First response should not be null or empty.");
            Assert.IsNullOrEmpty(firstResponse.ErrorMessage, "ErrorMessage on first response should be null or empty.");

            var secondResponse = await client.DownloadStringTaskAsync(url, timeoutInMilliseconds);

            Assert.AreEqual(WebClientResponseStatus.Error, secondResponse.Status,
                "Status on second response should be Error.");
            Assert.IsNullOrEmpty(secondResponse.Response, "Second response should be null or empty.");
            Assert.IsNotNullOrEmpty(secondResponse.ErrorMessage, "ErrorMessage on second response should not be null or empty.");
        }

        private WebClientWrapper CreateWebClientWrapper()
        {
            var timerFactory = new Mock<ITimerFactory>();
            timerFactory.Setup(x => x.Create()).Returns(new TimerWrapper());

            var elmahHelper = new Mock<IElmahHelper>();
            elmahHelper.Setup(x => x.Raise(It.IsAny<Exception>())).Callback(() => { });

            var webClientWrapper = new WebClientWrapper(timerFactory.Object, elmahHelper.Object);

            return webClientWrapper;
        }
    }
}
