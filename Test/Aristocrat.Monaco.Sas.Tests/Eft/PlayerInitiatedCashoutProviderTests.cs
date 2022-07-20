namespace Aristocrat.Monaco.Sas.Tests.Eft
{
    using System;
    using Application.Contracts.Extensions;
    using Aristocrat.Monaco.Test.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Hardware.Contracts.Persistence;
    using Aristocrat.Sas.Client;
    using Aristocrat.Monaco.Sas.Eft;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Accounting.Contracts.Handpay;

    /// <summary>
    ///     Contains unit tests for the PlayerInitiatedCashoutProvider class.
    /// </summary>
    [TestClass]
    public class PlayerInitiatedCashoutProviderTests
    {
        private const string BlockFieldName = "PlayerLastCashoutAmount";
        private Mock<IEventBus> _eventBusMock;
        private Mock<ISasExceptionHandler> _sasExceptionHandlerMock;
        private Mock<IPersistentStorageManager> _persistentStorageManagerMock;

        private Action<VoucherIssuedEvent> _voucherIssuedEventCallbackHandler = null;
        private Action<HandpayCompletedEvent> _handpayCompletedEventCallbackHandler = null;
        private Action<TransferOutStartedEvent> _transferOutStartedEventCallbackHandler = null;
        private Action<TransferOutCompletedEvent> _transferOutCompletedEventCallbackHandler = null;
        private Action<TransferOutFailedEvent> _transferOutFailedEventCallbackHandler = null;
        private readonly static string BlockName = typeof(PlayerInitiatedCashoutProviderTests).ToString();

        [TestInitialize]
        public void MyTestInitialize()
        {
            var theTypeName = GetType().ToString();

            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);

            _eventBusMock = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _eventBusMock.Setup(
                    x => x.Subscribe(
                        It.IsAny<PlayerInitiatedCashoutProvider>(),
                        It.IsAny<Action<VoucherIssuedEvent>>()))
                .Callback<object, Action<VoucherIssuedEvent>>((y, x) => _voucherIssuedEventCallbackHandler = x);

            _eventBusMock.Setup(
                    x => x.Subscribe(
                        It.IsAny<PlayerInitiatedCashoutProvider>(),
                        It.IsAny<Action<HandpayCompletedEvent>>()))
                .Callback<object, Action<HandpayCompletedEvent>>((y, x) => _handpayCompletedEventCallbackHandler = x);

            _eventBusMock.Setup(
                    x => x.Subscribe(
                        It.IsAny<PlayerInitiatedCashoutProvider>(),
                        It.IsAny<Action<TransferOutStartedEvent>>()))
                .Callback<object, Action<TransferOutStartedEvent>>((y, x) => _transferOutStartedEventCallbackHandler = x);

            _eventBusMock.Setup(
                    x => x.Subscribe(
                        It.IsAny<PlayerInitiatedCashoutProvider>(),
                        It.IsAny<Action<TransferOutCompletedEvent>>()))
                .Callback<object, Action<TransferOutCompletedEvent>>((y, x) => _transferOutCompletedEventCallbackHandler = x);

            _eventBusMock.Setup(
                    x => x.Subscribe(
                        It.IsAny<PlayerInitiatedCashoutProvider>(),
                        It.IsAny<Action<TransferOutFailedEvent>>()))
                .Callback<object, Action<TransferOutFailedEvent>>((y, x) => _transferOutFailedEventCallbackHandler = x);

            
            _sasExceptionHandlerMock = new Mock<ISasExceptionHandler>(MockBehavior.Strict);

            _sasExceptionHandlerMock.Setup(sas => sas.ReportException(It.IsAny<GenericExceptionBuilder>()));

            _persistentStorageManagerMock = new Mock<IPersistentStorageManager>(MockBehavior.Strict);
        }

        [DataRow(true, false, false, DisplayName = "Null IPersistentStorageManager")]
        [DataRow(false, true, false, DisplayName = "Null IEventBus")]
        [DataRow(false, false, true, DisplayName = "Null ISasExceptionHandler")]
        [ExpectedException(typeof(ArgumentNullException))]
        [DataTestMethod]
        public void NullArgumentExceptionTests(bool nullPersistentStoage, bool nullEventBus, bool nullSasExceptionHandler)
        {
            new PlayerInitiatedCashoutProvider(
                nullPersistentStoage ? null : _persistentStorageManagerMock.Object,
                nullEventBus ? null : _eventBusMock.Object,
                nullSasExceptionHandler ? null : _sasExceptionHandlerMock.Object);
        }

        [TestMethod]
        public void ShouldCreateProviderInstanceIfBlockStorageExists()
        {
            _persistentStorageManagerMock.Setup(p => p.BlockExists(It.IsAny<string>())).Returns(true);

            var persistentStorageAccessorMock = new Mock<IPersistentStorageAccessor>();
            _persistentStorageManagerMock.Setup(p => p.GetBlock(It.IsAny<string>())).Returns(persistentStorageAccessorMock.Object);

            var playerCashoutProvider = new PlayerInitiatedCashoutProvider(
                _persistentStorageManagerMock.Object,
                _eventBusMock.Object,
                _sasExceptionHandlerMock.Object);

            Assert.IsNotNull(playerCashoutProvider);
            _persistentStorageManagerMock.Verify(p => p.BlockExists(It.IsAny<string>()), Times.Once);
            _eventBusMock.Verify(eb => eb.Subscribe(It.IsAny<object>(), It.IsAny<Action<VoucherIssuedEvent>>()), Times.Once);
            _eventBusMock.Verify(eb => eb.Subscribe(It.IsAny<object>(), It.IsAny<Action<HandpayCompletedEvent>>()), Times.Once);
            _eventBusMock.Verify(eb => eb.Subscribe(It.IsAny<object>(), It.IsAny<Action<TransferOutStartedEvent>>()), Times.Once);
            _eventBusMock.Verify(eb => eb.Subscribe(It.IsAny<object>(), It.IsAny<Action<TransferOutCompletedEvent>>()), Times.Once);
        }

        [TestMethod]
        public void ShouldCreateProviderInstanceAndBlockStorageIfBlockStorageDoesNotExists()
        {
            _persistentStorageManagerMock.Setup(p => p.BlockExists(It.IsAny<string>())).Returns(false);
            var persistentStorageAccessorMock = new Mock<IPersistentStorageAccessor>();
            _persistentStorageManagerMock.Setup(p => p.CreateBlock(It.IsAny<PersistenceLevel>(), It.IsAny<string>(), It.IsAny<int>())).Returns(persistentStorageAccessorMock.Object);
            
            var playerCashoutProvider = new PlayerInitiatedCashoutProvider(
                _persistentStorageManagerMock.Object,
                _eventBusMock.Object,
                _sasExceptionHandlerMock.Object);

            Assert.IsNotNull(playerCashoutProvider);
            _persistentStorageManagerMock.Verify(p => p.BlockExists(It.IsAny<string>()), Times.Once);
            _persistentStorageManagerMock.Verify(p => p.CreateBlock(It.IsAny<PersistenceLevel>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);

            //verify the event subscribers
            _eventBusMock.Verify(eb => eb.Subscribe(It.IsAny<object>(), It.IsAny<Action<VoucherIssuedEvent>>()), Times.Once);
            _eventBusMock.Verify(eb => eb.Subscribe(It.IsAny<object>(), It.IsAny<Action<HandpayCompletedEvent>>()), Times.Once);
            _eventBusMock.Verify(eb => eb.Subscribe(It.IsAny<object>(), It.IsAny<Action<TransferOutStartedEvent>>()), Times.Once);
            _eventBusMock.Verify(eb => eb.Subscribe(It.IsAny<object>(), It.IsAny<Action<TransferOutCompletedEvent>>()), Times.Once);
        }

        [TestMethod]
        public void ShouldTriggerVoucherIssuedEventSubscriber()
        {
            var mockTransactionAmount = 38474ul;

            var persistentStorageTransactionMock = new Mock<IPersistentStorageTransaction>();
            persistentStorageTransactionMock.Setup(t => t.Commit());

            var persistentStorageAccessorMock = new Mock<IPersistentStorageAccessor>();
            persistentStorageAccessorMock.Setup(ps => ps.StartTransaction()).Returns(persistentStorageTransactionMock.Object);

            _persistentStorageManagerMock.Setup(p => p.GetBlock(It.IsAny<string>())).Returns(persistentStorageAccessorMock.Object);
            _persistentStorageManagerMock.Setup(p => p.BlockExists(It.IsAny<string>())).Returns(true);

            var playerCashoutProvider = new PlayerInitiatedCashoutProvider(
                _persistentStorageManagerMock.Object,
                _eventBusMock.Object,
                _sasExceptionHandlerMock.Object);

            Assert.IsNotNull(playerCashoutProvider);
            _persistentStorageManagerMock.Verify(p => p.BlockExists(It.IsAny<string>()), Times.Once);
            _persistentStorageManagerMock.Verify(p => p.CreateBlock(It.IsAny<PersistenceLevel>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);

            _voucherIssuedEventCallbackHandler.Invoke(new VoucherIssuedEvent(new VoucherOutTransaction { Amount = ((long)mockTransactionAmount) }, null));

            _persistentStorageManagerMock.Verify(p => p.GetBlock(It.IsAny<string>()), Times.Once);
            persistentStorageTransactionMock.VerifySet(t => t[BlockFieldName] = (ulong)((long)mockTransactionAmount).MillicentsToCents(), Times.Once);
            persistentStorageTransactionMock.Verify(t => t.Commit(), Times.Once);
        }

        [TestMethod]
        public void ShouldTriggerHandpayCompletedEventSubscriber()
        {
            var mockTransactionAmount = 38474L;

            var persistentStorageTransactionMock = new Mock<IPersistentStorageTransaction>();
            persistentStorageTransactionMock.Setup(t => t.Commit());

            var persistentStorageAccessorMock = new Mock<IPersistentStorageAccessor>();
            persistentStorageAccessorMock.Setup(ps => ps.StartTransaction()).Returns(persistentStorageTransactionMock.Object);

            _persistentStorageManagerMock.Setup(p => p.GetBlock(It.IsAny<string>())).Returns(persistentStorageAccessorMock.Object);
            _persistentStorageManagerMock.Setup(p => p.BlockExists(It.IsAny<string>())).Returns(true);

            var playerCashoutProvider = new PlayerInitiatedCashoutProvider(
                _persistentStorageManagerMock.Object,
                _eventBusMock.Object,
                _sasExceptionHandlerMock.Object);

            Assert.IsNotNull(playerCashoutProvider);
            _persistentStorageManagerMock.Verify(p => p.BlockExists(It.IsAny<string>()), Times.Once);
            _persistentStorageManagerMock.Verify(p => p.CreateBlock(It.IsAny<PersistenceLevel>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);

            var theTxn = new HandpayTransaction
            {
                KeyOffCashableAmount = mockTransactionAmount,
                KeyOffPromoAmount = mockTransactionAmount,
                KeyOffNonCashAmount = mockTransactionAmount,
            };

            _handpayCompletedEventCallbackHandler.Invoke(new HandpayCompletedEvent(theTxn));

            _persistentStorageManagerMock.Verify(p => p.GetBlock(It.IsAny<string>()), Times.Once);
            persistentStorageTransactionMock.VerifySet(t => t[BlockFieldName] = (ulong)(theTxn.KeyOffCashableAmount + theTxn.KeyOffPromoAmount + theTxn.KeyOffNonCashAmount).MillicentsToCents(), Times.Once);
            persistentStorageTransactionMock.Verify(t => t.Commit(), Times.Once);
        }

        [TestMethod]
        public void ShouldTriggerTransferOutStartedEventSubscriber()
        {
            var persistentStorageTransactionMock = new Mock<IPersistentStorageTransaction>();
            persistentStorageTransactionMock.Setup(t => t.Commit());

            var persistentStorageAccessorMock = new Mock<IPersistentStorageAccessor>();
            persistentStorageAccessorMock.Setup(ps => ps.StartTransaction()).Returns(persistentStorageTransactionMock.Object);

            _persistentStorageManagerMock.Setup(p => p.GetBlock(It.IsAny<string>())).Returns(persistentStorageAccessorMock.Object);
            _persistentStorageManagerMock.Setup(p => p.BlockExists(It.IsAny<string>())).Returns(true);

            var playerCashoutProvider = new PlayerInitiatedCashoutProvider(
                _persistentStorageManagerMock.Object,
                _eventBusMock.Object,
                _sasExceptionHandlerMock.Object);

            Assert.IsNotNull(playerCashoutProvider);
            _persistentStorageManagerMock.Verify(p => p.BlockExists(It.IsAny<string>()), Times.Once);
            _persistentStorageManagerMock.Verify(p => p.CreateBlock(It.IsAny<PersistenceLevel>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);

            _transferOutStartedEventCallbackHandler.Invoke(new TransferOutStartedEvent(Guid.Empty, 0, 0, 0));

            _persistentStorageManagerMock.Verify(p => p.GetBlock(It.IsAny<string>()), Times.Once);
            persistentStorageTransactionMock.VerifySet(t => t[BlockFieldName] = 0ul, Times.Once);
            persistentStorageTransactionMock.Verify(t => t.Commit(), Times.Once);
        }

        [TestMethod]
        public void ShouldTriggerTransferOutCompletedEventSubscriberToReportException()
        {
            var persistentStorageTransactionMock = new Mock<IPersistentStorageTransaction>();
            var persistentStorageAccessorMock = new Mock<IPersistentStorageAccessor>();
            _persistentStorageManagerMock.Setup(p => p.GetBlock(It.IsAny<string>())).Returns(persistentStorageAccessorMock.Object);
            _persistentStorageManagerMock.Setup(p => p.BlockExists(It.IsAny<string>())).Returns(true);

            var playerCashoutProvider = new PlayerInitiatedCashoutProvider(
                _persistentStorageManagerMock.Object,
                _eventBusMock.Object,
                _sasExceptionHandlerMock.Object);

            Assert.IsNotNull(playerCashoutProvider);
            _persistentStorageManagerMock.Verify(p => p.BlockExists(It.IsAny<string>()), Times.Once);
            _persistentStorageManagerMock.Verify(p => p.CreateBlock(It.IsAny<PersistenceLevel>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);

            _transferOutCompletedEventCallbackHandler.Invoke(new TransferOutCompletedEvent(0, 0, 0, false, Guid.Empty));
            _persistentStorageManagerMock.Verify(p => p.GetBlock(It.IsAny<string>()), Times.Once);
            persistentStorageTransactionMock.Verify(t => t.Commit(), Times.Never);

            _sasExceptionHandlerMock.Verify(s => s.ReportException(It.IsAny<GenericExceptionBuilder>()), Times.Once);
        }

        [TestMethod]
        public void ShouldNotTriggerReportExceptionIfTransferOutFailedEventAndAmountIsZero()
        {
            var persistentStorageTransactionMock = new Mock<IPersistentStorageTransaction>();
            var persistentStorageAccessorMock = new Mock<IPersistentStorageAccessor>();
            _persistentStorageManagerMock.Setup(p => p.GetBlock(It.IsAny<string>())).Returns(persistentStorageAccessorMock.Object);
            _persistentStorageManagerMock.Setup(p => p.BlockExists(It.IsAny<string>())).Returns(true);

            ulong mockAmount = 0ul;
            persistentStorageAccessorMock.SetupGet(t => t[BlockFieldName]).Returns(mockAmount);

            var playerCashoutProvider = new PlayerInitiatedCashoutProvider(
                _persistentStorageManagerMock.Object,
                _eventBusMock.Object,
                _sasExceptionHandlerMock.Object);

            Assert.IsNotNull(playerCashoutProvider);
            _persistentStorageManagerMock.Verify(p => p.BlockExists(It.IsAny<string>()), Times.Once);
            _persistentStorageManagerMock.Verify(p => p.CreateBlock(It.IsAny<PersistenceLevel>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);

            _transferOutFailedEventCallbackHandler.Invoke(new TransferOutFailedEvent(0, 0, 0, Guid.Empty));
            _persistentStorageManagerMock.Verify(p => p.GetBlock(It.IsAny<string>()), Times.Once);
            persistentStorageTransactionMock.Verify(t => t.Commit(), Times.Never);

            _sasExceptionHandlerMock.Verify(s => s.ReportException(It.IsAny<GenericExceptionBuilder>()), Times.Never);
        }

        [TestMethod]
        public void ShouldReportExceptionIfPartialCashoutSucceededAndTransferOutFailedEventAndAmountGreaterThanZero()
        {
            var persistentStorageTransactionMock = new Mock<IPersistentStorageTransaction>();
            var persistentStorageAccessorMock = new Mock<IPersistentStorageAccessor>();
            _persistentStorageManagerMock.Setup(p => p.GetBlock(It.IsAny<string>())).Returns(persistentStorageAccessorMock.Object);
            _persistentStorageManagerMock.Setup(p => p.BlockExists(It.IsAny<string>())).Returns(true);

            ulong mockAmount = 10ul;
            persistentStorageAccessorMock.SetupGet(t => t[BlockFieldName]).Returns(mockAmount);

            var playerCashoutProvider = new PlayerInitiatedCashoutProvider(
                _persistentStorageManagerMock.Object,
                _eventBusMock.Object,
                _sasExceptionHandlerMock.Object);

            Assert.IsNotNull(playerCashoutProvider);
            _persistentStorageManagerMock.Verify(p => p.BlockExists(It.IsAny<string>()), Times.Once);
            _persistentStorageManagerMock.Verify(p => p.CreateBlock(It.IsAny<PersistenceLevel>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);

            _transferOutFailedEventCallbackHandler.Invoke(new TransferOutFailedEvent(0, 0, 0, Guid.Empty));
            _persistentStorageManagerMock.Verify(p => p.GetBlock(It.IsAny<string>()), Times.Once);
            persistentStorageTransactionMock.Verify(t => t.Commit(), Times.Never);

            _sasExceptionHandlerMock.Verify(s => s.ReportException(It.IsAny<GenericExceptionBuilder>()), Times.Once);
        }

        [TestMethod]
        public void ShouldGetCashoutAmount()
        {
            var persistentStorageTransactionMock = new Mock<IPersistentStorageTransaction>();
            persistentStorageTransactionMock.Setup(t => t.Commit());
            
            var persistentStorageAccessorMock = new Mock<IPersistentStorageAccessor>();
            persistentStorageAccessorMock.Setup(ps => ps.StartTransaction()).Returns(persistentStorageTransactionMock.Object);
            ulong mockAmount = 12345ul;
            persistentStorageAccessorMock.SetupGet(t => t[BlockFieldName]).Returns(mockAmount);

            _persistentStorageManagerMock.Setup(p => p.GetBlock(It.IsAny<string>())).Returns(persistentStorageAccessorMock.Object);
            _persistentStorageManagerMock.Setup(p => p.BlockExists(It.IsAny<string>())).Returns(true);

            var playerCashoutProvider = new PlayerInitiatedCashoutProvider(
                _persistentStorageManagerMock.Object,
                _eventBusMock.Object,
                _sasExceptionHandlerMock.Object);

            Assert.IsNotNull(playerCashoutProvider);
            _persistentStorageManagerMock.Verify(p => p.BlockExists(It.IsAny<string>()), Times.Once);
            _persistentStorageManagerMock.Verify(p => p.CreateBlock(It.IsAny<PersistenceLevel>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);

            var cashoutAmount = playerCashoutProvider.GetCashoutAmount();
            Assert.AreEqual(mockAmount, cashoutAmount);
            _persistentStorageManagerMock.Verify(p => p.GetBlock(It.IsAny<string>()), Times.Once);
            persistentStorageTransactionMock.Verify(t => t.Commit(), Times.Never);
            _sasExceptionHandlerMock.Verify(s => s.ReportException(It.IsAny<GenericExceptionBuilder>()), Times.Never);
        }

        [TestMethod]
        public void ShouldClearCashoutAmount()
        {
            var persistentStorageTransactionMock = new Mock<IPersistentStorageTransaction>();
            persistentStorageTransactionMock.Setup(t => t.Commit());

            var persistentStorageAccessorMock = new Mock<IPersistentStorageAccessor>();
            persistentStorageAccessorMock.Setup(ps => ps.StartTransaction()).Returns(persistentStorageTransactionMock.Object);

            _persistentStorageManagerMock.Setup(p => p.GetBlock(It.IsAny<string>())).Returns(persistentStorageAccessorMock.Object);
            _persistentStorageManagerMock.Setup(p => p.BlockExists(It.IsAny<string>())).Returns(true);

            var playerCashoutProvider = new PlayerInitiatedCashoutProvider(
                _persistentStorageManagerMock.Object,
                _eventBusMock.Object,
                _sasExceptionHandlerMock.Object);

            Assert.IsNotNull(playerCashoutProvider);
            _persistentStorageManagerMock.Verify(p => p.BlockExists(It.IsAny<string>()), Times.Once);
            _persistentStorageManagerMock.Verify(p => p.CreateBlock(It.IsAny<PersistenceLevel>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);

            playerCashoutProvider.ClearCashoutAmount();

            //verify the clear cashout amount logic
            _persistentStorageManagerMock.Verify(p => p.GetBlock(It.IsAny<string>()), Times.Once);
            persistentStorageTransactionMock.VerifySet(t => t[BlockFieldName] = 0ul, Times.Once);
            persistentStorageTransactionMock.Verify(t => t.Commit(), Times.Once);
        }
    }
}