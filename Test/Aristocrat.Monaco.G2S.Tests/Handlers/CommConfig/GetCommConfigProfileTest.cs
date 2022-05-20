namespace Aristocrat.Monaco.G2S.Tests.Handlers.CommConfig
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers.CommConfig;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetCommConfigProfileTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetCommConfigProfile(null);
            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var handler = CreateHandlerWithEmptyDevice();
            Assert.IsNotNull(handler);
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
            var device = new Mock<ICommConfigDevice>();
            var handler = CreateHandlerWithDevice(device);
            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            const int invalidHostId = 99;

            var device = new Mock<ICommConfigDevice>();
            device.SetupGet(d => d.Owner).Returns(invalidHostId);

            var handler = CreateHandlerWithDevice(device);
            await VerificationTests.VerifyChecksForOwnerAndGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithGuestAccessExpectNoError()
        {
            const int otherHostId = 99;

            var device = new Mock<ICommConfigDevice>();

            device.SetupGet(d => d.Owner).Returns(otherHostId);
            device.SetupGet(d => d.Guests).Returns(new List<int> { TestConstants.HostId });

            var handler = CreateHandlerWithDevice(device);

            await VerificationTests.VerifyAllowsGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var device = new Mock<ICommConfigDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);
            device.SetupGet(d => d.Id).Returns(TestConstants.HostId);

            var handler = CreateHandlerWithDevice(device);

            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenSessionTypeIsG2SResponseExpectNoResult()
        {
            var device = new Mock<ICommConfigDevice>();
            var handler = CreateHandlerWithDevice(device);

            var command =
                ClassCommandUtilities.CreateClassCommand<commConfig, getCommConfigProfile>(
                    TestConstants.HostId,
                    TestConstants.EgmId);

            command.Class.sessionType = t_sessionTypes.G2S_response;

            await handler.Handle(command);

            Assert.AreEqual(0, command.Responses.Count());
        }

        [TestMethod]
        public async Task WhenSessionTypeIsG2SNotificationExpectNoResult()
        {
            var device = new Mock<ICommConfigDevice>();
            var handler = CreateHandlerWithDevice(device);

            var command =
                ClassCommandUtilities.CreateClassCommand<commConfig, getCommConfigProfile>(
                    TestConstants.HostId,
                    TestConstants.EgmId);

            command.Class.sessionType = t_sessionTypes.G2S_notification;

            await handler.Handle(command);

            Assert.AreEqual(0, command.Responses.Count());
        }

        [TestMethod]
        public async Task WhenHandleCommandExpectResponse()
        {
            var mockDevice = new Mock<ICommConfigDevice>();
            mockDevice.SetupGet(m => m.MinLogEntries).Returns(5);
            var egm = HandlerUtilities.CreateMockEgm(mockDevice);
            var device = mockDevice.Object;

            var command = ClassCommandUtilities.CreateClassCommand<commConfig, getCommConfigProfile>(
                TestConstants.HostId,
                TestConstants.EgmId);

            var handler = new GetCommConfigProfile(egm);
            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<commConfig, commConfigProfile>;

            Assert.IsNotNull(response);
            Assert.AreEqual(response.Command.configurationId, device.ConfigurationId);
            Assert.AreEqual(response.Command.minLogEntries, device.MinLogEntries);
            Assert.AreEqual(response.Command.noResponseTimer, device.NoResponseTimer.TotalMilliseconds);
            Assert.AreEqual(response.Command.configDateTime, device.ConfigDateTime);
            Assert.AreEqual(response.Command.configComplete, device.ConfigComplete);
        }

        private static GetCommConfigProfile CreateHandlerWithEmptyDevice()
        {
            var egm = new Mock<IG2SEgm>();
            return new GetCommConfigProfile(egm.Object);
        }

        private static GetCommConfigProfile CreateHandlerWithDevice(Mock<ICommConfigDevice> device)
        {
            return new GetCommConfigProfile(HandlerUtilities.CreateMockEgm(device));
        }
    }
}