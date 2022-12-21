namespace Aristocrat.Monaco.Application.UI.Tests.Views
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Threading;
    using Accounting.Contracts;
    using Contracts;
    using Contracts.OperatorMenu;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Persistence;
    using Hardware.Contracts.SharedDevice;
    using Kernel;
    using Kernel.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;
    using UI.ViewModels.NoteAcceptor;
    using UI.Views;
    using DisabledEvent = Hardware.Contracts.NoteAcceptor.DisabledEvent;
    using EnabledEvent = Hardware.Contracts.NoteAcceptor.EnabledEvent;
    using Printer = Hardware.Contracts.Printer;

    [TestClass]
    public class NoteAcceptorPageTest
    {
        private dynamic _accessor;
        private Mock<IPersistentStorageAccessor> _block;
        private Mock<IEventBus> _eventBus;
        private Mock<INoteAcceptor> _noteAcceptorService;
        private Mock<IPropertiesManager> _propertiesManager;

        // mocks for NoteAcceptorService
        private Mock<IPersistentStorageManager> _storageManager;
        private Mock<IMeterManager> _meterManager;
        private Mock<IMeter> _meter;
        private NoteAcceptorPage _targetView;
        private NoteAcceptorViewModel _target;
        private readonly ManualResetEvent _waiter = new ManualResetEvent(false);
        private const int Timeout = 1000;

        // Use TestInitialize to run code before running each test 
        [TestInitialize]
        public void MyTestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(m => m.GetProperty(PropertyKey.VoucherIn, false)).Returns(false);
            _propertiesManager.Setup(m => m.SetProperty("NoteAcceptorDiagnostics", false)).Verifiable();
            _propertiesManager.Setup(mock => mock.GetProperty("NoteAcceptorDiagnostics", false)).Returns(false);
            _propertiesManager.Setup(mock => mock.GetProperty("System.CheckCreditsIn", false)).Returns(false);
            _propertiesManager.Setup(mock => mock.GetProperty("System.AllowCreditUnderLimit", false)).Returns(false);
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.NoteAcceptorErrorBillJamText, string.Empty))
                .Returns("Bill Jam");
            _propertiesManager.Setup(
                    mock => mock.GetProperty(ApplicationConstants.NoteAcceptorErrorBillStackerJamText, string.Empty))
                .Returns("Bill Stacker Jam");
            _propertiesManager
                .Setup(mock => mock.GetProperty(ApplicationConstants.NoteAcceptorErrorBillStackerErrorText, string.Empty))
                .Returns("Bill Stacker Error");
            _propertiesManager.Setup(
                    mock => mock.GetProperty(ApplicationConstants.NoteAcceptorErrorBillStackerFullText, string.Empty))
                .Returns("Bill Stacker Full");
            _propertiesManager.Setup(
                    mock => mock.GetProperty(ApplicationConstants.NoteAcceptorErrorCashBoxRemovedText, string.Empty))
                .Returns("Cash Box Removed");
            _propertiesManager.Setup(m => m.SetProperty(ApplicationConstants.ActiveProtocol, "TEST"));
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ActiveProtocol, "")).Returns("TEST");
            _propertiesManager.Setup(m => m.GetProperty(PropertyKey.CurrentBalance, It.IsAny<long>()))
                .Returns(0L);
            _propertiesManager.Setup(m => m.GetProperty(PropertyKey.MaxCreditsIn, It.IsAny<long>())).Returns(0L);
            _propertiesManager.Setup(m => m.SetProperty(PropertyKey.MaxCreditsIn, It.IsAny<long>())).Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.MaxCreditMeter, long.MaxValue)).Returns(long.MaxValue);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ExcessiveDocumentRejectCount, -1)).Returns(-1);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ExcessiveDocumentRejectCountDefault, -1)).Returns(-1);

            _meter = MoqServiceManager.CreateAndAddService<IMeter>(MockBehavior.Strict);
            _meter.Setup(m => m.Lifetime).Returns(1);

            _meterManager = MoqServiceManager.CreateAndAddService<IMeterManager>(MockBehavior.Strict);
            _meterManager.Setup(m => m.GetMeter(It.IsAny<string>())).Returns(_meter.Object);
            _meterManager.Setup(m => m.IsMeterProvided(It.IsAny<string>())).Returns(true);

            var monitor = MoqServiceManager.CreateAndAddService<IOperatorMenuGamePlayMonitor>(MockBehavior.Strict);
            monitor.Setup(m => m.InGameRound).Returns(false);
            monitor.Setup(m => m.IsRecoveryNeeded).Returns(false);

            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            EventSubscriptionMocks();

            // mocks for NoteAcceptorService. We have to use a real implementation because the page asks ServiceManager for
            // the INoteAcceptor service and then casts it to IDeviceService. That doesn't work with Mocks.
            _block = MoqServiceManager.CreateAndAddService<IPersistentStorageAccessor>(MockBehavior.Strict);
            _block.SetupSet(m => m["DisabledReasons"] = It.IsAny<byte>());
            _block.SetupSet(m => m["Denominations"] = It.IsAny<byte>());
            _storageManager = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Strict);
            _storageManager.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(false);
            _storageManager.Setup(m => m.CreateBlock(PersistenceLevel.Transient, It.IsAny<string>(), 1))
                .Returns(_block.Object);
            _storageManager.Setup(m => m.CreateBlock(PersistenceLevel.Critical, It.IsAny<string>(), 1))
                .Returns(_block.Object);
            _storageManager.Setup(m => m.CreateBlock(PersistenceLevel.Static, It.IsAny<string>(), 1))
                .Returns(_block.Object);
            _storageManager.Setup(m => m.GetBlock(It.IsAny<string>())).Returns(_block.Object);
            _propertiesManager.Setup(m => m.AddPropertyProvider(It.IsAny<IPropertyProvider>()));
            _propertiesManager.Setup(mock => mock.GetProperty("Mono.SelectedAddinConfigurationHashCode", null)).Returns(null);
            _eventBus.Setup(m => m.Publish(It.IsAny<DisabledEvent>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<PageTitleEvent>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<NoteAcceptorMenuEnteredEvent>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<NoteAcceptorMenuExitedEvent>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<ClosedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<OpenEvent>>()));

            // get the note acceptor service to a state we can test with
            _noteAcceptorService = new Mock<INoteAcceptor>();
            _noteAcceptorService.As<IDeviceService>();
            _noteAcceptorService.SetupGet(a => a.Denominations).Returns(new List<int> { 1, 5, 10, 20, 50, 100 });
            _noteAcceptorService.Setup(a => a.GetSupportedNotes(It.IsAny<string>())).Returns(new Collection<int>{ 1, 5, 10, 20, 50, 100});
            MoqServiceManager.AddService(_noteAcceptorService);
            _noteAcceptorService.SetupGet(m => m.LogicalState).Returns(NoteAcceptorLogicalState.Idle);
            _noteAcceptorService.SetupGet(m => m.LastError).Returns(string.Empty);
            var device = new Mock<IDevice>();
            device.SetupGet(m => m.Protocol).Returns("Fake");
            device.SetupGet(m => m.Manufacturer).Returns("Fake");
            _noteAcceptorService.SetupGet(m => m.DeviceConfiguration).Returns(device.Object);

            MoqServiceManager.CreateAndAddService<ITime>(MockBehavior.Strict);

            var doors = new Mock<IDoorService>(MockBehavior.Default);
            MoqServiceManager.AddService<IDoorService>(doors.As<IService>().Object);
            MoqServiceManager.CreateAndAddService<IDialogService>(MockBehavior.Strict);

            var config = MoqServiceManager.CreateAndAddService<IOperatorMenuConfiguration>(MockBehavior.Strict);
            config.Setup(m => m.GetAccessRuleSet(It.IsAny<NoteAcceptorViewModel>())).Returns(It.IsAny<string>());
            config.Setup(m => m.GetAccessRuleSet(It.IsAny<NoteAcceptorViewModel>(), It.IsAny<string>())).Returns(It.IsAny<string>());
            config.Setup(m => m.GetPrintButtonEnabled(It.IsAny<NoteAcceptorViewModel>(), It.IsAny<bool>())).Returns(It.IsAny<bool>());
            config.Setup(m => m.GetSetting(It.IsAny<NoteAcceptorViewModel>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(It.IsAny<bool>());
            config.Setup(m => m.GetSetting(It.IsAny<string>(), It.IsAny<bool>())).Returns(It.IsAny<bool>());

            var access = MoqServiceManager.CreateAndAddService<IOperatorMenuAccess>(MockBehavior.Strict);
            access.Setup(m => m.UnregisterAccessRules(It.IsAny<NoteAcceptorViewModel>()));

            InitTargets();
            _accessor = new DynamicPrivateObject(_target);
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup]
        public void MyTestCleanup()
        {
            _accessor._noteAcceptorDiagnosticsEnabled = false;
            MoqServiceManager.Instance.Setup(m => m.IsServiceAvailable<INoteAcceptor>()).Returns(false);

            _eventBus.Setup(m => m.Publish(It.IsAny<PrintButtonStatusEvent>()));
            _eventBus.Setup(m => m.UnsubscribeAll(_target));

            _target.Dispose();
            /*
            try
            {
                _targetView.Dispatcher.PumpUntilDry();
            }
            catch (Exception)
            {
                // just eat the exception since it is due to other window threads
                // not shutting down
            }*/

            _target = null;

            MoqServiceManager.RemoveInstance();

            if (AddinManager.IsInitialized)
            {
                AddinManager.Shutdown();
            }
        }

        private void InitTargets()
        {
            _targetView = new NoteAcceptorPage();
            _target = new NoteAcceptorViewModel(false);
        }

        [TestMethod]
        public void ConstructorWithDisabledNoteAcceptorShowsStatusMessageTest()
        {
            _noteAcceptorService.SetupGet(m => m.LogicalState).Returns(NoteAcceptorLogicalState.Disabled);

            InitTargets();
            _accessor = new DynamicPrivateObject(_target);

            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void UpdateScreenExitsImmediatelyWhenEventHandlerStoppedTest()
        {
            _accessor.EventHandlerStopped = true;
            MoqServiceManager.Instance.Setup(mock => mock.GetService<IEventBus>()).Returns((IEventBus)null);

            // test passes if this doesn't crash
            _accessor.UpdateScreen();

            // restore event bus mock for Cleanup
            MoqServiceManager.Instance.Setup(mock => mock.GetService<IEventBus>()).Returns(_eventBus.Object);
        }

        [TestMethod]
        public void UpdateScreenHandleEventTest()
        {
            _accessor.EventHandlerStopped = false;

            _accessor.HandleHardwareNoteAcceptorEnabledEvent(new EnabledEvent(EnabledReasons.Reset));
            _accessor.HandleHardwareNoteAcceptorDisabledEvent(new DisabledEvent(DisabledReasons.System));
            _accessor.UpdateScreen();
        }

        private void EventSubscriptionMocks()
        {
            _eventBus.Setup(
                m => m.Subscribe(It.IsAny<NoteAcceptorViewModel>(), It.IsAny<Action<SystemEnabledEvent>>()));
            _eventBus.Setup(
                m => m.Subscribe(It.IsAny<NoteAcceptorViewModel>(), It.IsAny<Action<DisabledEvent>>()));
            _eventBus.Setup(
                m => m.Subscribe(It.IsAny<NoteAcceptorViewModel>(), It.IsAny<Action<EnabledEvent>>()));
            _eventBus.Setup(
                m => m.Subscribe(It.IsAny<NoteAcceptorViewModel>(), It.IsAny<Action<InspectedEvent>>()));
            _eventBus.Setup(
                m => m.Subscribe(It.IsAny<NoteAcceptorViewModel>(), It.IsAny<Action<DisconnectedEvent>>()));
            _eventBus.Setup(
                m => m.Subscribe(It.IsAny<NoteAcceptorViewModel>(), It.IsAny<Action<ConnectedEvent>>()));
            _eventBus.Setup(
                m => m.Subscribe(It.IsAny<NoteAcceptorViewModel>(), It.IsAny<Action<HardwareFaultEvent>>()));
            _eventBus.Setup(
                m => m.Subscribe(It.IsAny<NoteAcceptorViewModel>(), It.IsAny<Action<HardwareFaultClearEvent>>()));
            _eventBus.Setup(
                m => m.Subscribe(It.IsAny<NoteAcceptorViewModel>(), It.IsAny<Action<InspectionFailedEvent>>()));
            _eventBus.Setup(
                m => m.Subscribe(It.IsAny<NoteAcceptorViewModel>(), It.IsAny<Action<DocumentRejectedEvent>>()));
            _eventBus.Setup(
                m => m.Subscribe(It.IsAny<NoteAcceptorViewModel>(), It.IsAny<Action<CurrencyReturnedEvent>>()));
            _eventBus.Setup(
                m => m.Subscribe(It.IsAny<NoteAcceptorViewModel>(), It.IsAny<Action<VoucherReturnedEvent>>()));
            _eventBus.Setup(
                m => m.Subscribe(It.IsAny<NoteAcceptorViewModel>(), It.IsAny<Action<CurrencyStackedEvent>>()));
            _eventBus.Setup(
                m => m.Subscribe(It.IsAny<NoteAcceptorViewModel>(), It.IsAny<Action<CurrencyEscrowedEvent>>()));
            _eventBus.Setup(
                m => m.Subscribe(It.IsAny<NoteAcceptorViewModel>(), It.IsAny<Action<VoucherEscrowedEvent>>()));
            _eventBus.Setup(
                m => m.Subscribe(It.IsAny<NoteAcceptorViewModel>(), It.IsAny<Action<SelfTestPassedEvent>>()));
            _eventBus.Setup(
                m => m.Subscribe(It.IsAny<NoteAcceptorViewModel>(), It.IsAny<Action<SelfTestFailedEvent>>()));
            _eventBus.Setup(
                m => m.Subscribe(It.IsAny<NoteAcceptorViewModel>(), It.IsAny<Action<PrintButtonClickedEvent>>()));
            _eventBus.Setup(
                m => m.Subscribe(It.IsAny<NoteAcceptorViewModel>(), It.IsAny<Action<Printer.PrintCompletedEvent>>()));
            _eventBus.Setup(
                m => m.Subscribe(It.IsAny<NoteAcceptorViewModel>(), It.IsAny<Action<OperatorMenuExitingEvent>>()));
        }
    }
}
