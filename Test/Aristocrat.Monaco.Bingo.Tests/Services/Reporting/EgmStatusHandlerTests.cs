namespace Aristocrat.Monaco.Bingo.Tests.Services.Reporting
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Aristocrat.Monaco.Test.Common;
    using Common;
    using Gaming.Contracts;
    using Hardware.Contracts;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.Reel;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Vgt.Client12.Application.OperatorMenu;

    [TestClass]
    public class EgmStatusHandlerTests
    {
        private const string SerialNumber = "ABC123";

        private readonly Mock<IPropertiesManager> _propertiesManager = new();
        private readonly Mock<IDoorMonitor> _doorMonitor = new();
        private readonly Mock<IGamePlayState> _playState = new();
        private readonly Mock<IOperatorMenuLauncher> _menuLauncher = new();
        private readonly Mock<ISystemDisableManager> _systemDisable = new();
        private Mock<INoteAcceptor> _noteAcceptor;
        private Mock<IPrinter> _printer;
        private Mock<IReelController> _reelController;
        private EgmStatusHandler _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _noteAcceptor = MoqServiceManager.CreateAndAddService<INoteAcceptor>(MockBehavior.Default);
            _printer = MoqServiceManager.CreateAndAddService<IPrinter>(MockBehavior.Default);
            _reelController = MoqServiceManager.CreateAndAddService<IReelController>(MockBehavior.Default);

            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.SerialNumber, It.IsAny<string>())).Returns(SerialNumber);
            _propertiesManager.Setup(x => x.GetProperty(HardwareConstants.Battery1Low, It.IsAny<bool>()))
                .Returns(true);
            _propertiesManager.Setup(x => x.GetProperty(HardwareConstants.Battery2Low, It.IsAny<bool>()))
                .Returns(true);
            _printer.Setup(mock => mock.PaperState).Returns(PaperStates.Full);
            _printer.Setup(mock => mock.Faults).Returns(PrinterFaultTypes.None);
            _printer.Setup(mock => mock.LogicalState).Returns(PrinterLogicalState.Idle);
            _doorMonitor.Setup(mock => mock.GetLogicalDoors()).Returns(new Dictionary<int, bool> { { 0, false } });
            _menuLauncher.Setup(mock => mock.IsShowing).Returns(false);
            _noteAcceptor.Setup(mock => mock.Faults).Returns(NoteAcceptorFaultTypes.None);
            _reelController.Setup(mock => mock.Faults).Returns(new Dictionary<int, ReelFaults> { { 0, ReelFaults.None } } );
            _systemDisable.Setup(mock => mock.CurrentDisableKeys).Returns(new List<Guid> { new Guid("{F1BE3145-DF51-4C43-BAB6-F0E934681C74}") });
            _playState.Setup(mock => mock.Enabled).Returns(true);

            _target = CreateTarget();
        }

        [TestCleanup]
        public void CleanUp()
        {
            MoqServiceManager.RemoveInstance();
        }

        [DataRow(true, false, false, false, false, DisplayName = "Null Properties")]
        [DataRow(false, true, false, false, false, DisplayName = "Null Door Monitor")]
        [DataRow(false, false, true, false, false, DisplayName = "Null Play State")]
        [DataRow(false, false, false, true, false, DisplayName = "Null Menu Launcher")]
        [DataRow(false, false, false, false, true, DisplayName = "Null System Disable")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorArgumentsTest(
            bool nullProperties,
            bool nullDoor,
            bool nullPlayState,
            bool nullMenu,
            bool nullSystem)
        {
            _ = CreateTarget(nullProperties, nullDoor, nullPlayState, nullMenu, nullSystem);
        }

        [DataTestMethod]
        [DataRow(true, false, DisplayName = "Battery 1 Low")]
        [DataRow(false, true, DisplayName = "Battery 2 Low")]
        public void BatteryLowTest(bool battery1Low, bool battery2Low)
        {
            _propertiesManager.Setup(x => x.GetProperty(HardwareConstants.Battery1Low, It.IsAny<bool>()))
                .Returns(!battery1Low);
            _propertiesManager.Setup(x => x.GetProperty(HardwareConstants.Battery2Low, It.IsAny<bool>()))
                .Returns(!battery2Low);

            var status = _target.GetCurrentEgmStatus();
            Assert.AreEqual(EgmStatusFlag.NvramBatteryLow, status & EgmStatusFlag.NvramBatteryLow);
        }

        [TestMethod]
        public void EgmSerialNumberValidTest()
        {
            var status = _target.GetCurrentEgmStatus();
            Assert.AreEqual(EgmStatusFlag.None, status);
        }

        [TestMethod]
        public void EgmInvalidSerialNumberTest()
        {
            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.SerialNumber, It.IsAny<string>())).Returns("");

            var status = _target.GetCurrentEgmStatus();
            Assert.AreEqual(EgmStatusFlag.NotEnrolled, status);
        }

        [TestMethod]
        public void EgmDoorOpenTest()
        {
            _doorMonitor.Setup(mock => mock.GetLogicalDoors()).Returns(new Dictionary<int, bool>() { { 0, true } });

            var status = _target.GetCurrentEgmStatus();
            Assert.AreEqual(EgmStatusFlag.DoorOpen, status & EgmStatusFlag.DoorOpen);
        }

        [TestMethod]
        public void OperatorMenuOpenTest()
        {
            _menuLauncher.Setup(mock => mock.IsShowing).Returns(true);

            var status = _target.GetCurrentEgmStatus();
            Assert.AreEqual(EgmStatusFlag.InOperatorMenu, status & EgmStatusFlag.InOperatorMenu);
        }

        [TestMethod]
        public void PrinterOutOfPaperTest()
        {
            _printer.Setup(mock => mock.PaperState).Returns(PaperStates.Empty);

            var status = _target.GetCurrentEgmStatus();
            Assert.AreEqual(EgmStatusFlag.PrnNoPaper, status & EgmStatusFlag.PrnNoPaper);
        }

        [TestMethod]
        public void PrinterFaultTest()
        {
            _printer.Setup(mock => mock.Faults).Returns(PrinterFaultTypes.PaperJam);

            var status = _target.GetCurrentEgmStatus();
            Assert.AreEqual(EgmStatusFlag.PrinterError, status & EgmStatusFlag.PrinterError);
        }

        [TestMethod]
        public void PrinterIsPrintingTest()
        {
            _printer.Setup(mock => mock.LogicalState).Returns(PrinterLogicalState.Printing);

            var status = _target.GetCurrentEgmStatus();
            Assert.AreEqual(EgmStatusFlag.Printing, status & EgmStatusFlag.Printing);
        }

        [TestMethod]
        public void SystemDisabledTest()
        {
            _systemDisable.Setup(mock => mock.CurrentDisableKeys).Returns(new List<Guid>() { ApplicationConstants.SystemDisableGuid });

            var status = _target.GetCurrentEgmStatus();
            Assert.AreEqual(EgmStatusFlag.MachineDisabled, status & EgmStatusFlag.MachineDisabled);
        }

        [TestMethod]
        public void OperatingHoursDisabledTest()
        {
            _systemDisable.Setup(mock => mock.CurrentDisableKeys).Returns(new List<Guid>() { ApplicationConstants.OperatingHoursDisableGuid });

            var status = _target.GetCurrentEgmStatus();
            Assert.AreEqual(EgmStatusFlag.Operator, status & EgmStatusFlag.Operator);
        }

        [TestMethod]
        public void SystemDisabledByHost1KeyTest()
        {
            _systemDisable.Setup(mock => mock.CurrentDisableKeys).Returns(new List<Guid>() { ApplicationConstants.DisabledByHost1Key });

            var status = _target.GetCurrentEgmStatus();
            Assert.AreEqual(EgmStatusFlag.DisabledByCmsBackend, status & EgmStatusFlag.DisabledByCmsBackend);
        }

        [TestMethod]
        public void SystemDisabledByHost0KeyTest()
        {
            _systemDisable.Setup(mock => mock.CurrentDisableKeys).Returns(new List<Guid>() { ApplicationConstants.DisabledByHost0Key });

            var status = _target.GetCurrentEgmStatus();
            Assert.AreEqual(EgmStatusFlag.DisabledByCmsBackend, status & EgmStatusFlag.DisabledByCmsBackend);
        }

        [TestMethod]
        public void NoteAcceptorFaultTest()
        {
            _noteAcceptor.Setup(mock => mock.Faults).Returns(NoteAcceptorFaultTypes.MechanicalFault);

            var status = _target.GetCurrentEgmStatus();
            Assert.AreEqual(EgmStatusFlag.DbaError, status & EgmStatusFlag.DbaError);
        }

        [TestMethod]
        public void ReelControllerFaultTest()
        {
            _reelController.Setup(mock => mock.Faults).Returns(new Dictionary<int, ReelFaults>() { { 0, ReelFaults.ReelStall } });
            _reelController.Setup(mock => mock.ConnectedReels).Returns(new List<int>() { { 0 } });

            var status = _target.GetCurrentEgmStatus();
            Assert.AreEqual(EgmStatusFlag.ReelMalfunction, status & EgmStatusFlag.ReelMalfunction);
        }

        [TestMethod]
        public void PlayStateNotEnabledTest()
        {
            _playState.Setup(mock => mock.Enabled).Returns(false);

            var status = _target.GetCurrentEgmStatus();
            Assert.AreEqual(EgmStatusFlag.GameNotOnline, status & EgmStatusFlag.GameNotOnline);
        }

        private EgmStatusHandler CreateTarget(
            bool nullProperties = false,
            bool nullDoor = false,
            bool nullPlayState = false,
            bool nullMenu = false,
            bool nullSystem = false)
        {
            return new EgmStatusHandler(
                nullProperties ? null : _propertiesManager.Object,
                nullDoor ? null : _doorMonitor.Object,
                nullPlayState ? null : _playState.Object,
                nullMenu ? null : _menuLauncher.Object,
                nullSystem ? null : _systemDisable.Object);
        }
    }
}