namespace Aristocrat.Monaco.Accounting.Tests
{
    using System;
    using System.Threading.Tasks;
    using Contracts;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [DoNotParallelize]
    [TestClass]
    public class NoteAcceptorActivityTimeWatcherTests
    {
        private Mock<IEventBus> _eventBus;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<INoteAcceptor> _noteAcceptor;
        private NoteAcceptorActivityTimeWatcher _noteAcceptorActivityTimeWatcher;
        private Action<CurrencyEscrowedEvent> _currencyEscrowedEventAction;
        private Action<VoucherEscrowedEvent> _voucherEscrowedEventAction;

        [TestInitialize]
        public void Initialize()
        {
            _eventBus = new Mock<IEventBus>(MockBehavior.Strict);
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Strict);
            _noteAcceptor = new Mock<INoteAcceptor>(MockBehavior.Strict);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _eventBus.Setup(x => x.UnsubscribeAll(It.IsAny<NoteAcceptorActivityTimeWatcher>())).Verifiable();
            _noteAcceptorActivityTimeWatcher?.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEventBusIsNullExpectException()
        {
            _noteAcceptorActivityTimeWatcher = new NoteAcceptorActivityTimeWatcher(
                null,
                _propertiesManager.Object,
                _noteAcceptor.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenPropertiesManagerIsNullExpectException()
        {
            _noteAcceptorActivityTimeWatcher = new NoteAcceptorActivityTimeWatcher(
                _eventBus.Object,
                null,
                _noteAcceptor.Object);
        }

        [DataRow(1000, typeof(VoucherEscrowedEvent), DisplayName = "Successful Transaction in 1000s")]
        [DataRow(500, typeof(CurrencyEscrowedEvent), DisplayName = "Successful Transaction in 500s")]
        [DataRow(800, typeof(CurrencyEscrowedEvent), DisplayName = "Successful Transaction in 800s")]
        [DataTestMethod]
        public async Task SuccessfulTransaction(int timeOut, Type @event)
        {
            SetupEventHandlers();

            SetupProperties();

            _propertiesManager
                .Setup(m => m.GetProperty(AccountingConstants.NoteAcceptorTimeLimitValue, 5000))
                .Returns(timeOut);

            _noteAcceptorActivityTimeWatcher = new NoteAcceptorActivityTimeWatcher(
                _eventBus.Object,
                _propertiesManager.Object,
                _noteAcceptor.Object);

            _noteAcceptor.Setup(x => x.Return()).Verifiable();

            var note = new Note { NoteId = 1, ISOCurrencySymbol = "USD", Value = 1, Version = 1 };

            if (@event == typeof(VoucherEscrowedEvent))
            {
                _voucherEscrowedEventAction?.Invoke
                    (new VoucherEscrowedEvent(1, "Test Barcode"));
            }
            else
            {
                _currencyEscrowedEventAction?.Invoke(
                    new CurrencyEscrowedEvent(1, note));
            }

            //Change the noteAcceptors status as stacked
            _noteAcceptor.SetupGet(x => x.LastDocumentResult).Returns(DocumentResult.Stacked);

            await Task.Run(
                async () =>
                {
                    await Task.Delay(timeOut + 200);
                });

            _noteAcceptor.Verify(x => x.Return(), Times.Never);
        }

        [DataRow(1000, typeof(VoucherEscrowedEvent), DisplayName = "Returned Transaction in More than 1000")]
        [DataRow(500, typeof(VoucherEscrowedEvent), DisplayName = "Returned Transaction in More than 500s")]
        [DataRow(800, typeof(CurrencyEscrowedEvent), DisplayName = "Returned Transaction in More than 800s")]
        [DataTestMethod]
        public async Task ReturnedTransaction(int timeOut, Type @event)
        {
            SetupEventHandlers();

            SetupProperties();

            _propertiesManager
                .Setup(m => m.GetProperty(AccountingConstants.NoteAcceptorTimeLimitValue, 5000))
                .Returns(timeOut);

            _noteAcceptorActivityTimeWatcher = new NoteAcceptorActivityTimeWatcher(
                _eventBus.Object,
                _propertiesManager.Object,
                _noteAcceptor.Object);

            _noteAcceptor.Setup(x => x.Return()).Verifiable();

            var note = new Note { NoteId = 1, ISOCurrencySymbol = "USD", Value = 1, Version = 1 };

            if (@event == typeof(VoucherEscrowedEvent))
            {
                _voucherEscrowedEventAction?.Invoke
                    (new VoucherEscrowedEvent(1, "Test Barcode"));
                _noteAcceptor.SetupGet(x => x.LastDocumentResult).Returns(DocumentResult.Escrowed);
            }
            else
            {
                _currencyEscrowedEventAction?.Invoke(
                    new CurrencyEscrowedEvent(1, note));
                _noteAcceptor.SetupGet(x => x.LastDocumentResult).Returns(DocumentResult.Escrowed);
            }

            //await Task.Run(
            //    async () =>
            //    {
                    await Task.Delay((int)(timeOut * 0.1));
                    _noteAcceptor.Verify(x => x.Return(), Times.Never);
                    await Task.Delay(timeOut);
                    _noteAcceptor.Verify(x => x.Return(), Times.Once);
                //});
        }

        private void SetupProperties()
        {
            _propertiesManager
                .Setup(m => m.GetProperty(AccountingConstants.NoteAcceptorTimeLimitEnabled, false))
                .Returns(true);
            _propertiesManager
                .Setup(m => m.GetProperty(AccountingConstants.NoteAcceptorTimeLimitValue, 5000))
                .Returns(5000);
        }

        private void SetupEventHandlers()
        {
            _eventBus
                .Setup(
                    m => m.Subscribe(
                        It.IsAny<NoteAcceptorActivityTimeWatcher>(),
                        It.IsAny<Action<ServiceAddedEvent>>()));
            _eventBus
                .Setup(
                    m => m.Subscribe(
                        It.IsAny<NoteAcceptorActivityTimeWatcher>(),
                        It.IsAny<Action<CurrencyEscrowedEvent>>()))
                .Callback<object, Action<CurrencyEscrowedEvent>>(
                    (noteAcceptorActivityTimeWatcher, callback) => { _currencyEscrowedEventAction = callback; });
            _eventBus
                .Setup(
                    m => m.Subscribe(
                        It.IsAny<NoteAcceptorActivityTimeWatcher>(),
                        It.IsAny<Action<VoucherEscrowedEvent>>()))
                .Callback<object, Action<VoucherEscrowedEvent>>(
                    (noteAcceptorActivityTimeWatcher, callback) => { _voucherEscrowedEventAction = callback; });
        }
    }
}