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
    public class ProgressiveUpdateServiceTests
    {
        private readonly Mock<IClientEndpointProvider<ProgressiveApi.ProgressiveApiClient>> _clientEnpointProvider =
            new Mock<IClientEndpointProvider<ProgressiveApi.ProgressiveApiClient>>(MockBehavior.Default);
        private readonly Mock<IMessageHandlerFactory> _messageHandler = new Mock<IMessageHandlerFactory>(MockBehavior.Default);
        private readonly Mock<IProgressiveAuthorizationProvider> _authorizationProvider =
            new Mock<IProgressiveAuthorizationProvider>(MockBehavior.Default);

        private ProgressiveUpdateService _target;

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

        private ProgressiveUpdateService CreateTarget(bool nullMessage = false, bool nullEnpoint = false, bool nullAuthorization = false)
        {
            return new ProgressiveUpdateService(
                nullEnpoint ? null : _clientEnpointProvider.Object,
                nullMessage ? null : _messageHandler.Object,
                nullAuthorization ? null : _authorizationProvider.Object);
        }
    }
}