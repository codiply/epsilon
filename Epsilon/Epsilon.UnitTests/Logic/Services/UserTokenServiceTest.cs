using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.JsonModels;
using Epsilon.Logic.Services;
using Epsilon.Logic.Services.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Epsilon.UnitTests.Logic.Services
{
    [TestFixture]
    public class UserTokenServiceTest
    {
        #region CreateAccount
        
        [Test]
        public async Task CreateAccountTest()
        {
            string userId = "user-id";
            string userIdUsed = null;

            var pageSize = 2;
            IAppCache appCache = null;
            var config = CreateConfig(pageSize);
            var tokenAccountService = CreateTokenAccountService(
                createAccountCallback: uId => userIdUsed = uId);
            ITokenRewardService tokenRewardService = null;

            var service = new UserTokenService(appCache, config, tokenAccountService, tokenRewardService);

            await service.CreateAccount(userId);

            Assert.AreEqual(userId, userIdUsed,
                "The userId used is not the expected or method CreateAccount was not called on the TokenAccountService.");
        }

        #endregion

        #region GetMyTokenTransactionsNextPage

        [Test]
        public async Task GetMyTokenTransactionsNextPageTest()
        {
            string userId = "user-id";

            string accountIdUsed = null;
            MyTokenTransactionsPageRequest requestUsed = null;
            int? pageSizeUsed = null;

            var pageSize = 2;
            IAppCache appCache = null;
            var config = CreateConfig(pageSize);
            var myTokenTransactionsPageResponse = new MyTokenTransactionsPageResponse();
            var tokenAccountService = CreateTokenAccountService(
                getMyTokenTransactionsNextPageCallback: (accId, req, size) =>
                {
                    accountIdUsed = accId;
                    requestUsed = req;
                    pageSizeUsed = size;
                },
                myTokenTransactionsPageResponse: myTokenTransactionsPageResponse);
            ITokenRewardService tokenRewardService = null;

            var service = new UserTokenService(appCache, config, tokenAccountService, tokenRewardService);

            var request = new MyTokenTransactionsPageRequest();

            var response = await service.GetMyTokenTransactionsNextPage(userId, request);

            Assert.AreEqual(userId, accountIdUsed,
                "The accountId used is not the expected or TokenAccountService was not called.");
            Assert.AreSame(request, requestUsed, "The request was not passed on to the TokenAccountService.");
            Assert.AreEqual(pageSize, pageSizeUsed, "The page size used is not the expected");
            Assert.AreSame(myTokenTransactionsPageResponse, response,
                "The response of the TokenAccountService was not returned.");
        }

        #endregion

        #region MakeTransaction

        [Test]
        public async Task MakeTransaction_WrongQuantityTest()
        {
            var service = new UserTokenService(null, null, null, null);

            var accountId = "any-account-id";

            var status1 = await service.MakeTransaction(accountId, TokenRewardKey.EarnPerTenancyDetailsSubmission,
                Guid.NewGuid(), quantity: 0);
            Assert.AreEqual(TokenAccountTransactionStatus.WrongQuantity, status1,
                "Status1 was not the expected.");

            var status2 = await service.MakeTransaction(accountId, TokenRewardKey.EarnPerTenancyDetailsSubmission,
                Guid.NewGuid(), quantity: -1);
            Assert.AreEqual(TokenAccountTransactionStatus.WrongQuantity, status2,
                "Status2 was not the expected.");
        }

        #endregion

        #region SufficientFundsExistForTransaction

        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentException))]
        public async Task SufficientFundsExistForTransaction_ZeroQuantityTest()
        {
            var service = new UserTokenService(null, null, null, null);

            var accountId = "any-account-id";

            await service.SufficientFundsExistForTransaction(
                accountId, TokenRewardKey.EarnPerTenancyDetailsSubmission, quantity: 0);
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentException))]
        public async Task SufficientFundsExistForTransaction_NegativeQuantityTest()
        {
            var service = new UserTokenService(null, null, null, null);

            var accountId = "any-account-id";

            await service.SufficientFundsExistForTransaction(
                accountId, TokenRewardKey.EarnPerTenancyDetailsSubmission, quantity: -1);
        }

        #endregion

        #region Private Helpers

        private ITokenAccountService CreateTokenAccountService(
            Action<string> createAccountCallback = null,
            Action<string, MyTokenTransactionsPageRequest, int> getMyTokenTransactionsNextPageCallback = null,
            MyTokenTransactionsPageResponse myTokenTransactionsPageResponse = null)
        {
            var mockTokenAccountService = new Mock<ITokenAccountService>();

            if (createAccountCallback != null)
            {
                mockTokenAccountService.Setup(x => x.CreateAccount(It.IsAny<string>()))
                    .Callback(createAccountCallback)
                    .Returns(Task.FromResult(1));
            }

            if (getMyTokenTransactionsNextPageCallback != null && myTokenTransactionsPageResponse != null)
            {
                mockTokenAccountService.Setup(x => x.GetMyTokenTransactionsNextPage(
                    It.IsAny<string>(), It.IsAny<MyTokenTransactionsPageRequest>(), It.IsAny<int>()))
                    .Callback(getMyTokenTransactionsNextPageCallback)
                    .Returns(Task.FromResult(myTokenTransactionsPageResponse));
            }

            return mockTokenAccountService.Object;
        }

        private IUserTokenServiceConfig CreateConfig(int myTokenTransactionsPageSize)
        {
            var mockUserTokenServiceConfig = new Mock<IUserTokenServiceConfig>();

            mockUserTokenServiceConfig.Setup(x => x.MyTokenTransactions_PageSize)
                .Returns(myTokenTransactionsPageSize);

            return mockUserTokenServiceConfig.Object;
        }

        #endregion
    }
}
