namespace Aristocrat.Monaco.G2S.Tests.Handlers.CommConfig
{
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers;
    using G2S.Handlers.CommConfig;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class GetCommConfigModeStatusTest
    {
        private readonly Mock<ICommandBuilder<ICommConfigDevice, commConfigModeStatus>> _commandBuilderMock =
            new Mock<ICommandBuilder<ICommConfigDevice, commConfigModeStatus>>();

        private readonly Mock<IG2SEgm> _egmMock = new Mock<IG2SEgm>();

        [TestMethod]
        public void WhenConstructWithNullsExpectException()
        {
            ConstructorTest.TestConstructorNullChecks<GetCommConfigModeStatus>();
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var handler = new GetCommConfigModeStatus(_egmMock.Object, _commandBuilderMock.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            const int invalidHostId = 99;

            var device = new Mock<ICommConfigDevice>();
            device.SetupGet(d => d.Owner).Returns(invalidHostId);

            var handler = new GetCommConfigModeStatus(
                HandlerUtilities.CreateMockEgm(device),
                _commandBuilderMock.Object);

            await VerificationTests.VerifyChecksForOwnerAndGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var device = new Mock<ICommConfigDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);
            device.SetupGet(d => d.Id).Returns(TestConstants.HostId);

            var handler = new GetCommConfigModeStatus(
                HandlerUtilities.CreateMockEgm(device),
                _commandBuilderMock.Object);

            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenSessionTypeIsG2SNotificationExpectNoResult()
        {
            var handler = new GetCommConfigModeStatus(_egmMock.Object, _commandBuilderMock.Object);

            var command = ClassCommandUtilities.CreateClassCommand<commConfig, getCommConfigModeStatus>(
                TestConstants.HostId,
                TestConstants.EgmId);

            command.Class.sessionType = t_sessionTypes.G2S_notification;

            await handler.Handle(command);

            Assert.AreEqual(0, command.Responses.Count());
        }

        [TestMethod]
        public async Task WhenHandleCommandExpectSuccess()
        {
            var device = new Mock<ICommConfigDevice>();
            device.SetupGet(x => x.Id).Returns(1);

            _egmMock.Setup(x => x.GetDevice<ICommConfigDevice>(1)).Returns(device.Object);

            var handler = new GetCommConfigModeStatus(_egmMock.Object, _commandBuilderMock.Object);

            var command = ClassCommandUtilities.CreateClassCommand<commConfig, getCommConfigModeStatus>(
                TestConstants.HostId,
                TestConstants.EgmId);

            await handler.Handle(command);

            _commandBuilderMock.Verify(
                x => x.Build(It.Is<ICommConfigDevice>(d => d.Id == 1), It.IsAny<commConfigModeStatus>()),
                Times.Once);
        }
    }
}