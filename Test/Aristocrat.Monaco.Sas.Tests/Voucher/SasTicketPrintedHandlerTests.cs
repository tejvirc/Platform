namespace Aristocrat.Monaco.Sas.Tests.Voucher
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Aristocrat.Sas.Client;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Voucher;
    using Sas.VoucherValidation;


    [TestClass]
    public class SasTicketPrintedHandlerTests
    {
        private SasTicketPrintedHandler _target;
        private readonly Mock<ISasExceptionHandler> _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Strict);
        private readonly Mock<ITransactionHistory> _transactionHistory = new Mock<ITransactionHistory>(MockBehavior.Strict);
        private readonly Mock<ISasDisableProvider> _disableProvider = new Mock<ISasDisableProvider>(MockBehavior.Strict);
        private readonly Mock<IPropertiesManager> _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Strict);

        private readonly ManualResetEvent _waiter = new ManualResetEvent(false);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _transactionHistory.Setup(m => m.GetMaxTransactions<VoucherOutTransaction>()).Returns(2);
            _transactionHistory.Setup(m => m.GetMaxTransactions<HandpayTransaction>()).Returns(1);
            _propertiesManager.Setup(m => m.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { ValidationType = SasValidationType.SecureEnhanced });

            _target = new SasTicketPrintedHandler(_exceptionHandler.Object, _transactionHistory.Object, _disableProvider.Object, _propertiesManager.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            // Dispose twice to cover already disposed path
            _target.Dispose();
            _target.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullExceptionHandlerTest()
        {
            _target = new SasTicketPrintedHandler(null, _transactionHistory.Object, _disableProvider.Object, _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullTransactionHistoryTest()
        {
            _target = new SasTicketPrintedHandler(_exceptionHandler.Object, null, _disableProvider.Object, _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullDisableProviderTest()
        {
            _target = new SasTicketPrintedHandler(_exceptionHandler.Object, _transactionHistory.Object, null, _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullPropertiesManagerTest()
        {
            _target = new SasTicketPrintedHandler(_exceptionHandler.Object, _transactionHistory.Object, _disableProvider.Object, null);
        }

        [TestMethod]
        public void TicketPrintedAcknowledgedWhenNoTransactionIdTest()
        {
            _target.ClearPendingTicketPrinted();
            _exceptionHandler.Setup(m => m.RemoveException(It.IsAny<GenericExceptionBuilder>())).Verifiable();
            _exceptionHandler.Setup(m => m.RemoveHandler(GeneralExceptionCode.CashOutTicketPrinted)).Verifiable();
            _exceptionHandler.Setup(m => m.RemoveHandler(GeneralExceptionCode.HandPayValidated)).Verifiable();

            _target.TicketPrintedAcknowledged();

            _exceptionHandler.Verify(m => m.RemoveException(It.IsAny<GenericExceptionBuilder>()), Times.Never);
            _exceptionHandler.Verify(m => m.RemoveHandler(GeneralExceptionCode.CashOutTicketPrinted), Times.Never);
            _exceptionHandler.Verify(m => m.RemoveHandler(GeneralExceptionCode.HandPayValidated), Times.Never);
        }

        [TestMethod]
        public void TicketPrintedAcknowledgedWhenNoTransactionsTest()
        {
            var transactionId = 1234;
            _target.PendingTransactionId = transactionId;

            _exceptionHandler.Setup(m => m.RemoveException(It.IsAny<GenericExceptionBuilder>())).Verifiable();
            _exceptionHandler.Setup(m => m.RemoveHandler(GeneralExceptionCode.CashOutTicketPrinted)).Verifiable();
            _exceptionHandler.Setup(m => m.RemoveHandler(GeneralExceptionCode.HandPayValidated)).Verifiable();

            _transactionHistory.Setup(m => m.RecallTransactions()).Returns(new List<ITransaction>().OrderBy(x => x));
            _transactionHistory.Setup(m => m.RecallTransactions(true))
                .Returns(new List<ITransaction>().OrderBy(x => x))
                .Callback(() => _waiter.Set())
                .Verifiable();

            _target.TicketPrintedAcknowledged();

            Assert.IsTrue(_waiter.WaitOne(200));

            _exceptionHandler.Verify(m => m.RemoveException(It.IsAny<GenericExceptionBuilder>()), Times.Exactly(2));
            _exceptionHandler.Verify(m => m.RemoveHandler(GeneralExceptionCode.CashOutTicketPrinted), Times.Once);
            _exceptionHandler.Verify(m => m.RemoveHandler(GeneralExceptionCode.HandPayValidated), Times.Once);
            _transactionHistory.Verify(m => m.RecallTransactions(), Times.Once);
        }

        [TestMethod]
        public void ProcessPendingTicketsWhenNoTickets()
        {
            _transactionHistory.Setup(m => m.RecallTransactions(true)).Returns(new List<ITransaction>().OrderBy(x => x.TransactionId));
            _target.ProcessPendingTickets();

            _transactionHistory.Verify(m => m.RecallTransactions(true), Times.Once);
        }

        [TestMethod]
        public void ProcessPendingTicketsWhenOneTicketNoneValidation()
        {
            _propertiesManager.Setup(m => m.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { ValidationType = SasValidationType.None });

            // mark one transaction as not finished
            var voucherOutTransaction1 = new VoucherOutTransaction { HostAcknowledged = false, TransactionId = 1, Amount = 1234 };
            var transactions = new List<ITransaction> { voucherOutTransaction1 }.OrderBy(x => x.TransactionId);
            _transactionHistory.Setup(m => m.RecallTransactions()).Returns(transactions);
            _transactionHistory.Setup(m => m.RecallTransactions(true)).Returns(transactions);
            _transactionHistory.Setup(m => m.UpdateTransaction(It.IsAny<ITransaction>())).Verifiable();
            _disableProvider.Setup(m => m.IsDisableStateActive(DisableState.ValidationQueueFull)).Returns(false);

            // create target set for Validation None. This will clear any pending transactions
            _target = new SasTicketPrintedHandler(_exceptionHandler.Object, _transactionHistory.Object, _disableProvider.Object, _propertiesManager.Object);

            // add un-acknowledged transaction
            var voucherOutTransaction2 = new VoucherOutTransaction { HostAcknowledged = false, TransactionId = 2, Amount = 3456 };
            var transactions2 = new List<ITransaction> { voucherOutTransaction2 }.OrderBy(x => x.TransactionId);
            _transactionHistory.Setup(m => m.RecallTransactions(true)).Returns(transactions2);
            _transactionHistory.Setup(m => m.RecallTransactions(false)).Returns(transactions2);
            _transactionHistory.Setup(m => m.RecallTransactions()).Returns(transactions2);

            _exceptionHandler.Setup(m => m.AddHandler(It.IsAny<GeneralExceptionCode>(), It.IsAny<Action>())).Verifiable();
            _exceptionHandler.Setup(m => m.ReportException(It.IsAny<GenericExceptionBuilder>())).Verifiable();

            _target.ProcessPendingTickets();

            _transactionHistory.Verify(m => m.RecallTransactions(true), Times.Exactly(2));
            _transactionHistory.Verify(m => m.RecallTransactions(false), Times.Once);
            _exceptionHandler.Verify(m => m.AddHandler(It.IsAny<GeneralExceptionCode>(), It.IsAny<Action>()), Times.Once);
            _exceptionHandler.Verify(m => m.ReportException(It.IsAny<GenericExceptionBuilder>()), Times.Once);
        }

        [TestMethod]
        public void ProcessPendingTicketsWhenQueueFull()
        {
            var voucherOutTransaction = new VoucherOutTransaction { HostAcknowledged = false };
            var transactions = new List<ITransaction> { voucherOutTransaction };
            _transactionHistory.Setup(m => m.RecallTransactions(true)).Returns(transactions.OrderBy(x => x));
            _transactionHistory.Setup(m => m.RecallTransactions(false)).Returns(transactions.OrderBy(x => x));
            _transactionHistory.Setup(m => m.RecallTransactions()).Returns(transactions.OrderBy(x => x));
            _exceptionHandler.Setup(m => m.ReportException(It.IsAny<GenericExceptionBuilder>()));

            _disableProvider.Setup(m => m.Disable(SystemDisablePriority.Immediate, DisableState.ValidationQueueFull)).Returns(Task.FromResult(0));

            _target.ProcessPendingTickets();

            _transactionHistory.Verify(m => m.RecallTransactions(true), Times.Once);
            _transactionHistory.Verify(m => m.RecallTransactions(false), Times.Once);
        }
    }
}
