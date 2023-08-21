namespace Aristocrat.Monaco.G2S.Tests.Handlers.CommConfig
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.CommConfig;
    using G2S.Handlers.CommConfig;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class GetCommHostListTests
    {
        private Mock<ICommHostListCommandBuilder> _commandBuilderMock;

        private Mock<ICommConfigDevice> _commConfigDeviceMock;
        private Mock<IG2SEgm> _egmMock;

        [TestInitialize]
        public void Initialize()
        {
            _egmMock = new Mock<IG2SEgm>();
            _commandBuilderMock = new Mock<ICommHostListCommandBuilder>();
            _commConfigDeviceMock = new Mock<ICommConfigDevice>();
        }

        [TestMethod]
        public void WhenConstructWithNullsExpectException()
        {
            ConstructorTest.TestConstructorNullChecks<GetCommHostList>();
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var handler = new GetCommHostList(
                _egmMock.Object,
                _commandBuilderMock.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var handler = new GetCommHostList(
                _egmMock.Object,
                _commandBuilderMock.Object);
            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var handler = CreateHandlerWithDevice();
            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            var invalidHostId = 99;

            _commConfigDeviceMock.SetupGet(d => d.Owner).Returns(invalidHostId);

            var handler = CreateHandlerWithDevice();
            await VerificationTests.VerifyChecksForOwnerAndGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithGuestAccessExpectNoError()
        {
            const int otherHostId = 99;

            _commConfigDeviceMock.SetupGet(d => d.Owner).Returns(otherHostId);
            _commConfigDeviceMock.SetupGet(d => d.Guests).Returns(new List<int> { TestConstants.HostId });

            var handler = CreateHandlerWithDevice();

            await VerificationTests.VerifyAllowsGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            _commConfigDeviceMock.SetupGet(d => d.Owner).Returns(TestConstants.HostId);
            _commConfigDeviceMock.SetupGet(d => d.Id).Returns(TestConstants.HostId);

            var handler = CreateHandlerWithDevice();

            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenHandlerCommandExpectCommandBuild()
        {
            var command = ClassCommandUtilities.CreateClassCommand<commConfig, getCommHostList>(
                TestConstants.HostId,
                TestConstants.EgmId);

            command.Command.hostIndex = -1;
            command.Command.includeConfigDevices = true;
            command.Command.includeGuestDevices = true;
            command.Command.includeOwnerDevices = true;

            var handler = CreateHandlerWithDevice();

            await handler.Handle(command);

            _commandBuilderMock
                .Verify(
                    m => m.Build(
                        _commConfigDeviceMock.Object,
                        It.IsAny<commHostList>(),
                        It.Is<CommHostListCommandBuilderParameters>(
                            obj => obj.IncludeConfigDevices == command.Command.includeConfigDevices &&
                                   obj.IncludeGuestDevices == command.Command.includeGuestDevices &&
                                   obj.IncludeOwnerDevices == command.Command.includeOwnerDevices &&
                                   obj.HostIndexes.SequenceEqual(new[] { command.Command.hostIndex }))));
        }

        private GetCommHostList CreateHandlerWithDevice()
        {
            var handler = new GetCommHostList(
                HandlerUtilities.CreateMockEgm(_commConfigDeviceMock),
                _commandBuilderMock.Object);

            return handler;
        }
    }
}