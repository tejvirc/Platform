namespace Aristocrat.Bingo.Client.Messages.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.ServerApiGateway;
    using Grpc.Core;
    using Grpc.Core.Testing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class RegistrationServiceTests
    {
        private readonly Mock<IClient> _client = new Mock<IClient>(MockBehavior.Default);
        private readonly Mock<IClientEndpointProvider<ClientApi.ClientApiClient>> _clientEnpointProvider =
            new Mock<IClientEndpointProvider<ClientApi.ClientApiClient>>(MockBehavior.Default);
        private readonly Mock<IAuthorizationProvider> _authorizationProvider = new Mock<IAuthorizationProvider>(MockBehavior.Default);

        private RegistrationService _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = CreateTarget();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _target.Dispose();
        }

        [DataRow(true, false, false)]
        [DataRow(false, true, false)]
        [DataRow(false, false, true)]
        [ExpectedException(typeof(ArgumentNullException))]
        [DataTestMethod]
        public void NullConstructorArgumentsTest(
            bool nullEnpoint,
            bool nullClient,
            bool nullAuthorize)
        {
            _target = CreateTarget(nullEnpoint, nullClient, nullAuthorize);
        }

        [DataRow(RegistrationResponse.Types.ResultType.Accepted, true, ResponseCode.Ok)]
        [DataRow(RegistrationResponse.Types.ResultType.Rejected, false, ResponseCode.Rejected)]
        [DataTestMethod]
        public async Task RegisterClientTest(RegistrationResponse.Types.ResultType resultType, bool authorizationSet, ResponseCode responseCode)
        {
            var client = new Mock<ClientApi.ClientApiClient>(MockBehavior.Default);
            var message = new RegistrationMessage("123", "1234", "V1");
            _clientEnpointProvider.Setup(x => x.IsConnected).Returns(true);
            _clientEnpointProvider.Setup(x => x.Client).Returns(client.Object);
            var fakeCall = TestCalls.AsyncUnaryCall(
                Task.FromResult(new RegistrationResponse { AuthToken = "Test Token", ResultType = resultType }),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });

            client.Setup(x => x.RequestRegisterAsync(
                It.IsAny<RegistrationRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
                .Returns(fakeCall);

            var result = await _target.RegisterClient(message, CancellationToken.None);
            _authorizationProvider.VerifySet(x => x.AuthorizationData = It.IsAny<Metadata>(), authorizationSet ? Times.Once() : Times.Never());
            Assert.AreEqual(responseCode, result.ResponseCode);
        }

        [TestMethod]
        public void ClientDisconnectedTest()
        {
            _client.Raise(x => x.Disconnected += null, this, new DisconnectedEventArgs());
            _authorizationProvider.VerifySet(x => x.AuthorizationData = null, Times.Once);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public async Task NoClientConnectedTest()
        {
            var client = new Mock<ClientApi.ClientApiClient>(MockBehavior.Default);
            _clientEnpointProvider.Setup(x => x.IsConnected).Returns(false);
            _clientEnpointProvider.Setup(x => x.Client).Returns(client.Object);
            var message = new RegistrationMessage("123", "1234", "V1");
            _ = await _target.RegisterClient(message, CancellationToken.None);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public async Task NullClientTest()
        {
            _clientEnpointProvider.Setup(x => x.IsConnected).Returns(true);
            _clientEnpointProvider.Setup(x => x.Client).Returns((ClientApi.ClientApiClient)null);
            var message = new RegistrationMessage("123", "1234", "V1");
            _ = await _target.RegisterClient(message, CancellationToken.None);
        }

        private RegistrationService CreateTarget(
            bool nullEnpoint = false,
            bool nullClient = false,
            bool nullAuthorize = false)
        {
            return new RegistrationService(
                nullEnpoint ? null : _clientEnpointProvider.Object,
                nullClient ? null : _client.Object,
                nullAuthorize ? null : _authorizationProvider.Object);
        }
    }
}