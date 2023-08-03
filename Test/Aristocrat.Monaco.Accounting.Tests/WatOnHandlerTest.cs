namespace Aristocrat.Monaco.Accounting.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Contracts;
    using Contracts.Wat;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class WatOnHandlerTest
    {
        private const int WaitTime = 2000;
        private const string HostId = "unit-test";

        private WatOnHandler _target;
        private Mock<IBank> _bank;
        private Mock<IEventBus> _eventBus;
        private Mock<IMeterManager> _meterManager;
        private Mock<IPersistentStorageManager> _persistentStorage;
        private Mock<IPropertiesManager> _propertiesManger;
        private Mock<IMessageDisplay> _messageDisplay;

        private Mock<ITransactionCoordinator> _transactionCoordinator;
        private Mock<ITransactionHistory> _transactionHistory;
        private Mock<IWatTransferOnProvider> _transferOnProvider;
        private Mock<IFundTransferProvider> _fundTransferProvider;
        private Mock<IMeter> _cashableAmountMeter;
        private Mock<IMeter> _cashableCountMeter;
        private Mock<IMeter> _promoAmountMeter;
        private Mock<IMeter> _promoCountMeter;
        private Mock<IMeter> _nonCashAmountMeter;
        private Mock<IMeter> _nonCashCountMeter;
        private Mock<IScopedTransaction> _scopedTransaction;

        private Mock<IDisposable> _disposable;

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _transferOnProvider = MoqServiceManager.CreateAndAddService<IWatTransferOnProvider>(MockBehavior.Default);
            _fundTransferProvider = MoqServiceManager.CreateAndAddService<IFundTransferProvider>(MockBehavior.Default);
            _eventBus = new Mock<IEventBus>(MockBehavior.Default);
            _bank = new Mock<IBank>(MockBehavior.Default);
            _meterManager = new Mock<IMeterManager>(MockBehavior.Default);
            _persistentStorage = new Mock<IPersistentStorageManager>(MockBehavior.Default);
            _transactionCoordinator = new Mock<ITransactionCoordinator>(MockBehavior.Default);
            _propertiesManger = new Mock<IPropertiesManager>(MockBehavior.Default);
            _transactionHistory = new Mock<ITransactionHistory>(MockBehavior.Default);
            _cashableCountMeter = new Mock<IMeter>(MockBehavior.Default);
            _cashableAmountMeter = new Mock<IMeter>(MockBehavior.Default);
            _promoAmountMeter = new Mock<IMeter>(MockBehavior.Default);
            _promoCountMeter = new Mock<IMeter>(MockBehavior.Default);
            _nonCashCountMeter = new Mock<IMeter>(MockBehavior.Default);
            _nonCashAmountMeter = new Mock<IMeter>(MockBehavior.Default);
            _scopedTransaction = new Mock<IScopedTransaction>(MockBehavior.Default);
            _messageDisplay= new Mock<IMessageDisplay>(MockBehavior.Default);

            _disposable = new Mock<IDisposable>(MockBehavior.Default);
            _disposable.Setup(d => d.Dispose()).Verifiable();

            _meterManager.Setup(x => x.GetMeter(AccountingMeters.WatOnCashableAmount))
                .Returns(_cashableAmountMeter.Object);
            _meterManager.Setup(x => x.GetMeter(AccountingMeters.WatOnCashableCount))
                .Returns(_cashableCountMeter.Object);
            _meterManager.Setup(x => x.GetMeter(AccountingMeters.WatOnCashablePromoAmount))
                .Returns(_promoAmountMeter.Object);
            _meterManager.Setup(x => x.GetMeter(AccountingMeters.WatOnCashablePromoCount))
                .Returns(_promoCountMeter.Object);
            _meterManager.Setup(x => x.GetMeter(AccountingMeters.WatOnNonCashableAmount))
                .Returns(_nonCashAmountMeter.Object);
            _meterManager.Setup(x => x.GetMeter(AccountingMeters.WatOnNonCashableCount))
                .Returns(_nonCashCountMeter.Object);

            _persistentStorage.Setup(x => x.ScopedTransaction()).Returns(_scopedTransaction.Object);
            _scopedTransaction.Setup(x => x.Complete());
            _fundTransferProvider.Setup(x => x.GetWatTransferOnProvider(false)).Returns(_transferOnProvider.Object);
            _target = new WatOnHandler(
                _eventBus.Object,
                _transactionCoordinator.Object,
                _transactionHistory.Object,
                _persistentStorage.Object,
                _propertiesManger.Object,
                _meterManager.Object,
                _messageDisplay.Object,
                _bank.Object,
                _fundTransferProvider.Object);
        }

        [TestCleanup]
        public void CleanUp()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void ServiceTypeTest()
        {
            Assert.AreEqual(1, _target.ServiceTypes.Count);
            Assert.IsTrue(_target.ServiceTypes.Contains(typeof(IWatTransferOnHandler)));
        }

        [TestMethod]
        public void NameTest()
        {
            Assert.AreEqual(typeof(IWatTransferOnHandler).ToString(), _target.Name);
        }

        [TestMethod]
        public void RequestTransferWhenProviderCantTransferTest()
        {
            _transferOnProvider.Setup(x => x.CanTransfer).Returns(false);
            Assert.IsFalse(_target.RequestTransfer(Guid.NewGuid(), HostId, 100, 200, 300, false));
        }

        [TestMethod]
        public void NonHostIdFailsTransfers()
        {
            Assert.IsFalse(_target.RequestTransfer(Guid.NewGuid(), null, 100, 200, 300, false));
        }

        [TestMethod]
        public void UnableToGetTransactionIdFailsTransfer()
        {
            _transferOnProvider.Setup(x => x.CanTransfer).Returns(true);
            _transactionCoordinator
                .Setup(x => x.RequestTransaction(It.IsAny<Guid>(), It.IsAny<int>(), TransactionType.Write))
                .Returns(Guid.Empty);
            Assert.IsFalse(_target.RequestTransfer(Guid.Empty, HostId, 100, 200, 300, false));
        }
        
        [TestMethod]
        public void CanRecoverTest()
        {
            var guid = Guid.NewGuid();
            _transactionHistory.Setup(x => x.RecallTransactions<WatOnTransaction>()).Returns(
                new List<WatOnTransaction>
                {
                    new WatOnTransaction
                    {
                        BankTransactionId = guid,
                        Status = WatStatus.Initiated
                    }
                });

            Assert.IsFalse(_target.CanRecover(guid));
        }

        [TestMethod]
        public void AcknowledgeTransferTest()
        {
            const long transactionId = 100;
            var transaction = new WatOnTransaction() { Status = WatStatus.Committed, TransactionId = transactionId };
            _transactionHistory.Setup(x => x.RecallTransactions<WatOnTransaction>())
                .Returns(new List<WatOnTransaction> { (WatOnTransaction)transaction.Clone() });
            _transactionHistory.Setup(
                x => x.UpdateTransaction(
                    It.Is<WatOnTransaction>(
                        t => t.TransactionId == transactionId && t.Status == WatStatus.Complete))).Verifiable();

            _target.AcknowledgeTransfer(transaction);
            _transactionHistory.Verify();
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void AcknowledgeTransferNullTransactionTest()
        {
            _target.AcknowledgeTransfer(null);
        }

        [TestMethod]
        public void RequestTransferSuccessfulTest()
        {
            const long cashableAmount = 100;
            const long promoAmount = 200;
            const long nonCashAmount = 300;
            var expectedGuid = Guid.NewGuid();

            var waiter = new ManualResetEvent(false);
            _transferOnProvider.Setup(x => x.CanTransfer).Returns(true);
            _eventBus.Setup(x => x.Publish(It.IsAny<WatOnStartedEvent>()));
            _eventBus.Setup(
                x => x.Publish(
                    It.Is<WatOnCompleteEvent>(
                        evt => evt.Transaction.TransferredCashableAmount == cashableAmount &&
                               evt.Transaction.TransferredPromoAmount == promoAmount &&
                               evt.Transaction.TransferredNonCashAmount == nonCashAmount &&
                               evt.Transaction.RequestId == HostId))).Callback(() => waiter.Set());
            _transferOnProvider.Setup(x => x.InitiateTransfer(
                It.Is<WatOnTransaction>(
                    transaction => transaction.CashableAmount == cashableAmount &&
                                   transaction.PromoAmount == promoAmount &&
                                   transaction.NonCashAmount == nonCashAmount &&
                                   transaction.RequestId == HostId)))
                .Returns(Task.FromResult(true))
                .Callback((WatOnTransaction transaction) =>
                {
                    transaction.AuthorizedCashableAmount = cashableAmount;
                    transaction.AuthorizedPromoAmount = promoAmount;
                    transaction.AuthorizedNonCashAmount = nonCashAmount;
                });

            _transferOnProvider.Setup(
                x => x.CommitTransfer(
                    It.Is<WatOnTransaction>(
                        transaction => transaction.TransferredCashableAmount == cashableAmount &&
                                       transaction.TransferredPromoAmount == promoAmount &&
                                       transaction.TransferredNonCashAmount == nonCashAmount &&
                                       transaction.RequestId == HostId)))
                .Returns(Task.CompletedTask);

            _propertiesManger
                .Setup(x => x.GetProperty(AccountingConstants.AllowCreditsInAboveMaxCredit, It.IsAny<bool>()))
                .Returns(true);
            _bank.Setup(x => x.Deposit(AccountType.Cashable, cashableAmount, expectedGuid)).Verifiable();
            _cashableCountMeter.Setup(x => x.Increment(1)).Verifiable();
            _cashableAmountMeter.Setup(x => x.Increment(cashableAmount)).Verifiable();
            _bank.Setup(x => x.Deposit(AccountType.Promo, promoAmount, expectedGuid)).Verifiable();
            _promoCountMeter.Setup(x => x.Increment(1)).Verifiable();
            _promoAmountMeter.Setup(x => x.Increment(promoAmount)).Verifiable();
            _bank.Setup(x => x.Deposit(AccountType.NonCash, nonCashAmount, expectedGuid)).Verifiable();
            _nonCashCountMeter.Setup(x => x.Increment(1)).Verifiable();
            _nonCashAmountMeter.Setup(x => x.Increment(nonCashAmount)).Verifiable();
            _transactionHistory.Setup(x => x.UpdateTransaction(It.IsAny<WatOnTransaction>()));

            Assert.IsTrue(_target.RequestTransfer(expectedGuid, HostId, cashableAmount, promoAmount, nonCashAmount, false));
            Assert.IsTrue(waiter.WaitOne(WaitTime));

            _bank.Verify();
            _cashableAmountMeter.Verify();
            _cashableCountMeter.Verify();
            _promoAmountMeter.Verify();
            _promoCountMeter.Verify();
            _nonCashAmountMeter.Verify();
            _nonCashCountMeter.Verify();
        }

        [TestMethod]
        public void RequestTransferSuccessfulZeroTest()
        {
            const long cashableAmount = 100;
            const long promoAmount = 200;
            const long nonCashAmount = 300;
            var expectedGuid = Guid.NewGuid();

            var waiter = new ManualResetEvent(false);
            _transferOnProvider.Setup(x => x.CanTransfer).Returns(true);
            _eventBus.Setup(x => x.Publish(It.IsAny<WatOnStartedEvent>()));
            _eventBus.Setup(
                x => x.Publish(
                    It.Is<WatOnCompleteEvent>(
                        evt => evt.Transaction.TransferredCashableAmount == 0 &&
                               evt.Transaction.TransferredPromoAmount == 0 &&
                               evt.Transaction.TransferredNonCashAmount == 0 &&
                               evt.Transaction.RequestId == HostId))).Callback(() => waiter.Set());
            _transferOnProvider.Setup(x => x.InitiateTransfer(
                It.Is<WatOnTransaction>(
                    transaction => transaction.CashableAmount == cashableAmount &&
                                   transaction.PromoAmount == promoAmount &&
                                   transaction.NonCashAmount == nonCashAmount &&
                                   transaction.RequestId == HostId)))
                .Returns(Task.FromResult(true))
                .Callback((WatOnTransaction transaction) =>
                {
                    transaction.AuthorizedCashableAmount = 0;
                    transaction.AuthorizedPromoAmount = 0;
                    transaction.AuthorizedNonCashAmount = 0;
                });

            _transferOnProvider.Setup(
                x => x.CommitTransfer(
                    It.Is<WatOnTransaction>(
                        transaction => transaction.TransferredCashableAmount == 0 &&
                                       transaction.TransferredPromoAmount == 0 &&
                                       transaction.TransferredNonCashAmount == 0 &&
                                       transaction.RequestId == HostId)))
                .Returns(Task.CompletedTask);

            _propertiesManger
                .Setup(x => x.GetProperty(AccountingConstants.AllowCreditsInAboveMaxCredit, It.IsAny<bool>()))
                .Returns(true);
            _transactionHistory.Setup(x => x.UpdateTransaction(It.IsAny<WatOnTransaction>()));

            Assert.IsTrue(_target.RequestTransfer(expectedGuid, HostId, cashableAmount, promoAmount, nonCashAmount, false));
            Assert.IsTrue(waiter.WaitOne(WaitTime));
        }

        [TestMethod]
        public void FailedInitializeTest()
        {
            const long cashableAmount = 100;
            const long promoAmount = 200;
            const long nonCashAmount = 300;
            var expectedGuid = Guid.NewGuid();

            var waiter = new ManualResetEvent(false);
            _transferOnProvider.Setup(x => x.CanTransfer).Returns(true);
            _eventBus.Setup(x => x.Publish(It.IsAny<WatOnStartedEvent>()));
            _eventBus.Setup(
                x => x.Publish(
                    It.Is<WatOnCompleteEvent>(
                        evt => evt.Transaction.TransferredCashableAmount == 0 &&
                               evt.Transaction.TransferredPromoAmount == 0 &&
                               evt.Transaction.TransferredNonCashAmount == 0 &&
                               evt.Transaction.RequestId == HostId))).Callback(() => waiter.Set());
            _transferOnProvider.Setup(x => x.InitiateTransfer(
                It.Is<WatOnTransaction>(
                    transaction => transaction.CashableAmount == cashableAmount &&
                                   transaction.PromoAmount == promoAmount &&
                                   transaction.NonCashAmount == nonCashAmount &&
                                   transaction.RequestId == HostId)))
                .Returns(Task.FromResult(false));
            _transactionHistory.Setup(x => x.UpdateTransaction(It.IsAny<WatOnTransaction>()));

            Assert.IsTrue(_target.RequestTransfer(expectedGuid, HostId, cashableAmount, promoAmount, nonCashAmount, false));
            Assert.IsTrue(waiter.WaitOne(WaitTime));
        }

        [TestMethod]
        public void OverBankLimitTest()
        {
            const long cashableAmount = 100;
            const long promoAmount = 200;
            const long nonCashAmount = 300;
            var expectedGuid = Guid.NewGuid();

            var waiter = new ManualResetEvent(false);
            _transferOnProvider.Setup(x => x.CanTransfer).Returns(true);
            _eventBus.Setup(x => x.Publish(It.IsAny<WatOnStartedEvent>()));
            _eventBus.Setup(
                x => x.Publish(
                    It.Is<WatOnCompleteEvent>(
                        evt => evt.Transaction.TransferredCashableAmount == 0 &&
                               evt.Transaction.TransferredPromoAmount == 0 &&
                               evt.Transaction.TransferredNonCashAmount == 0 &&
                               evt.Transaction.RequestId == HostId))).Callback(() => waiter.Set());
            _transferOnProvider.Setup(x => x.InitiateTransfer(
                    It.Is<WatOnTransaction>(
                        transaction => transaction.CashableAmount == cashableAmount &&
                                       transaction.PromoAmount == promoAmount &&
                                       transaction.NonCashAmount == nonCashAmount &&
                                       transaction.RequestId == HostId)))
                .Returns(Task.FromResult(true))
                .Callback((WatOnTransaction transaction) =>
                {
                    transaction.AuthorizedCashableAmount = cashableAmount;
                    transaction.AuthorizedPromoAmount = promoAmount;
                    transaction.AuthorizedNonCashAmount = nonCashAmount;
                });

            _propertiesManger
                .Setup(x => x.GetProperty(AccountingConstants.AllowCreditsInAboveMaxCredit, It.IsAny<bool>()))
                .Returns(false);
            _bank.Setup(x => x.Limit).Returns(1000);
            _bank.Setup(x => x.QueryBalance()).Returns(999);
            _transactionHistory.Setup(x => x.UpdateTransaction(It.IsAny<WatOnTransaction>()));

            Assert.IsTrue(_target.RequestTransfer(expectedGuid, HostId, cashableAmount, promoAmount, nonCashAmount, false));
            Assert.IsTrue(waiter.WaitOne(WaitTime));
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullTransactionCancelTest()
        {
            _target.CancelTransfer((WatOnTransaction)null, 0);
        }

        [TestMethod]
        public void NoTransactionToCancelTest()
        {
            _transactionHistory.Setup(x => x.RecallTransactions<WatOnTransaction>())
                .Returns(new List<WatOnTransaction>());
            Assert.IsFalse(_target.CancelTransfer(HostId, 0));
        }

        [DataRow(WatStatus.Complete, false)]
        [DataRow(WatStatus.Committed, false)]
        [DataRow(WatStatus.Authorized, false)]
        [DataRow(WatStatus.Initiated, true)]
        [DataRow(WatStatus.RequestReceived, true)]
        [DataTestMethod]
        public void CancelTest(WatStatus status, bool cancelled)
        {
            const int hostException = 100;
            var waiter = new ManualResetEvent(false);

            _transactionHistory.Setup(x => x.RecallTransactions<WatOnTransaction>())
                .Returns(
                    new List<WatOnTransaction>
                    {
                        new WatOnTransaction(1, DateTime.Now, 100, 200, 300, true, HostId)
                        {
                            Status = status,
                            TransactionId = 200
                        }
                    });
            if (cancelled)
            {
                _transactionHistory.Setup(
                    x => x.UpdateTransaction(
                        It.Is<WatOnTransaction>(
                            transaction => transaction.CashableAmount == 0 &&
                                           transaction.PromoAmount == 0 &&
                                           transaction.NonCashAmount == 0 &&
                                           transaction.Status == WatStatus.CancelReceived))).Verifiable();
                _transferOnProvider.Setup(x => x.CanTransfer).Returns(true);
                _propertiesManger
                    .Setup(x => x.GetProperty(AccountingConstants.AllowCreditsInAboveMaxCredit, It.IsAny<bool>()))
                    .Returns(true);
                _eventBus.Setup(x => x.Publish(It.IsAny<WatOnStartedEvent>()));
                _eventBus.Setup(
                    x => x.Publish(
                        It.Is<WatOnCompleteEvent>(
                            evt => evt.Transaction.TransferredCashableAmount == 0 &&
                                   evt.Transaction.TransferredPromoAmount == 0 &&
                                   evt.Transaction.TransferredNonCashAmount == 0 &&
                                   evt.Transaction.RequestId == HostId))).Callback(() => waiter.Set());
            }
            else
            {
                waiter.Set();
            }

            Assert.AreEqual(cancelled, _target.CancelTransfer(HostId, hostException));
            Assert.IsTrue(waiter.WaitOne(WaitTime));

            _transactionHistory.Verify();
        }

        [TestMethod]
        public void NothingToRecoverTest()
        {
            _transactionHistory.Setup(x => x.RecallTransactions<WatOnTransaction>())
                .Returns(new List<WatOnTransaction>());
            var recover = _target.Recover(Guid.NewGuid(), CancellationToken.None);
            Assert.IsTrue(recover.Wait(WaitTime));
            Assert.IsFalse(recover.Result);
        }
        
        [TestMethod]
        public void InitiatedRecoveryTest()
        {
            const long cashableAmount = 100;
            const long promoAmount = 200;
            const long nonCashAmount = 300;
            var expectedGuid = Guid.NewGuid();

            _transactionHistory.Setup(x => x.RecallTransactions<WatOnTransaction>())
                .Returns(
                    new List<WatOnTransaction>
                    {
                        new WatOnTransaction(1, DateTime.Now, cashableAmount, promoAmount, nonCashAmount, true, HostId)
                        {
                            Status = WatStatus.Initiated,
                            BankTransactionId = expectedGuid
                        }
                    });

            var recover = _target.Recover(expectedGuid, CancellationToken.None);
            Assert.IsFalse(recover.Result);
        }

        [DataRow(WatStatus.Authorized)]
        [DataRow(WatStatus.CancelReceived)]
        [DataRow(WatStatus.Initiated)]
        [DataRow(WatStatus.RequestReceived)]
        [DataTestMethod]
        public void InitializationCancelsActiveTransactions(WatStatus watStatus)
        {
            var waiter = new ManualResetEvent(false);
            const long cashableAmount = 100;
            const long promoAmount = 200;
            const long nonCashAmount = 300;
            var expectedGuid = Guid.NewGuid();

            _transactionHistory.Setup(x => x.RecallTransactions<WatOnTransaction>())
                .Returns(
                    new List<WatOnTransaction>
                    {
                        new WatOnTransaction(1, DateTime.Now, cashableAmount, promoAmount, nonCashAmount, true, HostId)
                        {
                            Status = watStatus,
                            TransactionId = 200,
                            BankTransactionId = expectedGuid,
                            AuthorizedCashableAmount = cashableAmount,
                            AuthorizedPromoAmount = promoAmount,
                            AuthorizedNonCashAmount = nonCashAmount
                        }
                    });

            _transactionHistory.Setup(
                x => x.UpdateTransaction(
                    It.Is<WatOnTransaction>(
                        transaction => transaction.TransferredCashableAmount == 0 &&
                                       transaction.TransferredPromoAmount == 0 &&
                                       transaction.TransferredNonCashAmount == 0 &&
                                       transaction.Status == WatStatus.Committed))).Verifiable();
            _transferOnProvider.Setup(x => x.CanTransfer).Returns(true);
            _propertiesManger
                .Setup(x => x.GetProperty(AccountingConstants.AllowCreditsInAboveMaxCredit, It.IsAny<bool>()))
                .Returns(true);

            _target.Initialize();
            _transactionHistory.Verify();
        }
    }
}