using Epsilon.IntegrationTests.BaseFixtures;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Services.Interfaces;
using Ninject;
using NUnit.Framework;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class IpAddressActivityService : BaseIntegrationTestWithRollback
    {
        [Test]
        public async Task RecordRegistrationTest()
        {
            var email = "test@test.com";
            var ipAddress = "1.2.3.4";

            var container = CreateContainer();

            var user = await CreateUser(container, email, ipAddress, false);

            var service = container.Get<IIpAddressActivityService>();
            await service.RecordRegistration(user.Id, ipAddress);

            var ipAddressActivity = await DbProbe.IpAddressActivities.SingleOrDefaultAsync(x => x.UserId == user.Id);

            Assert.IsNotNull(ipAddressActivity, "The IpAddressActivity was not found");
            Assert.AreEqual(IpAddressActivityType.Registration, ipAddressActivity.ActivityTypeAsEnum,
                "The ActivityType was not Registration.");
        }

        [Test]
        public async Task RecordLoginTest()
        {
            var email = "test@test.com";
            var ipAddress = "1.2.3.4";

            var container = CreateContainer();

            var user = await CreateUser(container, email, ipAddress, false);

            var service = container.Get<IIpAddressActivityService>();
            await service.RecordLogin(email, ipAddress);

            var ipAddressActivity = await DbProbe.IpAddressActivities.SingleOrDefaultAsync(x => x.UserId == user.Id);

            Assert.IsNotNull(ipAddressActivity, "The IpAddressActivity was not found");
            Assert.AreEqual(IpAddressActivityType.Login, ipAddressActivity.ActivityTypeAsEnum,
                "The ActivityType was not Login.");
        }
    }
}
