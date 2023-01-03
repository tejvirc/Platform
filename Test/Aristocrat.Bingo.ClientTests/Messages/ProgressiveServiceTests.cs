namespace Aristocrat.Bingo.Client.Messages.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Grpc.Core;
    using Grpc.Core.Testing;
    using Messages;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Progressives;
    using ServerApiGateway;

    [TestClass]
    public class ProgressiveServiceTests
    {
        private readonly Mock<IClientEndpointProvider<ProgressiveApi.ProgressiveApiClient>> _clientEnpointProvider =
            new Mock<IClientEndpointProvider<ProgressiveApi.ProgressiveApiClient>>(MockBehavior.Default);
        private readonly Mock<IMessageHandlerFactory> _messageHandler = new Mock<IMessageHandlerFactory>(MockBehavior.Default);
        private readonly Mock<IProgressiveAuthorizationProvider> _authorizationProvider =
            new Mock<IProgressiveAuthorizationProvider>(MockBehavior.Default);

        private ProgressiveService _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = CreateTarget();
        }

        [DataRow(true, false, false)]
        [DataRow(false, true, false)]
        [DataRow(false, false, true)]
        [ExpectedException(typeof(ArgumentNullException))]
        [DataTestMethod]
        public void NullConstructorArgumentsTest(bool nullMessage, bool nullEnpoint, bool nullAuthorization)
        {
            _target = CreateTarget(nullEnpoint, nullMessage, nullAuthorization);
        }

        [TestMethod]
        public async Task RequestProgressiveInfoTest()
        {
            var machineSerial = "123";
            var gameTitleId = 456;
            var maxBet = 25;
            var denom = 1;

            var client = new Mock<ProgressiveApi.ProgressiveApiClient>(MockBehavior.Default);
            _clientEnpointProvider.Setup(x => x.IsConnected).Returns(true).Verifiable();
            _clientEnpointProvider.Setup(x => x.Client).Returns(client.Object).Verifiable();

            var request = new ProgressiveInfoRequest
            {
                GameTitleId = gameTitleId,
                MachineSerial = machineSerial,
                MaxBet = maxBet,
                Denomination = denom
            };

            var info = CreateProgressiveInfoResponse(gameTitleId);

            var response = TestCalls.AsyncUnaryCall(
                Task.FromResult(info),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });

            client.Setup(x => x.RequestProgressiveInfoAsync(
                It.IsAny<ProgressiveInfoRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
                .Returns(response);

            var handledResponse = new ProgressiveInformationResponse(ResponseCode.Ok);
            _messageHandler.Setup(x => x.Handle<ProgressiveInformationResponse, ProgressiveInfoMessage>(It.IsAny<ProgressiveInfoMessage>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(handledResponse))
                .Verifiable();

            var message = new ProgressiveInfoRequestMessage(machineSerial, gameTitleId);
            var result = await _target.RequestProgressiveInfo(message, CancellationToken.None);

            _clientEnpointProvider.Verify();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ProgressiveUpdates()
        {
            var machineSerial = "123";
            var progressiveId = 10001;
            var progressiveAmount = 1000;

            var client = new Mock<ProgressiveApi.ProgressiveApiClient>(MockBehavior.Default);
            _clientEnpointProvider.Setup(x => x.IsConnected).Returns(true).Verifiable();
            _clientEnpointProvider.Setup(x => x.Client).Returns(client.Object).Verifiable();

            var request = new ProgressiveUpdate
            {
                ProgressiveMeta = Google.Protobuf.WellKnownTypes.Any.Pack(new ProgressiveLevelUpdate()
                {
                    ProgressiveLevel = progressiveId,
                    NewValue = progressiveAmount
                })
            };

            var requestStream = new Mock<IClientStreamWriter<Progressive>>(MockBehavior.Default);
            var responseStream = new Mock<IAsyncStreamReader<ProgressiveUpdate>>(MockBehavior.Default);

            // Now send an update to the stream
            var duplexStream = TestCalls.AsyncDuplexStreamingCall(
                requestStream.Object,
                responseStream.Object,
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });

            client.Setup(x => x.ProgressiveUpdates(
                null,
                null,
                It.IsAny<CancellationToken>()))
                .Returns(duplexStream);

            requestStream.Setup(x => x.WriteAsync(It.IsAny<Progressive>())).Returns(Task.FromResult(true));

            responseStream.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Returns(Task.FromResult(false));
            responseStream.SetupSequence(x => x.Current)
                .Returns(request)
                .Returns(null);

            var handledResponse = new ProgressiveUpdateResponse(ResponseCode.Ok);
            _messageHandler.Setup(x => x.Handle<ProgressiveUpdateResponse, ProgressiveUpdateMessage>(It.IsAny<ProgressiveUpdateMessage>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(handledResponse))
                .Verifiable();

            var message = new ProgressiveUpdateRequestMessage(machineSerial);
            var result = await _target.ProgressiveUpdates(message, CancellationToken.None);

            _clientEnpointProvider.Verify();

            Assert.IsTrue(result);
        }

        private ProgressiveService CreateTarget(bool nullMessage = false, bool nullEnpoint = false, bool nullAuthorization = false)
        {
            return new ProgressiveService(
                nullEnpoint ? null : _clientEnpointProvider.Object,
                nullMessage ? null : _messageHandler.Object,
                nullAuthorization ? null : _authorizationProvider.Object);
        }

        private ProgressiveInfoResponse CreateProgressiveInfoResponse(int gameTitleId)
        {
            var info = new ProgressiveInfoResponse
            {
                GameTitleId = gameTitleId,
                AuthToken = "ABC123",
            };

            info.ProgressiveLevel.Add(new ProgressiveMapping
            {
                SequenceNumber = 1,
                ProgressiveLevel = 10001
            });
            info.ProgressiveLevel.Add(new ProgressiveMapping
            {
                SequenceNumber = 2,
                ProgressiveLevel = 10002
            });
            info.ProgressiveLevel.Add(new ProgressiveMapping
            {
                SequenceNumber = 3,
                ProgressiveLevel = 10003
            });

            info.MetersToReport.Add(1);
            info.MetersToReport.Add(10);
            info.MetersToReport.Add(30);

            return info;
        }
    }
}