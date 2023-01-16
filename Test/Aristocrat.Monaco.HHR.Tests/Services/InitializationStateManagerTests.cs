namespace Aristocrat.Monaco.Hhr.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Subjects;
    using System.Threading;
    using System.Threading.Tasks;
    using Kernel.Contracts.MessageDisplay;
    using Client.Data;
    using Client.Messages;
    using Client.WorkFlow;
    using Events;
    using Hhr.Services;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Storage.Helpers;

    [TestClass]
    public class InitializationStateManagerTests
    {
        private readonly Mock<IEventBus> _eventBus
            = new Mock<IEventBus>(MockBehavior.Default);

        private readonly Mock<ICommunicationService> _communicationService
            = new Mock<ICommunicationService>(MockBehavior.Default);

        private readonly Mock<IGameDataService> _gameDataService
            = new Mock<IGameDataService>(MockBehavior.Default);

        private readonly Mock<ICentralManager> _centralManager
            = new Mock<ICentralManager>(MockBehavior.Default);

        private readonly Mock<IPlayerSessionService> _playerSessionService
            = new Mock<IPlayerSessionService>(MockBehavior.Default);

        private readonly Mock<ISystemDisableManager> _systemDisableManager
            = new Mock<ISystemDisableManager>(MockBehavior.Default);

        private readonly Mock<ITransactionIdProvider> _transactionIdProvider
            = new Mock<ITransactionIdProvider>(MockBehavior.Default);

        private readonly Mock<IPendingRequestEntityHelper> _pendingRequestEntityHelper
            = new Mock<IPendingRequestEntityHelper>(MockBehavior.Default);

        private readonly Mock<IPropertiesManager> _properties
            = new Mock<IPropertiesManager>(MockBehavior.Default);

        private readonly Subject<Response> _responseSubject = new Subject<Response>();
        private readonly ManualResetEvent _waitForReadyToPlay = new ManualResetEvent(false);
        private readonly ManualResetEvent _waitForConnection = new ManualResetEvent(false);

        private InitializationStateManager _target;
        private Action<CentralServerOnline> _centralServerOnline;
        private Action<CentralServerOffline> _centralServerOffline;

        [TestInitialize]
        public void Initialize()
        {
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Action<CentralServerOnline>>()))
                .Callback<object, Action<CentralServerOnline>>((o, a) => _centralServerOnline = a);
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Action<CentralServerOffline>>()))
                .Callback<object, Action<CentralServerOffline>>((o, a) => _centralServerOffline = a);

            _pendingRequestEntityHelper.Setup(x => x.PendingRequests)
                .Returns(new List<KeyValuePair<Request, Type>>()).Verifiable();

            _transactionIdProvider.Setup(x => x.SetLastId(It.IsAny<uint>())).Verifiable();

            _communicationService.Setup(x => x.ConnectTcp(It.IsAny<CancellationToken>())).Returns(Task.FromResult(true))
                .Callback<CancellationToken>(
                    c =>
                    {
                        _centralServerOnline.Invoke(new CentralServerOnline());
                        _waitForConnection.Set();
                    })
                .Verifiable();
            _communicationService.Setup(x => x.ConnectUdp(It.IsAny<CancellationToken>())).Returns(Task.FromResult(true))
                .Verifiable();
            _communicationService.Setup(x => x.Disconnect(It.IsAny<CancellationToken>())).Returns(Task.FromResult(true))
                .Callback<CancellationToken>(
                    c =>
                    {
                        Setup();
                        _centralServerOffline.Invoke(new CentralServerOffline());
                    });

            Setup(false, false, false, false, false);

            _centralManager.Setup(x => x.UnsolicitedResponses).Returns(_responseSubject);
            _centralManager
                .Setup(
                    x => x.Send<ReadyToPlayRequest, CloseTranResponse>(
                        It.IsAny<ReadyToPlayRequest>(),
                        It.IsAny<CancellationToken>())).Returns(Task.FromResult(new CloseTranResponse()))
                .Callback<ReadyToPlayRequest, CancellationToken>(
                    (_, __) => { _waitForReadyToPlay.Set(); }).Verifiable();

            // Shorten some timeouts so the tests happen more quickly
            HhrConstants.ReconnectTimeInMilliseconds = 200.0;
            HhrConstants.ReInitializationTimeInMilliseconds = 200.0;
            HhrConstants.RetryReadyToPlay = 200.0;

            _properties.Setup(x => x.GetProperty(HHRPropertyNames.ManualHandicapMode, It.IsAny<string>())).Returns(HhrConstants.DetectPickMode);
            ClientProperties.ManualHandicapMode = HhrConstants.DetectPickMode;

            _systemDisableManager.SetupGet(x => x.CurrentDisableKeys).Returns(new List<Guid>());

            CreateInitializationStateManager();
        }

        [DataRow(false, false, false, false, true, DisplayName = "Nothing fails, expect success.")]
        [DataRow(true, false, false, false, false, DisplayName = "Connect Request fails, expect Initialization Failed")]
        [DataRow(false, true, false, false, false, DisplayName = "Parameter Request fails, expect Initialization Failed")]
        [DataRow(false, false, true, false, false, DisplayName = "Game Info Request fails, expect Initialization Failed")]
        [DataRow(false, false, false, true, false, DisplayName = "Player Id Request fails, expect Initialization Failed")]
        [DataTestMethod]
        public void ConnectToServerExpectInitializationFailure(
            bool connectFails,
            bool parameterRequestFails,
            bool gameInfoRequestFails,
            bool playerIdRequestFails,
            bool isSuccess)
        {
            Setup(connectFails, parameterRequestFails, gameInfoRequestFails, playerIdRequestFails);
            _waitForConnection.WaitOne();

            if (isSuccess)
            {
                _waitForReadyToPlay.WaitOne();
                SimulateServerResponse(GtCommand.Play);
                Assert.AreEqual(HhrConstants.QuickPickMode, ClientProperties.ManualHandicapMode);
                Success();
            }
            else
            {
                Failed();
            }
        }

        [TestMethod]
        public void ReceivePlayPauseWhileWaitingForReadyToPlayExpectInitializationInProgress()
        {
            _waitForReadyToPlay.WaitOne();
            SimulateServerResponse(GtCommand.PlayPause);

            _eventBus.Verify(x => x.Publish(It.IsAny<ProtocolInitializationInProgress>()), Times.Once);
            _eventBus.Verify(x => x.Publish(It.IsAny<ProtocolInitializationFailed>()), Times.Never);
            _eventBus.Verify(x => x.Publish(It.IsAny<ProtocolInitializationComplete>()), Times.Never);

            _gameDataService.Verify();
            _centralManager.Verify();
            _playerSessionService.Verify();

            // Reset calls so to verify that we call ReadyToPlay again.
            _waitForReadyToPlay.Reset();
            _centralManager.ResetCalls();

            _waitForReadyToPlay.WaitOne();
            _eventBus.Verify(x => x.Publish(It.IsAny<ProtocolInitializationComplete>()), Times.Never);

            _centralManager.Verify(
                x => x.Send<ReadyToPlayRequest, CloseTranResponse>(
                    It.IsAny<ReadyToPlayRequest>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [TestMethod]
        public void ReceiveParameterRequestDuringInitializationExpectInitializationFailedAndReInitialization()
        {
            _waitForConnection.WaitOne();
            _responseSubject.OnNext(new ParameterResponse());

            _communicationService.Verify(x => x.Disconnect(It.IsAny<CancellationToken>()), Times.Once);
            _systemDisableManager.Verify(
                x => x.Disable(
                    HhrConstants.CentralServerOffline,
                    SystemDisablePriority.Immediate,
                    It.IsAny<string>(),
                    It.IsAny<CultureProviderType>(),
                    It.IsAny<bool>(),
                    It.IsAny<Func<string>>(),
                    It.IsAny<Type>()),
                Times.AtLeastOnce);

            _eventBus.Verify(x => x.Publish(It.IsAny<ProtocolInitializationInProgress>()), Times.Once);
            _eventBus.Verify(x => x.Publish(It.IsAny<ProtocolInitializationComplete>()), Times.Never);
            Setup(false, false, false, false, false);

            // Reset calls so to verify that we call ReadyToPlay again.
            _waitForReadyToPlay.Reset();
            _centralManager.ResetCalls();
            _gameDataService.ResetCalls();
            _eventBus.ResetCalls();
            _playerSessionService.ResetCalls();

            _waitForReadyToPlay.WaitOne();
            SimulateServerResponse(GtCommand.Play);

            Success();
        }

        [TestMethod]
        public async Task DisconnectFromServerExpectConnectionAttempt()
        {
            _centralServerOffline.Invoke(new CentralServerOffline());
            await Task.Delay(Convert.ToInt32(HhrConstants.ReInitializationTimeInMilliseconds));

            _centralServerOnline.Invoke(new CentralServerOnline());
            _waitForReadyToPlay.WaitOne();
            SimulateServerResponse(GtCommand.Play);
            Success();
        }

        private void CreateInitializationStateManager()
        {
            _target = new InitializationStateManager(
                _eventBus.Object,
                _communicationService.Object,
                _gameDataService.Object,
                _centralManager.Object,
                _playerSessionService.Object,
                _systemDisableManager.Object,
                _transactionIdProvider.Object,
                _pendingRequestEntityHelper.Object,
                _properties.Object);
        }

        private void Setup(
            bool connectFails = true,
            bool parameterRequestFails = true,
            bool gameInfoRequestFails = true,
            bool playerIdRequestFails = true,
            bool progressiveInfoFails = true)
        {
            if (connectFails)
            {
                _centralManager
                    .Setup(
                        x => x.Send(
                            It.IsAny<ConnectRequest>(),
                            It.IsAny<CancellationToken>())).Throws(new UnexpectedResponseException(new Response()))
                    .Verifiable();
            }
            else
            {
                _centralManager
                    .Setup(
                        x => x.Send(
                            It.IsAny<ConnectRequest>(),
                            It.IsAny<CancellationToken>())).Returns(Task.FromResult(new Response())).Verifiable();
            }

            if (parameterRequestFails)
            {
                _gameDataService.Setup(x => x.GetGameParameters(It.IsAny<bool>()))
                    .Returns(() => Task.FromResult((ParameterResponse)null));
            }
            else
            {
                _gameDataService.Setup(x => x.GetGameParameters(It.IsAny<bool>()))
                    .Returns(Task.FromResult(new ParameterResponse { GameIdCount = 1 })).Verifiable();
            }

            if (gameInfoRequestFails)
            {
                _gameDataService.Setup(x => x.GetGameInfo(It.IsAny<bool>())).Returns(
                    () => Task.FromResult(new List<GameInfoResponse>().AsEnumerable()));
            }
            else
            {
                _gameDataService.Setup(x => x.GetGameInfo(It.IsAny<bool>())).Returns(
                        () =>
                            Task.FromResult(new List<GameInfoResponse> { new GameInfoResponse() }.AsEnumerable()))
                    .Verifiable();
            }

            if (playerIdRequestFails)
            {
                _playerSessionService.Setup(x => x.GetCurrentPlayerId(It.IsAny<int>()))
                    .Throws(new UnexpectedResponseException(new Response()));
            }
            else
            {
                _playerSessionService.Setup(x => x.GetCurrentPlayerId(It.IsAny<int>())).Returns(Task.FromResult("xyz")).Verifiable();
            }

            if (progressiveInfoFails)
            {
                _gameDataService.Setup(x => x.GetProgressiveInfo(It.IsAny<bool>())).Returns(
                    () => Task.FromResult(new List<ProgressiveInfoResponse>().AsEnumerable()));
            }
            else
            {
                _gameDataService.Setup(x => x.GetProgressiveInfo(It.IsAny<bool>()))
                    .Returns(() => Task.FromResult(new List<ProgressiveInfoResponse>().AsEnumerable())).Verifiable();
            }
        }

        private void SimulateServerResponse(GtCommand command)
        {
            _responseSubject.OnNext(new CommandResponse { ECommand = command });
        }

        private void Success()
        {
            _eventBus.Verify(x => x.Publish(It.IsAny<ProtocolInitializationInProgress>()), Times.AtLeastOnce);
            _eventBus.Verify(x => x.Publish(It.IsAny<ProtocolInitializationFailed>()), Times.Never);
            _eventBus.Verify(x => x.Publish(It.IsAny<ProtocolInitializationComplete>()), Times.Once);

            _gameDataService.Verify();
            _centralManager.Verify();
            _playerSessionService.Verify();
        }

        private void Failed()
        {
            _eventBus.Verify(x => x.Publish(It.IsAny<ProtocolInitializationInProgress>()), Times.AtLeastOnce);
            _eventBus.Verify(x => x.Publish(It.IsAny<ProtocolInitializationFailed>()), Times.Once);
            _eventBus.Verify(x => x.Publish(It.IsAny<ProtocolInitializationComplete>()), Times.Never);
        }
    }
}