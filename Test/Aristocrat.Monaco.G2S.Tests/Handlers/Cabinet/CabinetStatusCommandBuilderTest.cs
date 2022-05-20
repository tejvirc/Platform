namespace Aristocrat.Monaco.G2S.Tests.Handlers.Cabinet
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using G2S.Handlers.Cabinet;
    using Gaming.Contracts;
    using Hardware.Contracts.Display;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.HardMeter;
    using Hardware.Contracts.TowerLight;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;

    [TestClass]
    public class CabinetStatusCommandBuilderTest
    {
        private Mock<ICabinetDevice> _cabinetDeviceMock;
        private Mock<ICabinetService> _cabinetServiceMock;
        private Mock<IDoorService> _doorMock;
        private Mock<IGameHistory> _gameHistory;
        private Mock<IGameProvider> _gameProvider;
        private Mock<IHardMeter> _hardMeterMock;
        private Mock<ILocalization> _localeProvider;
        private Mock<IPropertiesManager> _properties;
        private Mock<IDisplayService> _displayService;
        private Mock<ITowerLight> _towerLight;

        [TestInitialize]
        public void Initialize()
        {
            _cabinetServiceMock = new Mock<ICabinetService>();
            _doorMock = new Mock<IDoorService>();
            _hardMeterMock = new Mock<IHardMeter>();
            _cabinetDeviceMock = new Mock<ICabinetDevice>();
            _localeProvider = new Mock<ILocalization>();
            _gameProvider = new Mock<IGameProvider>();
            _gameHistory = new Mock<IGameHistory>();
            _properties = new Mock<IPropertiesManager>();
            _displayService = new Mock<IDisplayService>();
            _towerLight = new Mock<ITowerLight>();
        }

        [TestMethod]
        public void WhenConstructWithNullsExpectException()
        {
            ConstructorTest.TestConstructorNullChecks<CabinetStatusCommandBuilder>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task WhenBuildWithNullDeviceExpectException()
        {
            var builder = CreateBuilder();
            await builder.Build(null, new cabinetStatus());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task WhenBuildWithNullCabinetStatusExpectException()
        {
            var builder = CreateBuilder();
            await builder.Build(new Mock<ICabinetDevice>().Object, null);
        }

        [TestMethod]
        public async Task WhenBuildWithValidParamsExpectSuccess()
        {
            ConfigureMocks();

            var builder = CreateBuilder();

            var command = new cabinetStatus();

            await builder.Build(_cabinetDeviceMock.Object, command);

            var device = _cabinetDeviceMock.Object;
            var cabinetSerivce = _cabinetServiceMock.Object;
            var hardMeter = _hardMeterMock.Object;
            var door = _doorMock.Object;

            Assert.AreEqual(command.configurationId, device.ConfigurationId);
            Assert.AreEqual(command.egmEnabled, device.Enabled);
            Assert.AreEqual(command.hostEnabled, device.HostEnabled);
            Assert.AreEqual(command.hostLocked, device.HostLocked);
            Assert.AreEqual(command.egmState, t_egmStates.G2S_enabled);
            Assert.AreEqual(command.deviceClass, DeviceClass.G2S_commConfig);
            Assert.AreEqual(command.logicDoorOpen, !door.GetDoorClosed((int)DoorLogicalId.Logic));
            Assert.AreEqual(command.logicDoorDateTime, door.GetDoorLastOpened((int)DoorLogicalId.Logic));
            Assert.AreEqual(command.logicDoorDateTimeSpecified, true);
            Assert.AreEqual(command.auxDoorOpen, !door.GetDoorClosed((int)DoorLogicalId.TopBox));
            Assert.AreEqual(command.auxDoorDateTime, door.GetDoorLastOpened((int)DoorLogicalId.TopBox));
            Assert.AreEqual(command.auxDoorDateTimeSpecified, true);
            Assert.AreEqual(command.cabinetDoorOpen, !door.GetDoorClosed((int)DoorLogicalId.Main));
            Assert.AreEqual(command.cabinetDoorDateTime, door.GetDoorLastOpened((int)DoorLogicalId.Main));
            Assert.AreEqual(command.logicDoorDateTimeSpecified, true);
            Assert.AreEqual(command.hardMetersDisconnected, hardMeter.LogicalState == HardMeterLogicalState.Disabled);
            Assert.AreEqual(command.enableMoneyOut, true);
            Assert.AreEqual(command.configDateTime, device.ConfigDateTime);
            Assert.AreEqual(command.configComplete, device.ConfigComplete);
            Assert.AreEqual(command.egmIdle, cabinetSerivce.Idle);
            Assert.AreEqual(command.videoDisplayFault, _displayService.Object.IsFaulted);
        }


        [TestMethod]
        public async Task WhenBuildWithValidParamsAndReelsExpectSuccess()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            var serviceManager = MoqServiceManager.CreateInstance(MockBehavior.Default);
            var reelController = new Mock<IReelController>();
            reelController.Setup(m => m.LogicalState).Returns(ReelControllerState.Tilted);
            reelController.Setup(m => m.Faults).Returns(new Dictionary<int, ReelFaults>() { { 0, ReelFaults.LowVoltage }, { 1, ReelFaults.ReelStall } });
            serviceManager.Setup(m => m.TryGetService<IReelController>()).Returns(reelController.Object);

            ConfigureMocks();

            var builder = CreateBuilder();

            var command = new cabinetStatus();

            await builder.Build(_cabinetDeviceMock.Object, command);

            var device = _cabinetDeviceMock.Object;
            var cabinetSerivce = _cabinetServiceMock.Object;
            var hardMeter = _hardMeterMock.Object;
            var door = _doorMock.Object;

            Assert.AreEqual(command.reelTilt, true);
            Assert.AreEqual(command.reelsTilted, "0,1");
        }

        private CabinetStatusCommandBuilder CreateBuilder()
        {
            var builder = new CabinetStatusCommandBuilder(
                _cabinetServiceMock.Object,
                _doorMock.Object,
                _hardMeterMock.Object,
                _localeProvider.Object,
                _gameProvider.Object,
                _gameHistory.Object,
                _properties.Object,
                _displayService.Object,
                _towerLight.Object);

            return builder;
        }

        private void ConfigureMocks()
        {
            _cabinetDeviceMock.SetupGet(m => m.ConfigurationId).Returns(1);
            _cabinetDeviceMock.SetupGet(m => m.Enabled).Returns(true);
            _cabinetDeviceMock.SetupGet(m => m.HostEnabled).Returns(true);
            _cabinetDeviceMock.SetupGet(m => m.HostLocked).Returns(true);
            _cabinetDeviceMock.SetupGet(m => m.State).Returns(EgmState.Enabled);

            var commConfigDeviceMock = new Mock<ICommConfigDevice>();
            commConfigDeviceMock.SetupGet(m => m.Id).Returns(1);
            commConfigDeviceMock.SetupGet(m => m.DeviceClass).Returns(DeviceClass.G2S_commConfig.TrimmedDeviceClass());
            _cabinetDeviceMock.SetupGet(m => m.Device).Returns(commConfigDeviceMock.Object);

            _cabinetDeviceMock.SetupGet(m => m.ConfigDateTime).Returns(DateTime.MaxValue);
            _cabinetDeviceMock.SetupGet(m => m.ConfigComplete).Returns(true);

            _doorMock.Setup(m => m.GetDoorClosed((int)DoorLogicalId.Logic)).Returns(false);
            _doorMock.Setup(m => m.GetDoorLastOpened((int)DoorLogicalId.Logic)).Returns(DateTime.MaxValue);
            _doorMock.Setup(m => m.GetDoorClosed((int)DoorLogicalId.TopBox)).Returns(false);
            _doorMock.Setup(m => m.GetDoorLastOpened((int)DoorLogicalId.TopBox)).Returns(DateTime.MaxValue);
            _doorMock.Setup(m => m.GetDoorClosed((int)DoorLogicalId.Main)).Returns(false);
            _doorMock.Setup(m => m.GetDoorLastOpened((int)DoorLogicalId.Main)).Returns(DateTime.MaxValue);

            _hardMeterMock.SetupGet(m => m.LogicalState).Returns(HardMeterLogicalState.Disabled);

            _cabinetServiceMock.Setup(m => m.Idle).Returns(true);

            _localeProvider.SetupGet(m => m.CurrentCulture).Returns(CultureInfo.CurrentCulture);

            _displayService.SetupGet(m => m.IsFaulted).Returns(true);

            _properties.Setup(m => m.GetProperty(AccountingConstants.MoneyInEnabled, It.IsAny<bool>())).Returns(true);
            _properties.Setup(p => p.GetProperty(ApplicationConstants.TimeZoneOffsetKey, It.IsAny<object>())).Returns(TimeSpan.Zero);

            _gameProvider.Setup(m => m.GetGames()).Returns(Enumerable.Empty<IGameDetail>().ToList);
        }
    }
}