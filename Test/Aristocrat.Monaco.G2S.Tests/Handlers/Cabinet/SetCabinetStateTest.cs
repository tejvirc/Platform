namespace Aristocrat.Monaco.G2S.Tests.Handlers.Cabinet
{
    using Accounting.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers;
    using G2S.Handlers.Cabinet;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Threading.Tasks;

    [TestClass]
    public class SetCabinetStateTest
    {
        private Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>> _commandBuilder;
        private Mock<IG2SEgm> _egm;
        private Mock<IEventLift> _eventLift;
        private Mock<IPropertiesManager> _properties;
        private Mock<ISystemDisableManager> _systemDisableManager;

        [TestInitialize]
        public void Initialize()
        {
            _egm = new Mock<IG2SEgm>();
            _commandBuilder = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();
            _systemDisableManager = new Mock<ISystemDisableManager>();
            _eventLift = new Mock<IEventLift>();
            _properties = new Mock<IPropertiesManager>();

            _properties.Setup(m => m.GetProperty(AccountingConstants.MoneyInEnabled, It.IsAny<bool>())).Returns(true);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new SetCabinetState(null, null, null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullCommandBuilderExpectException()
        {
            var handler = new SetCabinetState(_egm.Object, null, null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmStateManagerExpectException()
        {
            var handler = new SetCabinetState(_egm.Object, _commandBuilder.Object, null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullGamePlayStateExpectException()
        {
            var handler = new SetCabinetState(_egm.Object, _commandBuilder.Object, null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullPropertyManagerExpectException()
        {
            var handler = new SetCabinetState(
                _egm.Object,
                _commandBuilder.Object,
                _systemDisableManager.Object,
                null,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEventLiftExpectException()
        {
            var handler = new SetCabinetState(
                _egm.Object,
                _commandBuilder.Object,
                _systemDisableManager.Object,
                _properties.Object,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyOwnerExpectSuccess()
        {
            var egm = HandlerUtilities.CreateMockEgm<ICabinetDevice>();
            var handler = CreateHandler(egm);

            await VerificationTests.VerifyChecksForOwner(handler);
        }

        [TestMethod]
        public async Task WhenHandleCommandWithEnableCabinetStateExpectSuccess()
        {
            var deviceMock = new Mock<ICabinetDevice>();
            deviceMock.SetupGet(m => m.DeviceClass).Returns(DeviceClass.G2S_cabinet);
            var egm = HandlerUtilities.CreateMockEgm(deviceMock);
            var handler = CreateHandler(egm);

            var command = CreateCommand();
            command.Command.enable = true;

            await handler.Handle(command);

            deviceMock.VerifySet(d => d.DisableText = command.Command.disableText);
            deviceMock.VerifySet(d => d.HostEnabled = true);
        }

        [TestMethod]
        public async Task WhenHandleCommandWithDisableCabinetStateExpectSuccess()
        {
            var deviceMock = new Mock<ICabinetDevice>();
            deviceMock.SetupGet(m => m.DeviceClass).Returns(DeviceClass.G2S_cabinet);
            deviceMock.SetupGet(m => m.HostEnabled).Returns(true);
            deviceMock.SetupGet(m => m.RequiredForPlay).Returns(true);

            var egm = HandlerUtilities.CreateMockEgm(deviceMock);
            var handler = CreateHandler(egm);

            var command = CreateCommand();
            command.Command.enable = false;

            command.Command.disableText = "disable_text";

            await handler.Handle(command);

            deviceMock.VerifySet(d => d.DisableText = command.Command.disableText);
            deviceMock.VerifySet(d => d.HostEnabled = false);
        }

        [TestMethod]
        public async Task WhenHandleCommandWithRepeatCabinetStateRequestExpectNoActions()
        {
            var deviceMock = new Mock<ICabinetDevice>();
            deviceMock.SetupGet(m => m.DeviceClass).Returns(DeviceClass.G2S_cabinet);
            deviceMock.SetupGet(m => m.HostEnabled).Returns(true);

            var egm = HandlerUtilities.CreateMockEgm(deviceMock);
            var handler = CreateHandler(egm);

            var command = CreateCommand();
            command.Command.enable = true;

            await handler.Handle(command);

            deviceMock.VerifySet(d => d.HostEnabled = false, Times.Never);
            deviceMock.VerifySet(d => d.HostEnabled = true, Times.Never);
        }

        [TestMethod]
        public async Task WhenHandleCommandWithEnableGamePlayStateExpectSuccess()
        {
            var deviceMock = new Mock<ICabinetDevice>();
            deviceMock.SetupGet(m => m.DeviceClass).Returns(DeviceClass.G2S_cabinet);

            var egm = HandlerUtilities.CreateMockEgm(deviceMock);
            var handler = CreateHandler(egm);

            var command = CreateCommand();
            command.Command.enableGamePlay = true;

            await handler.Handle(command);

            deviceMock.VerifySet(d => d.GamePlayEnabled = true);
            _eventLift
                .Verify(m => m.Report(deviceMock.Object, EventCode.G2S_CBE102, It.IsAny<deviceList1>()), Times.Once);
            _systemDisableManager.Verify(m => m.Enable(It.IsAny<Guid>()));
        }

        [TestMethod]
        public async Task WhenHandleCommandWithDisableGamePlayStateExpectSuccess()
        {
            var deviceMock = new Mock<ICabinetDevice>();
            deviceMock.SetupGet(m => m.DeviceClass).Returns(DeviceClass.G2S_cabinet);
            deviceMock.SetupGet(m => m.GamePlayEnabled).Returns(true);

            var egm = HandlerUtilities.CreateMockEgm(deviceMock);
            var handler = CreateHandler(egm);

            var command = CreateCommand();
            command.Command.enableGamePlay = false;
            command.Command.disableText = "disable_text";

            await handler.Handle(command);

            deviceMock.VerifySet(d => d.GamePlayEnabled = false);
            _eventLift
                .Verify(m => m.Report(deviceMock.Object, EventCode.G2S_CBE101, It.IsAny<deviceList1>()), Times.Once);
            _systemDisableManager
                .Verify(m => m.Disable(It.IsAny<Guid>(), SystemDisablePriority.Normal, It.Is<Func<string>>(x => x.Invoke() == command.Command.disableText), null));
        }

        [TestMethod]
        public async Task WhenHandleCommandWithEnableMoneyInExpectSuccess()
        {
            var deviceMock = new Mock<ICabinetDevice>();
            deviceMock.SetupGet(m => m.DeviceClass).Returns(DeviceClass.G2S_cabinet);

            _properties.Setup(m => m.GetProperty(AccountingConstants.MoneyInEnabled, It.IsAny<bool>())).Returns(false);

            var egm = HandlerUtilities.CreateMockEgm(deviceMock);
            var handler = CreateHandler(egm);

            var command = CreateCommand();
            command.Command.enableMoneyIn = true;

            await handler.Handle(command);

            _properties.Verify(m => m.SetProperty(AccountingConstants.MoneyInEnabled, true));
            _eventLift
                .Verify(m => m.Report(deviceMock.Object, EventCode.G2S_CBE104, It.IsAny<deviceList1>()), Times.Once);
        }

        [TestMethod]
        public async Task WhenHandleCommandWithDisableMoneyInExpectSuccess()
        {
            var deviceMock = new Mock<ICabinetDevice>();
            deviceMock.SetupGet(m => m.DeviceClass).Returns(DeviceClass.G2S_cabinet);

            _properties.Setup(m => m.GetProperty(AccountingConstants.MoneyInEnabled, It.IsAny<bool>())).Returns(true);

            var egm = HandlerUtilities.CreateMockEgm(deviceMock);
            var handler = CreateHandler(egm);

            var command = CreateCommand();
            command.Command.enableMoneyIn = false;

            await handler.Handle(command);

            _properties.Verify(m => m.SetProperty(AccountingConstants.MoneyInEnabled, false));
            _eventLift
                .Verify(m => m.Report(deviceMock.Object, EventCode.G2S_CBE103, It.IsAny<deviceList1>()), Times.Once);
        }

        [TestMethod]
        public async Task WhenHandleCommandWithRepeatStateMoneyInExpectNoActions()
        {
            var deviceMock = new Mock<ICabinetDevice>();
            deviceMock.SetupGet(m => m.DeviceClass).Returns(DeviceClass.G2S_cabinet);
            _properties.Setup(m => m.GetProperty(AccountingConstants.MoneyInEnabled, It.IsAny<bool>())).Returns(true);

            var egm = HandlerUtilities.CreateMockEgm(deviceMock);
            var handler = CreateHandler(egm);

            var command = CreateCommand();
            command.Command.enableMoneyIn = true;

            await handler.Handle(command);

            _properties.Verify(m => m.SetProperty(AccountingConstants.MoneyInEnabled, It.IsAny<bool>()), Times.Never);
        }

        [TestMethod]
        public async Task WhenHandleCommandWithEnableMoneyOutExpectSuccess()
        {
            var deviceMock = new Mock<ICabinetDevice>();
            deviceMock.SetupGet(m => m.DeviceClass).Returns(DeviceClass.G2S_cabinet);
            deviceMock.SetupGet(m => m.MoneyOutEnabled).Returns(false);

            var egm = HandlerUtilities.CreateMockEgm(deviceMock);
            var handler = CreateHandler(egm);

            var command = CreateCommand();
            command.Command.enable = true;
            command.Command.enableMoneyOut = true;

            await handler.Handle(command);

            deviceMock.VerifySet(m => m.MoneyOutEnabled = true);
            _eventLift
                .Verify(m => m.Report(deviceMock.Object, EventCode.G2S_CBE106, It.IsAny<deviceList1>()), Times.Once);
        }

        [TestMethod]
        public async Task WhenHandleCommandWithDisableMoneyOutExpectSuccess()
        {
            var deviceMock = new Mock<ICabinetDevice>();
            deviceMock.SetupGet(m => m.DeviceClass).Returns(DeviceClass.G2S_cabinet);
            deviceMock.SetupGet(m => m.MoneyOutEnabled).Returns(true);

            var egm = HandlerUtilities.CreateMockEgm(deviceMock);
            var handler = CreateHandler(egm);

            var command = CreateCommand();
            command.Command.enableMoneyOut = false;
            command.Command.enable = false;

            await handler.Handle(command);

            deviceMock.VerifySet(m => m.MoneyOutEnabled = false);
            _eventLift
                .Verify(m => m.Report(deviceMock.Object, EventCode.G2S_CBE105, It.IsAny<deviceList1>()), Times.Once);
        }

        [TestMethod]
        public async Task WhenHandleCommandWithRepeatStateMoneyOutExpectNoActions()
        {
            var deviceMock = new Mock<ICabinetDevice>();
            deviceMock.SetupGet(m => m.DeviceClass).Returns(DeviceClass.G2S_cabinet);
            deviceMock.SetupGet(m => m.MoneyOutEnabled).Returns(true);

            var egm = HandlerUtilities.CreateMockEgm(deviceMock);
            var handler = CreateHandler(egm);

            var command = CreateCommand();
            command.Command.enableMoneyOut = true;

            await handler.Handle(command);

            deviceMock.VerifySet(m => m.MoneyOutEnabled = true, Times.Never);
        }

        private SetCabinetState CreateHandler(IG2SEgm egm = null)
        {
            var handler = new SetCabinetState(
                egm ?? _egm.Object,
                _commandBuilder.Object,
                _systemDisableManager.Object,
                _properties.Object,
                _eventLift.Object);

            return handler;
        }

        private ClassCommand<cabinet, setCabinetState> CreateCommand()
        {
            var command = ClassCommandUtilities.CreateClassCommand<cabinet, setCabinetState>(
                TestConstants.HostId,
                TestConstants.EgmId);

            return command;
        }
    }
}