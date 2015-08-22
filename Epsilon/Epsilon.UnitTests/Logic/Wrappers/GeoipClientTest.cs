using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers;
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
    public class GeoipClientTest
    {
        static GeoipProviderName[] GeoipProviderNames = EnumsHelper.GeoipProviderName.GetValues().ToArray();

        [Test, TestCaseSource("GeoipProviderNames")]
        public async Task Geoip_SuccessfulCaseForEachProvider(GeoipProviderName provider)
        {
            var timeoutInMilliseconds = 10 * 1000;
            Exception exceptionLogged = null;
            var ipAddress = "8.8.8.8";
            var expectedLatitude = 37.386;
            var expectedLongitude = -122.084;
            var acceptableDelta = 0.001;

            var elmahHelper = CreateElmahHelper((ex) => exceptionLogged = ex);
            var geoipClient = CreateGeoipClient(timeoutInMilliseconds, () => CreateWebClient(elmahHelper), elmahHelper);

            exceptionLogged = null;
            var providerToString = EnumsHelper.GeoipProviderName.ToString(provider);
            var geoipClientResponse = await geoipClient.Geoip(provider, ipAddress);
            Assert.IsNullOrEmpty(geoipClientResponse.ErrorMessage,
                string.Format("ErrorMessage is not the expected for provider '{0}'.", providerToString));
            Assert.AreEqual(WebClientResponseStatus.Success, geoipClientResponse.Status,
                string.Format("Status is not the expected for provider '{0}'.", providerToString));
            Assert.AreEqual(provider, geoipClientResponse.GeoipProviderName,
                string.Format("ProviderName is not the expected for provider '{0}'.", providerToString));
            Assert.AreEqual(expectedLatitude, geoipClientResponse.Latitude, acceptableDelta,
                string.Format("Latitude is not the expected for provider '{0}'.", providerToString));
            Assert.AreEqual(expectedLongitude, geoipClientResponse.Longitude, acceptableDelta,
                string.Format("Longitude is not the expected for provider '{0}'.", providerToString));
            Assert.IsNull(exceptionLogged,
                string.Format("No exception should be logged for provider '{0}'.", providerToString));
        }

        [Test, TestCaseSource("GeoipProviderNames")]
        public async Task Geoip_InternalIpAddressForEachProvider_HandledGracefully(GeoipProviderName provider)
        {
            var timeoutInMilliseconds = 3 * 1000;
            Exception exceptionLogged = null;
            var ipAddress = "192.168.1.1";

            var elmahHelper = CreateElmahHelper((ex) => exceptionLogged = ex);
            var geoipClient = CreateGeoipClient(timeoutInMilliseconds, () => CreateWebClient(elmahHelper), elmahHelper);

            exceptionLogged = null;
            var providerToString = EnumsHelper.GeoipProviderName.ToString(provider);
            var geoipClientResponse = await geoipClient.Geoip(provider, ipAddress);
            Assert.IsNullOrEmpty(geoipClientResponse.ErrorMessage,
                string.Format("ErrorMessage is not the expected for provider '{0}'.", providerToString));
            Assert.AreEqual(WebClientResponseStatus.Success, geoipClientResponse.Status,
                string.Format("Status is not the expected for provider '{0}'.", providerToString));
            Assert.AreEqual(provider, geoipClientResponse.GeoipProviderName,
                string.Format("ProviderName is not the expected for provider '{0}'.", providerToString));
            Assert.IsNullOrEmpty(geoipClientResponse.CountryCode,
                string.Format("CountryCode is not the expected for provider '{0}'.", providerToString));
            Assert.IsNull(exceptionLogged,
                string.Format("No exception should be logged for provider '{0}'.", providerToString));
        }

        [Test]
        public async Task Geoip_WebClientReturnsError()
        {
            var timeoutInMilliseconds = 3 * 1000;
            Exception exceptionLogged = null;
            var ipAddress = "8.8.8.8";
            var errorMessage = "error-message";

            var webClientResponse = new WebClientResponse
            {
                Status = WebClientResponseStatus.Error,
                ErrorMessage = errorMessage
            };

            var elmahHelper = CreateElmahHelper((ex) => exceptionLogged = ex);
            var geoipClient = CreateGeoipClient(
                timeoutInMilliseconds, 
                () => CreateWebClientThatReturns(webClientResponse), 
                elmahHelper);

            foreach (var provider in EnumsHelper.GeoipProviderName.GetValues())
            {
                exceptionLogged = null;
                var providerToString = EnumsHelper.GeoipProviderName.ToString(provider);
                var geoipClientResponse = await geoipClient.Geoip(provider, ipAddress);
                Assert.AreEqual(WebClientResponseStatus.Error, geoipClientResponse.Status,
                    string.Format("Status is not the expected for provider '{0}'.", providerToString));
                Assert.AreEqual(provider, geoipClientResponse.GeoipProviderName,
                    string.Format("ProviderName is not the expected for provider '{0}'.", providerToString));
                Assert.IsNull(geoipClientResponse.Latitude,
                    string.Format("Latitude is not the expected for provider '{0}'.", providerToString));
                Assert.IsNull(geoipClientResponse.Longitude,
                    string.Format("Longitude is not the expected for provider '{0}'.", providerToString));
                Assert.AreEqual(errorMessage, geoipClientResponse.ErrorMessage,
                    string.Format("ErrorMessage is not the expected for provider '{0}'.", providerToString));
                Assert.IsNull(exceptionLogged,
                    string.Format("No exception should be logged for provider '{0}'.", providerToString));
            }

        }

        [Test]
        public async Task Geoip_WebClientReturnsTimeout()
        {
            var timeoutInMilliseconds = 10 * 1000;
            Exception exceptionLogged = null;
            var ipAddress = "8.8.8.8";

            var webClientResponse = new WebClientResponse
            {
                Status = WebClientResponseStatus.Timeout,
                ErrorMessage = null
            };

            var elmahHelper = CreateElmahHelper((ex) => exceptionLogged = ex);
            var geoipClient = CreateGeoipClient(
                timeoutInMilliseconds,
                () => CreateWebClientThatReturns(webClientResponse),
                elmahHelper);

            foreach (var provider in EnumsHelper.GeoipProviderName.GetValues())
            {
                exceptionLogged = null;
                var providerToString = EnumsHelper.GeoipProviderName.ToString(provider);
                var geoipClientResponse = await geoipClient.Geoip(provider, ipAddress);
                Assert.AreEqual(WebClientResponseStatus.Timeout, geoipClientResponse.Status,
                    string.Format("Status is not the expected for provider '{0}'.", providerToString));
                Assert.AreEqual(provider, geoipClientResponse.GeoipProviderName,
                    string.Format("ProviderName is not the expected for provider '{0}'.", providerToString));
                Assert.IsNull(geoipClientResponse.Latitude,
                    string.Format("Latitude is not the expected for provider '{0}'.", providerToString));
                Assert.IsNull(geoipClientResponse.Longitude,
                    string.Format("Longitude is not the expected for provider '{0}'.", providerToString));
                Assert.IsNull(geoipClientResponse.ErrorMessage,
                    string.Format("ErrorMessage is not the expected for provider '{0}'.", providerToString));
                Assert.IsNull(exceptionLogged,
                    string.Format("No exception should be logged for provider '{0}'.", providerToString));
            }
        }

        [Test]
        public async Task Geoip_WebClientThrowsException()
        {
            // The WebClient cannot throw exception, but this way I can simulate an Exception being
            // thrown and make sure that the GeoipClient swallows the exception and logs it.

            var timeoutInMilliseconds = 10 * 1000;
            Exception exceptionLogged = null;
            var ipAddress = "8.8.8.8";
            var exceptionErrorMessage = "exception-error-message";

            var exception = new Exception(exceptionErrorMessage); 

            var elmahHelper = CreateElmahHelper((ex) => exceptionLogged = ex);
            var geoipClient = CreateGeoipClient(
                timeoutInMilliseconds,
                () => CreateWebClientThatThrows(exception),
                elmahHelper);

            foreach (var provider in EnumsHelper.GeoipProviderName.GetValues())
            {
                exceptionLogged = null;
                var providerToString = EnumsHelper.GeoipProviderName.ToString(provider);
                var geoipClientResponse = await geoipClient.Geoip(provider, ipAddress);
                Assert.AreEqual(WebClientResponseStatus.Error, geoipClientResponse.Status,
                    string.Format("Status is not the expected for provider '{0}'.", providerToString));
                Assert.AreEqual(provider, geoipClientResponse.GeoipProviderName,
                    string.Format("ProviderName is not the expected for provider '{0}'.", providerToString));
                Assert.IsNull(geoipClientResponse.Latitude,
                    string.Format("Latitude is not the expected for provider '{0}'.", providerToString));
                Assert.IsNull(geoipClientResponse.Longitude,
                    string.Format("Longitude is not the expected for provider '{0}'.", providerToString));
                Assert.AreEqual(exceptionErrorMessage, geoipClientResponse.ErrorMessage,
                    string.Format("ErrorMessage is not the expected for provider '{0}'.", providerToString));
                Assert.IsNotNull(exceptionLogged,
                    string.Format("An exception should be logged for provider '{0}'.", providerToString));
                Assert.AreEqual(exceptionErrorMessage, exceptionLogged.Message,
                    string.Format("An exception should be logged for provider '{0}'.", providerToString));
            }
        }


        private IGeoipClient CreateGeoipClient(
            double timeoutInMilliseconds,
            Func<IWebClientWrapper> webClientValueFuction,
            IElmahHelper elmahHelper)
        {
            var mockConfig = new Mock<IGeoipClientConfig>();
            mockConfig.Setup(x => x.TimeoutInMilliseconds).Returns(timeoutInMilliseconds);
            
            var mockFactory = new Mock<IWebClientFactory>();
            mockFactory.Setup(x => x.Create()).Returns(webClientValueFuction);

            return new GeoipClient(mockFactory.Object, mockConfig.Object, elmahHelper);
        }

        private IElmahHelper CreateElmahHelper(Action<Exception> elmahHelperRaiseHandler)
        {
            var elmahHelper = new Mock<IElmahHelper>();
            elmahHelper.Setup(x => x.Raise(It.IsAny<Exception>())).Callback(elmahHelperRaiseHandler);

            return elmahHelper.Object;
        }

        private IWebClientWrapper CreateWebClient(IElmahHelper elmahHelper)
        {
            var mockTimerFactory = new Mock<ITimerFactory>();
            mockTimerFactory.Setup(x => x.Create()).Returns(() => new TimerWrapper());

            return new WebClientWrapper(mockTimerFactory.Object, elmahHelper);
        }

        private IWebClientWrapper CreateWebClientThatReturns(WebClientResponse response)
        {
            var mockWebClient = new Mock<IWebClientWrapper>();
            mockWebClient
                .Setup(x => x.DownloadStringTaskAsync(It.IsAny<string>(), It.IsAny<double>()))
                .Returns(Task.FromResult(response));
            return mockWebClient.Object;
        }

        private IWebClientWrapper CreateWebClientThatThrows(Exception ex)
        {
            var mockWebClient = new Mock<IWebClientWrapper>();
            mockWebClient
                .Setup(x => x.DownloadStringTaskAsync(It.IsAny<string>(), It.IsAny<double>()))
                .Callback<string, double>((x, y) => { throw ex; });
            return mockWebClient.Object;
        }
    }
}
