namespace Aristocrat.Bingo.Client.Messages.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Messages;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Moq;
    using ServerApiGateway;
    using System.Threading;
    using GamePlay;
    using Grpc.Core.Testing;
    using Grpc.Core;
    using Google.Protobuf.WellKnownTypes;
    using GameOutcome = GamePlay.GameOutcome;

    [TestClass]
    public class GameOutcomeServiceTests
    {
        private readonly Mock<IClientEndpointProvider<ClientApi.ClientApiClient>> _clientEnpointProvider =
            new Mock<IClientEndpointProvider<ClientApi.ClientApiClient>>(MockBehavior.Default);
        private readonly Mock<IMessageHandlerFactory> _messageHandler = new Mock<IMessageHandlerFactory>(MockBehavior.Default);

        private GameOutcomeService _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = CreateTarget();
        }

        [DataRow(true, false)]
        [DataRow(false, true)]
        [ExpectedException(typeof(ArgumentNullException))]
        [DataTestMethod]
        public void NullConstructorArgumentsTest(bool nullMessage, bool nullEnpoint)
        {
            _target = CreateTarget(nullMessage, nullEnpoint);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public async Task NoClientConnectedOnClaimRequestTest()
        {
            var client = new Mock<ClientApi.ClientApiClient>(MockBehavior.Default);
            _clientEnpointProvider.Setup(x => x.IsConnected).Returns(false);
            _clientEnpointProvider.Setup(x => x.Client).Returns(client.Object);
            var message = new RequestClaimWinMessage("123", 123, 1234);
            _ = await _target.ClaimWin(message, CancellationToken.None);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public async Task NullClientOnClaimRequestTest()
        {
            _clientEnpointProvider.Setup(x => x.IsConnected).Returns(true);
            _clientEnpointProvider.Setup(x => x.Client).Returns((ClientApi.ClientApiClient)null);
            var message = new RequestClaimWinMessage("123", 123, 1234);
            _ = await _target.ClaimWin(message, CancellationToken.None);
        }

        [TestMethod]
        public async Task RequestClaimWinTest()
        {
            const bool expectedAcceptedStatus = true;
            var expectedResponseCode = ResponseCode.Ok;
            var cardSerial = 123u;
            var gameSerial = 123L;

            var client = new Mock<ClientApi.ClientApiClient>(MockBehavior.Default);
            _clientEnpointProvider.Setup(x => x.IsConnected).Returns(true).Verifiable();
            _clientEnpointProvider.Setup(x => x.Client).Returns(client.Object).Verifiable();

            var outcome = new ClaimWinResponse
            {
                WinResult = ClaimWinResponse.Types.ClaimWinResult.Accepted,
                GameSerial = gameSerial,
                ClaimWinMeta = Any.Pack(new BingoGameClaimWinMeta()
                {
                    CardSerial = cardSerial
                }),
            };

            var response = TestCalls.AsyncUnaryCall(
                Task.FromResult(outcome),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });

            client.Setup(x => x.RequestClaimWinAsync(
                It.IsAny<ClaimWinRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
                .Returns(response);

            var message = new RequestClaimWinMessage("123", 123, 1234);
            var results = await _target.ClaimWin(message, CancellationToken.None);

            _clientEnpointProvider.Verify();

            Assert.AreEqual(expectedAcceptedStatus, results.Accepted);
            Assert.AreEqual(expectedResponseCode, results.ResponseCode);
            Assert.AreEqual(gameSerial, results.GameSerial);
            Assert.AreEqual(cardSerial, results.CardSerial);
        }

        [TestMethod]
        public async Task RequestClaimWinFailTest()
        {
            const bool expectedAcceptedStatus = false;
            var expectedResponseCode = ResponseCode.Rejected;
            var cardSerial = 123u;
            var gameSerial = 123L;

            var client = new Mock<ClientApi.ClientApiClient>(MockBehavior.Default);
            _clientEnpointProvider.Setup(x => x.IsConnected).Returns(true);
            _clientEnpointProvider.Setup(x => x.Client).Returns(client.Object);

            var outcome = new ClaimWinResponse
            {
                WinResult = ClaimWinResponse.Types.ClaimWinResult.Denied,
                GameSerial = gameSerial,
                ClaimWinMeta = Any.Pack(new BingoGameClaimWinMeta()
                {
                    CardSerial = cardSerial
                }),
            };

            var response = TestCalls.AsyncUnaryCall(
                Task.FromResult(outcome),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });

            client.Setup(x => x.RequestClaimWinAsync(
                It.IsAny<ClaimWinRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
                .Returns(response);

            var message = new RequestClaimWinMessage("123", 123, 1234);
            var results = await _target.ClaimWin(message, CancellationToken.None);

            Assert.AreEqual(expectedAcceptedStatus, results.Accepted);
            Assert.AreEqual(expectedResponseCode, results.ResponseCode);
            Assert.AreEqual(gameSerial, results.GameSerial);
            Assert.AreEqual(cardSerial, results.CardSerial);
        }

        [ExpectedException(typeof(RpcException))]
        [TestMethod]
        public async Task RequestClaimWinRpcExceptionTest()
        {
            var client = new Mock<ClientApi.ClientApiClient>(MockBehavior.Default);
            _clientEnpointProvider.Setup(x => x.IsConnected).Returns(true);
            _clientEnpointProvider.Setup(x => x.Client).Returns(client.Object);

            client.Setup(x => x.RequestClaimWinAsync(
                It.IsAny<ClaimWinRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
                .Throws(new RpcException(new Status(StatusCode.Cancelled, string.Empty)));

            var message = new RequestClaimWinMessage("123", 123, 1234);
            _ = await _target.ClaimWin(message, CancellationToken.None);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public async Task NullClientOnRequestPlayTest()
        {
            _clientEnpointProvider.Setup(x => x.IsConnected).Returns(true);
            _clientEnpointProvider.Setup(x => x.Client).Returns((ClientApi.ClientApiClient)null);
            var message = new RequestGameOutcomeMessage("123", 250, 1000, 25, 1, 10, 0, "TestTitle");
            await _target.RequestGame(message, CancellationToken.None);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public async Task NoClientConnectedOnRequestPlayTest()
        {
            var client = new Mock<ClientApi.ClientApiClient>(MockBehavior.Default);
            _clientEnpointProvider.Setup(x => x.IsConnected).Returns(false);
            _clientEnpointProvider.Setup(x => x.Client).Returns(client.Object);
            var message = new RequestGameOutcomeMessage("123", 250, 1000, 25, 1, 10, 0, "TestTitle");
            await _target.RequestGame(message, CancellationToken.None);
        }

        [ExpectedException(typeof(RpcException))]
        [TestMethod]
        public async Task RpcExceptionCancelledTest()
        {
            var client = new Mock<ClientApi.ClientApiClient>(MockBehavior.Default);
            _clientEnpointProvider.Setup(x => x.IsConnected).Returns(true);
            _clientEnpointProvider.Setup(x => x.Client).Returns(client.Object);

            client.Setup(x => x.RequestGamePlay(
                It.IsAny<GamePlayRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
                .Throws(new RpcException(new Status(StatusCode.Cancelled, string.Empty)));

            var message = new RequestGameOutcomeMessage("123", 250, 1000, 25, 1, 10, 0, "TestTitle");
            await _target.RequestGame(message, CancellationToken.None);
        }

        [ExpectedException(typeof(Exception))]
        [TestMethod]
        public async Task InnerRpcExceptionCancelledTest()
        {
            var client = new Mock<ClientApi.ClientApiClient>(MockBehavior.Default);
            _clientEnpointProvider.Setup(x => x.IsConnected).Returns(true);
            _clientEnpointProvider.Setup(x => x.Client).Returns(client.Object);

            client.Setup(x => x.RequestGamePlay(
                It.IsAny<GamePlayRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
                .Throws(new Exception(string.Empty ,new RpcException(new Status(StatusCode.Cancelled, string.Empty))));

            var message = new RequestGameOutcomeMessage("123", 250, 1000, 25, 1, 10, 0, "TestTitle");
            await _target.RequestGame(message, CancellationToken.None);
        }

        [ExpectedException(typeof(RpcException))]
        [TestMethod]
        public async Task ReportGameOutcomeRpcExceptionTest()
        {
            var client = new Mock<ClientApi.ClientApiClient>(MockBehavior.Default);
            _clientEnpointProvider.Setup(x => x.IsConnected).Returns(true);
            _clientEnpointProvider.Setup(x => x.Client).Returns(client.Object);

            client.Setup(x => x.ReportGameOutcomeAsync(
                    It.IsAny<ServerApiGateway.GameOutcome>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Throws(new RpcException(new Status(StatusCode.Cancelled, string.Empty)));

            var message = new ReportGameOutcomeMessage { JoinTime = DateTime.UtcNow, StartTime = DateTime.UtcNow };
            await _target.ReportGameOutcome(message, CancellationToken.None);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public async Task NullClientOnReportGameOutcomeTest()
        {
            _clientEnpointProvider.Setup(x => x.IsConnected).Returns(true);
            _clientEnpointProvider.Setup(x => x.Client).Returns((ClientApi.ClientApiClient)null);
            var message = new ReportGameOutcomeMessage { JoinTime = DateTime.UtcNow, StartTime = DateTime.UtcNow };
            await _target.ReportGameOutcome(message, CancellationToken.None);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public async Task NoClientConnectedOnReportGameOutcomeTest()
        {
            var client = new Mock<ClientApi.ClientApiClient>(MockBehavior.Default);
            _clientEnpointProvider.Setup(x => x.IsConnected).Returns(false);
            _clientEnpointProvider.Setup(x => x.Client).Returns(client.Object);
            var message = new ReportGameOutcomeMessage { JoinTime = DateTime.UtcNow, StartTime = DateTime.UtcNow };
            await _target.ReportGameOutcome(message, CancellationToken.None);
        }

        [TestMethod]
        public async Task StreamClosesWhenGameEndFound()
        {
            var client = new Mock<ClientApi.ClientApiClient>(MockBehavior.Default);
            _clientEnpointProvider.Setup(x => x.IsConnected).Returns(true);
            _clientEnpointProvider.Setup(x => x.Client).Returns(client.Object);

            var stream = new Mock<IAsyncStreamReader<GamePlayResponse>>(MockBehavior.Default);

            var outcome = new GamePlayResponse
            {
                GamePlayResponseMeta = Any.Pack(new BingoGamePlayResponseMeta
                {
                    Cards =
                    {
                        new BingoGamePlayResponseMeta.Types.CardPlayed()
                    },
                    BallCall = string.Empty
                }),
                Status = true,
                ReportType = GamePlayResponse.Types.ReportType.End
            };

            var handledResponse = new GameOutcomeResponse(ResponseCode.Ok);
            stream.Setup(x => x.MoveNext(It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));
            stream.Setup(x => x.Current).Returns(outcome);
            var response = TestCalls.AsyncServerStreamingCall(
                stream.Object,
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });

            client.Setup(x => x.RequestGamePlay(
                It.IsAny<GamePlayRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
                .Returns(response);
            _messageHandler.Setup(x => x.Handle<GameOutcomeResponse, GameOutcome>(It.IsAny<GameOutcome>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(handledResponse))
                .Verifiable();

            var message = new RequestGameOutcomeMessage("123", 250, 1000, 25, 1, 10, 0, "TestTitle");
            Assert.IsTrue(await _target.RequestGame(message, CancellationToken.None));
        }

        [TestMethod]
        public async Task ResponseHandledFailedEndsStream()
        {
            var client = new Mock<ClientApi.ClientApiClient>(MockBehavior.Default);
            _clientEnpointProvider.Setup(x => x.IsConnected).Returns(true);
            _clientEnpointProvider.Setup(x => x.Client).Returns(client.Object);

            var stream = new Mock<IAsyncStreamReader<GamePlayResponse>>(MockBehavior.Default);

            var outcome = new GamePlayResponse
            {
                GamePlayResponseMeta = Any.Pack(new BingoGamePlayResponseMeta
                {
                    Cards =
                    {
                        new BingoGamePlayResponseMeta.Types.CardPlayed()
                    },
                    BallCall = string.Empty
                }),
                Status = true,
                ReportType = GamePlayResponse.Types.ReportType.Update,
            };

            var handledResponse = new GameOutcomeResponse(ResponseCode.Rejected);
            stream.Setup(x => x.MoveNext(It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));
            stream.Setup(x => x.Current).Returns(outcome);
            var response = TestCalls.AsyncServerStreamingCall(
                stream.Object,
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });

            client.Setup(x => x.RequestGamePlay(
                It.IsAny<GamePlayRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
                .Returns(response);
            _messageHandler.Setup(x => x.Handle<GameOutcomeResponse, GameOutcome>(It.IsAny<GameOutcome>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(handledResponse))
                .Verifiable();

            var message = new RequestGameOutcomeMessage("123", 250, 1000, 25, 1, 10, 0, "TestTitle");
            Assert.IsTrue(await _target.RequestGame(message, CancellationToken.None));
        }

        [TestMethod]
        public async Task StreamClosesWhenResponseIsRejected()
        {
            var client = new Mock<ClientApi.ClientApiClient>(MockBehavior.Default);
            _clientEnpointProvider.Setup(x => x.IsConnected).Returns(true);
            _clientEnpointProvider.Setup(x => x.Client).Returns(client.Object);

            var stream = new Mock<IAsyncStreamReader<GamePlayResponse>>(MockBehavior.Default);

            var outcome = new GamePlayResponse
            {
                Status = false
            };

            var handledResponse = new GameOutcomeResponse(ResponseCode.Ok);
            stream.Setup(x => x.MoveNext(It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));
            stream.Setup(x => x.Current).Returns(outcome);
            var response = TestCalls.AsyncServerStreamingCall(
                stream.Object,
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });

            client.Setup(x => x.RequestGamePlay(
                It.IsAny<GamePlayRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
                .Returns(response);
            _messageHandler.Setup(x => x.Handle<GameOutcomeResponse, GameOutcome>(It.IsAny<GameOutcome>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(handledResponse))
                .Verifiable();

            var message = new RequestGameOutcomeMessage("123", 250, 1000, 25, 1, 10, 0, "TestTitle");
            Assert.IsTrue(await _target.RequestGame(message, CancellationToken.None));
        }

        [DataRow(false, ResponseCode.Rejected)]
        [DataRow(true, ResponseCode.Ok)]
        [DataTestMethod]
        public async Task ReportOutcomeTest(bool succeeded, ResponseCode expectedCode)
        {
            var winResult = new WinResult(12345, 100, 13, 2658, 5678, "Test Pattern", 123, false, 1);
            var cardPlayed = new CardPlayed(123, 52345, false);
            var message = new ReportGameOutcomeMessage
            {
                BallCall = Enumerable.Range(1, 52),
                WinResults = new [] { winResult },
                BetAmount = 100,
                TotalWin = 100,
                PaidAmount = 100,
                StartingBalance = 25000,
                CardsPlayed = new[] { cardPlayed },
                TransactionId = 1,
                MachineSerial = "Test serial",
                DenominationId = 100,
                FacadeKey = 123,
                FinalBalance = 25000,
                GameEndWinEligibility = 1,
                GameSerial = 1234,
                Paytable = "Test Paytable",
                JoinBall = 0,
                StartTime = DateTime.UtcNow - TimeSpan.FromSeconds(3),
                JoinTime = DateTime.UtcNow,
                ProgressiveLevels = new []{ 1L, 2L, 3L },
                GameTitleId = 123,
                ThemeId = 123
            };

            var client = new Mock<ClientApi.ClientApiClient>(MockBehavior.Default);
            _clientEnpointProvider.Setup(x => x.IsConnected).Returns(true);
            _clientEnpointProvider.Setup(x => x.Client).Returns(client.Object);
            var response = TestCalls.AsyncUnaryCall(
                Task.FromResult(new GameOutcomeAck { Succeeded = succeeded }),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });

            client.Setup(x => x.ReportGameOutcomeAsync(
                    It.IsAny<ServerApiGateway.GameOutcome>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(response);

            var result = await _target.ReportGameOutcome(message, CancellationToken.None);
            Assert.AreEqual(expectedCode, result.ResponseCode);
        }

        private GameOutcomeService CreateTarget(bool nullMessage = false, bool nullEnpoint = false)
        {
            return new GameOutcomeService(
                nullMessage ? null : _messageHandler.Object,
                nullEnpoint ? null : _clientEnpointProvider.Object);
        }
    }
}