namespace Aristocrat.Monaco.G2S.Tests.Handlers.OptionConfig
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers.OptionConfig;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class GetOptionConfigProfileTests
    {
        [TestMethod]
        public void WhenConstructWithNullsExpectException()
        {
            ConstructorTest.TestConstructorNullChecks<GetOptionConfigProfile>();
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var handler = CreateHandlerWithEmptyDevice();
            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var device = new Mock<IOptionConfigDevice>();
            var handler = CreateHandlerWithDevice(device);
            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            const int invalidHostId = 99;

            var device = new Mock<IOptionConfigDevice>();
            device.SetupGet(d => d.Owner).Returns(invalidHostId);

            var handler = CreateHandlerWithDevice(device);
            await VerificationTests.VerifyChecksForOwnerAndGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithGuestAccessExpectNoError()
        {
            const int otherHostId = 99;

            var device = new Mock<IOptionConfigDevice>();

            device.SetupGet(d => d.Owner).Returns(otherHostId);
            device.SetupGet(d => d.Guests).Returns(new List<int> { TestConstants.HostId });

            var handler = CreateHandlerWithDevice(device);

            await VerificationTests.VerifyAllowsGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var device = new Mock<IOptionConfigDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);
            device.SetupGet(d => d.Id).Returns(TestConstants.HostId);

            var handler = CreateHandlerWithDevice(device);

            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenSessionTypeIsG2SResponseExpectNoResponse()
        {
            var egm = HandlerUtilities.CreateMockEgm<IOptionConfigDevice>();

            var command = ClassCommandUtilities.CreateClassCommand<optionConfig, getOptionConfigProfile>(
                TestConstants.HostId,
                TestConstants.EgmId);

            command.Class.sessionType = t_sessionTypes.G2S_response;

            var handler = new GetOptionConfigProfile(egm);
            await handler.Handle(command);

            await handler.Handle(command);

            Assert.AreEqual(0, command.Responses.Count());
        }

        [TestMethod]
        public async Task WhenSessionTypeIsG2SNotificationExpectNoResponse()
        {
            var egm = HandlerUtilities.CreateMockEgm<IOptionConfigDevice>();

            var command = ClassCommandUtilities.CreateClassCommand<optionConfig, getOptionConfigProfile>(
                TestConstants.HostId,
                TestConstants.EgmId);

            command.Class.sessionType = t_sessionTypes.G2S_notification;

            var handler = new GetOptionConfigProfile(egm);
            await handler.Handle(command);

            await handler.Handle(command);

            Assert.AreEqual(0, command.Responses.Count());
        }

        [TestMethod]
        public async Task WhenHandleCommandExpectResponse()
        {
            var mock = new Mock<IOptionConfigDevice>();
            mock.SetupGet(m => m.MinLogEntries).Returns(5);
            mock.SetupGet(m => m.ConfigDateTime).Returns(DateTime.UtcNow);
            var egm = HandlerUtilities.CreateMockEgm(mock);
            var device = mock.Object;

            var command = ClassCommandUtilities.CreateClassCommand<optionConfig, getOptionConfigProfile>(
                TestConstants.HostId,
                TestConstants.EgmId);

            var handler = new GetOptionConfigProfile(egm);
            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<optionConfig, optionConfigProfile>;

            Assert.IsNotNull(response);
            Assert.AreEqual(response.Command.configurationId, device.ConfigurationId);
            Assert.AreEqual(response.Command.minLogEntries, device.MinLogEntries);
            Assert.AreEqual(response.Command.noResponseTimer, device.NoResponseTimer.TotalMilliseconds);
            Assert.AreEqual(response.Command.configDateTime, device.ConfigDateTime);
            Assert.AreEqual(response.Command.configComplete, device.ConfigComplete);
            Assert.AreEqual(true, response.Command.configDateTimeSpecified);
        }

        private GetOptionConfigProfile CreateHandlerWithEmptyDevice()
        {
            var egm = new Mock<IG2SEgm>();
            return new GetOptionConfigProfile(egm.Object);
        }

        private GetOptionConfigProfile CreateHandlerWithDevice(Mock<IOptionConfigDevice> device)
        {
            return new GetOptionConfigProfile(HandlerUtilities.CreateMockEgm(device));
        }
    }
}