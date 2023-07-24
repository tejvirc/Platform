namespace Aristocrat.Monaco.Hhr.Client.Tests.WorkFlow
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reactive.Subjects;
    using System.Threading;
    using System.Threading.Tasks;
    using Client.Communication;
    using Client.Messages;
    using Client.WorkFlow;
    using Messages;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [DoNotParallelize]
    [TestClass]
    public class CentralManagerTests
    {
        private readonly BehaviorSubject<ConnectionStatus> _connectionStatus = new BehaviorSubject<ConnectionStatus>(
            new ConnectionStatus { ConnectionState = ConnectionState.Connected });

        private readonly InvalidResponse _invalidResponse = new InvalidResponse();
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1);
        private readonly Packet _packet = new Packet(new byte[] { 0 }, 1);
        private readonly List<(Request request, Response response)> _responseReceived = new List<(Request, Response)>();
        private readonly ushort _retry = 3;
        private const int Timeout = 100;

        private readonly List<(Request request, Type responseType)> _sentRequests =
            new List<(Request request, Type responseType)>();

        private readonly Subject<Packet> _subject = new Subject<Packet>();
        private readonly Mock<IMessageFlow> _messageFlow = new Mock<IMessageFlow>(MockBehavior.Default);

        private readonly Mock<ISequenceIdManager> _sequenceIdManager =
            new Mock<ISequenceIdManager>(MockBehavior.Default);

        private readonly Mock<ITcpConnection> _tcpConnection = new Mock<ITcpConnection>(MockBehavior.Default);
        private CentralManager _centralManager;
        private Response _unsolicitedResponse;

        private readonly ManualResetEvent _waitForUnsolicitedResponse = new ManualResetEvent(false);

        [TestInitialize]
        public void TestInitialize()
        {
            _tcpConnection.Setup(x => x.IncomingBytes).Returns(_subject);
            _tcpConnection.Setup(x => x.ConnectionStatus).Returns(_connectionStatus);
            _messageFlow.Setup(x => x.Send(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));
            _centralManager = new CentralManager(_tcpConnection.Object, _messageFlow.Object, _sequenceIdManager.Object);
            _messageFlow.Setup(x => x.Receive(It.IsAny<Packet>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new Response(Command.CmdInvalidCommand)));
            _sequenceIdManager.Setup(x => x.NextSequenceId).Returns(1);
            _centralManager.RequestObservable.Subscribe(OnRequestSent, error => { });
            _centralManager.RequestResponseObservable.Subscribe(OnResponseReceived, error => { });
            _centralManager.UnsolicitedResponses.Subscribe(OnUnsolicitedResponseReceived, error => { });
            _unsolicitedResponse = null;
        }

        [DataRow(3, 100, MessageStatus.UnableToSend)]
        [DataRow(3, 100, MessageStatus.TimedOut)]
        [DataTestMethod]
        public async Task SendARequestExpectStatusResponse(int retry, int timeout, MessageStatus status)
        {
            if (status == MessageStatus.UnableToSend)
            {
                _messageFlow.Setup(x => x.Send(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(false));
            }

            var request = GetRequest((ushort)retry, (ushort)timeout);
            try
            {
                await _centralManager.Send<InvalidRequest, InvalidResponse>(request);
            }
            catch (UnexpectedResponseException e)
            {
                Assert.AreEqual(status, e.Response.MessageStatus);
            }

            Assert.IsTrue(_sentRequests.Any(x => x.request.SequenceId == request.SequenceId));
            Assert.IsTrue(_responseReceived.Any(x => x.response.ReplyId == request.SequenceId));
            _messageFlow.Verify(x => x.Send(request, It.IsAny<CancellationToken>()), Times.Exactly(retry + 1));
            Assert.IsNull(_unsolicitedResponse);
        }

        [TestMethod]
        public async Task SendRequestAndCancelItWhileWaitingForResponse()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();
            var request = GetRequest(_retry, Timeout);
            _messageFlow.Setup(x => x.Send(It.IsAny<Request>(), It.IsAny<CancellationToken>())).Throws(new TaskCanceledException());
            try
            {
                await _centralManager.Send<InvalidRequest, InvalidResponse>(request, cts.Token);
            }
            catch (UnexpectedResponseException e)
            {
                Assert.AreEqual(e.Response.MessageStatus, MessageStatus.Cancelled);
            }

            Assert.IsTrue(_sentRequests.Any(x => x.request.SequenceId == request.SequenceId));
            Assert.IsTrue(_responseReceived.Any(x => x.response.ReplyId == request.SequenceId));
            _messageFlow.Verify(x => x.Send(request, cts.Token), Times.Never);
            Assert.IsNull(_unsolicitedResponse);
        }

        [TestMethod]
        public async Task SendRequestWhenMessageFlowThrowsExceptionExpectPipelineErrorStatus()
        {
            _messageFlow.Setup(x => x.Send(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                .Throws(new InvalidDataException());
            var request = GetRequest(_retry, Timeout);
            try
            {
                await _centralManager.Send<InvalidRequest, InvalidResponse>(request);
            }
            catch (UnexpectedResponseException e)
            {
                Assert.AreEqual(e.Response.MessageStatus, MessageStatus.PipelineError);
            }

            Assert.IsTrue(_sentRequests.Any(x => x.request.SequenceId == request.SequenceId));
            Assert.IsTrue(_responseReceived.Any(x => x.response.ReplyId == request.SequenceId));
            _messageFlow.Verify(x => x.Send(request, It.IsAny<CancellationToken>()), Times.Exactly(_retry + 1));
            Assert.IsNull(_unsolicitedResponse);
        }

        [TestMethod]
        public async Task SendRequestWhenResponseDemandsRetryExpectRetries()
        {
            var request = GetRequest(_retry, Timeout);
            Response response = new CloseTranErrorResponse
            {
                Status = Status.Retry,
                RetryTime = TimeSpan.FromMilliseconds(100),
                ReplyId = request.SequenceId
            };
            _messageFlow.Setup(x => x.Send(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                .Callback<Request, CancellationToken>((r, c) => Task.Delay(Timeout / 2).ContinueWith(x => _subject.OnNext(_packet), c))
                .Returns(Task.FromResult(true));

            _messageFlow.Setup(x => x.Receive(It.IsAny<Packet>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(response));
            try
            {
                var returnedResponse = _centralManager.Send<InvalidRequest, InvalidResponse>(request);

                await returnedResponse;
            }
            catch (UnexpectedResponseException e)
            {
                Assert.IsNull(_unsolicitedResponse);

                var returnedResponse = e.Response;

                Assert.AreEqual(returnedResponse.MessageStatus, MessageStatus.UnexpectedResponse);
                Assert.IsTrue(returnedResponse is CloseTranErrorResponse);
                Assert.AreEqual(returnedResponse.ReplyId, request.SequenceId);
                _messageFlow.Verify(x => x.Send(request, It.IsAny<CancellationToken>()), Times.Exactly(_retry + 1));
                _messageFlow.Verify(x => x.Receive(_packet, It.IsAny<CancellationToken>()), Times.AtLeast(_retry + 1));
            }

            Assert.IsTrue(_sentRequests.Any(x => x.request.SequenceId == request.SequenceId));
            Assert.IsTrue(_responseReceived.Any(x => x.response.ReplyId == request.SequenceId));
        }

        [TestMethod]
        public async Task SendMultipleRequestsFromDifferentThreadsAndExpectSuccess()
        {
            await _lock.WaitAsync();
            var response1 = SendARequest(1, new InvalidResponse());
            await _lock.WaitAsync();
            var response2 = SendARequest(2, new InvalidResponse());
            await _lock.WaitAsync();
            var response3 = SendARequest(3, new InvalidResponse());

            var responses = await Task.WhenAll(response1, response2, response3);

            Assert.AreEqual(_sentRequests.Count, 3);

            uint count = 1;
            foreach (var item in responses)
            {
                Assert.AreEqual(item.ReplyId, count++);
            }

            Assert.AreEqual(_responseReceived.Count, 3);
            Assert.IsNull(_unsolicitedResponse);
        }

        [TestMethod]
        public async Task SendAMessageWhenCommsIsDisconnectedExpectFailure()
        {
            _connectionStatus.OnNext(
                new ConnectionStatus { ConnectionState = ConnectionState.Disconnected });
            try
            {
                await SendARequest(1, _invalidResponse);
            }
            catch (UnexpectedResponseException e)
            {
                Assert.AreEqual(e.Response.MessageStatus, MessageStatus.Disconnected);
                _messageFlow.Verify(x => x.Send(It.IsAny<Request>(), It.IsAny<CancellationToken>()), Times.Never);
            }

            Assert.IsTrue(_sentRequests.Any(x => x.request.SequenceId == 1));
            Assert.IsTrue(_responseReceived.Any(x => x.response.ReplyId == 1));
        }

        [TestMethod]
        public async Task SendAMessageWithNoResponseExpectSuccess()
        {
            var request = GetRequest(_retry, 0);
            try
            {
                await _centralManager.Send(request);
            }
            catch (UnexpectedResponseException e)
            {
                Assert.AreEqual(e.Response.MessageStatus, MessageStatus.Success);
                _messageFlow.Verify(x => x.Send(request, It.IsAny<CancellationToken>()), Times.Once);
            }

            Assert.IsTrue(_sentRequests.Any(x => x.request.SequenceId == request.SequenceId));
            Assert.IsTrue(_responseReceived.Any(x => x.response.ReplyId == request.SequenceId));
            Assert.IsNull(_unsolicitedResponse);
        }

        [TestMethod]
        public async Task SendAMessageWithErrorResponseExpectNoRetries()
        {
            var response = new CloseTranErrorResponse { Status = Status.Error, RetryTime = TimeSpan.Zero, ReplyId = 1 };

            var result = await SendARequest(1, response);

            Assert.IsTrue(result != null);
            _messageFlow.Verify(x => x.Send(It.IsAny<Request>(), It.IsAny<CancellationToken>()), Times.Once);
            _messageFlow.Verify(x => x.Receive(_packet, It.IsAny<CancellationToken>()), Times.Once);
            Assert.AreEqual(result.MessageStatus, MessageStatus.Success);
            Assert.AreEqual(result.Status, Status.Error);

            Assert.IsTrue(_sentRequests.Any(x => x.request.SequenceId == 1));
            Assert.IsTrue(_responseReceived.Any(x => x.response.ReplyId == 1));
            Assert.IsNull(_unsolicitedResponse);
        }

        [TestMethod]
        public async Task SendAMessageWithoutRetryExpectNoRetries()
        {
            var request = GetRequest(0, Timeout);
            try
            {
                await _centralManager.Send<InvalidRequest, InvalidResponse>(request);
            }
            catch (UnexpectedResponseException e)
            {
                Assert.IsNull(_unsolicitedResponse);
                Assert.AreEqual(MessageStatus.TimedOut, e.Response.MessageStatus);
            }

            _messageFlow.Verify(x => x.Send(request, It.IsAny<CancellationToken>()), Times.Once);
            Assert.IsTrue(_sentRequests.Any(x => x.request.SequenceId == request.SequenceId));
            Assert.IsTrue(_responseReceived.Any(x => x.response.ReplyId == request.SequenceId));
        }

        [TestMethod]
        public async Task SendAMessageWithLockupTimeoutExpectRetriesAndFailedRequestTobeOne()
        {
            var request = GetRequest(_retry, Timeout);
            request.RequestTimeout = new LockupRequestTimeout();
            Response response = new CloseTranErrorResponse
            {
                Status = Status.Retry,
                RetryTime = TimeSpan.FromMilliseconds(100),
                ReplyId = request.SequenceId
            };
            _messageFlow.Setup(x => x.Send(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                .Callback<Request, CancellationToken>((r, c) => Task.Delay(Timeout / 2).ContinueWith(x => _subject.OnNext(_packet), c))
                .Returns(Task.FromResult(true));

            _messageFlow.Setup(x => x.Receive(It.IsAny<Packet>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(response));
            try
            {
                var returnedResponse = _centralManager.Send<InvalidRequest, InvalidResponse>(request);

                await returnedResponse;
            }
            catch (UnexpectedResponseException e)
            {
                Assert.IsNull(_unsolicitedResponse);
                var returnedResponse = e.Response;

                Assert.AreEqual(returnedResponse.MessageStatus, MessageStatus.UnexpectedResponse);
                Assert.IsTrue(returnedResponse is CloseTranErrorResponse);
                Assert.AreEqual(returnedResponse.ReplyId, request.SequenceId);
                _messageFlow.Verify(x => x.Send(request, It.IsAny<CancellationToken>()), Times.Exactly(_retry + 1));
                _messageFlow.Verify(x => x.Receive(_packet, It.IsAny<CancellationToken>()), Times.AtLeast(_retry + 1));
                Assert.AreEqual(_sentRequests.Count, 1);
            }
        }

        [TestMethod]
        public async Task SendARequestAndExpectPendingRequest()
        {
            var request = GetRequest(_retry, Timeout);
            try
            {
                var t = _centralManager.Send<InvalidRequest, InvalidResponse>(request);

                Assert.IsTrue(_sentRequests.Any(x => x.request.SequenceId == request.SequenceId));

                await t;
            }
            catch (UnexpectedResponseException e)
            {
                Assert.AreEqual(e.Response.MessageStatus, MessageStatus.TimedOut);
                Assert.IsTrue(_responseReceived.Any(x => x.Item2.ReplyId == request.SequenceId));

                _messageFlow.Verify(x => x.Send(request, It.IsAny<CancellationToken>()), Times.Exactly(_retry + 1));
            }

            Assert.IsNull(_unsolicitedResponse);
        }

        [TestMethod]
        public void ReceiveAnUnsolicitedResponseExpectObserverToReceiveIt()
        {
            _subject.OnNext(_packet);
            _waitForUnsolicitedResponse.WaitOne();
            Assert.IsNotNull(_unsolicitedResponse);
        }

        [TestMethod]
        public async Task SendARequestCommsDownExpectCancelledRequest()
        {
            var request = GetRequest(_retry, Timeout);
            try
            {
                var t = _centralManager.Send<InvalidRequest, InvalidResponse>(request);

                Assert.IsTrue(_sentRequests.Any(x => x.request.SequenceId == request.SequenceId));

                _connectionStatus.OnNext(new ConnectionStatus
                {
                    ConnectionState = ConnectionState.Disconnected
                });

                await t;
            }
            catch (UnexpectedResponseException e)
            {
                Assert.AreEqual(MessageStatus.Cancelled, e.Response.MessageStatus);
                Assert.IsTrue(_responseReceived.Any(x => x.Item2.ReplyId == request.SequenceId));

                _messageFlow.Verify(x => x.Send(request, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            }
            Assert.IsNull(_unsolicitedResponse);
        }

        [TestMethod]
        public async Task RequestPendingReceiveACommandResponseExpectUnsolicitedResponse()
        {
            _messageFlow.Setup(x => x.Receive(It.IsAny<Packet>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new CommandResponse { ReplyId = 0 } as Response));
            var request = GetRequest(_retry, Timeout);
            try
            {
                var t = _centralManager.Send<InvalidRequest, InvalidResponse>(request);

                Assert.IsTrue(_sentRequests.Any(x => x.request.SequenceId == request.SequenceId));
                _subject.OnNext(_packet);
                _waitForUnsolicitedResponse.WaitOne(Timeout);
                await t;
            }
            catch (UnexpectedResponseException e)
            {
                Assert.AreEqual(MessageStatus.TimedOut, e.Response.MessageStatus);
                Assert.IsTrue(_responseReceived.Any(x => x.Item2.ReplyId == request.SequenceId));

                _messageFlow.Verify(x => x.Send(request, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            }

            Assert.IsNotNull(_unsolicitedResponse);
        }
        private void OnUnsolicitedResponseReceived(Response obj)
        {
            _unsolicitedResponse = obj;
            _waitForUnsolicitedResponse.Set();
        }

        private void OnResponseReceived((Request, Response) obj)
        {
            _responseReceived.Add(obj);
        }

        private void OnRequestSent((Request request, Type responseType) obj)
        {
            _sentRequests.Add(obj);
        }

        private Task<TResponse> SendARequest<TResponse>(uint sequenceId, TResponse response)
            where TResponse : Response
        {
            return Task.Run(
                async () =>
                {
                    _sequenceIdManager.Setup(x => x.NextSequenceId).Returns(sequenceId);
                    var request = GetRequest(_retry, Timeout, sequenceId);
                    response.ReplyId = sequenceId;
                    _messageFlow.Setup(x => x.Receive(It.IsAny<Packet>(), It.IsAny<CancellationToken>()))
                        .Callback<Packet, CancellationToken>((_, token) => { _lock.Release(); })
                        .Returns(Task.FromResult(response as Response));

                    var res = _centralManager.Send<InvalidRequest, TResponse>(request);
                    await Task.Delay(Timeout / 2);

                    _subject.OnNext(_packet);

                    return await res;
                });
        }
        
        private InvalidRequest GetRequest(ushort retryCount, int responseTimeout, uint sequenceId = 1)
        {
            return new InvalidRequest
            {
                RetryCount = retryCount,
                TimeoutInMilliseconds = responseTimeout,
                SequenceId = sequenceId
            };
        }
    }
}