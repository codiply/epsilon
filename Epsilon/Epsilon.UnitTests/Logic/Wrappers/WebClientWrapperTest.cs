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

        [Test]
        public async Task DownloadStringTaskAsync_DoesNotThrowButLogsException()
        {
            var errorMessage = "error-message";
            var exception = new Exception(errorMessage);

            Exception exceptionLogged = null;
            var client = CreateWebClientWrapperWhereTimerThrowsException(exception, x => exceptionLogged = x);

            var timeoutInMilliseconds = 10 * 1000;
            var url = "http://www.google.com";

            var response = await client.DownloadStringTaskAsync(url, timeoutInMilliseconds);

            Assert.AreEqual(WebClientResponseStatus.Error, response.Status,
                "Status on response should be error.");
            Assert.AreEqual(errorMessage, response.ErrorMessage, "ErrorMessage is not the expected.");
            Assert.IsNotNull(exceptionLogged, "The exception was not logged.");
            Assert.AreEqual(errorMessage, exceptionLogged.Message,
                "The Message on the exception logged is not the expected.");
        }

        [Test]
        public async Task DownloadStringTaskAsync_TimeoutTest()
        {
            var client = CreateWebClientWrapper();

            var timeoutInMilliseconds = 10;
            var url = "http://www.google.com";

            var response = await client.DownloadStringTaskAsync(url, timeoutInMilliseconds);

            Assert.AreEqual(WebClientResponseStatus.Timeout, response.Status,
                "Status on response should be error.");
            Assert.IsNullOrEmpty(response.Response, "Response on response should be emtpy.");
            Assert.IsNullOrEmpty(response.ErrorMessage, "ErrorMessage on response should be empty.");

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

        private WebClientWrapper CreateWebClientWrapperWhereTimerThrowsException(
            Exception exception,
            Action<Exception> elmahHandler)
        {
            var timerFactory = new Mock<ITimerFactory>();
            timerFactory.Setup(x => x.Create()).Returns(() => { throw exception; });

            var elmahHelper = new Mock<IElmahHelper>();
            elmahHelper.Setup(x => x.Raise(It.IsAny<Exception>())).Callback(elmahHandler);

            var webClientWrapper = new WebClientWrapper(timerFactory.Object, elmahHelper.Object);

            return webClientWrapper;
        }
    }
}
