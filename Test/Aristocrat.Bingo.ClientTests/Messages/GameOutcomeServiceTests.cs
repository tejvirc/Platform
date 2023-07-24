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

        private GameOutcomeService CreateTarget(bool nullMessage = false, bool nullEnpoint = false)
        {
            return new GameOutcomeService(
                nullMessage ? null : _messageHandler.Object,
                nullEnpoint ? null : _clientEnpointProvider.Object);
        }
    }
}