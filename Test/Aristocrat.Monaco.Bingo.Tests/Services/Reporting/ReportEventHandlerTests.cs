namespace Aristocrat.Monaco.Bingo.Tests.Services.Reporting
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Common;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ReportEventHandlerTests
    {
        private const string MachineSerial = "1";
        private const int ReportId = 123;
        private readonly ReportEventResponse _ack = new(ResponseCode.Ok, ReportId);
        private ReportEventHandler _target;
        private readonly Mock<IAcknowledgedQueueHelper<ReportEventMessage, long>> _helper = new (MockBehavior.Strict);
        private AcknowledgedQueue<ReportEventMessage, long> _queue;
        private readonly Mock<IPropertiesManager> _properties = new(MockBehavior.Strict);
        private readonly Mock<IReportEventService> _reportEventService = new(MockBehavior.Strict);
        private readonly Mock<IIdProvider> _idProvider = new(MockBehavior.Strict);
        private readonly Mock<IBingoClientConnectionState> _clientConnection = new(MockBehavior.Default);

        [TestInitialize]
        public void Initialize()
        {
            _properties.Setup(m => m.GetProperty(ApplicationConstants.SerialNumber, string.Empty)).Returns(MachineSerial);
            _idProvider.Setup(m => m.GetNextLogSequence<ReportEventHandler>()).Returns(ReportId);
            _helper.Setup(m => m.WritePersistence(It.IsAny<List<ReportEventMessage>>()));
            _helper.Setup(m => m.ReadPersistence()).Returns(new List<ReportEventMessage>());
            _helper.Setup(m => m.GetId(It.IsAny<ReportEventMessage>())).Returns((ReportEventMessage m) => m.EventId);

            _queue = new(_helper.Object);

            _target = new ReportEventHandler(
                _properties.Object,
                _clientConnection.Object,
                _queue,
                _reportEventService.Object,
                _idProvider.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _target.Dispose();
        }

        [DataRow(true, false, false, false, false, DisplayName = "Null Properties")]
        [DataRow(false, true, false, false, false, DisplayName = "Null Queue")]
        [DataRow(false, false, true, false, false, DisplayName = "Null Reporting Service")]
        [DataRow(false, false, false, true, false, DisplayName = "Null Id Provider")]
        [DataRow(false, false, false, false, true, DisplayName = "Null Client connection")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest(
            bool properties,
            bool queue,
            bool service,
            bool id,
            bool clientConnection)
        {
            _target = new ReportEventHandler(
                properties ? null : _properties.Object,
                clientConnection ? null : _clientConnection.Object,
                queue ? null : _queue,
                service ? null : _reportEventService.Object,
                id ? null : _idProvider.Object);
        }

        [TestMethod]
        public void AddNewEventToQueue()
        {
            Assert.IsTrue(_queue.IsEmpty);
            _target.AddNewEventToQueue(ReportableEvent.MainDoorOpened);

            Assert.AreEqual(1, _queue.Count);
        }

        [TestMethod]
        public void AddNewEventToQueueAndConnect()
        {
            var wait = new AutoResetEvent(false);

            _reportEventService.Setup(m => m.ReportEvent(It.IsAny<ReportEventMessage>(), It.IsAny<CancellationToken>()))
                .Callback(() => wait.Set())
                .Returns(Task.FromResult(_ack));

            Assert.IsTrue(_queue.IsEmpty);
            _target.AddNewEventToQueue(ReportableEvent.MainDoorOpened);

            Assert.AreEqual(1, _queue.Count);

            _clientConnection.Raise(x => x.ClientConnected += null, this, EventArgs.Empty);

            wait.WaitOne();
            Thread.Sleep(100); // give time for the event acknowledge to be processed

            Assert.IsTrue(_queue.IsEmpty);
            _clientConnection.Raise(x => x.ClientDisconnected += null, this, EventArgs.Empty);

            Thread.Sleep(100); // give time for the Cancel to be processed
        }

        [TestMethod]
        public void DisconnectWhileProcessingQueue()
        {
            var wait = new AutoResetEvent(false);
            var completion = new TaskCompletionSource<ReportEventResponse>();

            _reportEventService.Setup(m => m.ReportEvent(It.IsAny<ReportEventMessage>(), It.IsAny<CancellationToken>()))
                .Callback(() => wait.Set())
                .Returns(completion.Task);

            Assert.IsTrue(_queue.IsEmpty);
            _target.AddNewEventToQueue(ReportableEvent.MainDoorOpened);

            Assert.AreEqual(1, _queue.Count);

            _clientConnection.Raise(x => x.ClientConnected += null, this, EventArgs.Empty);

            wait.WaitOne(); // wait for one event to be processed

            _clientConnection.Raise(x => x.ClientDisconnected += null, this, EventArgs.Empty);
            completion.SetResult(null);

            Assert.IsFalse(_queue.IsEmpty);
        }
    }
}