namespace Aristocrat.Monaco.Application.UI.Tests.Views
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Threading;
    using Accounting.Contracts;
    using Cabinet.Contracts;
    using Contracts;
    using Contracts.ConfigWizard;
    using Contracts.Localization;
    using Contracts.MeterPage;
    using Contracts.OperatorMenu;
    using Contracts.Tickets;
    using Events;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.IO;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Kernel.Contracts;
    using MeterPage;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.UI.Common.Events;
    using Mono.Addins;
    using Moq;
    using Test.Common;
    using UI.ViewModels;
    using IDevice = Hardware.Contracts.SharedDevice.IDevice;

    /// <summary>
    ///     Summary description for HardwareMeterScreenTest
    /// </summary>
    [TestClass]
    public class HardwareMetersPageViewModelTest
    {
        private dynamic _accessor;
        private Mock<IEventBus> _eventBus;
        private Mock<IMeter> _meter;
        private Mock<IMeterManager> _meterManager;
        private Mock<IPropertiesManager> _propertiesManager;
        private HardwareMetersPageViewModel _target;
        private readonly ManualResetEvent _waiter = new ManualResetEvent(false);
        private const int Timeout = 2000;

        // Use TestInitialize to run code before running each test 
        [TestInitialize]
        public void MyTestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);
            _propertiesManager.Setup(mock => mock.GetProperty("Mono.SelectedAddinConfigurationHashCode", null))
                .Returns(null);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.OccurrenceMeterRolloverText, It.IsAny<int>()))
                .Returns(100L);
            _propertiesManager.Setup(m => m.GetProperty("OperatorMenu.MetersScreen.PrintButton.Visibility", false)).Returns(false);
            _propertiesManager.Setup(m => m.GetProperty(PropertyKey.CurrentBalance, It.IsAny<long>()))
                .Returns(0L);

            MeterClassification classification = new OccurrenceMeterClassification();
            _meter = new Mock<IMeter>(MockBehavior.Strict);
            _meterManager = MoqServiceManager.CreateAndAddService<IMeterManager>(MockBehavior.Strict);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            _eventBus.Setup(a => a.Subscribe(It.IsAny<HardwareMetersPageViewModel>(), It.IsAny<Action<PeriodOrMasterButtonClickedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<ClosedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<OpenEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<DialogClosedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<OperatorCultureChangedEvent>>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPopupEvent>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<MeterPageLoadedEvent>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuWarningMessageEvent>()));

            _meter.SetupGet(m => m.Lifetime).Returns(0);
            _meter.Setup(m => m.Classification).Returns(classification);

            var monitor = MoqServiceManager.CreateAndAddService<IOperatorMenuGamePlayMonitor>(MockBehavior.Strict);
            monitor.Setup(m => m.InGameRound).Returns(false);
            monitor.Setup(m => m.IsRecoveryNeeded).Returns(false);

            var doors = new Mock<IDoorService>(MockBehavior.Default);
            MoqServiceManager.AddService<IDoorService>(doors.As<IService>().Object);
            MoqServiceManager.CreateAndAddService<ICabinetDetectionService>(MockBehavior.Loose);

            var config = MoqServiceManager.CreateAndAddService<IOperatorMenuConfiguration>(MockBehavior.Strict);
            config.Setup(m => m.GetAccessRuleSet(It.IsAny<HardwareMetersPageViewModel>())).Returns(It.IsAny<string>());
            config.Setup(m => m.GetAccessRuleSet(It.IsAny<HardwareMetersPageViewModel>(), It.IsAny<string>())).Returns(It.IsAny<string>());
            config.Setup(m => m.GetPrintAccessRuleSet(It.IsAny<HardwareMetersPageViewModel>())).Returns(It.IsAny<string>());
            config.Setup(m => m.GetPrintButtonEnabled(It.IsAny<HardwareMetersPageViewModel>(), It.IsAny<bool>())).Returns(It.IsAny<bool>());
            config.Setup(m => m.GetSetting(It.IsAny<string>(), It.IsAny<bool>())).Returns(It.IsAny<bool>());

            var access = MoqServiceManager.CreateAndAddService<IOperatorMenuAccess>(MockBehavior.Strict);

            _meterManager.Setup(m => m.GetMeter(It.IsAny<string>())).Returns(_meter.Object);
            _meterManager.Setup(m => m.Meters).Returns(new List<string>());
            _meterManager.Setup(m => m.IsMeterProvided(It.IsAny<string>())).Returns(true);
            _target = new HardwareMetersPageViewModel();
            _accessor = new DynamicPrivateObject(_target);

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
            try
            {
                Dispatcher.CurrentDispatcher.PumpUntilDry();
            }
            catch (Exception)
            {
                // just eat the exception since it is due to other window threads
                // not shutting down
            }

            _target = null;
            _accessor = null;

            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_target);
        }

        /// <summary> This test is no longer just for statement coverage </summary>
        [TestMethod]
        public void PrintButtonClickedTest()
        {
            SetupCabinetMock();

            var ticketCreator = MoqServiceManager.CreateAndAddService<IMetersTicketCreator>(MockBehavior.Strict);
            ticketCreator.Setup(m => m.CreateEgmMetersTicket(It.IsAny<IList<Tuple<IMeter, string>>>(), It.IsAny<bool>()))
                .Returns(new Ticket())
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
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<BankBalanceChangedEvent>>())).Verifiable();
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<PrintButtonStatusEvent>>())).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPageLoadedEvent>())).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPrintJobStartedEvent>())).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<LampTestLampStateEvent>()));

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

            _target.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "IsLoadingData")
                {
                    if (!_target.IsLoadingData)
                    {
                        _waiter.Set();
                    }
                }
            };

            MoqServiceManager.CreateAndAddService<ICabinetDetectionService>(MockBehavior.Loose);
            MoqServiceManager.CreateAndAddService<IDialogService>(MockBehavior.Strict);
            MoqServiceManager.CreateAndAddService<IInspectionService>(MockBehavior.Strict);

            _target.LoadedCommand.Execute(null);
            Assert.IsTrue(_waiter.WaitOne(Timeout));
            _waiter.Reset();
            onPrinterClicked(new PrintButtonClickedEvent());

            Assert.IsTrue(_waiter.WaitOne(Timeout));
            ticketCreator.Verify();
            printer.Verify();
        }

        private static void SetupCabinetMock()
        {
            var cabinetService = MoqServiceManager.CreateAndAddService<ICabinetDetectionService>(MockBehavior.Strict);
            cabinetService.SetupGet(m => m.ExpectedDisplayDevices).Returns(
                new List<IDisplayDevice> { new Mock<IDisplayDevice>().Object, new Mock<IDisplayDevice>().Object });
            cabinetService.SetupGet(m => m.ExpectedTouchDevices).Returns(
                new List<ITouchDevice> { new Mock<ITouchDevice>().Object, new Mock<ITouchDevice>().Object, });
            cabinetService.Setup(m => m.GetDisplayDeviceByItsRole(DisplayRole.VBD)).Returns(null as IDisplayDevice);
        }

        [TestMethod]
        public void PeriodOrMasterButtonClickedTest()
        {
            SetupCabinetMock();

            _meter.SetupGet(m => m.Period).Returns(0);

            if (Application.Current == null)
            {
                new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            }

            _target.Meters.CollectionChanged += (s, e) => { _waiter.Set(); };

            Action<PeriodOrMasterButtonClickedEvent> onButtonClicked = null;
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<BankBalanceChangedEvent>>())).Verifiable();
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<PrintButtonStatusEvent>>())).Verifiable();
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<PrintButtonClickedEvent>>())).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPageLoadedEvent>())).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPrintJobStartedEvent>())).Verifiable();

            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<PeriodOrMasterButtonClickedEvent>>()))
                .Callback<object, Action<PeriodOrMasterButtonClickedEvent>>(
                    (tar, act) =>
                    {
                        onButtonClicked = act;
                    });

            _eventBus.Setup(m => m.Publish(It.IsAny<LampTestLampStateEvent>()));

            _target.LoadedCommand.Execute(null);

            onButtonClicked(new PeriodOrMasterButtonClickedEvent(false));
            // Have to do checks on background thread since UI thread will signal the _waiter
            Task.Run(() =>
                {
                    Assert.IsTrue(_waiter.WaitOne(Timeout));
                    Assert.IsFalse(_target.ShowLifetime);
                    _waiter.Reset();
                    onButtonClicked(new PeriodOrMasterButtonClickedEvent(true));
                    Assert.IsTrue(_waiter.WaitOne(Timeout));
                    Assert.IsTrue(_target.ShowLifetime);
                });
        }

        [TestMethod]
        public void PageLoadedTest()
        {
            _meter.SetupGet(m => m.Lifetime).Returns(100);

            _target.Meters.Clear();
            _target.Meters.Add(new DisplayMeter("Main Door Opened", _meter.Object, true));

            Assert.AreEqual("Main Door Opened", _target.Meters[0].Name);
            Assert.AreEqual("100", _target.Meters[0].Value);
        }

        [TestMethod]
        public void PageUnloadedTestWithMeterNotProvided()
        {
            // redo the MeterManager mock to return the test meter
            _meterManager.Setup(m => m.GetMeter(It.IsAny<string>())).Throws(new MeterNotFoundException());

            // nothing to check since this path just logs the exception
        }
    }
}