namespace Aristocrat.Monaco.Application.UI.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Media;
    using Accounting.Contracts;
    using Common;
    using Contracts;
    using Contracts.OperatorMenu;
    using Contracts.Tickets;
    using Events;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.Persistence;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.SerialPorts;
    using Hardware.Contracts.SharedDevice;
    using Hardware.Contracts.Ticket;
    using Helpers;
    using Kernel;
    using Kernel.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;
    using UI.ViewModels;
    using DisabledEvent = Hardware.Contracts.Printer.DisabledEvent;
    using EnabledEvent = Hardware.Contracts.Printer.EnabledEvent;
    using InspectedEvent = Hardware.Contracts.Printer.InspectedEvent;
    using InspectionFailedEvent = Hardware.Contracts.Printer.InspectionFailedEvent;
    using StatusMode = Common.StatusMode;

    [Ignore]
    [TestClass]
    public class PrinterPageTest
    {
        private dynamic _accessor;
        private Mock<IDevice> _device;
        private Mock<IEventBus> _eventBus;
        private Mock<IMessageDisplay> _messageDisplay;
        private Mock<IPersistentStorageManager> _persistentStorage;
        private Mock<IPersistentStorageAccessor> _persistentStorageBlock;
        private Mock<IPrinter> _printer;
        private Mock<IPropertiesManager> _propertiesManager;
        private PrinterViewModel _target;
        private Mock<IInformationTicketCreator> _ticketCreator;
        private Mock<ITime> _time;
        private readonly ManualResetEvent _waiter = new ManualResetEvent(false);
        private const int Timeout = 1000; // One second

        /// <summary>
        ///     Initializes class members and prepares for execution of a TestMethod.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            //_propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.OperatorMenuDiagnosticsEnabled, false)).Returns(true);
            //_propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.OperatorMenuPrinterPageActivationTimeVisibility, false)).Returns(true);
            //_propertiesManager.Setup(
            //        mock => mock.GetProperty(ApplicationConstants.OperatorMenuPrinterPageActivationTimeVisibility, false))
            //    .Returns(false);
            //_propertiesManager.Setup(
            //        mock => mock.GetProperty(ApplicationConstants.OperatorMenuPrinterPagePrintButtonVisibility, false))
            //    .Returns(false);
            //_propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.OperatorMenuPrintButtonOptionsPrintLast15, true))
            //    .Returns(true);
            //_propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.OperatorMenuTechnicianModeRestrictions, false))
            //    .Returns(false);
            //_propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.OperatorMenuPrintButtonOptionsPrintCurrentPage, true))
            //    .Returns(true);
            //_propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.OperatorMenuPrintButtonOptionsPrintSelected, true))
            //    .Returns(true);
            _propertiesManager.Setup(m => m.GetProperty(PropertyKey.CurrentBalance, It.IsAny<long>()))
                .Returns(0L);

            _persistentStorageBlock =
                MoqServiceManager.CreateAndAddService<IPersistentStorageAccessor>(MockBehavior.Strict);
            _persistentStorageBlock.Setup(m => m["InProgress"]).Returns(true);
            _persistentStorageBlock.Setup(m => m["TicketTitle"]).Returns("TEST TITLE");
            _persistentStorageBlock.Setup(m => m["TicketValidationNumber"]).Returns("TEST VALIDATION NUMBER");
            _persistentStorageBlock.Setup(m => m["TicketValue"]).Returns("TEST VALUE");

            _persistentStorage = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Strict);
            _persistentStorage.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(true);
            _persistentStorage.Setup(m => m.GetBlock(It.IsAny<string>())).Returns(_persistentStorageBlock.Object);

            _device = MoqServiceManager.CreateAndAddService<IDevice>(MockBehavior.Strict);
            _device.Setup(m => m.Manufacturer).Returns("Manufacturer");
            _device.Setup(m => m.Model).Returns("Model");
            _device.Setup(m => m.FirmwareId).Returns("FirmwareId");
            _device.Setup(m => m.FirmwareRevision).Returns("FirmwareRevision");
            _device.Setup(m => m.FirmwareCyclicRedundancyCheck).Returns("FirmwareCyclicRedundancyCheck");
            _device.Setup(m => m.VariantName).Returns("VariantName");
            _device.Setup(m => m.Protocol).Returns("Protocol");
            _device.Setup(m => m.PortName).Returns("PortName");
            _device.Setup(m => m.SerialNumber).Returns("SerialNumber");
            _device.Setup(m => m.PollingFrequency).Returns(2000);

            _printer = new Mock<IPrinter>(MockBehavior.Strict);
            _printer.Setup(m => m.ReasonDisabled).Returns(DisabledReasons.Error);
            _printer.Setup(m => m.LastError).Returns(string.Empty);
            _printer.Setup(m => m.Enabled).Returns(false);
            _printer.Setup(m => m.Faults).Returns(PrinterFaultTypes.None);
            _printer.Setup(m => m.Warnings).Returns(PrinterWarningTypes.None);
            MoqServiceManager.AddService<IPrinter>(_printer.As<IService>().Object);
            _printer.Setup(m => m.SelfTest(It.IsAny<bool>())).Returns(Task.FromResult(true));
            _printer.Setup(m => m.DeviceConfiguration).Returns(_device.Object);
            _printer.Setup(m => m.LogicalState).Returns(PrinterLogicalState.Idle);
            _printer.Setup(m => m.ActivationTime).Returns(DateTime.Now);
            _printer.Setup(m => m.CanPrint).Returns(true);

            _messageDisplay = MoqServiceManager.CreateAndAddService<IMessageDisplay>(MockBehavior.Strict);
            _messageDisplay.Setup(m => m.DisplayMessage(It.IsAny<DisplayableMessage>()));

            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            _eventBus.Setup(m => m.Publish(It.IsAny<PageTitleEvent>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPopupEvent>()));

            _time = MoqServiceManager.CreateAndAddService<ITime>(MockBehavior.Strict);
            _time.Setup(t => t.GetLocationTime(It.IsAny<DateTime>())).Returns(DateTime.Now);

            var monitor = MoqServiceManager.CreateAndAddService<IOperatorMenuGamePlayMonitor>(MockBehavior.Strict);
            monitor.Setup(m => m.InGameRound).Returns(false);
            monitor.Setup(m => m.IsRecoveryNeeded).Returns(false);

            InitTarget();
            _accessor = new DynamicPrivateObject(_target);
        }

        private void InitTarget()
        {
            _target = new PrinterViewModel(false);
            SetupEventSubscriptionMocks();
            _target.LoadedCommand.Execute(null);
        }

        /// <summary>
        ///     Cleans up class members after execution of a TestMethod.
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<PrintButtonStatusEvent>()));
            _eventBus.Setup(m => m.UnsubscribeAll(_target));

            _target.Dispose();

            _target = null;

            MoqServiceManager.RemoveInstance();

            if (AddinManager.IsInitialized)
            {
                try
                {
                    AddinManager.Shutdown();
                }
                catch (InvalidOperationException)
                {
                    // temporarily swallow exception
                }
            }
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void ConstructorPrinterDisabledTest()
        {
            _printer.Setup(m => m.LogicalState).Returns(PrinterLogicalState.Disabled);

            _eventBus.Setup(m => m.UnsubscribeAll(It.IsAny<object>())).Verifiable();
            _target.Dispose();
            InitTarget();

            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void ConstructorPortNameExistsTest()
        {
            var portNames = ServiceManager.GetInstance().GetService<ISerialPortsService>().GetAllLogicalPortNames().ToArray();
            if (portNames.Length > 0)
            {
                var foundPortName = portNames[0];
                _device.Setup(m => m.PortName).Returns(foundPortName);
                _eventBus.Setup(m => m.UnsubscribeAll(It.IsAny<object>())).Verifiable();

                _target.Dispose();
                InitTarget();

                Assert.IsNotNull(_target);
                Assert.AreEqual(foundPortName, _target.PortText);
            }
        }

        [TestMethod]
        public void ConstructorUninitializedTest()
        {
            _printer.Setup(m => m.LogicalState).Returns(PrinterLogicalState.Uninitialized);
            _eventBus.Setup(m => m.UnsubscribeAll(It.IsAny<object>())).Verifiable();
            _target.Dispose();
            InitTarget();
            
            Assert.IsNotNull(_target);
            Assert.AreEqual("?", _target.ManufacturerText);
        }

        [TestMethod]
        public void GetFormattedBarcodeTest()
        {
            string actual = string.Empty.GetFormattedBarcode();
            Assert.AreEqual("XX-XXXX-XXXX-XXXX-0000", actual);

            actual = "1234".GetFormattedBarcode();
            Assert.AreEqual("XX-XXXX-XXXX-XXXX-1234", actual);

            actual = "123456789012345678".GetFormattedBarcode();
            Assert.AreEqual("XX-XXXX-XXXX-XXXX-5678", actual);

            actual = "1234567890123456789012345678901234567890".GetFormattedBarcode();
            Assert.AreEqual("XXXX-XXXX-XXXX-XXXX-XXXX-XXXX-XXXX-XXXX-XXXX-7890", actual);
        }

        [TestMethod]
        public void SubscribeTest()
        {
            _accessor.SubscribeToEvents();
        }

        [TestMethod]
        public void UnsubscribeTest()
        {
            _eventBus.Setup(m => m.UnsubscribeAll(_target)).Verifiable();

            _accessor.UnsubscribeFromEvents();

            _eventBus.Verify();
        }

        [TestMethod]
        public void DisplayPropertiesTest()
        {
            var printer = MoqServiceManager.CreateAndAddService<IPrinter>(MockBehavior.Strict);

            // Negative test
            var expectedText = string.Empty;
            var expectedPrinterState = PrinterLogicalState.Idle;
            var selfTestState = SelfTestState.Initial;
            var stateMode = StateMode.Normal;
            var statusMode = StatusMode.Normal;


           _target.ShowDiagnostics = true;
            _target.SelfTestCurrentState = selfTestState;
            _target.DiagnosticsEnabled = false;
            _target.StateText = expectedText;
            _target.StateCurrentMode = stateMode;
            _target.StatusText = expectedText;
            _target.StatusCurrentMode = statusMode;
            printer.Setup(m => m.LogicalState).Returns(expectedPrinterState);

            Assert.IsTrue(_target.ShowDiagnostics);
            Assert.AreEqual(selfTestState, _target.SelfTestCurrentState);
            Assert.IsFalse(_target.DiagnosticsEnabled);
            Assert.AreEqual(expectedText, _target.StateText);
            Assert.AreEqual(stateMode, _target.StateCurrentMode);
            Assert.AreEqual(expectedText, _target.StatusText);
            Assert.AreEqual(statusMode, _target.StatusCurrentMode);
            Assert.IsTrue(_target.IsSelfTestVisible);
            Assert.AreEqual(string.Empty, _target.SelfTestText);
            Assert.AreEqual(Brushes.Black, _target.SelfTestForeground);
            Assert.IsFalse(_target.PrinterButtonsEnabled);
            Assert.AreEqual(Brushes.White, _target.StateForeground);
            Assert.AreEqual(Brushes.White, _target.StatusForeground);

            // Positive test
            expectedText = "ExpectedText";
            expectedPrinterState = PrinterLogicalState.Idle;
            selfTestState = SelfTestState.Passed;
            stateMode = StateMode.Error;
            statusMode = StatusMode.Error;

            _target.SelfTestButtonEnabled = false;
            _target.ShowDiagnostics = true;
            _target.SelfTestCurrentState = selfTestState;
            _target.DiagnosticsEnabled = true;
            _target.StateText = expectedText;
            _target.StateCurrentMode = stateMode;
            _target.StatusText = expectedText;
            _target.StatusCurrentMode = statusMode;
            printer.Setup(m => m.LogicalState).Returns(expectedPrinterState);

            Assert.IsTrue(_target.ShowDiagnostics);
            Assert.AreEqual(selfTestState, _target.SelfTestCurrentState);
            Assert.IsTrue(_target.DiagnosticsEnabled);
            Assert.AreEqual(expectedText, _target.StateText);
            Assert.AreEqual(stateMode, _target.StateCurrentMode);
            Assert.AreEqual(expectedText, _target.StatusText);
            Assert.IsTrue(_target.IsSelfTestVisible);
            Assert.IsFalse(_target.SelfTestButtonEnabled);
            Assert.AreEqual("Self Test Passed", _target.SelfTestText);
            Assert.AreEqual(Brushes.LightGreen, _target.SelfTestForeground);
            Assert.IsFalse(_target.PrinterButtonsEnabled);
            Assert.AreEqual(Brushes.Red, _target.StateForeground);
        }

        [TestMethod]
        public void UpdateScreenUninitializedTest()
        {
            _target.DiagnosticsEnabled = false;
            _printer.Setup(m => m.CanPrint).Returns(true);
            _printer.Setup(m => m.LogicalState).Returns(PrinterLogicalState.Uninitialized);

            _accessor.UpdateScreen();

            Assert.AreEqual("?", _target.ManufacturerText);
        }

        [TestMethod]
        public void UpdateStatusErrorTest()
        {
            // Empty no clear, expect Empty
            _target.StatusText = string.Empty;
            _target.StatusCurrentMode = StatusMode.Normal;
            _accessor.UpdateStatusError(string.Empty, false);
            Assert.AreEqual(string.Empty, _target.StatusText);
            Assert.AreEqual(StatusMode.Normal, _target.StatusCurrentMode);

            // Warning no clear, expect Warning
            _target.StatusText = string.Empty;
            _target.StatusCurrentMode = StatusMode.Normal;
            _accessor.UpdateStatusError(PrinterEventsDescriptor.WarningTexts[PrinterWarningTypes.PaperLow], false);
            Assert.AreEqual(PrinterEventsDescriptor.WarningTexts[PrinterWarningTypes.PaperLow], _target.StatusText);
            Assert.AreEqual(StatusMode.Warning, _target.StatusCurrentMode);

            // Error no clear, expect Error
            _target.StatusText = string.Empty;
            _target.StatusCurrentMode = StatusMode.Normal;
            _accessor.UpdateStatusError(PrinterEventsDescriptor.FaultTexts[PrinterFaultTypes.ChassisOpen], false);
            Assert.AreEqual(PrinterEventsDescriptor.FaultTexts[PrinterFaultTypes.ChassisOpen], _target.StatusText);
            Assert.AreEqual(StatusMode.Error, _target.StatusCurrentMode);

            // Error no clear after existing error, expect both errors
            _target.StatusText = PrinterEventsDescriptor.FaultTexts[PrinterFaultTypes.NvmFault];
            _target.StatusCurrentMode = StatusMode.Error;
            _accessor.UpdateStatusError(PrinterEventsDescriptor.FaultTexts[PrinterFaultTypes.ChassisOpen], false);
            Assert.AreEqual(
                PrinterEventsDescriptor.FaultTexts[PrinterFaultTypes.NvmFault] + "\n" + PrinterEventsDescriptor.FaultTexts[PrinterFaultTypes.ChassisOpen],
                _target.StatusText);
            Assert.AreEqual(StatusMode.Error, _target.StatusCurrentMode);

            // Error no clear after enabled by something, expect only error
            _target.StatusText = "Enabled By ";
            _target.StatusCurrentMode = StatusMode.Normal;
            _accessor.UpdateStatusError(PrinterEventsDescriptor.FaultTexts[PrinterFaultTypes.ChassisOpen], false);
            Assert.AreEqual(PrinterEventsDescriptor.FaultTexts[PrinterFaultTypes.ChassisOpen], _target.StatusText);
            Assert.AreEqual(StatusMode.Error, _target.StatusCurrentMode);

            // Warning clear, expect Empty
            _target.StatusText = string.Empty;
            _target.StatusCurrentMode = StatusMode.Normal;
            _accessor.UpdateStatusError(PrinterEventsDescriptor.WarningTexts[PrinterWarningTypes.PaperLow], true);
            Assert.AreEqual(string.Empty, _target.StatusText);
            Assert.AreEqual(StatusMode.Normal, _target.StatusCurrentMode);

            // Error clear, expect Empty
            _target.StatusText = string.Empty;
            _target.StatusCurrentMode = StatusMode.Normal;
            _accessor.UpdateStatusError(PrinterEventsDescriptor.FaultTexts[PrinterFaultTypes.ChassisOpen], true);
            Assert.AreEqual(string.Empty, _target.StatusText);
            Assert.AreEqual(StatusMode.Normal, _target.StatusCurrentMode);

            // Error clear after two errors, expect one error
            _target.StatusText = PrinterEventsDescriptor.FaultTexts[PrinterFaultTypes.NvmFault] + "\n" + PrinterEventsDescriptor.FaultTexts[PrinterFaultTypes.ChassisOpen];
            _target.StatusCurrentMode = StatusMode.Error;
            _accessor.UpdateStatusError(PrinterEventsDescriptor.FaultTexts[PrinterFaultTypes.ChassisOpen], true);
            Assert.AreEqual(PrinterEventsDescriptor.FaultTexts[PrinterFaultTypes.NvmFault], _target.StatusText);
            Assert.AreEqual(StatusMode.Error, _target.StatusCurrentMode);

            // Error clear after two errors, expect one error
            _target.StatusText = PrinterEventsDescriptor.FaultTexts[PrinterFaultTypes.NvmFault] + "\n" + PrinterEventsDescriptor.FaultTexts[PrinterFaultTypes.ChassisOpen];
            _target.StatusCurrentMode = StatusMode.Error;
            _accessor.UpdateStatusError(PrinterEventsDescriptor.FaultTexts[PrinterFaultTypes.NvmFault], true);
            Assert.AreEqual(PrinterEventsDescriptor.FaultTexts[PrinterFaultTypes.ChassisOpen], _target.StatusText);
            Assert.AreEqual(StatusMode.Error, _target.StatusCurrentMode);

            // Error clear after enabled by something, expect only error
            _target.StatusText = "Enabled By " + "System" + "\n" +
                PrinterEventsDescriptor.FaultTexts[PrinterFaultTypes.ChassisOpen];
            _target.StatusCurrentMode = StatusMode.Error;
            _accessor.UpdateStatusError(PrinterEventsDescriptor.FaultTexts[PrinterFaultTypes.ChassisOpen], true);
            Assert.AreEqual("Enabled By " + "System", _target.StatusText);
            Assert.AreEqual(StatusMode.Normal, _target.StatusCurrentMode);
        }

        [TestMethod]
        public void UpdateStatusNoPrinterServiceTest()
        {
            MoqServiceManager.RemoveService<IPrinter>();

            _accessor.UpdateStatus();
        }

        [TestMethod]
        public void HandleEventNoPrinterServiceTest()
        {
            MoqServiceManager.RemoveService<IPrinter>();

            _target.StatusText = "Inspection Failed";
            _target.StatusCurrentMode = StatusMode.Error;
            _accessor.HandleEvent(new InspectedEvent());

            // No changes should happen because printer service is not found
            Assert.AreEqual("Inspection Failed", _target.StatusText);
            Assert.AreEqual(StatusMode.Error, _target.StatusCurrentMode);
        }

        [TestMethod]
        public void HandleEventTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<PrintButtonStatusEvent>())).Verifiable();
            //_eventBus.Setup(m => m.GetEvent(It.IsAny<PrinterPage>())).Returns(new PaperLowEvent());

            _printer.As<IDeviceService>().Setup(m => m.ReasonDisabled).Returns(DisabledReasons.System);
            _accessor.HandleEvent(new DisabledEvent(DisabledReasons.Operator));
            Assert.AreEqual(StatusMode.Warning, _target.StatusCurrentMode);

            _printer.As<IDeviceService>().Setup(m => m.ReasonDisabled).Returns(DisabledReasons.Error);
            _accessor.HandleEvent(new DisabledEvent(DisabledReasons.Operator));

            _target.StatusText = "Not Empty";
            _printer.As<IDeviceService>().Setup(m => m.LastError).Returns("Test");
            _printer.Setup(m => m.LogicalState).Returns(PrinterLogicalState.Idle);
            _accessor.HandleEvent(new EnabledEvent(EnabledReasons.Operator));
            Assert.AreEqual(string.Empty, _target.StatusText);

            _target.StatusText = "Not Empty";
            _printer.As<IDeviceService>().Setup(m => m.LastError).Returns((string)null);
            _printer.Setup(m => m.LogicalState).Returns(PrinterLogicalState.Idle);
            _accessor.HandleEvent(new EnabledEvent(EnabledReasons.Operator));
            Assert.AreEqual(string.Empty, _target.StatusText);

            _target.StatusText = "Not Empty";
            _printer.Setup(m => m.LogicalState).Returns(PrinterLogicalState.Idle);
            _accessor.HandleEvent(new EnabledEvent(EnabledReasons.System));
            Assert.IsTrue(_target.StatusText.Contains("Enabled By "));

            _target.StatusText = string.Empty;
            _accessor.HandleEvent(new PrintStartedEvent());
            Assert.AreEqual("Print Started", _target.StatusText);
        }

        [TestMethod]
        public void HandleStatusChangeAndResetEventsTest()
        {
            _target.StatusText = string.Empty;
            _accessor.SetWarning(new HardwareWarningEvent(PrinterWarningTypes.PaperLow));
            Assert.AreEqual(PrinterEventsDescriptor.WarningTexts[PrinterWarningTypes.PaperLow], _target.StatusText);
            _accessor.ClearWarning(new HardwareWarningClearEvent(PrinterWarningTypes.PaperLow));
            Assert.AreEqual(string.Empty, _target.StatusText);

            _target.StatusText = string.Empty;
            _accessor.SetFault(new HardwareFaultEvent(PrinterFaultTypes.PrintHeadDamaged));
            Assert.AreEqual(PrinterEventsDescriptor.FaultTexts[PrinterFaultTypes.PrintHeadDamaged], _target.StatusText);
            _accessor.ClearFault(new HardwareFaultClearEvent(PrinterFaultTypes.PrintHeadDamaged));
            Assert.AreEqual(string.Empty, _target.StatusText);

            _target.StatusText = string.Empty;
            _accessor.SetFault(new HardwareFaultEvent(PrinterFaultTypes.PaperEmpty));
            Assert.AreEqual(PrinterEventsDescriptor.FaultTexts[PrinterFaultTypes.PaperEmpty], _target.StatusText);
            _accessor.ClearFault(new HardwareFaultClearEvent(PrinterFaultTypes.PaperEmpty));
            Assert.AreEqual(string.Empty, _target.StatusText);

            _target.StatusText = string.Empty;
            _accessor.SetFault(new HardwareFaultEvent(PrinterFaultTypes.PrintHeadOpen));
            Assert.AreEqual(PrinterEventsDescriptor.FaultTexts[PrinterFaultTypes.PrintHeadOpen], _target.StatusText);
            _accessor.ClearFault(new HardwareFaultClearEvent(PrinterFaultTypes.PrintHeadOpen));
            Assert.AreEqual(string.Empty, _target.StatusText);

            _target.StatusText = string.Empty;
            _accessor.SetFault(new HardwareFaultEvent(PrinterFaultTypes.OtherFault));
            Assert.AreEqual(PrinterEventsDescriptor.FaultTexts[PrinterFaultTypes.OtherFault], _target.StatusText);
            _accessor.ClearFault(new HardwareFaultClearEvent(PrinterFaultTypes.OtherFault));
            Assert.AreEqual(string.Empty, _target.StatusText);

            _target.StatusText = string.Empty;
            _accessor.SetFault(new HardwareFaultEvent(PrinterFaultTypes.NvmFault));
            Assert.AreEqual(PrinterEventsDescriptor.FaultTexts[PrinterFaultTypes.NvmFault], _target.StatusText);
            _accessor.ClearFault(new HardwareFaultClearEvent(PrinterFaultTypes.NvmFault));
            Assert.AreEqual(string.Empty, _target.StatusText);

            _target.StatusText = string.Empty;
            _accessor.SetFault(new HardwareFaultEvent(PrinterFaultTypes.TemperatureFault));
            Assert.AreEqual(PrinterEventsDescriptor.FaultTexts[PrinterFaultTypes.TemperatureFault], _target.StatusText);
            _accessor.ClearFault(new HardwareFaultClearEvent(PrinterFaultTypes.TemperatureFault));
            Assert.AreEqual(string.Empty, _target.StatusText);

            _target.StatusText = string.Empty;
            _accessor.SetFault(new HardwareFaultEvent(PrinterFaultTypes.PaperNotTopOfForm));
            Assert.AreEqual(PrinterEventsDescriptor.FaultTexts[PrinterFaultTypes.PaperNotTopOfForm], _target.StatusText);
            _accessor.ClearFault(new HardwareFaultClearEvent(PrinterFaultTypes.PaperNotTopOfForm));
            Assert.AreEqual(string.Empty, _target.StatusText);

            _target.StatusText = string.Empty;
            _accessor.ErrorEvent(new DisconnectedEvent());
            Assert.AreEqual("Printer Offline", _target.StatusText);
            _accessor.ErrorClearEvent(new ConnectedEvent());
            Assert.AreEqual(string.Empty, _target.StatusText);

            _target.StatusText = string.Empty;
            _accessor.SetFault(new HardwareFaultEvent(PrinterFaultTypes.FirmwareFault));
            Assert.AreEqual(PrinterEventsDescriptor.FaultTexts[PrinterFaultTypes.FirmwareFault], _target.StatusText);
            _accessor.ClearFault(new HardwareFaultClearEvent(PrinterFaultTypes.FirmwareFault));
            Assert.AreEqual(string.Empty, _target.StatusText);

            _target.StatusText = string.Empty;
            _accessor.SetFault(new HardwareFaultEvent(PrinterFaultTypes.PaperJam));
            Assert.AreEqual(PrinterEventsDescriptor.FaultTexts[PrinterFaultTypes.PaperJam], _target.StatusText);
            _accessor.ClearFault(new HardwareFaultClearEvent(PrinterFaultTypes.PaperJam));
            Assert.AreEqual(string.Empty, _target.StatusText);

            _target.StatusText = string.Empty;
            _accessor.SetFault(new HardwareFaultEvent(PrinterFaultTypes.ChassisOpen));
            Assert.AreEqual(PrinterEventsDescriptor.FaultTexts[PrinterFaultTypes.ChassisOpen], _target.StatusText);
            _accessor.ClearFault(new HardwareFaultClearEvent(PrinterFaultTypes.ChassisOpen));
            Assert.AreEqual(string.Empty, _target.StatusText);

            _target.StatusText = string.Empty;
            _accessor.ErrorEvent(new TransferStatusEvent());
            Assert.AreEqual("Printer Transfer Status Error", _target.StatusText);
            _accessor.ErrorClearEvent(new LoadingRegionsAndTemplatesEvent());
            Assert.AreEqual(string.Empty, _target.StatusText);
        }

        [TestMethod]
        public void SetDeviceInformationTest()
        {
            _accessor.SetDeviceInformation();
            Assert.AreEqual("Manufacturer", _target.ManufacturerText);
            Assert.AreEqual("Model", _target.ModelText);
            Assert.AreEqual("FirmwareId", _target.FirmwareVersionText);
            Assert.AreEqual("FirmwareRevision", _target.FirmwareRevisionText);
            Assert.AreEqual("FirmwareCyclicRedundancyCheck", _target.FirmwareCrcText);
            Assert.AreEqual("SerialNumber", _target.SerialNumberText);
            Assert.AreEqual("Protocol", _target.ProtocolText);
            Assert.AreEqual("PortName", _target.PortText);
        }

        [TestMethod]
        public void SetDeviceInformationUnknownTest()
        {
            var expected = "?";
            _accessor.SetDeviceInformationUnknown();
            Assert.AreEqual(expected, _target.ManufacturerText);
            Assert.AreEqual(expected, _target.ModelText);
            Assert.AreEqual(expected, _target.FirmwareVersionText);
            Assert.AreEqual(expected, _target.FirmwareRevisionText);
            Assert.AreEqual(expected, _target.FirmwareRevisionText);
            Assert.AreEqual(expected, _target.SerialNumberText);
            Assert.AreEqual(expected, _target.SerialNumberText);
            Assert.AreEqual(expected, _target.PortText);
        }

        [TestMethod]
        public void SetLastErrorStatusUsingErrorTextDictionaryTest()
        {
            PrinterFaultTypes faults = PrinterFaultTypes.None;
            PrinterWarningTypes warnings = PrinterWarningTypes.None;
            var expected = string.Empty;

            var first = true;
            foreach (var pair in PrinterEventsDescriptor.FaultTexts)
            {
                faults |= pair.Key;

                if (string.IsNullOrEmpty(pair.Value))
                {
                    continue;
                }

                if (!first)
                {
                    expected += "\n";
                }

                expected += pair.Value;
                first = false;
            }

            _target.StatusText = string.Empty;
            _accessor.SetLastErrorStatus(faults, warnings);
            Assert.AreEqual(expected, _target.StatusText);
        }

        [TestMethod]
        public void SetLastErrorStatusUsingWarningTextDictionaryTest()
        {
            PrinterFaultTypes faults = PrinterFaultTypes.None;
            PrinterWarningTypes warnings = PrinterWarningTypes.None;
            var expected = string.Empty;

            var first = true;
            foreach (var pair in PrinterEventsDescriptor.WarningTexts)
            {
                warnings |= pair.Key;

                if (string.IsNullOrEmpty(pair.Value))
                {
                    continue;
                }

                if (!first)
                {
                    expected += "\n";
                }

                expected += pair.Value;
                first = false;
            }

            _target.StatusText = string.Empty;
            _accessor.SetLastErrorStatus(faults, warnings);
            Assert.AreEqual(expected, _target.StatusText);
        }

        [TestMethod]
        public void SetLastErrorStatusPaperInChuteTest()
        {
            Assert.IsTrue(_accessor.SetLastErrorStatus(PrinterFaultTypes.None, PrinterWarningTypes.PaperInChute));
        }

        [TestMethod]
        public void PrintDiagnosticButtonTest()
        {
            _target.ManufacturerText = _device.Object.Manufacturer;
            _target.ModelText = _device.Object.Model;
            _target.FirmwareVersionText = _device.Object.FirmwareId;
            _target.FirmwareRevisionText = _device.Object.FirmwareRevision;
            _target.FirmwareCrcText = _device.Object.FirmwareCyclicRedundancyCheck;
            _target.SerialNumberText = _device.Object.VariantName;
            _target.ProtocolText = _device.Object.Protocol;
            _target.PortText = _device.Object.PortName;
            _printer.Setup(m => m.LogicalState).Returns(PrinterLogicalState.Idle);
            _printer.Setup(m => m.Print(It.IsAny<Ticket>()))
                .Returns(Task.FromResult(true));
            _propertiesManager.Setup(m => m.GetProperty("Cabinet.SerialNumber", It.IsAny<int>())).Returns(0);
            _propertiesManager.Setup(m => m.GetProperty("Cabinet.MachineId", It.IsAny<string>())).Returns(string.Empty);
            _propertiesManager.Setup(m => m.GetProperty("Cabinet.ZoneName", It.IsAny<string>())).Returns(string.Empty);
            _propertiesManager.Setup(m => m.GetProperty("Cabinet.BankId", It.IsAny<string>())).Returns(string.Empty);
            _propertiesManager.Setup(m => m.GetProperty("Cabinet.EgmPosition", It.IsAny<string>()))
                .Returns(string.Empty);
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPrintJobStartedEvent>())).Verifiable();

            _accessor.PrintDiagnosticButtonCommand.Execute(null);
            _printer.Verify();

            MoqServiceManager.RemoveService<IPrinter>();
            _accessor.PrintDiagnosticButtonCommand.Execute(null);
        }

        [Ignore]
        [TestMethod]
        public void FormFeedButtonTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<PrintStartedEvent>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPrintJobStartedEvent>())).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPrintJobCompletedEvent>())).Verifiable();
            _printer.Setup(m => m.Enabled).Returns(true);
            //_printer.Setup(m => m.FormFeed())
            //    .Returns(Task.FromResult(true))
            //    .Verifiable();
            _accessor.FormFeedButtonClicked(null);
            _printer.Verify();
        }

        [TestMethod]
        public void SelfTestButtonTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPrintJobStartedEvent>())).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPrintJobCompletedEvent>())).Verifiable();

            _target.SelfTestCurrentState = SelfTestState.Initial;
            _accessor.SelfTestButtonClicked(null);
            Assert.AreEqual(SelfTestState.Running, _target.SelfTestCurrentState);

            _target.SelfTestCurrentState = SelfTestState.Initial;
            _accessor.SelfTestClearNvmButtonClicked(null);

            Assert.AreEqual(SelfTestState.Running, _target.SelfTestCurrentState);
            _accessor.HandleEvent(new SelfTestPassedEvent());
            Assert.AreEqual(SelfTestState.Passed, _target.SelfTestCurrentState);
            MoqServiceManager.RemoveService<IPrinter>();
            _target.SelfTestCurrentState = SelfTestState.Initial;
            _accessor.SelfTestButtonClicked(null);
            Assert.AreEqual(SelfTestState.Initial, _target.SelfTestCurrentState); 
            _eventBus.Verify();
        }

        [TestMethod]
        public void SelfTestEventTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPrintJobCompletedEvent>())).Verifiable();
            _target.SelfTestCurrentState = SelfTestState.Running;
            _accessor.HandleEvent(new SelfTestPassedEvent());
            Assert.AreEqual(SelfTestState.Passed, _target.SelfTestCurrentState);

            _target.SelfTestCurrentState = SelfTestState.Running;
            _accessor.HandleEvent(new SelfTestFailedEvent());
            Assert.AreEqual(SelfTestState.Failed, _target.SelfTestCurrentState);
            _eventBus.Verify();
        }

        [TestMethod]
        public void PrintDiagnosticButtonClickedTest()
        {
            // Ticket generation is now on a separate thread
            _ticketCreator = MoqServiceManager.CreateAndAddService<IInformationTicketCreator>(MockBehavior.Strict);
            _ticketCreator.Setup(m => m.CreateInformationTicket(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new Ticket())
                .Verifiable();
            _printer.Setup(m => m.Print(It.IsAny<Ticket>()))
                .Returns(Task.FromResult(true))
                .Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPrintJobStartedEvent>())).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPrintJobEvent>())).Callback(() => _waiter.Set()).Verifiable();

            _accessor.PrintDiagnosticButtonCommand.Execute(null);
            Assert.IsTrue(_waiter.WaitOne(Timeout));

            _ticketCreator.Verify();
            _eventBus.Verify();
        }

        [TestMethod]
        public void PrintButtonClickedTest()
        {
            _ticketCreator = MoqServiceManager.CreateAndAddService<IInformationTicketCreator>(MockBehavior.Strict);
            _ticketCreator.Setup(m => m.CreateInformationTicket(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new Ticket())
                .Verifiable();

            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<BankBalanceChangedEvent>>())).Verifiable();
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<PrintButtonStatusEvent>>())).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPageLoadedEvent>())).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPrintJobStartedEvent>())).Verifiable();

            Action<PrintButtonClickedEvent> onPrinterClicked = null;
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<PrintButtonClickedEvent>>()))
                .Callback<object, Action<PrintButtonClickedEvent>>(
                    (tar, act) =>
                    {
                        onPrinterClicked = act;
                    });

            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPrintJobEvent>()))
                .Callback<OperatorMenuPrintJobEvent>(
                    e =>
                    {
                        Assert.IsTrue(e.TicketsToPrint != null && e.TicketsToPrint.Count() == 1);
                        _waiter.Set();
                    });

            _target.ProtocolText = "Protocol";
            _target.LoadedCommand.Execute(null);
            onPrinterClicked(new PrintButtonClickedEvent());

            Assert.IsTrue(_waiter.WaitOne(Timeout));
            _ticketCreator.Verify();
            _printer.Verify();
            _eventBus.Verify();
        }

        [TestMethod]
        public void FinalizeTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<PrintButtonStatusEvent>())).Verifiable();
            _eventBus.Setup(m => m.UnsubscribeAll(_target));

            _accessor.OnUnloaded();
        }

        [TestMethod]
        public void SetStateInformationNoPrinterTest()
        {
            MoqServiceManager.RemoveService<IPrinter>();
            _accessor.StateCurrentMode = StateMode.Normal;

            _accessor.SetStateInformation(true);

            Assert.AreEqual(StateMode.Error, _accessor.StateCurrentMode);
        }

        [TestMethod]
        public void SetStateInformationInspectingTest()
        {
            _printer.Setup(m => m.LogicalState).Returns(PrinterLogicalState.Inspecting);
            _accessor.StateCurrentMode = StateMode.Normal;

            _accessor.SetStateInformation(true);

            Assert.AreEqual(StateMode.Processing, _accessor.StateCurrentMode);
        }

        [TestMethod]
        public void SetStateInformationPrintingTest()
        {
            _printer.Setup(m => m.LogicalState).Returns(PrinterLogicalState.Printing);
            _accessor.StateCurrentMode = StateMode.Normal;
            _accessor.StatusCurrentMode = StatusMode.Error;

            _accessor.SetStateInformation(true);

            Assert.AreEqual(StateMode.Processing, _accessor.StateCurrentMode);
            Assert.AreEqual(StatusMode.Working, _accessor.StatusCurrentMode);
        }

        private void SetupEventSubscriptionMocks()
        {
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<DisabledEvent>>())).Verifiable();
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<EnabledEvent>>())).Verifiable();
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<InspectedEvent>>())).Verifiable();
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<PrintStartedEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<PrintCompletedEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<ConnectedEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<DisconnectedEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<HardwareFaultClearEvent>>())).Verifiable();
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<HardwareFaultEvent>>())).Verifiable();
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<HardwareWarningClearEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<HardwareWarningEvent>>()))
                .Verifiable();
            _eventBus.Setup(
                    m => m.Subscribe(
                        _target,
                        It.IsAny<Action<InspectionFailedEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<ResolverErrorEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<PrintButtonClickedEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<TransferStatusEvent>>()))
                .Verifiable();
            _eventBus.Setup(
                    m => m.Subscribe(
                        _target,
                        It.IsAny<Action<LoadingRegionsAndTemplatesEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<SelfTestPassedEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<SelfTestFailedEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<BankBalanceChangedEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<PrintButtonStatusEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPageLoadedEvent>())).Verifiable();
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<ClosedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<OpenEvent>>()));
        }
    }
}
