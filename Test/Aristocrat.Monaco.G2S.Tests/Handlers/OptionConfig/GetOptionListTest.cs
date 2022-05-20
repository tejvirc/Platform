namespace Aristocrat.Monaco.G2S.Tests.Handlers.OptionConfig
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.OptionConfig;
    using G2S.Handlers;
    using G2S.Handlers.OptionConfig;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using DeviceClass = Data.Model.DeviceClass;

    [TestClass]
    public class GetOptionListTests
    {
        private Mock<IOptionListCommandBuilder> _commandBuilder;
        private Mock<IG2SEgm> _egmMock;

        [TestInitialize]
        public void Initialize()
        {
            _egmMock = new Mock<IG2SEgm>();
            _commandBuilder = new Mock<IOptionListCommandBuilder>();
        }

        [TestMethod]
        public void WhenConstructWithNullsExpectException()
        {
            ConstructorTest.TestConstructorNullChecks<GetOptionList>();
        }

        [TestMethod]
        public async Task WhenVerifyWithNoActiveDevicesExpectError()
        {
            var device = CreateDevice<INoteAcceptorDevice>(DeviceClass.NoteAcceptor, 1, false);

            _egmMock.SetupGet(m => m.Devices).Returns(new[] { device });

            var handler = CreateHandler();
            var command = CreateCommand();

            command.Command.deviceClass = DeviceClass.All.DeviceClassToG2SString();

            var error = await handler.Verify(command);

            Assert.AreEqual(ErrorCode.G2S_OCX001, error.Code);
        }

        [TestMethod]
        public async Task WhenSpecifiedDeviceNotExistsExpectError()
        {
            var device = CreateDevice<INoteAcceptorDevice>(DeviceClass.NoteAcceptor, 1, true);

            _egmMock.SetupGet(m => m.Devices).Returns(new[] { device });

            var handler = CreateHandler();
            var command = CreateCommand();

            command.Command.deviceClass = DeviceClass.CommConfig.DeviceClassToG2SString();
            command.Command.deviceId = 2;

            var error = await handler.Verify(command);

            Assert.AreEqual(ErrorCode.G2S_OCX001, error.Code);
        }

        [TestMethod]
        public async Task WhenVerifyWithSpecifiedDeviceNotActiveExpectError()
        {
            var firstDevice = CreateDevice<INoteAcceptorDevice>(DeviceClass.NoteAcceptor, 1, true);

            var secondDevice = CreateDevice<INoteAcceptorDevice>(DeviceClass.NoteAcceptor, 2, false);

            _egmMock.SetupGet(x => x.Devices).Returns(new List<IDevice> { firstDevice, secondDevice });

            var handler = CreateHandler(_egmMock.Object);
            var command = CreateCommand();

            command.Command.deviceClass = DeviceClass.NoteAcceptor.DeviceClassToG2SString();
            command.Command.deviceId = 2;

            var error = await handler.Verify(command);

            Assert.AreEqual(ErrorCode.G2S_OCX001, error.Code);
        }

        [TestMethod]
        public async Task WhenVerifyWithOwnerAndGuestsExpectError()
        {
            var device = CreateDevice<INoteAcceptorDevice>(DeviceClass.NoteAcceptor, 2, true);

            _egmMock.SetupGet(x => x.Devices).Returns(new List<IDevice> { device });

            var handler = CreateHandler();
            var command = CreateCommand();

            command.Command.deviceClass = DeviceClass.NoteAcceptor.DeviceClassToG2SString();
            command.Command.deviceId = 2;

            var error = await handler.Verify(command);

            Assert.IsFalse(string.IsNullOrEmpty(error.Code));
        }

        [TestMethod]
        public async Task WhenHandleWithValidCommandExpectCommandBuild()
        {
            var device = CreateDevice<IOptionConfigDevice>(DeviceClass.OptionConfig, 1, true);
            _egmMock.Setup(m => m.GetDevice<IOptionConfigDevice>(It.IsAny<int>())).Returns(device);

            var command = CreateCommand();

            command.Command.deviceClass = DeviceClass.All.DeviceClassToG2SString();

            var handler = CreateHandler();

            await handler.Handle(command);

            _commandBuilder
                .Verify(m => m.Build(device, It.IsAny<optionList>(), It.IsAny<OptionListCommandBuilderParameters>()));
        }

        private GetOptionList CreateHandler(IG2SEgm egm = null)
        {
            var handler = new GetOptionList(
                egm ?? _egmMock.Object,
                _commandBuilder.Object);

            return handler;
        }

        private ClassCommand<optionConfig, getOptionList> CreateCommand()
        {
            var command = ClassCommandUtilities.CreateClassCommand<optionConfig, getOptionList>(
                TestConstants.HostId,
                TestConstants.EgmId);

            command.Command.optionGroupId = Aristocrat.G2S.DeviceClass.G2S_all;
            command.Command.optionId = Aristocrat.G2S.DeviceClass.G2S_all;

            return command;
        }

        private T CreateDevice<T>(DeviceClass deviceClass, int deviceId, bool active)
            where T : class, IDevice
        {
            var deviceMock = new Mock<T>();

            deviceMock.SetupGet(x => x.Active).Returns(active);
            deviceMock.SetupGet(x => x.Id).Returns(deviceId);
            deviceMock.SetupGet(x => x.DeviceClass)
                .Returns(deviceClass.DeviceClassToG2SString());

            return deviceMock.Object;
        }
    }
}