namespace Aristocrat.Monaco.Bingo.Tests.Services.Reporting
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Google.Protobuf.WellKnownTypes;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using ServerApiGateway;

    [TestClass]
    public class AcknowledgedQueueTests
    {
        private const int MaxQueueCount = 30;
        private const int AlmostFullThreshold = 24; // 80% full
        private const int TestTransactionId = 4;
        private AcknowledgedQueue<TransactionReport, int> _target;
        private readonly Mock<IAcknowledgedQueueHelper<TransactionReport, int>> _helper = new();
        private readonly TransactionReport _report =
            new()
            {
                MachineSerial = "1",
                TimeStamp = DateTime.UtcNow.ToTimestamp(),
                Amount = 123456,
                GameSerial = 123,
                GameTitleId = 1,
                TransactionId = TestTransactionId,
                PaytableId = 7,
                Denomination = 8,
                TransactionType = 2
            };

        [TestInitialize]
        public void Initialize()
        {
            _helper.Setup(m => m.ReadPersistence())
                .Returns(new List<TransactionReport>());
            _helper.Setup(m => m.GetId(It.IsAny<TransactionReport>()))
                .Returns(TestTransactionId);
            _target = new AcknowledgedQueue<TransactionReport, int>(_helper.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _target.Dispose();
        }

        [TestMethod]
        public void EnqueueItemCausesQueueNotEmptyTest()
        {
            Assert.IsTrue(_target.IsEmpty());
            _target.Enqueue(_report);

            Assert.IsFalse(_target.IsEmpty());
        }

        [TestMethod]
        public void WhenQueueFullNewItemsAreDroppedTest()
        {
            Assert.IsTrue(_target.IsEmpty());
            for (int i = 0; i < MaxQueueCount; i++)
            {
                _target.Enqueue(_report);
            }

            Assert.AreEqual(MaxQueueCount, _target.Count());

            // add one more entry which will get dropped
            _target.Enqueue(_report);
            Assert.AreEqual(MaxQueueCount, _target.Count());
        }

        [TestMethod]
        public void AcknowledgeMatchingIdRemovesItemTest()
        {
            Assert.IsTrue(_target.IsEmpty());
            _target.Enqueue(_report);

            _target.Acknowledge(TestTransactionId);
            Assert.IsTrue(_target.IsEmpty());
        }

        [TestMethod]
        public void AcknowledgeWrongIdDoesntRemoveItemTest()
        {
            Assert.IsTrue(_target.IsEmpty());
            _target.Enqueue(_report);

            _target.Acknowledge(1);
            Assert.IsFalse(_target.IsEmpty());
        }

        [TestMethod]
        public void EventReportTypeCanEnqueueAndAcknowledgeTest()
        {
            var eventId = 3;
            var report = new EventReport
            {
                MachineSerial = "1",
                TimeStamp = DateTime.UtcNow.ToTimestamp(),
                EventId = eventId,
                EventType = 1
            };

            Mock<IAcknowledgedQueueHelper<EventReport, int>> helper = new();
            helper.Setup(m => m.ReadPersistence())
                .Returns(new List<EventReport>());
            helper.Setup(m => m.GetId(It.IsAny<EventReport>()))
                .Returns(eventId);

            var target = new AcknowledgedQueue<EventReport, int>(helper.Object);

            Assert.IsTrue(target.IsEmpty());
            target.Enqueue(report);

            target.Acknowledge(3);
            Assert.IsTrue(target.IsEmpty());
        }

        [TestMethod]
        public void EnableIfQueueBelowThresholdTest()
        {
            _helper.Setup(m => m.AlmostFullDisable()).Verifiable();
            _helper.Setup(m => m.AlmostFullClear()).Verifiable();

            // fill queue over threshold
            Assert.IsTrue(_target.IsEmpty());
            for (int i = 0; i < AlmostFullThreshold; i++)
            {
                _target.Enqueue(_report);
            }

            Assert.AreEqual(AlmostFullThreshold, _target.Count());

            // acknowledge one element
            _target.Acknowledge(TestTransactionId);

            // verify almost full clear was called
            _helper.Verify(m => m.AlmostFullDisable(), Times.Once());
            _helper.Verify(m => m.AlmostFullClear(), Times.Once());
        }

        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task GetNextItemCancelTestAsync()
        {
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            var result = _target.GetNextItem(token);

            // cancel the token while we're waiting for an item
            tokenSource.Cancel();
            var result1 = await result;

            Assert.IsTrue(result.IsCanceled);
            Assert.IsNull(result1);
        }

        [TestMethod]
        public async Task GetNextItemAddTestAsync()
        {
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            var result = _target.GetNextItem(token);

            // add an item to the queue while we're waiting
            _target.Enqueue(_report);
            var result1 = await result;

            Assert.AreEqual(_report, result1);
        }
    }
}