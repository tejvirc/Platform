using Aristocrat.Monaco.Sas.Eft;
using Aristocrat.Monaco.Sas.Storage;
using Aristocrat.Monaco.Sas.Storage.Models;
using Aristocrat.Monaco.Sas.Storage.Repository;
using Aristocrat.Sas.Client;
using Aristocrat.Sas.Client.Eft;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aristocrat.Monaco.Sas.Tests.Eft
{
    /// <summary>
    ///     Contains the tests for the EftHistoryLogProvider class
    /// </summary>
    [TestClass]
    public class EftHistoryLogProviderTests
    {
        private EftHistoryLogProvider _target;
        private readonly Mock<IStorageDataProvider<EftHistoryItem>> _historyProvider = new Mock<IStorageDataProvider<EftHistoryItem>>(MockBehavior.Default);

        [TestInitialize]
        public void MyTestInitialize()
        {
            SetupPersistenceMockForEmptyLogList();
            _target = new EftHistoryLogProvider(_historyProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullHistoryProviderTest()
        {
            _target = new EftHistoryLogProvider(null);
        }

        [TestMethod]
        public void TestEmptyLogEntryList()
        {
            Assert.IsNull(_target.GetLastTransaction());
            Assert.AreEqual(0, _target.GetHistoryLogs().Count());
        }

        [TestMethod]
        public void AddSecondMessageLogEntryWithoutFirstMessageTest()
        {
            var waiter = new AutoResetEvent(false);
            _historyProvider.Setup(x => x.Save(It.IsAny<EftHistoryItem>()))
                .Returns(Task.CompletedTask)
                .Callback(() => waiter.Set()).Verifiable();

            //First stage response
            _target.AddOrUpdateEntry(
                new EftTransferData
                {
                    Command = LongPoll.EftTransferCashableCreditsToMachine,
                    TransferType = EftTransferType.In,
                    TransactionNumber = 10,
                    Acknowledgement = true,
                    TransferAmount = 10
                },
                new EftTransactionResponse
                {
                    TransactionNumber = 10,
                    Acknowledgement = true,
                    TransferAmount = 10,
                    Status = TransactionStatus.OperationSuccessful
                });

            waiter.WaitOne(100);
            Assert.AreEqual(10, _target.GetLastTransaction().TransactionNumber);
            Assert.AreEqual(10u, _target.GetLastTransaction().RequestedTransactionAmount);
            Assert.AreEqual(10u, _target.GetLastTransaction().ReportedTransactionAmount);
        }

        [TestMethod]
        public void AddValidResponseDataEntryTest()
        {
            var waiter = new AutoResetEvent(false);
            _historyProvider.Setup(x => x.Save(It.IsAny<EftHistoryItem>()))
                .Returns(Task.CompletedTask)
                .Callback(() => waiter.Set()).Verifiable();

            //First stage message
            _target.AddOrUpdateEntry(
                new EftTransferData
                {
                    Command = LongPoll.EftTransferCashableCreditsToMachine,
                    TransferType = EftTransferType.In,
                    TransactionNumber = 10,
                    Acknowledgement = false,
                    TransferAmount = 100
                },
                new EftTransactionResponse
                {
                    TransactionNumber = 10,
                    Acknowledgement = false,
                    TransferAmount = 50,
                    Status = TransactionStatus.OperationSuccessful
                });

            waiter.WaitOne(100);
            Assert.AreEqual(10, _target.GetLastTransaction().TransactionNumber);
            Assert.AreEqual(100u, _target.GetLastTransaction().RequestedTransactionAmount);
            Assert.AreEqual(50u, _target.GetLastTransaction().ReportedTransactionAmount);
            Assert.AreEqual(TransactionStatus.OperationSuccessful, _target.GetLastTransaction().ReportedTransactionStatus);
            Assert.AreEqual(false, _target.GetLastTransaction().ToBeProcessed);
            _historyProvider.Verify(x => x.Save(It.IsAny<EftHistoryItem>()), Times.Once);
        }

        [TestMethod]
        public void CheckLogEntryUpdatedTest()
        {
            var waiter = new AutoResetEvent(false);
            _historyProvider.Setup(x => x.Save(It.IsAny<EftHistoryItem>()))
                .Returns(Task.CompletedTask)
                .Callback(() => waiter.Set()).Verifiable();

            //First stage message
            _target.AddOrUpdateEntry(
                new EftTransferData
                {
                    Command = LongPoll.EftTransferCashableCreditsToMachine,
                    TransferType = EftTransferType.In,
                    TransactionNumber = 10,
                    Acknowledgement = false,
                    TransferAmount = 100
                },
                new EftTransactionResponse
                {
                    TransactionNumber = 10,
                    Acknowledgement = false,
                    TransferAmount = 50,
                    Status = TransactionStatus.OperationSuccessful
                });

            Assert.AreEqual(10, _target.GetLastTransaction().TransactionNumber);
            Assert.AreEqual(false, _target.GetLastTransaction().Acknowledgement);
            Assert.AreEqual(100u, _target.GetLastTransaction().RequestedTransactionAmount);
            Assert.AreEqual(50u, _target.GetLastTransaction().ReportedTransactionAmount);
            Assert.AreEqual(TransactionStatus.OperationSuccessful, _target.GetLastTransaction().ReportedTransactionStatus);
            Assert.AreEqual(false, _target.GetLastTransaction().ToBeProcessed);

            //Second stage message
            _target.AddOrUpdateEntry(
                new EftTransferData
                {
                    Command = LongPoll.EftTransferCashableCreditsToMachine,
                    TransferType = EftTransferType.In,
                    TransactionNumber = 10,
                    Acknowledgement = true,
                    TransferAmount = 100
                },
                new EftTransactionResponse
                {
                    TransactionNumber = 10,
                    Acknowledgement = true,
                    TransferAmount = 50,
                    Status = TransactionStatus.OperationSuccessful
                });

            waiter.WaitOne(100);
            Assert.AreEqual(false, _target.GetLastTransaction().Acknowledgement);
            Assert.AreEqual(true, _target.GetLastTransaction().ToBeProcessed);
            _historyProvider.Verify(x => x.Save(It.IsAny<EftHistoryItem>()), Times.AtLeastOnce);
        }

        [TestMethod]
        public void UpdateTransferStatusLogEntryTest()
        {
            var waiter = new AutoResetEvent(false);
            _historyProvider.Setup(x => x.Save(It.IsAny<EftHistoryItem>()))
                .Returns(Task.CompletedTask)
                .Callback(() => waiter.Set()).Verifiable();

            //First stage message
            _target.AddOrUpdateEntry(
                new EftTransferData
                {
                    Command = LongPoll.EftTransferCashableCreditsToMachine,
                    TransferType = EftTransferType.In,
                    TransactionNumber = 10,
                    Acknowledgement = false,
                    TransferAmount = 100
                },
                new EftTransactionResponse
                {
                    TransactionNumber = 10,
                    Acknowledgement = false,
                    TransferAmount = 50,
                    Status = TransactionStatus.OperationSuccessful
                });

            Assert.AreEqual(10, _target.GetLastTransaction().TransactionNumber);
            Assert.AreEqual(false, _target.GetLastTransaction().Acknowledgement);
            Assert.AreEqual(100u, _target.GetLastTransaction().RequestedTransactionAmount);
            Assert.AreEqual(50u, _target.GetLastTransaction().ReportedTransactionAmount);
            Assert.AreEqual(TransactionStatus.OperationSuccessful, _target.GetLastTransaction().ReportedTransactionStatus);
            Assert.AreEqual(false, _target.GetLastTransaction().ToBeProcessed);

            //Second stage message
            _target.AddOrUpdateEntry(
                new EftTransferData
                {
                    Command = LongPoll.EftTransferCashableCreditsToMachine,
                    TransferType = EftTransferType.In,
                    TransactionNumber = 10,
                    Acknowledgement = true,
                    TransferAmount = 100
                },
                new EftTransactionResponse
                {
                    TransactionNumber = 10,
                    Acknowledgement = true,
                    TransferAmount = 50,
                    Status = TransactionStatus.OperationSuccessful
                });

            waiter.WaitOne(100);
            Assert.AreEqual(false, _target.GetLastTransaction().Acknowledgement);
            Assert.AreEqual(true, _target.GetLastTransaction().ToBeProcessed);
            _historyProvider.Verify(x => x.Save(It.IsAny<EftHistoryItem>()), Times.AtLeastOnce);

            _target.UpdateLogEntryForRequestCompleted(LongPoll.EftTransferCashableCreditsToMachine, 10, 100);

            waiter.WaitOne(100);
            Assert.AreEqual(true, _target.GetLastTransaction().Acknowledgement);
            Assert.AreEqual(false, _target.GetLastTransaction().ToBeProcessed);

            //Try to update the entry which does not exist in the log
            Assert.ThrowsException<InvalidOperationException>(() => _target.UpdateLogEntryForRequestCompleted(LongPoll.EftTransferCashableCreditsToMachine, 11, 100));
        }

        [TestMethod]
        public void GetHistoryLogsTest()
        {
            SetupPersistenceMockForLogList();
            _target = new EftHistoryLogProvider(_historyProvider.Object);

            var result = _target.GetHistoryLogs();

            Assert.AreEqual(result.Length, 5);
            Assert.AreEqual(result[0].TransactionNumber, 5);
            Assert.AreEqual(result[4].TransactionNumber, 1);
            Assert.AreEqual(result[0].ReportedTransactionStatus, TransactionStatus.OperationSuccessful);
        }

        [TestMethod]
        public void GetHistoryTest()
        {
            SetupPersistenceMockForLogList();
            _target = new EftHistoryLogProvider(_historyProvider.Object);

            var result = _target.GetLastTransaction();

            Assert.AreEqual(5, result.TransactionNumber);
            Assert.AreEqual(result.ReportedTransactionStatus, TransactionStatus.OperationSuccessful);
        }

        private void SetupPersistenceMockForEmptyLogList()
        {
            var histoyLog = new IEftHistoryLogEntry[0];

            _historyProvider.Setup(x => x.GetData()).Returns(new EftHistoryItem
            {
                EftHistoryLog = string.Empty//StorageHelpers.Serialize(histoyLog)
            });
        }

        private void SetupPersistenceMockForLogList()
        {
            var histoyLog = Enumerable.Range(1, 5).Select(i => new EftHistoryLogEntry
            {
                TransactionNumber = i,
                Command = LongPoll.EftTransferPromotionalCreditsToMachine,
                TransactionDateTime = DateTime.Now,
                Acknowledgement = true,
                RequestedTransactionAmount = 10,
                ReportedTransactionAmount = 10,
                ReportedTransactionStatus = TransactionStatus.OperationSuccessful,
                ToBeProcessed = false
            }).ToArray();

            _historyProvider.Setup(x => x.GetData()).Returns(new EftHistoryItem
            {
                EftHistoryLog = StorageHelpers.Serialize(histoyLog)
            });
        }
    }
}
