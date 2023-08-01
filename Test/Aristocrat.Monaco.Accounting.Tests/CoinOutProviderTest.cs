namespace Aristocrat.Monaco.Accounting.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using Accounting.Hopper;
    using Application.Contracts;
    using Aristocrat.Monaco.Accounting.Contracts.CoinAcceptor;
    using Aristocrat.Monaco.Accounting.Contracts.Hopper;
    using Aristocrat.Monaco.Hardware.Contracts.CoinAcceptor;
    using Aristocrat.Monaco.Hardware.Contracts.Hopper;
    using Contracts;
    using Hardware.Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;
    using HardwareFaultEvent = Hardware.Contracts.Hopper.HardwareFaultEvent;

    [TestClass]
    public class CoinOutProviderTest
    {
        private const int RequestTimeoutLength = 1000; // It's in milliseconds
        private static readonly Guid RequestorId = new Guid("{EBB8B24C-771F-474A-8315-4F25DDBDBEA3}");
        private static readonly Guid TransactionId = new Guid("{0241B14C-C962-4DBA-B080-260412CA7435}");
        private Mock<IHopper> _hopperService;
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
        private Mock<IMeter> _trueCoinOutCount;
        private Mock<IMeter> _excessCoinOutCount;

        private Mock<IDisposable> _disposable;

        private CoinOutProvider _target;
        private dynamic _accessor;
        private ITransaction _result;
        private Action<CoinOutEvent> _onCoinOutEvent;
        private Action<HardwareFaultEvent> _hardwareFaultEvent;
        private Mock<IPropertiesManager> _propertiesManager;

        [TestInitialize]
        public void TestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();

            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _hopperService = new Mock<IHopper>(MockBehavior.Default);
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
            _propertiesManager.Setup(m => m.GetProperty(HardwareConstants.CoinValue, 100000L)).Returns((long)100000L);

            _trueCoinOutCount = new Mock<IMeter>(MockBehavior.Strict);
            _excessCoinOutCount = new Mock<IMeter>(MockBehavior.Strict);

            _meterManager.Setup(x => x.GetMeter(AccountingMeters.TrueCoinOutCount))
                .Returns(_trueCoinOutCount.Object);
            _meterManager.Setup(x => x.GetMeter(AccountingMeters.CoinToCashBoxCount))
                .Returns(_excessCoinOutCount.Object);
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
        ///     Tests for Coin Out Provider constructor
        /// </summary>
        [TestMethod]
        public void CoinOutProviderConstructorSetupTest()
        {
            const long currentBankBalance = 999;
            SetupCoinOutProvider(currentBankBalance);

            _target.Initialize();

            _eventBus.Verify();
        }

        /// <summary>
        ///     Tests for Coin Out Provider constructor
        /// </summary>
        [TestMethod]
        public void CoinOutEventTest()
        {
            const long currentBankBalance = 999;
            _trueCoinOutCount.Setup(x => x.Increment(1)).Verifiable();
            SetupCoinOutProvider(currentBankBalance);
            _target.Initialize();
            _accessor._transactionGuid = TransactionId;
            Assert.IsNotNull(_onCoinOutEvent);

            var coinOutEvent = CoinOutEventSetupTest(100000);
            _onCoinOutEvent(coinOutEvent);

            _scopedTransaction.Verify(x => x.Complete());
            _bank.Verify(x => x.Withdraw(AccountType.Cashable, 100000L, TransactionId), Times.Once);

            _hopperService.Verify(x => x.StopHopperMotor(), Times.Once);
        }

        /// <summary>
        ///     Tests for Coin Out Provider constructor
        /// </summary>
        [TestMethod]
        public void CoinOutEventTestTokenValue2()
        {
            _propertiesManager.Setup(m => m.GetProperty(HardwareConstants.CoinValue, 100000L)).Returns((long)200000L);
            const long currentBankBalance = 999;
            _trueCoinOutCount.Setup(x => x.Increment(1)).Verifiable();
            SetupCoinOutProvider(currentBankBalance);
            _bank.Object.Withdraw(AccountType.Cashable, 200000L, new Guid("{1241B14C-C962-4DBA-B080-260412CA7436}"));
            _target.Initialize();
            _accessor._transactionGuid = TransactionId;
            Assert.IsNotNull(_onCoinOutEvent);

            var coinOutEvent = CoinOutEventSetupTest(200000);
            _onCoinOutEvent(coinOutEvent);

            _scopedTransaction.Verify(x => x.Complete());
            _bank.Verify(x => x.Withdraw(AccountType.Cashable, 200000L, TransactionId), Times.Once);

            _hopperService.Verify(x => x.StopHopperMotor(), Times.Once);
        }

        /// <summary>
        ///     Tests for Coin Out Provider constructor
        /// </summary>
        [TestMethod]
        public void CoinInEventWithEmptyTransactionIdTest()
        {
            const long currentBankBalance = 999;
            SetupCoinOutProvider(currentBankBalance);
            _transactionCoordinator
                .Setup(m => m.RequestTransaction(RequestorId, RequestTimeoutLength, TransactionType.Write))
                .Returns(Guid.Empty);

            _target.Initialize();

            Assert.IsNotNull(_onCoinOutEvent);

            var coinOutEvent = CoinOutEventSetupTest(100000);
            _onCoinOutEvent(coinOutEvent);

            _scopedTransaction.Verify(x => x.Complete(), Times.Never);
            _bank.Verify(x => x.Withdraw(AccountType.Cashable, 100000, Guid.Empty), Times.Never);
            _hopperService.Verify(x => x.StopHopperMotor(), Times.Never);
        }

        /// <summary>
        ///     Tests for Illegal Coin Out.
        /// </summary>
        [TestMethod]
        public void IllegalCoinOutEventTest()
        {
            const long currentBankBalance = 999;
            _excessCoinOutCount.Setup(x => x.Increment(1)).Verifiable();
            SetupCoinOutProvider(currentBankBalance);

            _target.Initialize();
            _accessor._transactionGuid = TransactionId;
            Assert.IsNotNull(_hardwareFaultEvent);

            _hardwareFaultEvent(HardwareFaultEventSetupTest());

            _transactionHistory.Verify(x => x.UpdateTransaction(It.IsAny<CoinOutTransaction>()), Times.Once);
            _scopedTransaction.Verify(x => x.Complete(), Times.Once);
        }

        private CoinOutEvent CoinOutEventSetupTest(int coinValue)
        {
            var coin = new Coin { Value = coinValue };
            var coinInEvent = new CoinOutEvent(coin);
            return coinInEvent;
        }

        private HardwareFaultEvent HardwareFaultEventSetupTest()
        {
            var faultEvent = new HardwareFaultEvent(Hardware.Contracts.Hopper.HopperFaultTypes.IllegalCoinOut);
            return faultEvent;
        }

        private CoinOutProvider GetTarget()
        {
            var target = new CoinOutProvider(
                _bank.Object,
                _transactionHistory.Object,
                _meterManager.Object,
                _storageManager.Object,
                _eventBus.Object,
                _propertiesManager.Object,
                _iidProvider.Object,
                _hopperService.Object,
                _messageDisplay.Object
                );

            return target;
        }

        private void SetupCoinOutProvider(long currentBankBalance)
        {
            SetupStorage();

            const long bankLimit = 1000;
            SetupBank(currentBankBalance, bankLimit);

            SetupTransaction();

            _target = GetTarget();
            _accessor = new DynamicPrivateObject(_target);
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<CoinOutEvent>>()))
                   .Callback<object, Action<CoinOutEvent>>(
                       (tar, func) => { _onCoinOutEvent = func; }).Verifiable();

            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<HardwareFaultEvent>>()))
                   .Callback<object, Action<HardwareFaultEvent>>(
                       (tar, func) => { _hardwareFaultEvent = func; }).Verifiable();

            _iidProvider.Setup(m => m.GetNextLogSequence<CoinInTransaction>()).Returns(1);

            var meterMock = new Mock<IMeter>(MockBehavior.Default);
            _meterManager.Setup(m => m.GetMeter(It.IsAny<string>())).Returns(meterMock.Object);
        }

        private void SetupBank(long currentBankBalance, long bankLimit)
        {
            var accountType = AccountType.Cashable;
            _bank.Setup(x => x.Limit).Returns(bankLimit);
            _bank.Setup(x => x.QueryBalance()).Returns(currentBankBalance);
            _bank.Object.Withdraw(accountType, 100000L, new Guid("{1241B14C-C962-4DBA-B080-260412CA7436}"));
        }

        private void SetupTransaction()
        {
            _scopedTransaction.Setup(x => x.Complete());

            _transactionCoordinator
                .Setup(m => m.RequestTransaction(RequestorId, RequestTimeoutLength, TransactionType.Write))
                .Returns(TransactionId);

            IReadOnlyCollection<CoinOutTransaction> coinOutTrans =
                new ReadOnlyCollection<CoinOutTransaction>(new[] { new CoinOutTransaction() { BankTransactionId = TransactionId, TransferredCashableAmount = 0, AuthorizedCashableAmount = 100000L } });
            _transactionHistory.Setup(m => m.RecallTransactions<CoinOutTransaction>()).Returns(coinOutTrans);
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
