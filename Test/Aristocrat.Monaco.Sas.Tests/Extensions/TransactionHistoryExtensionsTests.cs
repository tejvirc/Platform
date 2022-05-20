namespace Aristocrat.Monaco.Sas.Tests.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Aristocrat.Sas.Client;
    using Test.Common;

    [TestClass]
    public class TransactionHistoryExtensionsTests
    {
        private const int SasMaxQueueSize = 31;
        private const int CurrentValidationInformationIndex = 0;
        private const int FirstValidationInformationIndex = 1;
        private const int InvalidValidationIndex = 32;

        private const int TransactionCashableAmount = 123;

        private const string ValidBarcode = "123-456-789";

        private Mock<ITransactionHistory> _transactionHistory;
        private Mock<IPropertiesManager> _propertiesManager;

        private HandpayTransaction firstTransactionValid;
        private VoucherOutTransaction secondTransactionInvalid, thirdTransactionValid;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);

            _transactionHistory = new Mock<ITransactionHistory>(MockBehavior.Default);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);

            _propertiesManager.Setup(m => m.GetProperty("Mono.SelectedAddinConfigurationHashCode", null)).Returns(null);

            SetupTransactions();

            _transactionHistory.Setup(t => t.RecallTransactions<ITransaction>()).Returns(new List<ITransaction>());
        }

        [TestCleanup]
        public void MyTestCleanUp()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void AnyPendingHostAckNoPendingTest()
        {
            _transactionHistory.Setup(t => t.RecallTransactions(true)).Returns(new List<ITransaction>().OrderByDescending(l => l.TransactionId));
            Assert.IsFalse(_transactionHistory.Object.AnyPendingHostAcknowledged());
        }

        [TestMethod]
        public void AnyPendingHostAckPendingTest()
        {
            SetupTransactionHistoryRecall();
            Assert.IsTrue(_transactionHistory.Object.AnyPendingHostAcknowledged());
        }

        [TestMethod]
        public void GetPendingHostAckCountNoPendingTest()
        {
            _transactionHistory.Setup(t => t.RecallTransactions(true)).Returns(new List<ITransaction>().OrderByDescending(l => l.TransactionId));
            var count = _transactionHistory.Object.GetPendingHostAcknowledgedCount();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void GetPendingHostAckCountPendingTest()
        {
            SetupTransactionHistoryRecall();
            // There are 2 pending transactions now.
            var count = _transactionHistory.Object.GetPendingHostAcknowledgedCount();
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public void GetNextNeedingHostAckTransactionNoTransactionsTest()
        {
            _transactionHistory.Setup(t => t.RecallTransactions(false)).Returns(new List<ITransaction>().OrderBy(l => l.TransactionId));
            var transaction = _transactionHistory.Object.GetNextNeedingHostAcknowledgedTransaction();
            Assert.IsNull(transaction);
        }

        [TestMethod]
        public void GetNextNeedingHostAckTransactionTest()
        {
            SetupTransactionHistoryRecall(false);
            var transaction = _transactionHistory.Object.GetNextNeedingHostAcknowledgedTransaction();
            Assert.AreEqual(firstTransactionValid.TransactionId, transaction.TransactionId);
        }

        [TestMethod]
        public void GetCurrentTransactionNoInformationTest()
        {
            _transactionHistory.Setup(t => t.RecallTransactions(true)).Returns(new List<ITransaction>().OrderByDescending(l => l.TransactionId));
            var transaction = _transactionHistory.Object.GetTransaction( CurrentValidationInformationIndex, SasMaxQueueSize);
            Assert.IsNull(transaction);
        }

        [TestMethod]
        public void GetTransactionInvalidIndexTest()
        {
            SetupTransactionHistoryRecall();
            var transaction = _transactionHistory.Object.GetTransaction( InvalidValidationIndex, SasMaxQueueSize);
            Assert.IsNull(transaction);
        }

        [TestMethod]
        public void GetTransactionZeroIndexTest()
        {
            _transactionHistory.Setup(t => t.RecallTransactions(true)).Returns(new List<ITransaction>().OrderByDescending(l => l.TransactionId));
            var transaction = _transactionHistory.Object.GetTransaction(CurrentValidationInformationIndex, SasMaxQueueSize);
            Assert.IsNull(transaction);
        }

        [TestMethod]
        public void GetTransactionNoValidTransactionTest()
        {
            _transactionHistory.Setup(t => t.RecallTransactions(true)).Returns(new List<ITransaction>().OrderByDescending(l => l.TransactionId));
            var transaction = _transactionHistory.Object.GetTransaction( FirstValidationInformationIndex, SasMaxQueueSize);
            Assert.IsNull(transaction);
        }

        [TestMethod]
        public void GetTransactionFirstValidTransactionTest()
        {
            var transaction = GetTransactionFromHistory(1);
            Assert.IsNotNull(transaction);
            Assert.AreEqual(typeof(HandpayTransaction), transaction.GetType());
            Assert.AreEqual(firstTransactionValid.TransactionId, transaction.TransactionId);
        }

        [TestMethod]
        public void GetTransactionSecondValidTransactionTest()
        {
            var transaction = GetTransactionFromHistory(2);
            Assert.IsNotNull(transaction);
            Assert.AreEqual(typeof(VoucherOutTransaction), transaction.GetType());
            Assert.AreEqual(thirdTransactionValid.TransactionId, transaction.TransactionId);
        }

        [TestMethod]
        public void GetTransactionThirdValidTransactionTest()
        {
            Assert.IsNull(GetTransactionFromHistory(3));
        }

        [TestMethod]
        public void GetHostAckQueueSizeHostIsMinimumTest()
        {
            _transactionHistory.Setup(t => t.GetMaxTransactions<VoucherOutTransaction>()).Returns(123456);
            _transactionHistory.Setup(t => t.GetMaxTransactions<HandpayTransaction>()).Returns(654321);
            var size = _transactionHistory.Object.GetHostAcknowledgedQueueSize();
            Assert.AreEqual(SasConstants.MaxHostSequence, size);
        }

        [TestMethod]
        public void GetHostAckQueueSizeHostIsMaximumTest()
        {
            _transactionHistory.Setup(t => t.GetMaxTransactions<VoucherOutTransaction>()).Returns(1);
            _transactionHistory.Setup(t => t.GetMaxTransactions<HandpayTransaction>()).Returns(2);
            var size = _transactionHistory.Object.GetHostAcknowledgedQueueSize();
            Assert.AreEqual(1, size);
        }

        private ITransaction GetTransactionFromHistory(int index)
        {
            SetupTransactionHistoryRecall();
            return _transactionHistory.Object.GetTransaction(index, SasMaxQueueSize);
        }

        private void SetupTransactions()
        {
            firstTransactionValid = new HandpayTransaction(
                1,
                DateTime.Now,
                TransactionCashableAmount,
                0,
                0,
                HandpayType.CancelCredit,
                false,
                new Guid()
            )
            { Barcode = ValidBarcode, HostSequence = 1, State = HandpayState.Committed, TransactionId = 1 };

            secondTransactionInvalid = new VoucherOutTransaction(
                1,
                DateTime.Now,
                TransactionCashableAmount,
                AccountType.Cashable,
                string.Empty,
                0,
                string.Empty
            )
            { HostAcknowledged = false, HostSequence = 0, TransactionId = 2 };

            thirdTransactionValid = new VoucherOutTransaction(
                1,
                DateTime.Now,
                TransactionCashableAmount,
                AccountType.Cashable,
                string.Empty,
                0,
                string.Empty
            )
            { HostAcknowledged = true, HostSequence = 2, TransactionId = 3 };
        }

        // newestFirst is only ever false with GetNextNeedingHostAcknowledgedTransaction().
        private void SetupTransactionHistoryRecall(bool newestFirst = true)
        {
            var transactionList = new List<ITransaction>()
            {
                firstTransactionValid,
                secondTransactionInvalid,
                thirdTransactionValid,
                new BillTransaction(){ TransactionId = 4 }
            };

            if (newestFirst)
            {
                _transactionHistory.Setup(t => t.RecallTransactions(newestFirst)).Returns(transactionList.OrderByDescending(l => l.TransactionId));
            }
            else
            {
                _transactionHistory.Setup(t => t.RecallTransactions(newestFirst)).Returns(transactionList.OrderBy(l => l.TransactionId));
            }
        }

    }
}
