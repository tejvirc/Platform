namespace Aristocrat.Monaco.Application.UI.Tests.Views
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Windows;
    using Accounting.Contracts;
    using Contracts;
    using Contracts.MeterPage;
    using Contracts.OperatorMenu;
    using Events;
    using Hardware.Contracts;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.IO;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.SharedDevice;
    using Kernel;
    using Kernel.Contracts;
    using Loaders;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using OperatorMenu;
    using Test.Common;
    using UI.ViewModels;
    using Vgt.Client12.Application.OperatorMenu;
    using EnabledEvent = Hardware.Contracts.Printer.EnabledEvent;

    /// <summary>
    ///     Tests for MetersScreen
    /// </summary>
    [TestClass]
    public class MetersMainPageTest
    {
        public static readonly RoutedEvent TestRoutedEvent = EventManager.RegisterRoutedEvent(
            "TestEvent",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(MetersMainPageViewModel));

        private dynamic _accessor;
        private Mock<IEventBus> _eventBus;

        private Mock<IMeterManager> _meterManager;
        private Mock<IMeter> _meter;
        private Mock<IPrinter> _printer;
        private Mock<IPropertiesManager> _propertiesManager;
        private MetersMainPageViewModel _target;
        private Mock<ITime> _time;
        private const int Timeout = 1000; // One second
        private ManualResetEvent _waiter;

        // Use TestInitialize to run code before running each test 
        [TestInitialize]
        public void MyTestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());

            _waiter = new ManualResetEvent(false);

            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
            _meter = MoqServiceManager.CreateAndAddService<IMeter>(MockBehavior.Strict);
            _meter.Setup(m => m.Lifetime).Returns(1);

            _meterManager = MoqServiceManager.CreateAndAddService<IMeterManager>(MockBehavior.Strict);
            _meterManager.Setup(m => m.LastMasterClear).Returns(DateTime.MaxValue);
            _meterManager.Setup(m => m.LastPeriodClear).Returns(DateTime.MaxValue);
            _meterManager.Setup(m => m.GetMeter(It.IsAny<string>())).Returns(_meter.Object);
            _meterManager.Setup(m => m.IsMeterProvided(It.IsAny<string>())).Returns(true);

            _time = MoqServiceManager.CreateAndAddService<ITime>(MockBehavior.Strict);
            _time.Setup(m => m.GetLocationTime(It.IsAny<DateTime>())).Returns(DateTime.MaxValue);
            _time.Setup(m => m.TimeZoneInformation).Returns(TimeZoneInfo.Utc);

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Loose);
            _propertiesManager.Setup(m => m.GetProperty("Mono.SelectedAddinConfigurationHashCode", null)).Returns(null);
            _propertiesManager.Setup(m => m.GetProperty("OperatorMenu.MetersScreen.PrintButton.Visibility", false)).Returns(false);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.OccurrenceMeterRolloverText, It.IsAny<int>()))
                .Returns(100L);
            _propertiesManager.Setup(m => m.GetProperty(PropertyKey.CurrentBalance, It.IsAny<long>()))
                .Returns(0L);
            _propertiesManager.Setup(m => m.GetProperty(HardwareConstants.SimulateLcdButtonDeck, false))
                .Returns(false);
            _propertiesManager.Setup(m => m.GetProperty(HardwareConstants.SimulateVirtualButtonDeck, true))
                .Returns(false);
            _propertiesManager.Setup(m => m.GetProperty(HardwareConstants.UsbButtonDeck, false))
                .Returns(false);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ShowMode, false)).Returns(false);

            var monitor = MoqServiceManager.CreateAndAddService<IOperatorMenuGamePlayMonitor>(MockBehavior.Strict);
            monitor.Setup(m => m.InGameRound).Returns(false);
            monitor.Setup(m => m.IsRecoveryNeeded).Returns(false);

            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _eventBus.Setup(m => m.Publish(It.IsAny<MetersOperatorMenuEnteredEvent>()));
            SetupEventSubscriptions();

            _printer = new Mock<IPrinter>(MockBehavior.Strict);
            _printer.As<IDeviceService>();
            _printer.As<IDeviceService>().Setup(m => m.ReasonDisabled).Returns(DisabledReasons.Error);
            _printer.As<IDeviceService>().Setup(m => m.LastError).Returns(string.Empty);
            _printer.As<IDeviceService>().SetupGet(m => m.Enabled).Returns(false);
            MoqServiceManager.AddService<IPrinter>(_printer.As<IService>().Object);
            _printer.Setup(m => m.LogicalState).Returns(PrinterLogicalState.Idle);

            var doors = new Mock<IDoorService>(MockBehavior.Default);
            MoqServiceManager.AddService<IDoorService>(doors.As<IService>().Object);

            MoqServiceManager.CreateAndAddService<ICabinetDetectionService>(MockBehavior.Loose);

            var access = MoqServiceManager.CreateAndAddService<IOperatorMenuAccess>(MockBehavior.Strict);
            access.Setup(m => m.UnregisterAccessRules(It.IsAny<OperatorMenuPageViewModelBase>()));
            access.Setup(m => m.UnregisterAccessRules(It.IsAny<MainMetersPageLoader>()));

            var config = MoqServiceManager.CreateAndAddService<IOperatorMenuConfiguration>(MockBehavior.Strict);
            config.Setup(m => m.GetAccessRuleSet(It.IsAny<MetersMainPageViewModel>())).Returns(It.IsAny<string>());
            config.Setup(m => m.GetAccessRuleSet(It.IsAny<MetersMainPageViewModel>(), It.IsAny<string>())).Returns(It.IsAny<string>());
            config.Setup(m => m.GetPrintAccessRuleSet(It.IsAny<MetersMainPageViewModel>())).Returns(It.IsAny<string>());
            config.Setup(m => m.GetPrintButtonEnabled(It.IsAny<MetersMainPageViewModel>(), It.IsAny<bool>())).Returns(It.IsAny<bool>());
            config.Setup(m => m.GetSetting(It.IsAny<MainMetersPageViewModel>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(It.IsAny<bool>());

            config.Setup(m => m.GetSetting(It.IsAny<string>(), It.IsAny<bool>())).Returns(It.IsAny<bool>());
            config.Setup(m => m.GetSetting(It.IsAny<MetersMainPageViewModel>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(It.IsAny<bool>());

            _target = new MetersMainPageViewModel(null);

            _accessor = new DynamicPrivateObject(_target);

            var _buttonService = MoqServiceManager.CreateAndAddService<IButtonService>(MockBehavior.Strict);
            _buttonService.Setup(m => m.IsTestModeActive).Returns(It.IsAny<bool>());

            var _iio = MoqServiceManager.CreateAndAddService<IIO>(MockBehavior.Strict);
            _iio.Setup(m => m.SetButtonLamp(It.IsAny<uint>(), It.IsAny<bool>())).Returns(It.IsAny<uint>());
            _iio.Setup(m => m.SetButtonLampByMask(It.IsAny<uint>(), It.IsAny<bool>())).Returns(It.IsAny<uint>());
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup]
        public void MyTestCleanup()
        {
            //_testWindow?.Close();
            _eventBus.Setup(m => m.Publish(It.IsAny<MetersOperatorMenuExitedEvent>()));
            _eventBus.Setup(m => m.UnsubscribeAll(_target));
            _target.Dispose();
            try
            {
                //_testWindow?.Dispatcher.PumpUntilDry();
            }
            catch (Exception)
            {
                // just eat any exceptions since they are after the test has finished.
            }

            AddinManager.Shutdown();
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void MetersScreen_UnloadedTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<MetersOperatorMenuExitedEvent>())).Verifiable();
            _eventBus.Setup(m => m.UnsubscribeAll(_target));

            _accessor.OnUnloaded();
        }

        [TestMethod]
        public void PeriodOrMasterButtonClickedTest()
        {
            _meterManager.Setup(m => m.Meters).Returns(new List<string>());
            string pageTitle = string.Empty;
            bool master = false;
            var meter = new Mock<IMeter>(MockBehavior.Strict);
            meter.SetupGet(m => m.Lifetime).Returns(0);
            MeterClassification classification = new OccurrenceMeterClassification();
            meter.Setup(m => m.Classification).Returns(classification);
            meter.SetupGet(m => m.Period).Returns(0);

            if (Application.Current == null)
            {
                new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            }

            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<BankBalanceChangedEvent>>())).Verifiable();
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<PrintButtonStatusEvent>>())).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPageLoadedEvent>())).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPrintJobStartedEvent>())).Verifiable();

            _eventBus.Setup(m => m.Publish(It.IsAny<PageTitleEvent>()))
                .Callback<PageTitleEvent>(
                    e =>
                    {
                        pageTitle = e.Content;
                        _waiter.Set();
                    });

            _eventBus.Setup(m => m.Publish(It.IsAny<PeriodOrMasterButtonClickedEvent>()))
                .Callback<PeriodOrMasterButtonClickedEvent>(
                    e =>
                    {
                        master = e.MasterClicked;
                        _waiter.Set();
                    });

            _target.SelectedPage = new MainMetersPageLoader();
            _target.LoadedCommand.Execute(null);

            _target.IsPeriodMasterButtonChecked = true;
            _target.PeriodMasterButtonClickedCommand.Execute(null);
            Assert.IsTrue(_waiter.WaitOne(Timeout));
            Assert.IsTrue(pageTitle.Contains("Master"));
            Assert.IsTrue(master);
            _waiter.Reset();
            _target.IsPeriodMasterButtonChecked = false;
            _target.PeriodMasterButtonClickedCommand.Execute(null);
            Assert.IsTrue(_waiter.WaitOne(Timeout));
            Assert.IsTrue(pageTitle.Contains("Period"));
            Assert.IsFalse(master);
            meter.Verify();
        }

        [TestMethod]
        public void PeriodMetersDateTimeChangeRequestEventTest()
        {
            var clearPeriodMetersDate = DateTime.UtcNow;
            var clearPeriodMetersDateAsString = clearPeriodMetersDate.ToString(ApplicationConstants.DefaultDateTimeFormat, CultureInfo.CurrentCulture);
            _time.Setup(m => m.GetFormattedLocationTime(clearPeriodMetersDate, It.IsAny<string>())).Returns(clearPeriodMetersDateAsString);
            _target.IsPeriodMasterButtonChecked = false;
            _target.SelectedPage = new MainMetersPageLoader();
            _target.Pages.Add(_target.SelectedPage);
            var evt = new PeriodMetersDateTimeChangeRequestEvent(_target.SelectedPage.PageName, clearPeriodMetersDate);
            _accessor.HandlePeriodMetersDateTimeChangeRequestEvent(evt);
            Assert.IsTrue(_target.CurrentPageHeader.Contains(clearPeriodMetersDateAsString));
        }

        private void SetupEventSubscriptions()
        {
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<ExitRequestedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<PeriodMetersClearedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<PrintButtonClickedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<PrintStartedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<PrintCompletedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<InspectedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<HardwareFaultClearEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<HardwareFaultEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<ConnectedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<DisconnectedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<InspectionFailedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<ResolverErrorEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<LoadingRegionsAndTemplatesEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<PageTitleEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<EnabledEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<ClosedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<OpenEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<PeriodMetersDateTimeChangeRequestEvent>>()));
        }
    }
}
