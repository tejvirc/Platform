namespace Aristocrat.Monaco.G2S.Tests.Handlers.OptionConfig
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers;
    using G2S.Handlers.OptionConfig;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class GetOptionConfigModeStatusTests
    {
        private readonly Mock<ICommandBuilder<IOptionConfigDevice, optionConfigModeStatus>> _builder =
            new Mock<ICommandBuilder<IOptionConfigDevice, optionConfigModeStatus>>();

        [TestMethod]
        public void WhenConstructWithNullsExpectException()
        {
            ConstructorTest.TestConstructorNullChecks<GetOptionConfigModeStatus>();
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var handler = CreateHandlerWithEmptyDevice();
            Assert.IsNotNull(handler);
        }

        private GetOptionConfigModeStatus CreateHandlerWithEmptyDevice()
        {
            var egm = new Mock<IG2SEgm>();
            var builder = new Mock<ICommandBuilder<IOptionConfigDevice, optionConfigModeStatus>>();
            return new GetOptionConfigModeStatus(egm.Object, builder.Object);
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
            var handler = CreateHandlerWithDevice(new Mock<IOptionConfigDevice>());
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

            var handler = CreateHandlerWithDevice(device);
            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenSessionTypeIsG2SNotificationExpectNoResponse()
        {
            var command = ClassCommandUtilities.CreateClassCommand<optionConfig, getOptionConfigModeStatus>(
                TestConstants.HostId,
                TestConstants.EgmId);

            command.Class.sessionType = t_sessionTypes.G2S_notification;

            var handler = CreateHandlerWithDevice(new Mock<IOptionConfigDevice>());

            await handler.Handle(command);

            Assert.AreEqual(0, command.Responses.Count());
        }

        [TestMethod]
        public async Task WhenSessionTypeIsG2SResponseExpectNoResponse()
        {
            var command = ClassCommandUtilities.CreateClassCommand<optionConfig, getOptionConfigModeStatus>(
                TestConstants.HostId,
                TestConstants.EgmId);

            command.Class.sessionType = t_sessionTypes.G2S_response;

            var handler = CreateHandlerWithDevice(new Mock<IOptionConfigDevice>());

            await handler.Handle(command);

            Assert.AreEqual(0, command.Responses.Count());
        }

        [TestMethod]
        public async Task WhenHandleCommandExpectResponse()
        {
            var deviceId = 1;

            var device = new Mock<IOptionConfigDevice>();
            device.SetupGet(x => x.Id).Returns(deviceId);

            var egm = new Mock<IG2SEgm>();

            egm.Setup(e => e.GetDevice<IOptionConfigDevice>(It.Is<int>(id => id == deviceId)))
                .Returns(device.Object);

            var command = ClassCommandUtilities.CreateClassCommand<optionConfig, getOptionConfigModeStatus>(
                TestConstants.HostId,
                TestConstants.EgmId);

            var handler = CreateHandlerWithDevice(egm.Object);
            await handler.Handle(command);

            _builder.Verify(
                x =>
                    x.Build(
                        It.Is<IOptionConfigDevice>(d => d.Id == deviceId),
                        It.IsAny<optionConfigModeStatus>()),
                Times.Once);
        }

        private GetOptionConfigModeStatus CreateHandlerWithDevice(Mock<IOptionConfigDevice> device)
        {
            return CreateHandlerWithDevice(HandlerUtilities.CreateMockEgm(device));
        }

        private GetOptionConfigModeStatus CreateHandlerWithDevice(IG2SEgm egm)
        {
            return new GetOptionConfigModeStatus(egm, _builder.Object);
        }
    }
}