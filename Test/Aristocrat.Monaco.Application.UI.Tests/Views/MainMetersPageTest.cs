namespace Aristocrat.Monaco.Application.UI.Tests.Views
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Threading;
    using Accounting.Contracts;
    using Contracts;
    using Contracts.Localization;
    using Contracts.MeterPage;
    using Contracts.OperatorMenu;
    using Contracts.Tickets;
    using Events;
    using Hardware.Contracts;
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
    using OperatorMenu;
    using Test.Common;
    using UI.ViewModels;

    /// <summary>
    ///     Provides tests for the MetersMainPage class
    /// </summary>
    [TestClass]
    public class MainMetersPageViewModelTest
    {
        private dynamic _accessor;

        private Mock<IEventBus> _eventBus;
        private Mock<IMeterManager> _meterManager;
        private Mock<IOperatorMenuConfiguration> _config;
        private Mock<IMeter> _meter;
        private Mock<IPrinter> _printer;
        private Mock<IPropertiesManager> _propertiesManager;
        private MainMetersPageViewModel _target;
        private readonly ManualResetEvent _waiter = new ManualResetEvent(false);
        private const int Timeout = 1000; // One second
        private const string TicketModeInspection = "Inspection";

        [TestInitialize]
        public void MyTestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
            _printer = MoqServiceManager.CreateAndAddService<IPrinter>(MockBehavior.Strict);
            _meterManager = MoqServiceManager.CreateAndAddService<IMeterManager>(MockBehavior.Strict);
            _meterManager.Setup(m => m.Meters).Returns(new List<string>());
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Loose);

            _propertiesManager.Setup(m => m.GetProperty(HardwareConstants.SimulateLcdButtonDeck, false))
                .Returns(false);
            _propertiesManager.Setup(m => m.GetProperty(HardwareConstants.SimulateVirtualButtonDeck, true))
                .Returns(false);
            _propertiesManager.Setup(m => m.GetProperty(HardwareConstants.UsbButtonDeck, false))
                .Returns(false);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.MachineId, It.IsAny<uint>()))
                .Returns((uint)1);

            _printer.SetupGet(m => m.Enabled).Returns(true);
            _printer.SetupGet(m => m.CanPrint).Returns(true);
            _printer.SetupGet(m => m.LogicalState).Returns(PrinterLogicalState.Idle);
            _printer.Setup(m => m.Print(It.IsAny<Ticket>())).Returns(Task.FromResult(true));

            // mocks for MonoAddinsHelper
            _propertiesManager.Setup(m => m.GetProperty("Mono.SelectedAddinConfigurationHashCode", null)).Returns(null);
            _propertiesManager.Setup(
                    m => m.GetProperty(ApplicationConstants.StackerRemovedBehaviorAutoClearPeriodMetersText, false))
                .Returns(true);
            _propertiesManager.Setup(m => m.GetProperty(PropertyKey.CurrentBalance, It.IsAny<long>()))
                .Returns(0L);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.AuditTicketLineLimit, 36))
                .Returns(36);

            var monitor = MoqServiceManager.CreateAndAddService<IOperatorMenuGamePlayMonitor>(MockBehavior.Strict);
            monitor.Setup(m => m.InGameRound).Returns(false);
            monitor.Setup(m => m.IsRecoveryNeeded).Returns(false);

            _meter = MoqServiceManager.CreateAndAddService<IMeter>(MockBehavior.Strict);
            _meter.Setup(m => m.Lifetime).Returns(1);

            _meterManager.Setup(m => m.GetMeter(It.IsAny<string>())).Returns(_meter.Object);
            _meterManager.Setup(m => m.IsMeterProvided(It.IsAny<string>())).Returns(true);

            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);

            _eventBus.Setup(a => a.Subscribe(It.IsAny<MainMetersPageViewModel>(), It.IsAny<Action<PeriodOrMasterButtonClickedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<MainMetersPageViewModel>(), It.IsAny<Action<ExitRequestedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<MainMetersPageViewModel>(), It.IsAny<Action<PrintButtonClickedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<MainMetersPageViewModel>(), It.IsAny<Action<BankBalanceChangedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<MainMetersPageViewModel>(), It.IsAny<Action<PrintButtonStatusEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<MainMetersPageViewModel>(), It.IsAny<Action<OperatorMenuPageLoadedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<ClosedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<OpenEvent>>()));
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<DialogClosedEvent>>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPopupEvent>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<MeterPageLoadedEvent>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuWarningMessageEvent>()));

            var time = MoqServiceManager.CreateAndAddService<ITime>(MockBehavior.Strict);
            time.Setup(m => m.GetLocationTime()).Returns(DateTime.Now);

            var doors = new Mock<IDoorService>(MockBehavior.Default);
            MoqServiceManager.AddService<IDoorService>(doors.As<IService>().Object);

            MoqServiceManager.CreateAndAddService<ICabinetDetectionService>(MockBehavior.Loose);
            //_eventBus.Setup(m => m.Publish(It.IsAny<PrintButtonStatusEvent>()));
            var device = new Mock<IDevice>();
            device.SetupGet(m => m.Protocol).Returns("Fake");
            device.SetupGet(m => m.Manufacturer).Returns("Fake");
            _printer.SetupGet(m => m.DeviceConfiguration).Returns(device.Object);

            _config = MoqServiceManager.CreateAndAddService<IOperatorMenuConfiguration>(MockBehavior.Strict);
            _config.Setup(m => m.GetAccessRuleSet(It.IsAny<MainMetersPageViewModel>())).Returns(It.IsAny<string>());
            _config.Setup(m => m.GetAccessRuleSet(It.IsAny<MainMetersPageViewModel>(), It.IsAny<string>())).Returns(It.IsAny<string>());
            _config.Setup(m => m.GetPrintAccessRuleSet(It.IsAny<MainMetersPageViewModel>())).Returns(It.IsAny<string>());
            _config.Setup(m => m.GetPrintButtonEnabled(It.IsAny<MainMetersPageViewModel>(), It.IsAny<bool>())).Returns(It.IsAny<bool>());
            _config.Setup(m => m.GetSetting(It.IsAny<string>(), It.IsAny<bool>())).Returns(It.IsAny<bool>());
            _config.Setup(m => m.GetSetting(It.IsAny<MainMetersPageViewModel>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(It.IsAny<bool>());

            var access = MoqServiceManager.CreateAndAddService<IOperatorMenuAccess>(MockBehavior.Strict);
            access.Setup(m => m.UnregisterAccessRules(It.IsAny<MainMetersPageViewModel>()));

            _target = new MainMetersPageViewModel();
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
            _eventBus.Setup(m => m.UnsubscribeAll(_target));
            try
            {
                Dispatcher.CurrentDispatcher.PumpUntilDry();
            }
            catch (Exception)
            {
                // just eat the exception since it is due to other window threads
                // not shutting down
            }

            _target.Dispose();
            _target = null;

            MoqServiceManager.RemoveInstance();
            AddinManager.Shutdown();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void GenerateEgmMetersTicket()
        {
            var ticketCreator = MoqServiceManager.CreateAndAddService<IMetersTicketCreator>(MockBehavior.Strict);
            ticketCreator.Setup(
                    m => m.CreateEgmMetersTicket(It.IsAny<List<Tuple<IMeter, string>>>(), It.IsAny<bool>()))
                .Returns(new Ticket());
            ticketCreator.Setup(
                    m => m.CreateMetersTickets(It.IsAny<List<Tuple<IMeter, string>>>(), It.IsAny<bool>()))
                .Returns(new List<Ticket>() { new Ticket() });
            List<Ticket> tickets = _accessor.GenerateTicketsForPrint(OperatorMenuPrintData.Main);
            Assert.IsTrue(tickets != null && tickets.Count == 1);
            ticketCreator.Verify();
        }

        [TestMethod]
        public void GenerateInspectionTicket()
        {
            var ticketCreator = MoqServiceManager.CreateAndAddService<IMetersTicketCreator>(MockBehavior.Strict);
            ticketCreator.Setup(
                    m => m.CreateEgmMetersTicket(It.IsAny<List<Tuple<IMeter, string>>>(), It.IsAny<bool>()))
                .Returns(new Ticket());
            ticketCreator.Setup(
                    m => m.CreateMetersTickets(It.IsAny<List<Tuple<IMeter, string>>>(), It.IsAny<bool>()))
                .Returns(new List<Ticket>() { new Ticket() });
            _target = new MainMetersPageViewModel();
            _accessor = new DynamicPrivateObject(_target);

            List<Ticket> tickets = _accessor.GenerateTicketsForPrint(OperatorMenuPrintData.Main);
            Assert.IsTrue(tickets != null && tickets.Count == 1);
            ticketCreator.Verify();
        }

        [TestMethod]
        public void PrintButtonClickedTest()
        {
            var ticketCreator = MoqServiceManager.CreateAndAddService<IMetersTicketCreator>(MockBehavior.Strict);
            ticketCreator.Setup(
                    m => m.CreateEgmMetersTicket(It.IsAny<IList<Tuple<IMeter, string>>>(), It.IsAny<bool>()))
                .Returns(new Ticket());
            ticketCreator.Setup(
                    m => m.CreateMetersTickets(It.IsAny<List<Tuple<IMeter, string>>>(), It.IsAny<bool>()))
                .Returns(new List<Ticket>() { new Ticket() })
                .Verifiable();

            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<PrintButtonStatusEvent>>())).Verifiable();
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<ClosedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<OpenEvent>>()));
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<DialogClosedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<OperatorMenuPrintJobStartedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<OperatorMenuPrintJobCompletedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<OperatorCultureChangedEvent>>()));

            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPageLoadedEvent>())).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPrintJobStartedEvent>())).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<LampTestLampStateEvent>()));

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

            _target.LoadedCommand.Execute(null);
            Assert.IsTrue(_waiter.WaitOne(Timeout));
            _waiter.Reset();
            onPrinterClicked(new PrintButtonClickedEvent());

            Assert.IsTrue(_waiter.WaitOne(Timeout));
            ticketCreator.Verify();
            _eventBus.Verify();
            _printer.Verify();
        }

        [TestMethod]
        public void ClearPeriodClicked()
        {
            var ticketCreator = MoqServiceManager.CreateAndAddService<IPeriodicResetTicketCreator>(MockBehavior.Strict);
            var dialogService = MoqServiceManager.CreateAndAddService<IDialogService>(MockBehavior.Strict);
            var ticket = new Ticket();
            ticket["left"] = "test";
            ticket["center"] = "test";
            ticket["right"] = "test";
            _meterManager.Setup(m => m.ClearAllPeriodMeters()).Callback(() => _waiter.Set()).Verifiable();
            dialogService.Setup(
                m => m.ShowYesNoDialog(
                    It.IsAny<INotifyPropertyChanged>(),
                    It.IsAny<string>(),
                    It.IsAny<string>())).Returns(true);
            ticketCreator.Setup(m => m.Create()).Returns(ticket).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPrintJobStartedEvent>())).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPrintJobEvent>())).Verifiable();

            _accessor.ClearPeriodCommand.Execute(null);

            Assert.IsTrue(_waiter.WaitOne(Timeout));

            ticketCreator.Verify();
            _meterManager.Verify();
            _eventBus.Verify();
        }

        [TestMethod]
        public void PrintVerificationButtonClicked()
        {
            var ticketCreator = MoqServiceManager.CreateAndAddService<IVerificationTicketCreator>(MockBehavior.Strict);
            ticketCreator.Setup(m => m.Create(It.IsAny<int>(), null)).Returns(new Ticket()).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPrintJobStartedEvent>())).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPrintJobEvent>())).Callback(() => _waiter.Set()).Verifiable();
            _accessor.PrintVerificationButtonClickedCommand.Execute(null);
            Assert.IsTrue(_waiter.WaitOne(Timeout));
            ticketCreator.Verify();
            _eventBus.Verify();
        }

        [TestMethod]
        public void PrintPeriodicResetButtonClicked()
        {
            var ticket = new Ticket();
            ticket["left"] = "test";
            ticket["center"] = "test";
            ticket["right"] = "test";
            var ticketCreator = MoqServiceManager.CreateAndAddService<IPeriodicResetTicketCreator>(MockBehavior.Strict);
            ticketCreator.Setup(m => m.Create()).Returns(ticket).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPrintJobStartedEvent>())).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPrintJobEvent>())).Callback(() => _waiter.Set()).Verifiable();
            _accessor.PrintPeriodicResetButtonClickedCommand.Execute(null);
            Assert.IsTrue(_waiter.WaitOne(Timeout));
            ticketCreator.Verify();
            _eventBus.Verify();
        }

        [TestMethod]
        public void PageLoadedPrintButtonEnabledTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<PageTitleEvent>())).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<PrintButtonStatusEvent>()))
                .Callback((IEvent p) => Assert.IsTrue(((PrintButtonStatusEvent)p).Enabled));
            //_propertiesManager.Setup(
            //        m => m.GetProperty(ApplicationConstants.OperatorMenuMetersScreenPrintButtonVisibility, false))
            //    .Returns(true);

            // make a small window that holds the page
            // This is needed since the Page_Loaded method sets the window title
            // The window will display briefly during the test
            Window win = new Window
            {
                Content = _target,
                Width = 10,
                Height = 10
            };
            win.Show();
            win.Dispatcher.PumpUntilDry();

            // The Page_Loaded function will be called as part of the window.Show()
            win.Close();
        }

        [TestMethod]
        public void PageLoadedPrintButtonDisabledTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<PageTitleEvent>())).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<PrintButtonStatusEvent>()))
                .Callback((IEvent p) => Assert.IsFalse(((PrintButtonStatusEvent)p).Enabled));
            //_propertiesManager.Setup(
            //        m => m.GetProperty(ApplicationConstants.OperatorMenuMetersScreenPrintButtonVisibility, false))
            //    .Returns(false);

            // make a small window that holds the page
            // This is needed since the Page_Loaded method sets the window title
            // The window will display briefly during the test
            Window win = new Window
            {
                Content = _target,
                Width = 10,
                Height = 10
            };
            win.Show();
            win.Dispatcher.PumpUntilDry();

            // The Page_Loaded function will be called as part of the window.Show()
            win.Close();
        }

        [TestMethod]
        public void UnloadedCommandTest()
        {
            _eventBus.Setup(m => m.UnsubscribeAll(_target)).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPageLoadedEvent>())).Verifiable();
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<ClosedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<OpenEvent>>()));
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<DialogClosedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<OperatorMenuPrintJobStartedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<OperatorMenuPrintJobCompletedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<OperatorCultureChangedEvent>>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<LampTestLampStateEvent>()));
            _accessor.LoadedCommand.Execute(null);
            _accessor.UnloadedCommand.Execute(null);
            _eventBus.Verify();
        }
    }
}
