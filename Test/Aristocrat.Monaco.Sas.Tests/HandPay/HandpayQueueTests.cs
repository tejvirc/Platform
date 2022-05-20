namespace Aristocrat.Monaco.Sas.Tests.HandPay
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Protocol.Common.Storage.Entity;
    using Aristocrat.Monaco.Protocol.Common.Storage.Repositories;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.HandPay;
    using Storage;
    using Storage.Models;
    using Test.Common;

    [TestClass]
    public class HandpayQueueTests
    {
        private const int WaitTime = 1000;
        private const int ClientId = 1;
        private const int MaxQueueSize = 2;
        private Mock<ISasHandPayCommittedHandler> _handpayHandler;
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactory;
        private Mock<IUnitOfWork> _unitOfWork;
        private Mock<IRepository<HandpayReportData>> _handpayRespository;

        private HandpayQueue _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _unitOfWorkFactory = new Mock<IUnitOfWorkFactory>(MockBehavior.Default);
            _unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Default);
            _handpayRespository = new Mock<IRepository<HandpayReportData>>(MockBehavior.Default);
            _handpayHandler = new Mock<ISasHandPayCommittedHandler>(MockBehavior.Default);
            _unitOfWorkFactory.Setup(x => x.Invoke(It.IsAny<Func<IUnitOfWork, HandpayReportData>>())).Returns((HandpayReportData)null);

            _handpayHandler.Setup(x => x.RegisterHandpayQueue(It.IsAny<IHandpayQueue>(), ClientId));
            _target = new HandpayQueue(_unitOfWorkFactory.Object, _handpayHandler.Object, MaxQueueSize, ClientId);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            // dispose twice so we hit all paths
            _target.Dispose();
            _target.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullUnitOfWorkFactoryTest()
        {
            _target = new HandpayQueue(null, _handpayHandler.Object, MaxQueueSize, ClientId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullHandpayHandlerTest()
        {
            _target = new HandpayQueue(_unitOfWorkFactory.Object, null, MaxQueueSize, ClientId);
        }

        [TestMethod]
        public void EnqueueTest()
        {
            var data = new LongPollHandpayDataResponse();
            _unitOfWorkFactory.Setup(x => x.Create()).Returns(_unitOfWork.Object);
            _unitOfWork.Setup(x => x.Repository<HandpayReportData>()).Returns(_handpayRespository.Object);
            _handpayRespository.Setup(x => x.Queryable()).Returns(Enumerable.Empty<HandpayReportData>().AsQueryable());

            Assert.IsNull(_target.Peek());
            Assert.IsTrue(_target.Enqueue(data).Wait(WaitTime));

            Assert.AreEqual(1, _target.Count);
            ValidateHandpayResult(data, _target.Peek());
        }

        [TestMethod]
        public void ExceptionQueueFullTest()
        {
            _unitOfWorkFactory.Setup(x => x.Create()).Returns(_unitOfWork.Object);
            _unitOfWork.Setup(x => x.Repository<HandpayReportData>()).Returns(_handpayRespository.Object);
            _handpayRespository.Setup(x => x.Queryable()).Returns(Enumerable.Empty<HandpayReportData>().AsQueryable());
            for (var i = 0; i <= MaxQueueSize; i++)
            {
                Assert.IsTrue(_target.Enqueue(new LongPollHandpayDataResponse()).Wait(WaitTime));
            }

            Assert.AreEqual(MaxQueueSize, _target.Count);
        }

        [TestMethod]
        public void HandpayAcknowledgedTest()
        {
            var data = new LongPollHandpayDataResponse();
            _unitOfWorkFactory.Setup(x => x.Create()).Returns(_unitOfWork.Object);
            _unitOfWork.Setup(x => x.Repository<HandpayReportData>()).Returns(_handpayRespository.Object);
            _handpayRespository.Setup(x => x.Queryable()).Returns(Enumerable.Empty<HandpayReportData>().AsQueryable());
            Assert.IsTrue(_target.Enqueue(data).Wait(WaitTime));

            Assert.AreEqual(1, _target.Count);
            ValidateHandpayResult(data, _target.GetNextHandpayData());

            Assert.IsTrue(_target.HandpayAcknowledged().Wait(WaitTime));
            Assert.AreEqual(0, _target.Count);
        }

        [TestMethod]
        public void ClearPendingHandpayTest()
        {
            var data = new LongPollHandpayDataResponse();
            _unitOfWorkFactory.Setup(x => x.Create()).Returns(_unitOfWork.Object);
            _unitOfWork.Setup(x => x.Repository<HandpayReportData>()).Returns(_handpayRespository.Object);
            _handpayRespository.Setup(x => x.Queryable()).Returns(Enumerable.Empty<HandpayReportData>().AsQueryable());
            Assert.IsTrue(_target.Enqueue(data).Wait(WaitTime));

            Assert.AreEqual(1, _target.Count);
            ValidateHandpayResult(data, _target.GetNextHandpayData());
            _target.ClearPendingHandpay();
            Assert.IsTrue(_target.HandpayAcknowledged().Wait(WaitTime));
            Assert.AreEqual(1, _target.Count);
        }

        [TestMethod]
        public void RestoreTest()
        {
            var data = new LongPollHandpayDataResponse
            {
                Amount = 250,
                Level = LevelId.HandpayCanceledCredits,
                PartialPayAmount = 100,
                ProgressiveGroup = 123,
                ResetId = ResetId.OnlyStandardHandpayResetIsAvailable,
                SessionGamePayAmount = 100,
                SessionGameWinAmount = 100,
                TransactionId = 1234
            };

            var storage = StorageHelpers.Serialize(new ConcurrentQueue<LongPollHandpayDataResponse>(new[] { data }));
            _unitOfWorkFactory.Setup(x => x.Create()).Returns(_unitOfWork.Object);
            _unitOfWork.Setup(x => x.Repository<HandpayReportData>()).Returns(_handpayRespository.Object);
            _handpayRespository.Setup(x => x.Queryable()).Returns(Enumerable.Empty<HandpayReportData>().AsQueryable());
            _unitOfWorkFactory.Setup(x => x.Invoke(It.IsAny<Func<IUnitOfWork, HandpayReportData>>()))
                .Returns(new HandpayReportData { ClientId = 1, Queue = storage, Id = 1 });

            _target = new HandpayQueue(_unitOfWorkFactory.Object, _handpayHandler.Object, 30, 1);

            Assert.AreEqual(1, _target.Count);
            var actual = _target.Peek();

            ValidateHandpayResult(data, actual);
        }

        [TestMethod]
        public void GetNextHandpayDataNullResponseTest()
        {
            // haven't put anything in queue so expect null for data
            Assert.IsNull(_target.GetNextHandpayData());
        }

        [TestMethod]
        public void HandpayAcknowledgeWithNoDataTest()
        {
            // set that we have a handpay response pending
            dynamic accessor = new DynamicPrivateObject(_target);
            accessor._pendingHandpayRead = true;

            Assert.AreEqual(Task.CompletedTask, _target.HandpayAcknowledged());
        }

        [TestMethod]
        public void GetEnumeratorTest()
        {
            Assert.IsNotNull(_target.GetEnumerator());
        }

        [TestMethod]
        public void AckNackHandlerTest()
        {
            var waiter = new ManualResetEvent(false);
            var data = new LongPollHandpayDataResponse();
            _unitOfWorkFactory.Setup(x => x.Create()).Returns(_unitOfWork.Object);
            _unitOfWork.Setup(x => x.Repository<HandpayReportData>()).Returns(_handpayRespository.Object);
            _handpayRespository.Setup(x => x.Queryable()).Returns(Enumerable.Empty<HandpayReportData>().AsQueryable());
            _unitOfWork.Setup(m => m.Commit()).Callback(() => waiter.Set());

            Assert.IsTrue(_target.Enqueue(data).Wait(WaitTime));
            Assert.AreEqual(1, _target.Count);

            var response = _target.GetNextHandpayData();

            // call the Ack handler
            response.Handlers.ImpliedAckHandler.Invoke();

            // wait for the handler to finish
            Assert.IsTrue(waiter.WaitOne(WaitTime));

            // validate Nack handler
            Assert.IsNull(response.Handlers.ImpliedNackHandler);
        }

        private static void ValidateHandpayResult(LongPollHandpayDataResponse expected, LongPollHandpayDataResponse actual)
        {
            Assert.AreEqual(expected.SessionGamePayAmount, actual.SessionGamePayAmount);
            Assert.AreEqual(expected.Level, actual.Level);
            Assert.AreEqual(expected.TransactionId, actual.TransactionId);
            Assert.AreEqual(expected.Amount, actual.Amount);
            Assert.AreEqual(expected.ResetId, actual.ResetId);
            Assert.AreEqual(expected.PartialPayAmount, actual.PartialPayAmount);
            Assert.AreEqual(expected.SessionGameWinAmount, actual.SessionGameWinAmount);
            Assert.AreEqual(expected.ProgressiveGroup, actual.ProgressiveGroup);
        }
    }
}