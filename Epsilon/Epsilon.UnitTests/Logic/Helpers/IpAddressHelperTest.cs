using Epsilon.Logic.Helpers;
using Epsilon.Logic.Helpers.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Epsilon.UnitTests.Logic.Helpers
{
    [TestFixture]
    public class IpAddressHelperTest
    {
        IIpAddressHelper _helper = new IpAddressHelper();

        private const string XForwardedFor = "X_FORWARDED_FOR";
        private const string MalformedIpAddress = "MALFORMED";
        private const string DefaultIpAddress = "0.0.0.0";
        private const string GoogleIpAddress = "74.125.224.224";
        private const string MicrosoftIpAddress = "65.55.58.201";
        private const string Private24Bit = "10.0.0.0";
        private const string Private20Bit = "172.16.0.0";
        private const string Private16Bit = "192.168.0.0";
        private const string PrivateLinkLocal = "169.254.0.0";

        [Test]
        public void PublicIpAndNullXForwardedFor_Returns_CorrectIp()
        {
            // Arrange
            var request = CreateMockHttpRequest(GoogleIpAddress, null);

            // Act
            var ip = _helper.GetClientIpAddress(request);

            // Assert
            Assert.AreEqual(GoogleIpAddress, ip);
        }

        [Test]
        public void PublicIpAndEmptyXForwardedFor_Returns_CorrectIp()
        {
            // Arrange
            var request = CreateMockHttpRequest(GoogleIpAddress, string.Empty);

            // Act
            var ip = _helper.GetClientIpAddress(request);

            // Assert
            Assert.AreEqual(GoogleIpAddress, ip);
        }

        [Test]
        public void MalformedUserHostAddress_Returns_DefaultIpAddress()
        {
            // Arrange
            var request = CreateMockHttpRequest(MalformedIpAddress, null);

            // Act
            var ip = _helper.GetClientIpAddress(request);

            // Assert
            Assert.AreEqual(DefaultIpAddress, ip);
        }

        [Test]
        public void MalformedXForwardedFor_Returns_DefaultIpAddress()
        {
            // Arrange
            var request = CreateMockHttpRequest(GoogleIpAddress, MalformedIpAddress);

            // Act
            var ip = _helper.GetClientIpAddress(request);

            // Assert
            Assert.AreEqual(DefaultIpAddress, ip);
        }

        [Test]
        public void SingleValidPublicXForwardedFor_Returns_XForwardedFor()
        {
            // Arrange
            var request = CreateMockHttpRequest(GoogleIpAddress, MicrosoftIpAddress);

            // Act
            var ip = _helper.GetClientIpAddress(request);

            // Assert
            Assert.AreEqual(MicrosoftIpAddress, ip);
        }

        [Test]
        public void MultipleValidPublicXForwardedFor_Returns_LastXForwardedFor()
        {
            // Arrange
            var request = CreateMockHttpRequest(GoogleIpAddress, GoogleIpAddress + "," + MicrosoftIpAddress);

            // Act
            var ip = _helper.GetClientIpAddress(request);

            // Assert
            Assert.AreEqual(MicrosoftIpAddress, ip);
        }

        [Test]
        public void SinglePrivateXForwardedFor_Returns_UserHostAddress()
        {
            // Arrange
            var request = CreateMockHttpRequest(GoogleIpAddress, Private24Bit);

            // Act
            var ip = _helper.GetClientIpAddress(request);

            // Assert
            Assert.AreEqual(GoogleIpAddress, ip);
        }

        [Test]
        public void MultiplePrivateXForwardedFor_Returns_UserHostAddress()
        {
            // Arrange
            const string privateIpList = Private24Bit + "," + Private20Bit + "," + Private16Bit + "," + PrivateLinkLocal;
            var request = CreateMockHttpRequest(GoogleIpAddress, privateIpList);

            // Act
            var ip = _helper.GetClientIpAddress(request);

            // Assert
            Assert.AreEqual(GoogleIpAddress, ip);
        }

        [Test]
        public void MultiplePublicXForwardedForWithPrivateLast_Returns_LastPublic()
        {
            // Arrange
            const string privateIpList = Private24Bit + "," + Private20Bit + "," + MicrosoftIpAddress + "," + PrivateLinkLocal;
            var request = CreateMockHttpRequest(GoogleIpAddress, privateIpList);

            // Act
            var ip = _helper.GetClientIpAddress(request);

            // Assert
            Assert.AreEqual(MicrosoftIpAddress, ip);
        }

        private HttpRequestBase CreateMockHttpRequest(string userHostAddress, string xForwardedFor)
        {
            var httpRequest = new Mock<HttpRequestBase>();
            httpRequest.SetupGet(x => x.UserHostAddress).Returns(userHostAddress);
            var serverVariables = new NameValueCollection() {
                    { XForwardedFor, xForwardedFor },
                };
            httpRequest.Setup(x => x.ServerVariables.Get(It.IsAny<string>()))
                .Returns<string>(x =>
                {
                    return serverVariables[x];
                });

            return httpRequest.Object;
        }
    }
}
