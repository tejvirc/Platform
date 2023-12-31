﻿namespace Aristocrat.Monaco.Bingo.Tests.Services.Reporting
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Common.Events;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class TransactionHandlerTests
    {
        private const string MachineSerial = "1";
        private const long Amount = 10_00;
        private const int TransactionId = 123;
        private readonly ReportTransactionResponse _ack = new(ResponseCode.Ok, TransactionId);
        private TransactionHandler _target;
        private readonly Mock<IAcknowledgedQueueHelper<ReportTransactionMessage, long>> _helper = new(MockBehavior.Strict);
        private AcknowledgedQueue<ReportTransactionMessage, long> _queue;
        private readonly Mock<IPropertiesManager> _properties = new(MockBehavior.Strict);
        private readonly Mock<IReportTransactionService> _reportTransactionService = new(MockBehavior.Strict);
        private readonly Mock<IIdProvider> _idProvider = new(MockBehavior.Strict);
        private readonly Mock<IBingoClientConnectionState> _clientConnection = new(MockBehavior.Default);
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Strict);

        [TestInitialize]
        public void Initialize()
        {
            _properties.Setup(m => m.GetProperty(ApplicationConstants.SerialNumber, string.Empty)).Returns(MachineSerial);
            _idProvider.Setup(m => m.GetNextLogSequence<TransactionHandler>()).Returns(TransactionId);
            _helper.Setup(m => m.WritePersistence(It.IsAny<List<ReportTransactionMessage>>()));
            _helper.Setup(m => m.ReadPersistence()).Returns(new List<ReportTransactionMessage>());
            _helper.Setup(m => m.GetId(It.IsAny<ReportTransactionMessage>()))
                .Returns((ReportTransactionMessage m) => m.TransactionId);

            _queue = new(_helper.Object);

            _target = new TransactionHandler(
                _properties.Object,
                _clientConnection.Object,
                _queue,
                _reportTransactionService.Object,
                _idProvider.Object,
                _eventBus.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _target.Dispose();
        }

        [DataRow(true, false, false, false, false, false, DisplayName = "Null Properties")]
        [DataRow(false, true, false, false, false, false, DisplayName = "Null Queue")]
        [DataRow(false, false, true, false, false, false, DisplayName = "Null Reporting Service")]
        [DataRow(false, false, false, true, false, false, DisplayName = "Null Id Provider")]
        [DataRow(false, false, false, false, true, false, DisplayName = "Null Client Connection")]
        [DataRow(false, false, false, false, false, true, DisplayName = "Null Event Bus")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest(
            bool properties,
            bool queue,
            bool service,
            bool id,
            bool clientConnection,
            bool eventBus)
        {
            _target = new TransactionHandler(
                properties ? null : _properties.Object,
                clientConnection ? null : _clientConnection.Object,
                queue ? null : _queue,
                service ? null : _reportTransactionService.Object,
                id ? null : _idProvider.Object,
                eventBus ? null : _eventBus.Object);
        }

        [TestMethod]
        public void AddNewTransactionToQueue()
        {
            Assert.IsTrue(_queue.IsEmpty);
            _target.AddNewTransactionToQueue(Common.TransactionType.CashIn, Amount, 0, 0);

            Assert.AreEqual(1, _queue.Count);
        }

        [TestMethod]
        public void AddNewTransactionToQueueAndConnect()
        {
            var wait = new AutoResetEvent(false);
            _reportTransactionService.Setup(m => m.ReportTransaction(It.IsAny<ReportTransactionMessage>(), It.IsAny<CancellationToken>()))
                .Callback(() => wait.Set())
                .Returns(Task.FromResult(_ack));

            Assert.IsTrue(_queue.IsEmpty);
            _target.AddNewTransactionToQueue(Common.TransactionType.CashIn, Amount, 0, 0);

            Assert.AreEqual(1, _queue.Count);

            _clientConnection.Raise(x => x.ClientConnected += null, this, EventArgs.Empty);

            wait.WaitOne();
            Thread.Sleep(100); // give time for the transaction acknowledge to be processed

            Assert.IsTrue(_queue.IsEmpty);
            _clientConnection.Raise(x => x.ClientDisconnected += null, this, EventArgs.Empty);

            Thread.Sleep(100); // give time for the Cancel to be processed
        }

        [TestMethod]
        public void DisconnectWhileProcessingQueue()
        {
            var wait = new AutoResetEvent(false);
            var completion = new TaskCompletionSource<ReportTransactionResponse>();

            _reportTransactionService.Setup(m => m.ReportTransaction(It.IsAny<ReportTransactionMessage>(), It.IsAny<CancellationToken>()))
                .Callback(() => wait.Set())
                .Returns(completion.Task);

            Assert.IsTrue(_queue.IsEmpty);
            _target.AddNewTransactionToQueue(Common.TransactionType.CashIn, Amount, 0, 0);

            Assert.AreEqual(1, _queue.Count);

            _clientConnection.Raise(x => x.ClientConnected += null, this, EventArgs.Empty);

            wait.WaitOne(); // wait for one transaction to be processed

            _clientConnection.Raise(x => x.ClientDisconnected += null, this, EventArgs.Empty);
            completion.SetResult(null);

            Assert.IsFalse(_queue.IsEmpty);
        }

        [TestMethod]
        public void QueueFullTest()
        {
            _eventBus.Setup(x => x.Publish(It.IsAny<QueueFullEvent>()));
            _helper.Setup(x => x.AlmostFullDisable());

            while (!_queue.IsQueueFull)
            {
                _target.AddNewTransactionToQueue(Common.TransactionType.CashIn, Amount, 0, 0);
            }
        }
    }
}