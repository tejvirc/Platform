namespace Aristocrat.Monaco.Hhr.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Subjects;
    using System.Threading;
    using System.Threading.Tasks;
    using Client.Communication;
    using Client.Messages;
    using Client.Tests.Messages;
    using Client.WorkFlow;
    using Events;
    using Hardware.Contracts.Button;
    using Hhr.Services;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using SimpleInjector;
    using Storage.Helpers;
    using Test.Common;

    [TestClass]
    public class RequestTimeoutBehaviorHandlerTests
    {
        private readonly Mock<ICentralManager> _centralManager = new Mock<ICentralManager>(MockBehavior.Default);
        private readonly Mock<IConnectionOpener> _connectionOpener = new Mock<IConnectionOpener>(MockBehavior.Default);

        private readonly Mock<IPendingRequestEntityHelper> _pendingRequestEntityHelper =
            new Mock<IPendingRequestEntityHelper>(MockBehavior.Default);

        private readonly BehaviorSubject<ConnectionStatus> _connectionStatusSubject =
            new BehaviorSubject<ConnectionStatus>(new ConnectionStatus { ConnectionState = ConnectionState.Connected });

        private readonly Subject<(Request request, Type responseType)> _sentRequests =
            new Subject<(Request request, Type responseType)>();

        private readonly Subject<(Request request, Response response)> _receivedResponses =
            new Subject<(Request request, Response response)>();

        private readonly Guid _invalidRequestTimeoutLockup = new Guid("1E92667D-0368-444D-B728-5F0EE994C010");

        private readonly Mock<IPropertiesManager> _propertiesManager =
            new Mock<IPropertiesManager>(MockBehavior.Default);
        private IEnumerable<KeyValuePair<Request, Type>> _pendingRequests = new List<KeyValuePair<Request, Type>>();

        private readonly double _requestRetryTimeout = 100.0;

        private Container _container;
        private Mock<ISystemDisableManager> _disableManager;
        private Mock<IEventBus> _eventBus;
        private InvalidRequest _request;
        private InvalidRequest _sentRequest;
        private RequestTimeoutBehaviorHandler _target;
        private Action<ProtocolInitializationComplete> _protocolInitializationComplete;
        private Action<DownEvent> _jackpotKeyPressed;

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);

            _disableManager = MoqServiceManager.CreateAndAddService<ISystemDisableManager>(MockBehavior.Default);
            _disableManager.Setup(x => x.CurrentDisableKeys).Returns(new List<Guid>());
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            InitializeContainer();

            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Action<ProtocolInitializationComplete>>()))
                .Callback<object, Action<ProtocolInitializationComplete>
                >((c, a) => _protocolInitializationComplete = a);

            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Action<DownEvent>>(), It.IsAny<Predicate<DownEvent>>()))
                .Callback<object, Action<DownEvent>, Predicate<DownEvent>
                >((c, a, p) => _jackpotKeyPressed = a);

            _centralManager.Setup(
                    x =>
                        x.Send<InvalidRequest, InvalidResponse>(
                            It.IsAny<InvalidRequest>(),
                            It.IsAny<CancellationToken>()))
                .Callback<InvalidRequest, CancellationToken>(
                    (req, token) =>
                    {
                        _sentRequest = req;
                    })
                .Returns(Task.FromResult(new InvalidResponse()));
            _centralManager.Setup(x => x.RequestObservable).Returns(_sentRequests);
            _centralManager.Setup(x => x.RequestResponseObservable).Returns(_receivedResponses);

            _connectionOpener.Setup(x => x.ConnectionStatus).Returns(_connectionStatusSubject);
            _connectionOpener.Setup(x => x.CurrentStatus).Returns(_connectionStatusSubject.Value);

            _propertiesManager
                .Setup(x => x.GetProperty(HHRPropertyNames.FailedRequestRetryTimeout, It.IsAny<double>()))
                .Returns(_requestRetryTimeout);

            _pendingRequestEntityHelper.SetupSet(x => x.PendingRequests = It.IsAny<IEnumerable<KeyValuePair<Request, Type>>>()).Callback<IEnumerable<KeyValuePair<Request, Type>>>(x => _pendingRequests = x);

            _target = new RequestTimeoutBehaviorHandler(
                _centralManager.Object,
                _container,
                _pendingRequestEntityHelper.Object,
                _eventBus.Object);
            _request = new InvalidRequest
            {
                RequestTimeout = new LockupRequestTimeout { LockupKey = _invalidRequestTimeoutLockup }
            };
        }

        [DataTestMethod]
        [DataRow(true, false, false, false, true)]
        [DataRow(false, true, false, false, true)]
        [DataRow(false, false, true, false, true)]
        [DataRow(false, false, false, true, true)]
        [DataRow(false, false, false, false, false)]
        public void NullConstructorTest(
            bool nullCentralManager,
            bool nullContainer,
            bool nullEntityHelper,
            bool nullEventBus,
            bool throwsException)
        {
            if (throwsException)
            {
                Assert.ThrowsException<ArgumentNullException>(
                    () => CreateRequestTimeoutBehaviorHandler(
                        nullCentralManager,
                        nullContainer,
                        nullEntityHelper,
                        nullEventBus));
            }
            else
            {
                Assert.IsNotNull(_target);
            }
        }

        [TestMethod]
        public void RequestFailsExpectTimeoutLockupExecuted()
        {
            SendRequest(_request, typeof(InvalidResponse));
            ReceiveResponse(_request, GetUnexpectedResponse(MessageStatus.TimedOut));

            _disableManager.Verify(
                x => x.Disable(
                    _invalidRequestTimeoutLockup,
                    It.IsAny<SystemDisablePriority>(),
                    It.IsAny<Func<string>>(),
                    It.IsAny<bool>(),
                    It.IsAny<Func<string>>(),
                    It.IsAny<Type>()),
                Times.Once);
        }

        [TestMethod]
        public void RequestFailsExpectRetryWhenProtocolInitializationIsComplete()
        {
            SendRequest(_request, typeof(InvalidResponse));
            ReceiveResponse(_request, GetUnexpectedResponse(MessageStatus.TimedOut));

            _disableManager.Verify(
                x => x.Disable(
                    _invalidRequestTimeoutLockup,
                    It.IsAny<SystemDisablePriority>(),
                    It.IsAny<Func<string>>(),
                    It.IsAny<bool>(),
                    It.IsAny<Func<string>>(),
                    It.IsAny<Type>()),
                Times.AtLeastOnce);

            _protocolInitializationComplete.Invoke(new ProtocolInitializationComplete());

            _centralManager.Verify(
                x => x.Send<InvalidRequest, InvalidResponse>(It.IsAny<InvalidRequest>(), It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);
            Assert.AreEqual(true, _sentRequest.IsFailed);

            ReceiveResponse(_request, new InvalidResponse());

            _eventBus.Verify(x => x.Publish(It.IsAny<PendingRequestRemovedEvent>()), Times.Once);
        }

        [DataRow(true, DisplayName = "Request Pending")]
        [DataRow(false, DisplayName = "Request not pending")]
        [TestMethod]
        public void PowerCycleWithPendingRequestsResendAfterInitializationIsComplete(bool pendingRequest)
        {
            _request.IsFailed = pendingRequest;

            _pendingRequestEntityHelper.Setup(x => x.PendingRequests).Returns(
                new List<KeyValuePair<Request, Type>> { new KeyValuePair<Request, Type>(_request, typeof(InvalidResponse)) });
            _disableManager.Setup(x => x.CurrentDisableKeys)
                .Returns(new List<Guid> { HhrConstants.ProtocolInitializationInProgress });

            _centralManager.ResetCalls();
            _target.Dispose();
            InitializeContainer();

            _target = new RequestTimeoutBehaviorHandler(
                _centralManager.Object,
                _container,
                _pendingRequestEntityHelper.Object,
                _eventBus.Object);

            _disableManager.Setup(x => x.CurrentDisableKeys).Returns(new List<Guid>());

            _protocolInitializationComplete.Invoke(new ProtocolInitializationComplete());

            ReceiveResponse(_request, new InvalidResponse());

            _centralManager.Verify(
                x => x.Send<InvalidRequest, InvalidResponse>(It.IsAny<InvalidRequest>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _disableManager.Verify(x => x.Enable(_invalidRequestTimeoutLockup), Times.Once);
            _eventBus.Verify(x => x.Publish(It.IsAny<PendingRequestRemovedEvent>()), Times.Once);
        }

        [TestMethod]
        public void SendARequestReceiveAnUnexpectedResponseExpectRequestAsFailed()
        {
            SendRequest(_request, typeof(InvalidResponse));
            Assert.IsTrue(_pendingRequests.Any(x => x.Key.Command == Command.CmdInvalidCommand));
            ReceiveResponse(_request, new Response
            {
                MessageStatus = MessageStatus.UnexpectedResponse
            });
            Assert.IsTrue(_pendingRequests.Any(x => x.Key.Command == Command.CmdInvalidCommand && x.Key.IsFailed));
        }

        [TestMethod]
        public void SendARequestReceiveAValidResponseExpectRequestAsFailed()
        {
            SendRequest(_request, typeof(InvalidResponse));
            Assert.IsTrue(_pendingRequests.Any(x => x.Key.Command == Command.CmdInvalidCommand));
            ReceiveResponse(_request, new InvalidResponse());
            Assert.IsTrue(!_pendingRequests.Any());
        }

        [TestMethod]
        public void SendARequestWithIdleBehaviorExpectNoRequestPending()
        {
            SendRequest(new InvalidRequest(), typeof(InvalidResponse));
            Assert.IsTrue(!_pendingRequests.Any());
            ReceiveResponse(_request, new Response
            {
                MessageStatus = MessageStatus.UnexpectedResponse
            });
            Assert.IsTrue(!_pendingRequests.Any());
        }

        [TestMethod]
        public void HaveFailedRequestWhenJackpotKeyIsToggledFailedRequestAreCleared()
        {
            _request.IsFailed = true;
            _pendingRequestEntityHelper.Setup(x => x.PendingRequests).Returns(
                new List<KeyValuePair<Request, Type>> { new KeyValuePair<Request, Type>(_request, typeof(InvalidResponse)) });

            CreateRequestTimeoutBehaviorHandler();

            _jackpotKeyPressed.Invoke(new DownEvent((int)ButtonLogicalId.Button30));
            
            _disableManager.Verify(x => x.Enable(_invalidRequestTimeoutLockup), Times.Once);

            _eventBus.Verify(x => x.Publish(It.IsAny<PendingRequestRemovedEvent>()), Times.Once);
        }

        [TestMethod]
        public void HaveNoFailedRequestWhenJackpotKeyIsToggledDoNothing()
        {
            var _request = new InvalidRequest();
            _request.IsFailed = false;
            _pendingRequestEntityHelper.Setup(x => x.PendingRequests).Returns(
                new List<KeyValuePair<Request, Type>> { new KeyValuePair<Request, Type>(_request, typeof(InvalidResponse)) });

            CreateRequestTimeoutBehaviorHandler();

            _jackpotKeyPressed.Invoke(new DownEvent((int)ButtonLogicalId.Button30));

            _disableManager.Verify(x => x.Enable(_invalidRequestTimeoutLockup), Times.Never);

            _eventBus.Verify(x => x.Publish(It.IsAny<PendingRequestRemovedEvent>()), Times.Never);
        }

        [TestMethod]
        public void NoRequestPendingReceiveAUnexpectedResponseExpectNoExitBehavior()
        {
            ReceiveResponse(_request, new Response
            {
                MessageStatus = MessageStatus.UnexpectedResponse
            });

            _disableManager.Verify(
                x => x.Disable(
                    _invalidRequestTimeoutLockup,
                    It.IsAny<SystemDisablePriority>(),
                    It.IsAny<Func<string>>(),
                    It.IsAny<bool>(),
                    It.IsAny<Func<string>>(),
                    It.IsAny<Type>()),
                Times.Never);
        }

        [TestMethod]
        public void ResponseReceivedForFailedRequestWhenARequestIsPendingCallExitBehaviour()
        {
            _request.IsFailed = true;
            var anotherRequest = new InvalidRequest
            {
                RequestTimeout = new LockupRequestTimeout { LockupKey = _invalidRequestTimeoutLockup }
            };
            // Two request pending - one failed and one in progress
            _pendingRequestEntityHelper.Setup(x => x.PendingRequests).Returns(
                new List<KeyValuePair<Request, Type>> { new KeyValuePair<Request, Type>(_request, typeof(InvalidResponse)), new KeyValuePair<Request, Type>(anotherRequest, typeof(InvalidResponse)) });

            _target.Dispose();
            InitializeContainer();

            _target = new RequestTimeoutBehaviorHandler(
                _centralManager.Object,
                _container,
                _pendingRequestEntityHelper.Object,
                _eventBus.Object);

            // Receive a response for the failed request, and expect its ExitBehaviour is called despite another request with similar ExitBehaviour is pending.
            ReceiveResponse(_request, new InvalidResponse());
            _disableManager.Verify(
                x => x.Enable(_invalidRequestTimeoutLockup),
                Times.Once);
        }
        private void CreateRequestTimeoutBehaviorHandler(
            bool nullCentralManager = false,
            bool nullContainer = false,
            bool nullEntityHelper = false,
            bool nullEventBus = false)
        {
            _target = new RequestTimeoutBehaviorHandler(
                nullCentralManager ? null : _centralManager.Object,
                nullContainer ? null : _container,
                nullEntityHelper ? null : _pendingRequestEntityHelper.Object,
                nullEventBus ? null : _eventBus.Object);
        }

        private void InitializeContainer()
        {
            _container = new Container();
            _container.Collection.Register(typeof(IRequestTimeout), typeof(IRequestTimeout).Assembly);
            _container.Register(typeof(IRequestTimeoutBehavior<>), typeof(IRequestTimeoutBehavior<>).Assembly);
        }
        private Response GetUnexpectedResponse(MessageStatus status)
        {
            return new Response { MessageStatus = status };
        }

        private void SendRequest(Request request, Type responseType)
        {
            _sentRequests.OnNext((request, responseType));
        }

        private void ReceiveResponse(Request request, Response response)
        {
            _receivedResponses.OnNext((request, response));
        }
    }
}