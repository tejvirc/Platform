namespace Aristocrat.Monaco.Accounting.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using Accounting.CoinAcceptor;
    using Accounting.Contracts.CoinAcceptor;
    using Application.Contracts;
    using Contracts;
    using Hardware.Contracts;
    using Hardware.Contracts.Persistence;
    using Hardware.Contracts.CoinAcceptor;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;

    [TestClass]
    public class CoinInProviderTest
    {
        private const int RequestTimeoutLength = 1000; // It's in milliseconds
        private static readonly Guid RequestorId = new Guid("{EBB8B24C-771F-474A-8315-4F25DDBDBEA3}");
        private static readonly Guid TransactionId = new Guid("{0241B14C-C962-4DBA-B080-260412CA7435}");
        private Mock<ICoinAcceptor> _coinAcceptor;
        private Mock<IBank> _bank;
        private Mock<ITransactionCoordinator> _transactionCoordinator;
        private Mock<ITransactionHistory> _transactionHistory;
        private Mock<IEventBus> _eventBus;
        private Mock<IMeterManager> _meterManager;
        private Mock<IPersistentStorageManager> _storageManager;
        private Mock<IIdProvider> _iidProvider;
        private Mock<IMessageDisplay> _messageDisplay;
        private Mock<IScopedTransaction> _scopedTransaction;
        private Mock<IPersistentStorageAccessor> _persistentStorageAccessor;
        private Mock<IMeter> _trueCoinInCount;
        private Mock<IMeter> _trueCoinOutCount;
        private Mock<IMeter> _coinToCashBoxCount;
        private Mock<IMeter> _coinToHopperCount;
        private Mock<IMeter> _coinToCashBoxInsteadHopperCount;
        private Mock<IMeter> _coinToHopperInsteadCashBoxCount;
        private Mock<IMeter> _currentHopperLevelCount;
        private Mock<IDisposable> _disposable;
        private CoinInProvider _target;
        private dynamic _accessor;
        private ITransaction _result;
        private Action<CoinInEvent> _onCoinInEvent;
        private Action<CoinToCashboxInEvent> _onCoinToCashBoxEvent;
        private Action<CoinToHopperInEvent> _onCoinToHopperEvent;
        private Action<CoinToCashboxInsteadOfHopperEvent> _onCoinToCashBoxInsteadHopperEvent;
        private Action<CoinToHopperInsteadOfCashboxEvent> _onCoinToHopperInsteadCashBoxEvent;
        private Mock<IPropertiesManager> _propertiesManager;

        [TestInitialize]
        public void TestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();

            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _coinAcceptor = new Mock<ICoinAcceptor>(MockBehavior.Default);
            _bank = MoqServiceManager.CreateAndAddService<IBank>(MockBehavior.Default);
            _transactionCoordinator = new Mock<ITransactionCoordinator>(MockBehavior.Default);
            _transactionHistory = new Mock<ITransactionHistory>(MockBehavior.Default);
            _eventBus = new Mock<IEventBus>(MockBehavior.Default);
            _meterManager = new Mock<IMeterManager>(MockBehavior.Default);
            _storageManager = new Mock<IPersistentStorageManager>(MockBehavior.Default);
            _iidProvider = new Mock<IIdProvider>(MockBehavior.Default);
            _messageDisplay = new Mock<IMessageDisplay>(MockBehavior.Default);
            _scopedTransaction = new Mock<IScopedTransaction>(MockBehavior.Default);
            _persistentStorageAccessor = new Mock<IPersistentStorageAccessor>();
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);

            _disposable = new Mock<IDisposable>(MockBehavior.Default);
            _disposable.Setup(d => d.Dispose()).Verifiable();

            MockLocalization.Setup(MockBehavior.Default);

            _transactionHistory.Setup(h => h.UpdateTransaction(It.IsAny<ITransaction>()))
                .Callback<ITransaction>(r => _result = r);
            _propertiesManager.Setup(m => m.GetProperty(HardwareConstants.CoinAcceptorDiagnosticMode, false)).Returns(false);

            _trueCoinInCount = new Mock<IMeter>(MockBehavior.Strict);
            _trueCoinOutCount = new Mock<IMeter>(MockBehavior.Strict);
            _coinToCashBoxCount = new Mock<IMeter>(MockBehavior.Strict);
            _coinToHopperCount = new Mock<IMeter>(MockBehavior.Strict);
            _coinToCashBoxInsteadHopperCount = new Mock<IMeter>(MockBehavior.Strict);
            _coinToHopperInsteadCashBoxCount = new Mock<IMeter>(MockBehavior.Strict);
            _currentHopperLevelCount = new Mock<IMeter>(MockBehavior.Strict);

            _meterManager.Setup(x => x.GetMeter(AccountingMeters.TrueCoinInCount))
                .Returns(_trueCoinInCount.Object);
            _meterManager.Setup(x => x.GetMeter(AccountingMeters.TrueCoinOutCount))
                .Returns(_trueCoinOutCount.Object);
            _meterManager.Setup(x => x.GetMeter(AccountingMeters.CoinToCashBoxCount))
                .Returns(_coinToCashBoxCount.Object);
            _meterManager.Setup(x => x.GetMeter(AccountingMeters.CoinToHopperCount))
                .Returns(_coinToHopperCount.Object);
            _meterManager.Setup(x => x.GetMeter(AccountingMeters.CoinToCashBoxInsteadHopperCount))
                .Returns(_coinToCashBoxInsteadHopperCount.Object);
            _meterManager.Setup(x => x.GetMeter(AccountingMeters.CoinToHopperInsteadCashBoxCount))
                .Returns(_coinToHopperInsteadCashBoxCount.Object);
            _meterManager.Setup(x => x.GetMeter(AccountingMeters.CurrentHopperLevelCount))
                .Returns(_currentHopperLevelCount.Object);
        }

        /// <summary>
        ///     Cleans up class members after execution of a TestMethod.
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
            MoqServiceManager.RemoveInstance();
        }

        /// <summary>
        ///     Tests for Coin In Provider constructor
        /// </summary>
        [TestMethod]
        public void CoinInProviderConstructorSetupTest()
        {
            const long currentBankBalance = 999;
            SetupCoinInProvider(currentBankBalance);

            _target.Initialize();

            _eventBus.Verify();
        }

        /// <summary>
        ///     Tests for Coin In Provider constructor
        /// </summary>
        [TestMethod]
        public void CoinInEventTest()
        {
            const long currentBankBalance = 999;
            _trueCoinInCount.Setup(x => x.Increment(1)).Verifiable();
            SetupCoinInProvider(currentBankBalance);
            _target.Initialize();

            Assert.IsNotNull(_onCoinInEvent);

            var coinInEvent = CoinInEventSetupTest(100000);
            _onCoinInEvent(coinInEvent);

            _scopedTransaction.Verify(x => x.Complete());
            _eventBus.Verify(x => x.Publish(It.IsAny<CoinInStartedEvent>()), Times.Once);
            _eventBus.Verify(x => x.Publish(It.IsAny<CoinInCompletedEvent>()), Times.Once);
            _bank.Verify(x => x.Deposit(AccountType.Cashable, 100000, TransactionId), Times.Once);

            _transactionCoordinator.Verify(x => x.ReleaseTransaction(TransactionId), Times.Once);
            _coinAcceptor.Verify(x => x.DivertMechanismOnOff(), Times.Once);
        }

        /// <summary>
        ///     Tests for Coin In Provider constructor when coin acceptor diagnostic mode is enabled.
        /// </summary>
        [TestMethod]
        public void CoinInEventWithDiagnosticModeTest()
        {
            _propertiesManager.Setup(m => m.GetProperty(HardwareConstants.CoinAcceptorDiagnosticMode, false)).Returns(true);
            const long currentBankBalance = 999;
            SetupCoinInProvider(currentBankBalance);
            _target.Initialize();

            Assert.IsNotNull(_onCoinInEvent);

            var coinInEvent = CoinInEventSetupTest(100000);
            _onCoinInEvent(coinInEvent);

            _scopedTransaction.Verify(x => x.Complete(), Times.Never);
            _eventBus.Verify(x => x.Publish(It.IsAny<CoinInStartedEvent>()), Times.Never);
            _eventBus.Verify(x => x.Publish(It.IsAny<CoinInCompletedEvent>()), Times.Never);
            _bank.Verify(x => x.Deposit(AccountType.Cashable, 100000, Guid.Empty), Times.Never);
            _transactionCoordinator.Verify(x => x.ReleaseTransaction(Guid.Empty), Times.Never);
            _coinAcceptor.Verify(x => x.DivertMechanismOnOff(), Times.Never);
        }

        /// <summary>
        ///     Tests for Coin In Provider constructor
        /// </summary>
        [TestMethod]
        public void CoinInEventWithEmptyTransactionIdTest()
        {
            const long currentBankBalance = 999;
            SetupCoinInProvider(currentBankBalance);
            _transactionCoordinator
                .Setup(m => m.RequestTransaction(RequestorId, RequestTimeoutLength, TransactionType.Write))
                .Returns(Guid.Empty);

            _target.Initialize();

            Assert.IsNotNull(_onCoinInEvent);

            var coinInEvent = CoinInEventSetupTest(100000);
            _onCoinInEvent(coinInEvent);

            _scopedTransaction.Verify(x => x.Complete(), Times.Never);
            _eventBus.Verify(x => x.Publish(It.IsAny<CoinInStartedEvent>()), Times.Never);
            _eventBus.Verify(x => x.Publish(It.IsAny<CoinInCompletedEvent>()), Times.Never);
            _bank.Verify(x => x.Deposit(AccountType.Cashable, 100000, Guid.Empty), Times.Never);
            _transactionCoordinator.Verify(x => x.ReleaseTransaction(Guid.Empty), Times.Never);
            _coinAcceptor.Verify(x => x.DivertMechanismOnOff(), Times.Never);
        }

        /// <summary>
        ///     Tests for Coin In to CashBox.
        /// </summary>
        [TestMethod]
        public void CoinInToCashBoxEventTest()
        {
            const long currentBankBalance = 999;
            _coinToCashBoxCount.Setup(x => x.Increment(1)).Verifiable();
            SetupCoinInProvider(currentBankBalance);

            _target.Initialize();
            Assert.IsNotNull(_onCoinToCashBoxEvent);

            _onCoinToCashBoxEvent(new CoinToCashboxInEvent());

            _transactionHistory.Verify(x => x.UpdateTransaction(It.IsAny<CoinInTransaction>()), Times.Once);
            _scopedTransaction.Verify(x => x.Complete(), Times.Once);
        }

        /// <summary>
        ///     Tests for Coin In to CashBox when coin acceptor diagnostic mode is enabled.
        /// </summary>
        [TestMethod]
        public void CoinInToCashBoxEventWithDiagnosticModeTest()
        {
            _propertiesManager.Setup(m => m.GetProperty(HardwareConstants.CoinAcceptorDiagnosticMode, false)).Returns(true);
            const long currentBankBalance = 999;
            SetupCoinInProvider(currentBankBalance);

            _target.Initialize();
            Assert.IsNotNull(_onCoinToCashBoxEvent);

            _onCoinToCashBoxEvent(new CoinToCashboxInEvent());

            _transactionHistory.Verify(x => x.UpdateTransaction(It.IsAny<CoinInTransaction>()), Times.Never);
            _scopedTransaction.Verify(x => x.Complete(), Times.Never);
        }

        /// <summary>
        ///     Tests for Coin In to Hopper.
        /// </summary>
        [TestMethod]
        public void CoinInToHopperEventTest()
        {
            const long currentBankBalance = 999;
            _coinToHopperCount.Setup(x => x.Increment(1)).Verifiable();
            _currentHopperLevelCount.Setup(x => x.Increment(1)).Verifiable();
            SetupCoinInProvider(currentBankBalance);

            _target.Initialize();
            Assert.IsNotNull(_onCoinToHopperEvent);

            _onCoinToHopperEvent(new CoinToHopperInEvent());

            _transactionHistory.Verify(x => x.UpdateTransaction(It.IsAny<CoinInTransaction>()), Times.Once);
            _scopedTransaction.Verify(x => x.Complete(), Times.Once);
        }

        /// <summary>
        ///     Tests for Coin In to Hopper when coin acceptor diagnostic mode is enabled.
        /// </summary>
        [TestMethod]
        public void CoinInToHopperEventWithDiagnosticModeTest()
        {
            _propertiesManager.Setup(m => m.GetProperty(HardwareConstants.CoinAcceptorDiagnosticMode, false)).Returns(true);
            const long currentBankBalance = 999;
            SetupCoinInProvider(currentBankBalance);

            _target.Initialize();
            Assert.IsNotNull(_onCoinToHopperEvent);

            _onCoinToHopperEvent(new CoinToHopperInEvent());

            _transactionHistory.Verify(x => x.UpdateTransaction(It.IsAny<CoinInTransaction>()), Times.Never);
            _scopedTransaction.Verify(x => x.Complete(), Times.Never);
        }

        /// <summary>
        ///     Tests for Coin In to CashBox instead Hopper.
        /// </summary>
        [TestMethod]
        public void CoinInToCashBoxInsteadHopperEventTest()
        {
            const long currentBankBalance = 999;
            _coinToCashBoxInsteadHopperCount.Setup(x => x.Increment(1)).Verifiable();
            SetupCoinInProvider(currentBankBalance);

            _target.Initialize();
            _accessor = new DynamicPrivateObject(_target);
            _accessor._diverterErrors = 5;
            Assert.IsNotNull(_onCoinToCashBoxInsteadHopperEvent);

            _onCoinToCashBoxInsteadHopperEvent(new CoinToCashboxInsteadOfHopperEvent());

            _transactionHistory.Verify(x => x.UpdateTransaction(It.IsAny<CoinInTransaction>()), Times.Once);
            _scopedTransaction.Verify(x => x.Complete(), Times.Once);
            _eventBus.Verify(x => x.Publish(It.IsAny<HardwareFaultEvent>()), Times.Once);
        }

        /// <summary>
        ///     Tests for Coin In to CashBox instead Hopper when coin acceptor diagnostic mode is enabled.
        /// </summary>
        [TestMethod]
        public void CoinInToCashBoxInsteadHopperEventWithDiagnosticModeTest()
        {
            _propertiesManager.Setup(m => m.GetProperty(HardwareConstants.CoinAcceptorDiagnosticMode, false)).Returns(true);
            const long currentBankBalance = 999;
            SetupCoinInProvider(currentBankBalance);

            _target.Initialize();
            _accessor = new DynamicPrivateObject(_target);
            _accessor._diverterErrors = 5;
            Assert.IsNotNull(_onCoinToCashBoxInsteadHopperEvent);

            _onCoinToCashBoxInsteadHopperEvent(new CoinToCashboxInsteadOfHopperEvent());

            _transactionHistory.Verify(x => x.UpdateTransaction(It.IsAny<CoinInTransaction>()), Times.Never);
            _scopedTransaction.Verify(x => x.Complete(), Times.Never);
            _eventBus.Verify(x => x.Publish(It.IsAny<HardwareFaultEvent>()), Times.Never);
        }

        /// <summary>
        ///     Tests for Coin In to Hopper instead CashBox.
        /// </summary>
        [TestMethod]
        public void CoinInToHopperInsteadCashBoxEventTest()
        {
            const long currentBankBalance = 999;
            _coinToHopperInsteadCashBoxCount.Setup(x => x.Increment(1)).Verifiable();
            _currentHopperLevelCount.Setup(x => x.Increment(1)).Verifiable();
            SetupCoinInProvider(currentBankBalance);

            _target.Initialize();
            _accessor = new DynamicPrivateObject(_target);
            _accessor._diverterErrors = 5;
            Assert.IsNotNull(_onCoinToHopperInsteadCashBoxEvent);

            _onCoinToHopperInsteadCashBoxEvent(new CoinToHopperInsteadOfCashboxEvent());

            _transactionHistory.Verify(x => x.UpdateTransaction(It.IsAny<CoinInTransaction>()), Times.Once);
            _scopedTransaction.Verify(x => x.Complete(), Times.Once);
            _eventBus.Verify(x => x.Publish(It.IsAny<HardwareFaultEvent>()), Times.Once);
        }

        /// <summary>
        ///     Tests for Coin In to Hopper instead CashBox when coin acceptor diagnostic mode is enabled.
        /// </summary>
        [TestMethod]
        public void CoinInToHopperInsteadCashBoxEventWithDiagnosticModeTest()
        {
            _propertiesManager.Setup(m => m.GetProperty(HardwareConstants.CoinAcceptorDiagnosticMode, false)).Returns(true);
            const long currentBankBalance = 999;
            SetupCoinInProvider(currentBankBalance);

            _target.Initialize();
            _accessor = new DynamicPrivateObject(_target);
            _accessor._diverterErrors = 5;
            Assert.IsNotNull(_onCoinToHopperInsteadCashBoxEvent);

            _onCoinToHopperInsteadCashBoxEvent(new CoinToHopperInsteadOfCashboxEvent());

            _transactionHistory.Verify(x => x.UpdateTransaction(It.IsAny<CoinInTransaction>()), Times.Never);
            _scopedTransaction.Verify(x => x.Complete(), Times.Never);
            _eventBus.Verify(x => x.Publish(It.IsAny<HardwareFaultEvent>()), Times.Never);
        }
        
        private CoinInEvent CoinInEventSetupTest(int coinValue)
        {
            var coin = new Coin { Value = coinValue };
            var coinInEvent = new CoinInEvent(coin);
            return coinInEvent;
        }
        private CoinInProvider GetTarget()
        {
            var target = new CoinInProvider(
                _coinAcceptor.Object,
                _bank.Object,
                _transactionCoordinator.Object,
                _transactionHistory.Object,
                _eventBus.Object,
                _meterManager.Object,
                _storageManager.Object,
                _iidProvider.Object,
                _messageDisplay.Object,
                _propertiesManager.Object);

            return target;
        }

        private void SetupCoinInProvider(long currentBankBalance)
        {
            SetupStorage();

            const long bankLimit = 1000;
            SetupBank(currentBankBalance, bankLimit);

            SetupTransaction();

            _target = GetTarget();

            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<CoinInEvent>>()))
                   .Callback<object, Action<CoinInEvent>>(
                       (tar, func) => { _onCoinInEvent = func; }).Verifiable();

            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<CoinToCashboxInEvent>>()))
                   .Callback<object, Action<CoinToCashboxInEvent>>(
                       (tar, func) => { _onCoinToCashBoxEvent = func; }).Verifiable();

            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<CoinToHopperInEvent>>()))
                   .Callback<object, Action<CoinToHopperInEvent>>(
                       (tar, func) => { _onCoinToHopperEvent = func; }).Verifiable();

            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<CoinToCashboxInsteadOfHopperEvent>>()))
                   .Callback<object, Action<CoinToCashboxInsteadOfHopperEvent>>(
                       (tar, func) => { _onCoinToCashBoxInsteadHopperEvent = func; }).Verifiable();

            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<CoinToHopperInsteadOfCashboxEvent>>()))
                   .Callback<object, Action<CoinToHopperInsteadOfCashboxEvent>>(
                       (tar, func) => { _onCoinToHopperInsteadCashBoxEvent = func; }).Verifiable();

            _iidProvider.Setup(m => m.GetNextLogSequence<CoinInTransaction>()).Returns(1);

            var meterMock = new Mock<IMeter>(MockBehavior.Default);
            _meterManager.Setup(m => m.GetMeter(It.IsAny<string>())).Returns(meterMock.Object);
        }

        private void SetupBank(long currentBankBalance, long bankLimit)
        {
            var accountType = AccountType.Cashable;
            _bank.Setup(x => x.Limit).Returns(bankLimit);
            _bank.Setup(x => x.QueryBalance()).Returns(currentBankBalance);
            _bank.Object.Deposit(accountType, currentBankBalance, new Guid("{1241B14C-C962-4DBA-B080-260412CA7435}"));
        }

        private void SetupTransaction()
        {
            _scopedTransaction.Setup(x => x.Complete());

            _transactionCoordinator
                .Setup(m => m.RequestTransaction(RequestorId, RequestTimeoutLength, TransactionType.Write))
                .Returns(TransactionId);

            IReadOnlyCollection<CoinInTransaction> emptyTrans =
                new ReadOnlyCollection<CoinInTransaction>(new[] { new CoinInTransaction() });
            _transactionHistory.Setup(m => m.RecallTransactions<CoinInTransaction>()).Returns(emptyTrans);
        }

        private void SetupStorage()
        {
            _storageManager.Setup(x => x.BlockExists(It.IsAny<string>())).Returns(true);
            _storageManager.Setup(x => x.GetBlock(It.IsAny<string>())).Returns(_persistentStorageAccessor.Object);
            _storageManager.Setup(x => x.ScopedTransaction()).Returns(_scopedTransaction.Object);
            _persistentStorageAccessor.Setup(x => x.StartTransaction())
                .Returns(new Mock<IPersistentStorageTransaction>().Object);
        }
    }
}
