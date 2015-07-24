using Epsilon.IntegrationTests.BaseFixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using System.Data.Entity;
using Epsilon.Logic.Services.Interfaces;
using NUnit.Framework;

namespace Epsilon.IntegrationTests.Logic.Services
{
    public class ResponseTimingServiceTest : BaseIntegrationTestWithRollback
    {
        [Test]
        public async Task Record_IsApiTrue()
        {
            var languageId = "en";
            var controllerName = "ControllerName";
            var actionName = "ActionName";
            var httpVerb = "HttpVerb";
            var isApi = true;
            double timeInMilliseconds = 3.14;

            var container = CreateContainer();
            var service = container.Get<IResponseTimingService>();

            var timeBefore = DateTimeOffset.Now;
            service.Record(languageId, controllerName, actionName, httpVerb, isApi, timeInMilliseconds);
            var timeAfter = DateTimeOffset.Now;

            var retrievedResponseTiming = await DbProbe.ResponseTimings
                .SingleOrDefaultAsync(x => timeBefore <= x.MeasuredOn && x.MeasuredOn <= timeAfter);

            Assert.IsNotNull(retrievedResponseTiming, "A ResponseTiming was not created.");
            Assert.AreEqual(languageId, retrievedResponseTiming.LanguageId,
                "The field LanguageId is not the expected.");
            Assert.AreEqual(controllerName, retrievedResponseTiming.ControllerName,
                "The field ControllerName is not the expected.");
            Assert.AreEqual(actionName, retrievedResponseTiming.ActionName,
                "The field ActionName is not the expected.");
            Assert.AreEqual(httpVerb, retrievedResponseTiming.HttpVerb,
                "The field HttpVerb is not the expected.");
            Assert.AreEqual(isApi, retrievedResponseTiming.IsApi,
                "The field IsApi is not the expected.");
        }

        [Test]
        public async Task Record_IsApiFalse()
        {
            var languageId = "en";
            var controllerName = "ControllerName";
            var actionName = "ActionName";
            var httpVerb = "HttpVerb";
            var isApi = false;
            double timeInMilliseconds = 3.14;

            var container = CreateContainer();
            var service = container.Get<IResponseTimingService>();

            var timeBefore = DateTimeOffset.Now;
            service.Record(languageId, controllerName, actionName, httpVerb, isApi, timeInMilliseconds);
            var timeAfter = DateTimeOffset.Now;

            var retrievedResponseTiming = await DbProbe.ResponseTimings
                .SingleOrDefaultAsync(x => timeBefore <= x.MeasuredOn && x.MeasuredOn <= timeAfter);

            Assert.IsNotNull(retrievedResponseTiming, "A ResponseTiming was not created.");
            Assert.AreEqual(languageId, retrievedResponseTiming.LanguageId,
                "The field LanguageId is not the expected.");
            Assert.AreEqual(controllerName, retrievedResponseTiming.ControllerName,
                "The field ControllerName is not the expected.");
            Assert.AreEqual(actionName, retrievedResponseTiming.ActionName,
                "The field ActionName is not the expected.");
            Assert.AreEqual(httpVerb, retrievedResponseTiming.HttpVerb,
                "The field HttpVerb is not the expected.");
            Assert.AreEqual(isApi, retrievedResponseTiming.IsApi,
                "The field IsApi is not the expected.");
        }

        [Test]
        public async Task RecordAsync_IsApiTrue()
        {
            var languageId = "en";
            var controllerName = "ControllerName";
            var actionName = "ActionName";
            var httpVerb = "HttpVerb";
            var isApi = true;
            double timeInMilliseconds = 3.14;

            var container = CreateContainer();
            var service = container.Get<IResponseTimingService>();

            var timeBefore = DateTimeOffset.Now;
            await service.RecordAsync(languageId, controllerName, actionName, httpVerb, isApi, timeInMilliseconds);
            var timeAfter = DateTimeOffset.Now;

            var retrievedResponseTiming = await DbProbe.ResponseTimings
                .SingleOrDefaultAsync(x => timeBefore <= x.MeasuredOn);

            Assert.IsNotNull(retrievedResponseTiming, "A ResponseTiming was not created.");
            Assert.AreEqual(languageId, retrievedResponseTiming.LanguageId,
                "The field LanguageId is not the expected.");
            Assert.AreEqual(controllerName, retrievedResponseTiming.ControllerName,
                "The field ControllerName is not the expected.");
            Assert.AreEqual(actionName, retrievedResponseTiming.ActionName,
                "The field ActionName is not the expected.");
            Assert.AreEqual(httpVerb, retrievedResponseTiming.HttpVerb,
                "The field HttpVerb is not the expected.");
            Assert.AreEqual(isApi, retrievedResponseTiming.IsApi,
                "The field IsApi is not the expected.");
        }

        [Test]
        public async Task RecordAsync_IsApiFalse()
        {
            var languageId = "en";
            var controllerName = "ControllerName";
            var actionName = "ActionName";
            var httpVerb = "HttpVerb";
            var isApi = false;
            double timeInMilliseconds = 3.14;

            var container = CreateContainer();
            var service = container.Get<IResponseTimingService>();

            var timeBefore = DateTimeOffset.Now;
            await service.RecordAsync(languageId, controllerName, actionName, httpVerb, isApi, timeInMilliseconds);
            var timeAfter = DateTimeOffset.Now;

            var retrievedResponseTiming = await DbProbe.ResponseTimings
                .SingleOrDefaultAsync(x => timeBefore <= x.MeasuredOn);

            Assert.IsNotNull(retrievedResponseTiming, "A ResponseTiming was not created.");
            Assert.AreEqual(languageId, retrievedResponseTiming.LanguageId,
                "The field LanguageId is not the expected.");
            Assert.AreEqual(controllerName, retrievedResponseTiming.ControllerName,
                "The field ControllerName is not the expected.");
            Assert.AreEqual(actionName, retrievedResponseTiming.ActionName,
                "The field ActionName is not the expected.");
            Assert.AreEqual(httpVerb, retrievedResponseTiming.HttpVerb,
                "The field HttpVerb is not the expected.");
            Assert.AreEqual(isApi, retrievedResponseTiming.IsApi,
                "The field IsApi is not the expected.");
        }
    }
}
