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
        public async Task RecordWithUserId_Test()
        {
            var email = "test@test.com";
            var ipAddressActivityType = IpAddressActivityType.Registration;
            var ipAddress = "1.2.3.4";

            var container = CreateContainer();

            var user = await CreateUser(container, email, ipAddress, false);

            var service = container.Get<IIpAddressActivityService>();
            await service.RecordWithUserId(user.Id, ipAddressActivityType, ipAddress);

            var ipAddressActivity = await DbProbe.IpAddressActivities.SingleOrDefaultAsync(x => x.UserId == user.Id);

            Assert.IsNotNull(ipAddressActivity, "The IpAddressActivity was not found");
            Assert.AreEqual(ipAddressActivityType, ipAddressActivity.ActivityTypeAsEnum,
                "The ActivityType was not the expected.");
        }

        [Test]
        public async Task RecordWithUserEmail_Test()
        {
            var email = "test@test.com";
            var ipAddressActivityType = IpAddressActivityType.Registration;
            var ipAddress = "1.2.3.4";

            var container = CreateContainer();

            var user = await CreateUser(container, email, ipAddress, false);

            var service = container.Get<IIpAddressActivityService>();

            await service.RecordWithUserEmail(email, ipAddressActivityType, ipAddress);

            var ipAddressActivity = await DbProbe.IpAddressActivities.SingleOrDefaultAsync(x => x.UserId == user.Id);

            Assert.IsNotNull(ipAddressActivity, "The IpAddressActivity was not found");
            Assert.AreEqual(ipAddressActivityType, ipAddressActivity.ActivityTypeAsEnum,
                "The ActivityType was not the expected.");
        }
    }
}
