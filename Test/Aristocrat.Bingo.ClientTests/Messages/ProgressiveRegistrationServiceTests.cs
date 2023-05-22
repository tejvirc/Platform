namespace Aristocrat.Bingo.Client.Messages.Tests
{
    using System;
    using System.Collections.Generic;
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
    public class ProgressiveRegistrationServiceTests
    {
        private readonly Mock<IClientEndpointProvider<ProgressiveApi.ProgressiveApiClient>> _clientEnpointProvider =
            new Mock<IClientEndpointProvider<ProgressiveApi.ProgressiveApiClient>>(MockBehavior.Default);
        private readonly Mock<IProgressiveMessageHandlerFactory> _messageHandler = new Mock<IProgressiveMessageHandlerFactory>(MockBehavior.Default);
        private readonly Mock<IProgressiveAuthorizationProvider> _authorizationProvider =
            new Mock<IProgressiveAuthorizationProvider>(MockBehavior.Default);
        private readonly Mock<IProgressiveLevelInfoProvider> _progressiveLevelInfoProvider =
            new Mock<IProgressiveLevelInfoProvider>(MockBehavior.Default);
        private IEnumerable<IClient> _clients = new List<IClient>();
        private ProgressiveRegistrationService _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = CreateTarget();
        }

        [DataRow(true, false, false, false, false, DisplayName = "Null IMessageHandlerFactory")]
        [DataRow(false, true, false, false, false, DisplayName = "Null IClientEndpointProvider")]
        [DataRow(false, false, true, false, false, DisplayName = "Null IProgressiveAuthorizationProvider")]
        [DataRow(false, false, false, true, false, DisplayName = "Null IClient")]
        [DataRow(false, false, false, false, true, DisplayName = "Null IProgressiveLevelInfoProvider")]
        [ExpectedException(typeof(ArgumentNullException))]
        [DataTestMethod]
        public void NullConstructorArgumentsTest(bool nullMessage, bool nullEnpoint, bool nullAuthorization, bool nullClients, bool nullLevelInfoProvider)
        {
            _target = CreateTarget(nullEnpoint, nullMessage, nullAuthorization, nullClients, nullLevelInfoProvider);
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

            _progressiveLevelInfoProvider.Setup(x => x.ClearProgressiveLevelInfo());
            _progressiveLevelInfoProvider.Setup(x => x.AddProgressiveLevelInfo(It.IsAny<long>(), It.IsAny<int>()));

            var message = new ProgressiveRegistrationMessage(machineSerial, gameTitleId);
            var result = await _target.RegisterClient(message, CancellationToken.None);

            _clientEnpointProvider.Verify();

            Assert.IsTrue(result.ResponseCode == ResponseCode.Ok);
        }

        private ProgressiveRegistrationService CreateTarget(bool nullMessage = false, bool nullEnpoint = false, bool nullAuthorization = false, bool nullClients = false, bool nullLevelInfoProvider = false)
        {
            return new ProgressiveRegistrationService(
                nullEnpoint ? null : _clientEnpointProvider.Object,
                nullClients ? null : _clients,
                nullMessage ? null : _messageHandler.Object,
                nullAuthorization ? null : _authorizationProvider.Object,
                nullLevelInfoProvider ? null : _progressiveLevelInfoProvider.Object);
        }

        private ProgressiveInfoResponse CreateProgressiveInfoResponse(int gameTitleId)
        {
            var info = new ProgressiveInfoResponse
            {
                GameTitleId = gameTitleId,
                AuthToken = "ABC123",
            };

            info.ProgressiveLevels.Add(new ProgressiveMapping
            {
                SequenceNumber = 1,
                ProgressiveLevelId = 10001
            });
            info.ProgressiveLevels.Add(new ProgressiveMapping
            {
                SequenceNumber = 2,
                ProgressiveLevelId = 10002
            });
            info.ProgressiveLevels.Add(new ProgressiveMapping
            {
                SequenceNumber = 3,
                ProgressiveLevelId = 10003
            });

            info.MetersToReport.Add(1);
            info.MetersToReport.Add(10);
            info.MetersToReport.Add(30);

            return info;
        }
    }
}