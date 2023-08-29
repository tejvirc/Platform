namespace Aristocrat.Monaco.Accounting.UI.Tests
{
    #region Using

    using System;
    using System.Collections.Generic;
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

    #endregion

    /// <summary>
    ///     Summary description for VoucherMetersPageViewModelTest
    /// </summary>
    [TestClass]
    public class VoucherMetersPageViewModelTest
    {
        private Mock<IEventBus> _eventBus;
        private Mock<IPropertiesManager> _propertiesManager;

        private Mock<IMeterManager> _meterManager;
        private Mock<IMeterProvider> _meterProvider;
        private Mock<IMeter> _meter;
        private Mock<IServiceManager> _serviceManager;
        private Mock<ILocalizerFactory> _localizerFactory;
        private VoucherMetersPageViewModel _target;
        private readonly ManualResetEvent _waiter = new ManualResetEvent(false);
        private const int Timeout = 1000;

        // Use TestInitialize to run code before running each test 
        [TestInitialize]
        public void MyTestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();

            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);

            _serviceManager = MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _serviceManager.Setup(mock => mock.TryGetService<IOperatorMenuConfiguration>()).Returns((IOperatorMenuConfiguration)null);
            _serviceManager.Setup(mock => mock.GetService<ILocalizerFactory>()).Returns((ILocalizerFactory)null);

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);
            _propertiesManager.Setup(
                    m => m.GetProperty(ApplicationConstants.StackerRemovedBehaviorAutoClearPeriodMetersText, false))
                .Returns(true);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMultiplierKey, ApplicationConstants.DefaultCurrencyMultiplier)).Returns(1.0);
            _propertiesManager.Setup(m => m.GetProperty("System.VoucherIn", true)).Returns(true);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMeterRolloverText, It.IsAny<long>()))
                .Returns(100L);
            _propertiesManager.Setup(m => m.GetProperty(PropertyKey.CurrentBalance, It.IsAny<long>()))
                .Returns(0L);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ShowMode, false)).Returns(false);

            _localizerFactory = MoqServiceManager.CreateAndAddService<ILocalizerFactory>(MockBehavior.Strict);
            _localizerFactory.Setup(m => m.For(It.IsAny<string>())).Returns<string>(
                name =>
                {
                    var localizer = new Mock<ILocalizer>();
                    localizer.Setup(m => m.CurrentCulture).Returns(new CultureInfo("en-US"));
                    localizer.Setup(m => m.GetString(It.IsAny<string>())).Returns<string>(s => s);
                    return localizer.Object;
                });

            var monitor = MoqServiceManager.CreateAndAddService<IOperatorMenuGamePlayMonitor>(MockBehavior.Strict);
            monitor.Setup(m => m.InGameRound).Returns(false);
            monitor.Setup(m => m.IsRecoveryNeeded).Returns(false);

            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);

            _meter = MoqServiceManager.CreateAndAddService<IMeter>(MockBehavior.Strict);
            _meter.Setup(m => m.Lifetime).Returns(0);

            _meterProvider = MoqServiceManager.CreateAndAddService<IMeterProvider>(MockBehavior.Strict);
            _meterProvider.Setup(m => m.RegisterMeterClearDelegate(It.IsAny<ClearPeriodMeter>())).Verifiable();

            _meterManager = MoqServiceManager.CreateAndAddService<IMeterManager>(MockBehavior.Strict);
            _meterManager.Setup(m => m.GetMeter(It.IsAny<string>())).Returns(_meter.Object);
            _meterManager.Setup(m => m.IsMeterProvided(It.IsAny<string>())).Returns(true);

            var doors = new Mock<IDoorService>(MockBehavior.Default);
            MoqServiceManager.AddService<IDoorService>(doors.As<IService>().Object);
            string cultureName = "en-US";
            CultureInfo culture = new CultureInfo(cultureName);
            string minorUnitSymbol = "c";

            RegionInfo region = new RegionInfo(cultureName);
            CurrencyExtensions.Currency = new Currency(region.ISOCurrencySymbol, region, culture, minorUnitSymbol);
            CurrencyExtensions.SetCultureInfo(region.ISOCurrencySymbol, culture, null, null, true, true, minorUnitSymbol);

            MoqServiceManager.CreateAndAddService<ICabinetDetectionService>(MockBehavior.Loose);
            _target = new VoucherMetersPageViewModel();

            var config = MoqServiceManager.CreateAndAddService<IOperatorMenuConfiguration>(MockBehavior.Strict);
            config.Setup(m => m.GetAccessRuleSet(It.IsAny<VoucherMetersPageViewModel>())).Returns(It.IsAny<string>());
            config.Setup(m => m.GetAccessRuleSet(It.IsAny<VoucherMetersPageViewModel>(), It.IsAny<string>())).Returns(It.IsAny<string>());
            config.Setup(m => m.GetPrintAccessRuleSet(It.IsAny<VoucherMetersPageViewModel>())).Returns(It.IsAny<string>());
            config.Setup(m => m.GetPrintButtonEnabled(It.IsAny<VoucherMetersPageViewModel>(), It.IsAny<bool>())).Returns(It.IsAny<bool>());
            config.Setup(m => m.GetSetting(It.IsAny<string>(), It.IsAny<bool>())).Returns(It.IsAny<bool>());
            config.Setup(m => m.GetSetting(It.IsAny<VoucherMetersPageViewModel>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(It.IsAny<bool>());

            var access = MoqServiceManager.CreateAndAddService<IOperatorMenuAccess>(MockBehavior.Strict);
            access.Setup(m => m.UnregisterAccessRules(It.IsAny<VoucherMetersPageViewModel>()));

            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<PrintButtonStatusEvent>>())).Verifiable();
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<PrintButtonClickedEvent>>())).Verifiable();
            _eventBus.Setup(a => a.Subscribe(_target, It.IsAny<Action<PeriodOrMasterButtonClickedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<ClosedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<OpenEvent>>()));
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<DialogClosedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<OperatorCultureChangedEvent>>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPageLoadedEvent>())).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPopupEvent>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<MeterPageLoadedEvent>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuWarningMessageEvent>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<LampTestLampStateEvent>()));

            var buttonService = MoqServiceManager.CreateAndAddService<IButtonService>(MockBehavior.Strict);
            buttonService.Setup(m => m.IsTestModeActive).Returns(It.IsAny<bool>());

            var iio = MoqServiceManager.CreateAndAddService<IIO>(MockBehavior.Strict);
            iio.Setup(m => m.SetButtonLamp(It.IsAny<uint>(), It.IsAny<bool>())).Returns(It.IsAny<uint>());
            iio.Setup(m => m.SetButtonLampByMask(It.IsAny<uint>(), It.IsAny<bool>())).Returns(It.IsAny<uint>());

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
            var ticketCreator = MoqServiceManager.CreateAndAddService<IMetersTicketCreator>(MockBehavior.Strict);
            ticketCreator.Setup(m => m.CreateEgmMetersTicket(It.IsAny<IList<Tuple<Tuple<IMeter, IMeter>, string>>>(), It.IsAny<bool>()))
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
                        Assert.IsTrue(e.TicketsToPrint != null && e.TicketsToPrint.Count() <= 2);
                        _waiter.Set();
                    });

            _target.LoadedCommand.Execute(null);

            onPrinterClicked(new PrintButtonClickedEvent());

            Assert.IsTrue(_waiter.WaitOne(Timeout));
            ticketCreator.Verify();
            printer.Verify();
        }
    }
}
