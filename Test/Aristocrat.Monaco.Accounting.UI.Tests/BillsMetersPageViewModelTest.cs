namespace Aristocrat.Monaco.Accounting.UI.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.MeterPage;
    using Application.Contracts.OperatorMenu;
    using Application.Contracts.Tickets;
    using Application.UI.Events;
    using Application.Contracts.Currency;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Aristocrat.Monaco.Hardware.Contracts.Button;
    using Aristocrat.Monaco.Hardware.Contracts.IO;
    using Aristocrat.Monaco.UI.Common.Events;
    using Contracts;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.SharedDevice;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Kernel.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;
    using UI.ViewModels;

    /// <summary>
    ///     Summary description for BillsMetersPageViewModelTest
    /// </summary>
    [TestClass]
    public class BillsMetersPageViewModelTest
    {
        private Mock<IEventBus> _eventBus;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<INoteAcceptor> _noteAcceptor;
        private Mock<IMeterManager> _meterManager;
        private Mock<IMeterProvider> _meterProvider;
        private Mock<IMeter> _meter;
        private readonly Mock<ILocalizer> _localizer = new Mock<ILocalizer>();
        private Mock<ILocalizerFactory> _localizerFactory;
        private BillsMetersPageViewModel _target;
        private Mock<IOperatorMenuConfiguration> _config;
        private readonly ManualResetEvent _waiter = new ManualResetEvent(false);
        private const int Timeout = 1000;
        private const string PageName = "Bills";

        // Use TestInitialize to run code before running each test 
        [TestInitialize]
        public void MyTestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);
            _propertiesManager.Setup(
                    m => m.GetProperty(ApplicationConstants.StackerRemovedBehaviorAutoClearPeriodMetersText, false))
                .Returns(true);
            _propertiesManager.Setup(
                m => m.GetProperty(AccountingConstants.BillClearanceEnabled, false))
            .Returns(false);
            _propertiesManager.Setup(m => m.GetProperty("Denominations", null)).Returns(new Collection<int> { 1, 5, 10, 20 });
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMeterRolloverText, It.IsAny<long>()))
                .Returns(100L);
            _propertiesManager.Setup(m => m.GetProperty(PropertyKey.CurrentBalance, It.IsAny<long>()))
                .Returns(0L);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ShowMode, false)).Returns(false);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);

            _noteAcceptor = MoqServiceManager.CreateAndAddService<INoteAcceptor>(MockBehavior.Strict);
            _noteAcceptor.Setup(a => a.GetSupportedNotes(It.IsAny<String>())).Returns(new Collection<int> { 1, 5, 10, 20 });

            _meter = MoqServiceManager.CreateAndAddService<IMeter>(MockBehavior.Strict);
            _meter.Setup(m => m.Lifetime).Returns(0);

            _meterProvider = MoqServiceManager.CreateAndAddService<IMeterProvider>(MockBehavior.Strict);
            _meterProvider.Setup(m => m.RegisterMeterClearDelegate(It.IsAny<ClearPeriodMeter>())).Verifiable();

            _meterManager = MoqServiceManager.CreateAndAddService<IMeterManager>(MockBehavior.Strict);
            _meterManager.Setup(m => m.GetMeter(It.IsAny<string>())).Returns(_meter.Object);
            _meterManager.Setup(m => m.IsMeterProvided(It.IsAny<string>())).Returns(true);
            _meterManager.Setup(m => m.ClearPeriodMeters(It.IsAny<string>()));
            var clearDateTime = DateTime.UtcNow;
            _meterManager.Setup(m => m.GetPeriodMetersClearanceDate(It.IsAny<string>())).Returns(clearDateTime);
            MoqServiceManager.Instance.Setup(m => m.GetService<IMeterManager>()).Returns(_meterManager.Object);

            string cultureName = "en-US";
            CultureInfo culture = new CultureInfo(cultureName);

            _localizerFactory = MoqServiceManager.CreateAndAddService<ILocalizerFactory>(MockBehavior.Default);
            _localizerFactory.Setup(m => m.For(It.IsAny<string>())).Returns<string>(
            name =>
            {
                var localizer = new Mock<ILocalizer>();
                localizer.Setup(m => m.CurrentCulture).Returns(culture);
                localizer.Setup(m => m.GetString(It.IsAny<string>())).Returns<string>(s => s);
                return localizer.Object;
            });

            string minorUnitSymbol = "c";

            RegionInfo region = new RegionInfo(cultureName);
            CurrencyExtensions.Currency = new Currency(region.ISOCurrencySymbol, region, culture, minorUnitSymbol);

            CurrencyExtensions.SetCultureInfo(null, null, true, true, minorUnitSymbol);

            var monitor = MoqServiceManager.CreateAndAddService<IOperatorMenuGamePlayMonitor>(MockBehavior.Strict);
            monitor.Setup(m => m.InGameRound).Returns(false);
            monitor.Setup(m => m.IsRecoveryNeeded).Returns(false);

            var doors = new Mock<IDoorService>(MockBehavior.Default);
            MoqServiceManager.AddService<IDoorService>(doors.As<IService>().Object);

            MoqServiceManager.CreateAndAddService<ICabinetDetectionService>(MockBehavior.Loose);

            _config = MoqServiceManager.CreateAndAddService<IOperatorMenuConfiguration>(MockBehavior.Default);

            var access = MoqServiceManager.CreateAndAddService<IOperatorMenuAccess>(MockBehavior.Strict);
            access.Setup(m => m.UnregisterAccessRules(It.IsAny<BillsMetersPageViewModel>()));

            var _buttonService = MoqServiceManager.CreateAndAddService<IButtonService>(MockBehavior.Strict);
            _buttonService.Setup(m => m.IsTestModeActive).Returns(It.IsAny<bool>());

            var _iio = MoqServiceManager.CreateAndAddService<IIO>(MockBehavior.Strict);
            _iio.Setup(m => m.SetButtonLamp(It.IsAny<uint>(), It.IsAny<bool>())).Returns(It.IsAny<uint>());
            _iio.Setup(m => m.SetButtonLampByMask(It.IsAny<uint>(), It.IsAny<bool>())).Returns(It.IsAny<uint>());

            if (Application.Current == null)
            {
                new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            }

            _target = new BillsMetersPageViewModel(PageName);

            _eventBus.Setup(a => a.Subscribe(_target, It.IsAny<Action<PeriodOrMasterButtonClickedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<ClosedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<OpenEvent>>()));
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<DialogClosedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<OperatorCultureChangedEvent>>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPopupEvent>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<MeterPageLoadedEvent>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuWarningMessageEvent>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<PeriodMetersDateTimeChangeRequestEvent>()));
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
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<PrintButtonStatusEvent>>())).Verifiable();
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<PrintButtonClickedEvent>>())).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPageLoadedEvent>())).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<LampTestLampStateEvent>()));
            _target.LoadedCommand.Execute(null);
            _eventBus.Verify();
        }

        /// <summary> This test is no longer just for statement coverage </summary>
        [TestMethod]
        public void PrintButtonClickedTest()
        {
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<PrintButtonStatusEvent>>())).Verifiable();
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<PrintButtonClickedEvent>>())).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPageLoadedEvent>())).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<LampTestLampStateEvent>()));
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
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<OperatorMenuPrintJobStartedEvent>>())).Verifiable();
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

            _target.LoadedCommand.Execute(null);

            onPrinterClicked(new PrintButtonClickedEvent());

            Assert.IsTrue(_waiter.WaitOne(Timeout));
            ticketCreator.Verify();
            printer.Verify();
        }

        private void BillClearanceIsEnabled(bool value)
        {
            _propertiesManager.Setup(
                m => m.GetProperty(AccountingConstants.BillClearanceEnabled, false))
            .Returns(value);
        }

        [TestMethod]
        public void BillClearanceDisabledTest()
        {
            BillClearanceIsEnabled(false);
            _target = new BillsMetersPageViewModel(PageName);
            _eventBus.Verify(e => e.Publish(It.IsAny<PeriodMetersDateTimeChangeRequestEvent>()), Times.Never);
            Assert.IsFalse(_target.BillClearanceEnabled);
        }

        [TestMethod]
        public void BillClearanceClickedYesTest()
        {
            BillClearanceIsEnabled(true);
            _target = new BillsMetersPageViewModel(PageName);
            var dialogService = MoqServiceManager.CreateAndAddService<IDialogService>(MockBehavior.Strict);
            _eventBus.Setup(e => e.Publish(It.IsAny<PeriodMetersDateTimeChangeRequestEvent>())).Verifiable();
            dialogService.Setup(d => d.ShowYesNoDialog(It.IsAny<BillsMetersPageViewModel>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true).Verifiable();
            _meterManager.Setup(m => m.ClearPeriodMeters(typeof(CurrencyInMetersProvider).ToString())).Verifiable();
            _eventBus.Setup(e => e.Publish(It.IsAny<PeriodMetersDateTimeChangeRequestEvent>())).Verifiable();
            _target.BillClearanceButtonClickedCommand.Execute(null);
            dialogService.Verify();
            _eventBus.Verify();
            _meterManager.Verify();
        }

        [TestMethod]
        public void BillClearanceClickedNoTest()
        {
            BillClearanceIsEnabled(true);
            _target = new BillsMetersPageViewModel(PageName);
            var dialogService = MoqServiceManager.CreateAndAddService<IDialogService>(MockBehavior.Strict);
            dialogService.Setup(d => d.ShowYesNoDialog(It.IsAny<BillsMetersPageViewModel>(), It.IsAny<string>(), It.IsAny<string>())).Returns(false).Verifiable();
            _target.BillClearanceButtonClickedCommand.Execute(null);
            _meterManager.Verify(m => m.ClearPeriodMeters(It.IsAny<string>()), Times.Never());
            _eventBus.Verify(e => e.Publish(It.IsAny<PeriodMetersDateTimeChangeRequestEvent>()), Times.Never());
            dialogService.Verify();
        }
    }
}
