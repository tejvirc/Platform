namespace Aristocrat.Monaco.Application.UI.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using Contracts;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Contracts.Tickets;
    using Events;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.IO;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.SharedDevice;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Kernel.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.UI.Common.Events;
    using Mono.Addins;
    using Moq;
    using Test.Common;
    using UI.ViewModels;

    /// <summary>
    ///     Summary description for HardwareMeterScreenTest
    /// </summary>
    [TestClass]
    public class GeneralSetingsPageViewModelTests
    {
        private Mock<IEventBus> _eventBus;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IPathMapper> _pathMapper;
        private Mock<ITime> _timeMapper;
        private Mock<IMeterManager> _meterManager;
        private Mock<IMeter> _meter;
        private GeneralSettingsPageViewModel _target;
        private readonly ManualResetEvent _waiter = new ManualResetEvent(false);
        private const int Timeout = 5000;
        private const string HardBootTimeKey = "System.HardBoot.Time";
        private const string SoftBootTimeKey = "System.SoftBoot.Time";

        // Use TestInitialize to run code before running each test 
        [TestInitialize]
        public void MyTestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);
            _propertiesManager.Setup(mock => mock.GetProperty("Mono.SelectedAddinConfigurationHashCode", null))
                .Returns(null);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.OccurrenceMeterRolloverText, It.IsAny<int>()))
                .Returns(100L);
            _propertiesManager.Setup(m => m.GetProperty("OperatorMenu.MetersScreen.PrintButton.Visibility", false)).Returns(false);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.SerialNumber, It.IsAny<string>()))
                .Returns("0");
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.MachineId, It.IsAny<uint>()))
                .Returns((uint)0);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.TimeZoneKey, It.IsAny<TimeZoneInfo>()))
                .Returns(TimeZoneInfo.Local);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.TimeZoneOffsetKey, It.IsAny<TimeSpan>()))
                .Returns(TimeSpan.Zero);
            _propertiesManager.Setup(m => m.GetProperty(HardBootTimeKey, It.IsAny<DateTime>()))
                .Returns(DateTime.UtcNow);
            _propertiesManager.Setup(m => m.GetProperty(SoftBootTimeKey, It.IsAny<DateTime>()))
                .Returns(DateTime.UtcNow);
            _propertiesManager.Setup(m => m.GetProperty(PropertyKey.CurrentBalance, It.IsAny<long>()))
                .Returns(0L);

            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);

            _pathMapper = MoqServiceManager.CreateAndAddService<IPathMapper>(MockBehavior.Strict);
            _pathMapper.Setup(m => m.GetDirectory(It.IsAny<string>())).Returns(new DirectoryInfo(Directory.GetCurrentDirectory())).Verifiable();

            _timeMapper = MoqServiceManager.CreateAndAddService<ITime>(MockBehavior.Strict);
            _timeMapper.Setup(m => m.GetFormattedLocationTime()).Returns(new DateTime().ToString()).Verifiable();
            _timeMapper.Setup(m => m.GetLocationTime(It.IsAny<DateTime>(), It.IsAny<TimeZoneInfo>())).Returns(new DateTime()).Verifiable();

            _target = new GeneralSettingsPageViewModel();

            var monitor = MoqServiceManager.CreateAndAddService<IOperatorMenuGamePlayMonitor>(MockBehavior.Strict);
            monitor.Setup(m => m.InGameRound).Returns(false);
            monitor.Setup(m => m.IsRecoveryNeeded).Returns(false);

            _meter = MoqServiceManager.CreateAndAddService<IMeter>(MockBehavior.Strict);
            _meter.Setup(m => m.Lifetime).Returns(0);

            _meterManager = MoqServiceManager.CreateAndAddService<IMeterManager>(MockBehavior.Strict);
            _meterManager.Setup(m => m.GetMeter(It.IsAny<string>())).Returns(_meter.Object);
            _meterManager.Setup(m => m.IsMeterProvided(It.IsAny<string>())).Returns(true);

            var config = MoqServiceManager.CreateAndAddService<IOperatorMenuConfiguration>(MockBehavior.Strict);
            config.Setup(m => m.GetAccessRuleSet(It.IsAny<GeneralSettingsPageViewModel>())).Returns(It.IsAny<string>());
            config.Setup(m => m.GetAccessRuleSet(It.IsAny<GeneralSettingsPageViewModel>(), It.IsAny<string>())).Returns(It.IsAny<string>());
            config.Setup(m => m.GetPrintAccessRuleSet(It.IsAny<GeneralSettingsPageViewModel>())).Returns(It.IsAny<string>());
            config.Setup(m => m.GetPrintButtonEnabled(It.IsAny<GeneralSettingsPageViewModel>(), It.IsAny<bool>())).Returns(It.IsAny<bool>());
            config.Setup(m => m.GetSetting(It.IsAny<string>(), It.IsAny<bool>())).Returns(It.IsAny<bool>());

            var doors = new Mock<IDoorService>(MockBehavior.Default);
            MoqServiceManager.AddService<IDoorService>(doors.As<IService>().Object);
            MoqServiceManager.CreateAndAddService<ICabinetDetectionService>(MockBehavior.Loose);

            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<PrintButtonStatusEvent>>())).Verifiable();
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<PrintButtonClickedEvent>>())).Verifiable();
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<ClosedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<OpenEvent>>()));
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<DialogClosedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<OperatorCultureChangedEvent>>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPageLoadedEvent>())).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPopupEvent>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuWarningMessageEvent>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<LampTestLampStateEvent>()));

            var _buttonService = MoqServiceManager.CreateAndAddService<IButtonService>(MockBehavior.Strict);
            _buttonService.Setup(m => m.IsTestModeActive).Returns(It.IsAny<bool>());

            var _iio = MoqServiceManager.CreateAndAddService<IIO>(MockBehavior.Strict);
            _iio.Setup(m => m.SetButtonLamp(It.IsAny<uint>(), It.IsAny<bool>())).Returns(It.IsAny<uint>());
            _iio.Setup(m => m.SetButtonLampByMask(It.IsAny<uint>(), It.IsAny<bool>())).Returns(It.IsAny<uint>());

            // This will allow the UI Dispatcher to be used
            if (Application.Current == null)
            {
                new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            }
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup]
        public void MyTestCleanup()
        {
            _target = null;
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void OnPageLoadedTest()
        {
            _target.LoadedCommand.Execute(null);
            _eventBus.Verify();
        }

        /// <summary> This test is no longer just for statement coverage </summary>
        [TestMethod]
        public void PrintButtonClickedTest()
        {
            var ticketCreator = MoqServiceManager.CreateAndAddService<IMachineInfoTicketCreator>(MockBehavior.Strict);
            ticketCreator.Setup(m => m.Create())
                .Returns(new List<Ticket>() { new Ticket() })
                .Verifiable();

            Mock<IPrinter> printer = MoqServiceManager.CreateAndAddService<IPrinter>(MockBehavior.Strict);
            printer.SetupGet(m => m.Enabled).Returns(true);
            printer.SetupGet(m => m.CanPrint).Returns(true);
            printer.SetupGet(m => m.LogicalState).Returns(PrinterLogicalState.Idle);
            printer.Setup(m => m.Print(It.IsAny<Ticket>())).Returns(Task.FromResult(true));

            var device = new Mock<IDevice>();
            device.SetupGet(m => m.Protocol).Returns("Fake");
            device.SetupGet(m => m.Manufacturer).Returns("Fake");
            printer.SetupGet(m => m.DeviceConfiguration).Returns(device.Object);

            Action<PrintButtonClickedEvent> onPrinterClicked = null;
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<PrintButtonStatusEvent>>())).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPageLoadedEvent>())).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPrintJobStartedEvent>())).Verifiable();

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

            _eventBus.Setup(m => m.Publish(It.IsAny<LampTestLampStateEvent>()));

            _target.LoadedCommand.Execute(null);

            onPrinterClicked(new PrintButtonClickedEvent());

            Assert.IsTrue(_waiter.WaitOne(Timeout));
            ticketCreator.Verify();
            printer.Verify();
        }
    }
}
