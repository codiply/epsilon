using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Interfaces;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Infrastructure.Primitives;
using Epsilon.Logic.Services.Interfaces;
using Moq;
using Ninject;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class AntiAbuseServiceTest : BaseIntegrationTestWithRollback
    {
        [Test]
        public async Task CanRegister_WithBothDisableSwitchesTrue_ReturnsIsRejectedFalse()
        {
            var disableGlobalFrequencyCheck = true;
            var globalMaxFrequency = "1/D";
            var disableIpAddressFrequencyCheck = true;
            var maxFrequencyPerIpAddress = "1/D";

            var ipAddress = "1.2.3.4";

            var container = CreateContainer();
            SetupContainerForCanRegister(container,
                disableGlobalFrequencyCheck, globalMaxFrequency,
                disableIpAddressFrequencyCheck, maxFrequencyPerIpAddress);
            var service = container.Get<IAntiAbuseService>();

            for (int i = 0; i < 10; i++)
            {
                await CreateUser(container, String.Format("test{0}@test.com", i), ipAddress);
            }

            var response = await service.CanRegister(ipAddress);

            Assert.IsFalse(response.IsRejected, "The response IsRejected property should be false.");
        }

        private static void SetupContainerForCanRegister(IKernel container,
            bool disableGlobalFrequencyCheck, string globalMaxFrequency,
            bool disableIpAddressFrequencyCheck, string maxFrequencyPerIpAddress)

        {
            var parseHelper = new ParseHelper();

            var mockDbAppSetingHelper = new Mock<IDbAppSettingsHelper>();
            mockDbAppSetingHelper.Setup(x => x.GetBool(DbAppSettingKey.AntiAbuse_Register_DisableGlobalFrequencyCheck))
                .Returns(disableGlobalFrequencyCheck);
            mockDbAppSetingHelper.Setup(x => x.GetFrequency(DbAppSettingKey.AntiAbuse_Register_GlobalMaxFrequency, It.IsAny<Frequency>()))
                .Returns(parseHelper.ParseFrequency(globalMaxFrequency));
            mockDbAppSetingHelper.Setup(x => x.GetBool(DbAppSettingKey.AntiAbuse_Register_DisableIpAddressFrequencyCheck))
                .Returns(disableIpAddressFrequencyCheck);
            mockDbAppSetingHelper.Setup(x => x.GetFrequency(DbAppSettingKey.AntiAbuse_Register_MaxFrequencyPerIpAddress, It.IsAny<Frequency>()))
                .Returns(parseHelper.ParseFrequency(maxFrequencyPerIpAddress));
            container.Rebind<IDbAppSettingsHelper>().ToConstant(mockDbAppSetingHelper.Object);

            var mockAppSettingsDefaultValue = new Mock<IDbAppSettingDefaultValue>();
            mockAppSettingsDefaultValue.Setup(x => x.AntiAbuse_Register_GlobalMaxFrequency)
                .Returns(parseHelper.ParseFrequency(globalMaxFrequency));
            mockAppSettingsDefaultValue.Setup(x => x.AntiAbuse_Register_MaxFrequencyPerIpAddress)
                .Returns(parseHelper.ParseFrequency(maxFrequencyPerIpAddress));
            container.Rebind<IDbAppSettingDefaultValue>().ToConstant(mockAppSettingsDefaultValue.Object);
        }
    }
}
