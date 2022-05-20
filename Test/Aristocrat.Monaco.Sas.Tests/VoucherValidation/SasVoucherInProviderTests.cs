namespace Aristocrat.Monaco.Sas.Tests.VoucherValidation
{
    using System;
    using Aristocrat.Sas.Client;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.VoucherValidation;
    using Accounting.Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Sas.Contracts;
    using System.Collections.Generic;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using System.Threading;
    using Aristocrat.Monaco.Sas.Ticketing;
    using Aristocrat.Monaco.Sas.Storage.Models;
    using System.Threading.Tasks;

    [TestClass]
    public class SasVoucherInProviderTests
    {
        private const int WaitTime = 1000;

        private Mock<IEventBus> _bus;
        private Mock<ISasExceptionHandler> _exceptionHandler;
        private SasVoucherInProvider _target;
        private Mock<ITransactionHistory> _transactionHistory;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<ITicketingCoordinator> _ticketCoordinator;
        private Mock<IBank> _bank;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _bus = new Mock<IEventBus>(MockBehavior.Default);
            _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Default);
            _transactionHistory = new Mock<ITransactionHistory>(MockBehavior.Default);
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
            _ticketCoordinator = new Mock<ITicketingCoordinator>(MockBehavior.Default);
            _ticketCoordinator.Setup(x => x.Save(It.IsAny<TicketStorageData>())).Returns(Task.CompletedTask);
            _bank = new Mock<IBank>(MockBehavior.Default);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullExceptionHandlerTest()
        {
            _target = new SasVoucherInProvider(
                null,
                _transactionHistory.Object,
                _propertiesManager.Object,
                _bus.Object,
                _ticketCoordinator.Object,
                _bank.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullTransactionHistoryTest()
        {
            _target = new SasVoucherInProvider(
                _exceptionHandler.Object,
                null,
                _propertiesManager.Object,
                _bus.Object,
                _ticketCoordinator.Object,
                _bank.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPropertiesManagerTest()
        {
            _target = new SasVoucherInProvider(
                _exceptionHandler.Object,
                _transactionHistory.Object,
                null,
                _bus.Object,
                _ticketCoordinator.Object,
                _bank.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullEventBusTest()
        {
            _target = new SasVoucherInProvider(
                _exceptionHandler.Object,
                _transactionHistory.Object,
                _propertiesManager.Object,
                null,
                _ticketCoordinator.Object,
                _bank.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullTicketCoordinatorTest()
        {
            _target = new SasVoucherInProvider(
               _exceptionHandler.Object,
               _transactionHistory.Object,
               _propertiesManager.Object,
               _bus.Object,
               null,
               _bank.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullBankTest()
        {
            _target = new SasVoucherInProvider(
                _exceptionHandler.Object,
                _transactionHistory.Object,
                _propertiesManager.Object,
                _bus.Object,
                _ticketCoordinator.Object,
                null);
        }

        [TestMethod]
        public void ValidationTicketStateTest()
        {
            var waiter = new ManualResetEvent(false);
            var barcode = "1234567890";
            var transaction = new VoucherInTransaction(0, new DateTime(), barcode);
            IReadOnlyCollection<VoucherInTransaction> transactions = new List<VoucherInTransaction>
            {
                new VoucherInTransaction()
            };

            _ticketCoordinator.Setup(x => x.GetData()).Returns(new TicketStorageData());
            _transactionHistory.Setup(x => x.RecallTransactions<VoucherInTransaction>()).Returns(transactions);

            _target = new SasVoucherInProvider(
                _exceptionHandler.Object,
                _transactionHistory.Object,
                _propertiesManager.Object,
                _bus.Object,
                _ticketCoordinator.Object,
                _bank.Object);

            _exceptionHandler.Setup(x => x.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.TicketHasBeenInserted)))
                .Callback(
                () =>
                {
                    waiter.Set();
                });

            var validateTicketIn = _target.ValidationTicket(transaction);
            Assert.IsTrue(waiter.WaitOne(WaitTime));
            Assert.AreEqual(_target.CurrentState, SasVoucherInState.ValidationRequestPending);

            _target.Dispose();
        }

        [TestMethod]
        public void ValidationTicketDenyTest()
        {
            var barcode = "1234567890";
            var transaction = new VoucherInTransaction(0, new DateTime(), barcode);
            IReadOnlyCollection<VoucherInTransaction> transactions = new List<VoucherInTransaction>
            {
                new VoucherInTransaction()
            };

            _ticketCoordinator.Setup(x => x.GetData()).Returns(new TicketStorageData());
            _transactionHistory.Setup(x => x.RecallTransactions<VoucherInTransaction>()).Returns(transactions);

            _target = new SasVoucherInProvider(
                _exceptionHandler.Object,
                _transactionHistory.Object,
                _propertiesManager.Object,
                _bus.Object,
                _ticketCoordinator.Object,
                _bank.Object);

            var validateTicketIn = _target.ValidationTicket(transaction);
            _target.DenyTicket();

            Assert.IsTrue(validateTicketIn.Wait(WaitTime));
            Assert.AreEqual(_target.CurrentState, SasVoucherInState.Idle);
            Assert.AreEqual(_target.CurrentTicketInfo.Amount, 0UL);

            _target.Dispose();
        }

        [TestMethod]
        public void RequestValidationDataIdleStateTest()
        {
            var accessorTransaction = new Mock<IPersistentStorageTransaction>();
            IReadOnlyCollection<VoucherInTransaction> transactions = new List<VoucherInTransaction>
            {
                new VoucherInTransaction()
            };

            _ticketCoordinator.Setup(x => x.GetData()).Returns(new TicketStorageData());
            _transactionHistory.Setup(x => x.RecallTransactions<VoucherInTransaction>()).Returns(transactions);

            _target = new SasVoucherInProvider(
                _exceptionHandler.Object,
                _transactionHistory.Object,
                _propertiesManager.Object,
                _bus.Object,
                _ticketCoordinator.Object,
                _bank.Object);

            var dataResponse = _target.RequestValidationData();

            Assert.AreEqual(_target.CurrentState, SasVoucherInState.Idle);
            Assert.IsNull(dataResponse);

            _target.Dispose();
        }

        [TestMethod]
        public void RequestValidationDataStateTest()
        {
            var waiter = new ManualResetEvent(false);
            var barcode = "1234567890";
            var transaction = new VoucherInTransaction(0, new DateTime(), barcode);
            transaction.TransactionId = 10;
            IReadOnlyCollection<VoucherInTransaction> transactions = new List<VoucherInTransaction>
            {
                new VoucherInTransaction()
            };

            _ticketCoordinator.Setup(x => x.GetData()).Returns(new TicketStorageData());
            _transactionHistory.Setup(x => x.RecallTransactions<VoucherInTransaction>()).Returns(transactions);

            _target = new SasVoucherInProvider(
                _exceptionHandler.Object,
                _transactionHistory.Object,
                _propertiesManager.Object,
                _bus.Object,
                _ticketCoordinator.Object,
                _bank.Object);

            _exceptionHandler.Setup(x => x.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.TicketHasBeenInserted)))
                .Callback(
                () =>
                {
                    var dataResponse = _target.RequestValidationData();
                    Assert.IsNotNull(dataResponse);
                    Assert.AreEqual(dataResponse.Barcode, barcode);
                    Assert.AreEqual(dataResponse.ParsingCode, ParsingCode.Bcd);
                });

            _ticketCoordinator.Setup(x => x.Save(It.Is<TicketStorageData>(t => t.VoucherInState == SasVoucherInState.ValidationDataPending)))
                .Returns(Task.CompletedTask)
                .Callback(() => waiter.Set());

            var validateTicketIn = _target.ValidationTicket(transaction);

            Assert.IsTrue(waiter.WaitOne(WaitTime));
            Assert.AreEqual(_target.CurrentTicketInfo.Barcode, barcode);
            Assert.AreEqual(_target.CurrentTicketInfo.TransactionId, transaction.TransactionId);
            Assert.AreEqual(_target.CurrentTicketInfo.RedemptionStatusCode, RedemptionStatusCode.WaitingForLongPoll71);
            Assert.AreEqual(_target.CurrentState, SasVoucherInState.ValidationDataPending);

            _target.Dispose();
        }

        [TestMethod]
        public void RequestValidationDataRejectTest()
        {
            var waiter = new ManualResetEvent(false);
            var barcode = "1234567890";
            var transaction = new VoucherInTransaction(0, new DateTime(), barcode);
            transaction.TransactionId = 10;
            IReadOnlyCollection<VoucherInTransaction> transactions = new List<VoucherInTransaction>
            {
                new VoucherInTransaction()
            };

            _ticketCoordinator.Setup(x => x.GetData()).Returns(new TicketStorageData());
            _transactionHistory.Setup(x => x.RecallTransactions<VoucherInTransaction>()).Returns(transactions);

            _target = new SasVoucherInProvider(
                _exceptionHandler.Object,
                _transactionHistory.Object,
                _propertiesManager.Object,
                _bus.Object,
                _ticketCoordinator.Object,
                _bank.Object);

            _exceptionHandler.Setup(x => x.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.TicketHasBeenInserted)))
                .Callback(
                () =>
                {
                    var dataResponse = _target.RequestValidationData();
                    Assert.IsNotNull(dataResponse);
                    Assert.AreEqual(dataResponse.Barcode, barcode);
                    Assert.AreEqual(dataResponse.ParsingCode, ParsingCode.Bcd);
                });

            _ticketCoordinator.Setup(x => x.Save(It.Is<TicketStorageData>(t => t.VoucherInState == SasVoucherInState.ValidationDataPending)))
                .Returns(Task.CompletedTask)
                .Callback(() => _target.OnTicketInFailed(barcode, RedemptionStatusCode.GamingMachineUnableToAcceptTransfer, transaction.TransactionId));
            _ticketCoordinator.Setup(x => x.Save(It.Is<TicketStorageData>(t => t.VoucherInState == SasVoucherInState.Idle)))
                .Returns(Task.CompletedTask)
                .Callback(() => waiter.Set());

            var validateTicketIn = _target.ValidationTicket(transaction);

            Assert.IsTrue(waiter.WaitOne(WaitTime));
            Assert.AreEqual(_target.CurrentTicketInfo.Amount, 0UL);
            Assert.AreEqual(_target.CurrentTicketInfo.Barcode, barcode);
            Assert.AreEqual(_target.CurrentTicketInfo.TransactionId, transaction.TransactionId);
            Assert.AreEqual(_target.CurrentState, SasVoucherInState.Idle);
            Assert.AreEqual(_target.CurrentTicketInfo.RedemptionStatusCode, RedemptionStatusCode.GamingMachineUnableToAcceptTransfer);

            _target.Dispose();
        }

        [TestMethod]
        public void RequestPendingStateTest()
        {
            var waiter = new ManualResetEvent(false);
            var barcode = "1234567890";
            var transaction = new VoucherInTransaction(0, new DateTime(), barcode);
            transaction.TransactionId = 10;
            var expectedTransferAmount = 100;
            var redeemData = new RedeemTicketData
            {
                TransferCode = TicketTransferCode.ValidCashableTicket,
                TransferAmount = 100,
                ParsingCode = ParsingCode.Bcd,
                Barcode = barcode,
                RestrictedExpiration = 0,
                PoolId = 0,
                TargetId = ""
            };

            IReadOnlyCollection<VoucherInTransaction> transactions = new List<VoucherInTransaction>
            {
                new VoucherInTransaction()
            };

            _ticketCoordinator.Setup(x => x.GetData()).Returns(new TicketStorageData());
            _transactionHistory.Setup(x => x.RecallTransactions<VoucherInTransaction>()).Returns(transactions);

            _target = new SasVoucherInProvider(
                _exceptionHandler.Object,
                _transactionHistory.Object,
                _propertiesManager.Object,
                _bus.Object,
                _ticketCoordinator.Object,
                _bank.Object);

            _exceptionHandler.Setup(x => x.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.TicketHasBeenInserted)))
                .Callback(
                () =>
                {
                    var dataResponse = _target.RequestValidationData();
                    Assert.IsNotNull(dataResponse);
                    Assert.AreEqual(dataResponse.Barcode, barcode);
                    Assert.AreEqual(dataResponse.ParsingCode, ParsingCode.Bcd);
                });

            _ticketCoordinator.Setup(x => x.Save(It.Is<TicketStorageData>(t => t.VoucherInState == SasVoucherInState.ValidationDataPending)))
                .Returns(Task.CompletedTask)
                .Callback(() => _target.AcceptTicket(redeemData, RedemptionStatusCode.TicketRedemptionPending));
            _ticketCoordinator.Setup(x => x.Save(It.Is<TicketStorageData>(t => t.VoucherInState == SasVoucherInState.RequestPending)))
                .Returns(Task.CompletedTask)
                .Callback(() => waiter.Set());

            var validateTicketIn = _target.ValidationTicket(transaction);

            Assert.IsTrue(waiter.WaitOne(WaitTime));
            Assert.AreEqual(_target.CurrentState, SasVoucherInState.RequestPending);
            Assert.AreEqual(_target.CurrentTicketInfo.Barcode, barcode);
            Assert.AreEqual(_target.CurrentTicketInfo.Amount, (ulong)expectedTransferAmount);
            Assert.AreEqual(_target.CurrentTicketInfo.TransferCode, TicketTransferCode.ValidCashableTicket);
            Assert.AreEqual(_target.CurrentTicketInfo.RedemptionStatusCode, RedemptionStatusCode.TicketRedemptionPending);
            Assert.AreEqual(_target.CurrentTicketInfo.TransactionId, transaction.TransactionId);
            Assert.AreEqual(_target.CurrentTicketInfo.TransferCode, TicketTransferCode.ValidCashableTicket);

            _target.Dispose();
        }

        [TestMethod]
        public void RequestPendingStateFailTest()
        {
            var waiter = new ManualResetEvent(false);
            var barcode = "1234567890";
            var transaction = new VoucherInTransaction(0, new DateTime(), barcode);
            transaction.TransactionId = 10;
            var redeemData = new RedeemTicketData
            {
                TransferCode = TicketTransferCode.ValidCashableTicket,
                TransferAmount = 100,
                ParsingCode = ParsingCode.Bcd,
                Barcode = barcode,
                RestrictedExpiration = 0,
                PoolId = 0,
                TargetId = ""
            };

            IReadOnlyCollection<VoucherInTransaction> transactions = new List<VoucherInTransaction>
            {
                new VoucherInTransaction()
            };

            _ticketCoordinator.Setup(x => x.GetData()).Returns(new TicketStorageData());
            _transactionHistory.Setup(x => x.RecallTransactions<VoucherInTransaction>()).Returns(transactions);

            _target = new SasVoucherInProvider(
                _exceptionHandler.Object,
                _transactionHistory.Object,
                _propertiesManager.Object,
                _bus.Object,
                _ticketCoordinator.Object,
                _bank.Object);

            _exceptionHandler.Setup(x => x.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.TicketHasBeenInserted)))
                .Callback(
                () =>
                {
                    var dataResponse = _target.RequestValidationData();
                    Assert.IsNotNull(dataResponse);
                    Assert.AreEqual(dataResponse.Barcode, barcode);
                    Assert.AreEqual(dataResponse.ParsingCode, ParsingCode.Bcd);
                });

            _ticketCoordinator.Setup(x => x.Save(It.Is<TicketStorageData>(t => t.VoucherInState == SasVoucherInState.ValidationDataPending)))
                .Returns(Task.CompletedTask)
                .Callback(() => _target.DenyTicket(RedemptionStatusCode.NotAValidTransferFunction, redeemData.TransferCode));
            _ticketCoordinator.Setup(x => x.Save(It.Is<TicketStorageData>(t => t.VoucherInState == SasVoucherInState.Idle)))
                .Returns(Task.CompletedTask)
                .Callback(() => waiter.Set());

            var validateTicketIn = _target.ValidationTicket(transaction);

            Assert.IsTrue(waiter.WaitOne(WaitTime));
            Assert.AreEqual(_target.CurrentState, SasVoucherInState.Idle);
            Assert.AreEqual(_target.CurrentTicketInfo.Barcode, barcode);
            Assert.AreEqual(_target.CurrentTicketInfo.Amount, 0UL);
            Assert.AreEqual(_target.CurrentTicketInfo.TransferCode, TicketTransferCode.ValidCashableTicket);
            Assert.AreEqual(_target.CurrentTicketInfo.RedemptionStatusCode, RedemptionStatusCode.NotAValidTransferFunction);
            Assert.AreEqual(_target.CurrentTicketInfo.TransactionId, transaction.TransactionId);
            Assert.AreEqual(_target.CurrentTicketInfo.TransferCode, TicketTransferCode.ValidCashableTicket);

            _target.Dispose();
        }

        [TestMethod]
        public void AcknowledgementPendingStateTest()
        {
            var waiter = new ManualResetEvent(false);
            var barcode = "1234567890";
            var transaction = new VoucherInTransaction(0, new DateTime(), barcode);
            transaction.TransactionId = 10;
            var expectedTransferAmount = 100;
            var redeemData = new RedeemTicketData
            {
                TransferCode = TicketTransferCode.ValidCashableTicket,
                TransferAmount = 100,
                ParsingCode = ParsingCode.Bcd,
                Barcode = barcode,
                RestrictedExpiration = 0,
                PoolId = 0,
                TargetId = ""
            };

            IReadOnlyCollection<VoucherInTransaction> transactions = new List<VoucherInTransaction>
            {
                new VoucherInTransaction()
            };

            _ticketCoordinator.Setup(x => x.GetData()).Returns(new TicketStorageData());
            _transactionHistory.Setup(x => x.RecallTransactions<VoucherInTransaction>()).Returns(transactions);

            _target = new SasVoucherInProvider(
                _exceptionHandler.Object,
                _transactionHistory.Object,
                _propertiesManager.Object,
                _bus.Object,
                _ticketCoordinator.Object,
                _bank.Object);

            _exceptionHandler.Setup(x => x.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.TicketHasBeenInserted)))
                .Callback(
                () =>
                {
                    var dataResponse = _target.RequestValidationData();
                    Assert.IsNotNull(dataResponse);
                    Assert.AreEqual(dataResponse.Barcode, barcode);
                    Assert.AreEqual(dataResponse.ParsingCode, ParsingCode.Bcd);
                });

            _ticketCoordinator.Setup(x => x.Save(It.Is<TicketStorageData>(t => t.VoucherInState == SasVoucherInState.ValidationDataPending)))
                .Returns(Task.CompletedTask)
                .Callback(() => _target.AcceptTicket(redeemData, RedemptionStatusCode.TicketRedemptionPending));
            _ticketCoordinator.Setup(x => x.Save(It.Is<TicketStorageData>(t => t.VoucherInState == SasVoucherInState.RequestPending)))
                .Returns(Task.CompletedTask)
                .Callback(() => _target.OnTicketInCompleted(AccountType.Cashable));
            _ticketCoordinator.Setup(x => x.Save(It.Is<TicketStorageData>(t => t.VoucherInState == SasVoucherInState.AcknowledgementPending)))
                .Returns(Task.CompletedTask)
                .Callback(() => waiter.Set());

            var validateTicketIn = _target.ValidationTicket(transaction);

            Assert.IsTrue(waiter.WaitOne(WaitTime));
            Assert.AreEqual(_target.CurrentTicketInfo.Barcode, barcode);
            Assert.AreEqual(_target.CurrentTicketInfo.Amount, (ulong)expectedTransferAmount);
            Assert.AreEqual(_target.CurrentTicketInfo.TransactionId, transaction.TransactionId);
            Assert.AreEqual(_target.CurrentState, SasVoucherInState.AcknowledgementPending);
            Assert.AreEqual(_target.CurrentTicketInfo.RedemptionStatusCode, RedemptionStatusCode.CashableTicketRedeemed);
            Assert.AreEqual(_target.CurrentTicketInfo.TransferCode, TicketTransferCode.ValidCashableTicket);

            _target.Dispose();
        }

        [TestMethod]
        public void ImpliedAckStateTest()
        {
            var waiter = new ManualResetEvent(false);
            var barcode = "1234567890";
            var transaction = new VoucherInTransaction(0, new DateTime(), barcode);
            transaction.TransactionId = 10;
            var expectedTransferAmount = 100;
            var redeemData = new RedeemTicketData
            {
                TransferCode = TicketTransferCode.ValidCashableTicket,
                TransferAmount = 100,
                ParsingCode = ParsingCode.Bcd,
                Barcode = barcode,
                RestrictedExpiration = 0,
                PoolId = 0,
                TargetId = ""
            };

            IReadOnlyCollection<VoucherInTransaction> transactions = new List<VoucherInTransaction>
            {
                new VoucherInTransaction()
            };

            _ticketCoordinator.Setup(x => x.GetData()).Returns(new TicketStorageData());
            _transactionHistory.Setup(x => x.RecallTransactions<VoucherInTransaction>()).Returns(transactions);

            _target = new SasVoucherInProvider(
                _exceptionHandler.Object,
                _transactionHistory.Object,
                _propertiesManager.Object,
                _bus.Object,
                _ticketCoordinator.Object,
                _bank.Object);

            _exceptionHandler.Setup(x => x.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.TicketHasBeenInserted)))
                .Callback(
                () =>
                {
                    var dataResponse = _target.RequestValidationData();
                    Assert.IsNotNull(dataResponse);
                    Assert.AreEqual(dataResponse.Barcode, barcode);
                    Assert.AreEqual(dataResponse.ParsingCode, ParsingCode.Bcd);
                });

            _ticketCoordinator.Setup(x => x.Save(It.Is<TicketStorageData>(t => t.VoucherInState == SasVoucherInState.ValidationDataPending)))
                .Returns(Task.CompletedTask)
                .Callback(() => _target.AcceptTicket(redeemData, RedemptionStatusCode.TicketRedemptionPending));
            _ticketCoordinator.Setup(x => x.Save(It.Is<TicketStorageData>(t => t.VoucherInState == SasVoucherInState.RequestPending)))
                .Returns(Task.CompletedTask)
                .Callback(() => _target.OnTicketInCompleted(AccountType.Cashable));
            _ticketCoordinator.Setup(x => x.Save(It.Is<TicketStorageData>(t => t.VoucherInState == SasVoucherInState.AcknowledgementPending)))
                .Returns(Task.CompletedTask)
                .Callback(() => _target.RedemptionStatusAcknowledged());
            _ticketCoordinator.Setup(x => x.Save(It.Is<TicketStorageData>(t => t.VoucherInState == SasVoucherInState.Idle)))
                .Returns(Task.CompletedTask)
                .Callback(() => waiter.Set());

            var validateTicketIn = _target.ValidationTicket(transaction);

            Assert.IsTrue(waiter.WaitOne(WaitTime));
            Assert.AreEqual(_target.CurrentTicketInfo.Barcode, barcode);
            Assert.AreEqual(_target.CurrentTicketInfo.Amount, (ulong)expectedTransferAmount);
            Assert.AreEqual(_target.CurrentTicketInfo.TransactionId, transaction.TransactionId);
            Assert.AreEqual(_target.CurrentState, SasVoucherInState.Idle);
            Assert.AreEqual(_target.CurrentTicketInfo.RedemptionStatusCode, RedemptionStatusCode.CashableTicketRedeemed);
            Assert.AreEqual(_target.CurrentTicketInfo.TransferCode, TicketTransferCode.ValidCashableTicket);

            _target.Dispose();
        }

        [TestMethod]
        public void AcknowledgementPendingDenyTest()
        {
            var waiter = new ManualResetEvent(false);
            var barcode = "1234567890";
            var transaction = new VoucherInTransaction(0, new DateTime(), barcode);
            transaction.TransactionId = 10;
            var expectedTransferAmount = 100;
            var redeemData = new RedeemTicketData
            {
                TransferCode = TicketTransferCode.ValidCashableTicket,
                TransferAmount = 100,
                ParsingCode = ParsingCode.Bcd,
                Barcode = barcode,
                RestrictedExpiration = 0,
                PoolId = 0,
                TargetId = ""
            };

            IReadOnlyCollection<VoucherInTransaction> transactions = new List<VoucherInTransaction>
            {
                new VoucherInTransaction()
            };

            _ticketCoordinator.Setup(x => x.GetData()).Returns(new TicketStorageData());
            _transactionHistory.Setup(x => x.RecallTransactions<VoucherInTransaction>()).Returns(transactions);

            _target = new SasVoucherInProvider(
                _exceptionHandler.Object,
                _transactionHistory.Object,
                _propertiesManager.Object,
                _bus.Object,
                _ticketCoordinator.Object,
                _bank.Object);

            _exceptionHandler.Setup(x => x.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.TicketHasBeenInserted)))
                .Callback(
                () =>
                {
                    var dataResponse = _target.RequestValidationData();
                    Assert.IsNotNull(dataResponse);
                    Assert.AreEqual(dataResponse.Barcode, barcode);
                    Assert.AreEqual(dataResponse.ParsingCode, ParsingCode.Bcd);
                });

            _ticketCoordinator.Setup(x => x.Save(It.Is<TicketStorageData>(t => t.VoucherInState == SasVoucherInState.ValidationDataPending)))
                .Returns(Task.CompletedTask)
                .Callback(() => _target.AcceptTicket(redeemData, RedemptionStatusCode.TicketRedemptionPending));
            _ticketCoordinator.Setup(x => x.Save(It.Is<TicketStorageData>(t => t.VoucherInState == SasVoucherInState.RequestPending)))
                .Returns(Task.CompletedTask)
                .Callback(() => _target.OnTicketInFailed(barcode, RedemptionStatusCode.TransferAmountExceededGameLimit, transaction.TransactionId));
            _ticketCoordinator.Setup(x => x.Save(It.Is<TicketStorageData>(t => t.VoucherInState == SasVoucherInState.AcknowledgementPending)))
                .Returns(Task.CompletedTask)
                .Callback(() => waiter.Set());

            var validateTicketIn = _target.ValidationTicket(transaction);

            Assert.IsTrue(waiter.WaitOne());
            Assert.AreEqual(_target.CurrentTicketInfo.Barcode, barcode);
            Assert.AreEqual(_target.CurrentTicketInfo.Amount, (ulong)expectedTransferAmount);
            Assert.AreEqual(_target.CurrentTicketInfo.TransactionId, transaction.TransactionId);
            Assert.AreEqual(_target.CurrentState, SasVoucherInState.AcknowledgementPending);
            Assert.AreEqual(_target.CurrentTicketInfo.RedemptionStatusCode, RedemptionStatusCode.TransferAmountExceededGameLimit);
            Assert.AreEqual(_target.CurrentTicketInfo.TransferCode, TicketTransferCode.ValidCashableTicket);

            _target.Dispose();
        }
    }
}
