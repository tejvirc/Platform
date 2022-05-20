namespace Aristocrat.Bingo.Client.Tests.Messages
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Client.Messages;
    using Grpc.Core;
    using Grpc.Core.Testing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using ServerApiGateway;

    [TestClass]
    public class ConfigurationServiceTest
    {
        private readonly Mock<IClientEndpointProvider<ClientApi.ClientApiClient>> _endpointProvider =
            new Mock<IClientEndpointProvider<ClientApi.ClientApiClient>>(MockBehavior.Default);

        private readonly ConfigurationMessage _message = new ConfigurationMessage("123", "Game1,Game2,Game3");
        private ConfigurationService _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new ConfigurationService(_endpointProvider.Object);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullConstructorArgumentTest()
        {
            _target = new ConfigurationService(null);
        }

        [TestMethod]
        public async Task ReportEventTest()
        {
            var client = new Mock<ClientApi.ClientApiClient>(MockBehavior.Default);
            _endpointProvider.Setup(x => x.IsConnected).Returns(true);
            _endpointProvider.Setup(x => x.Client).Returns(client.Object);
            var fakeCall = TestCalls.AsyncUnaryCall(
                Task.FromResult(new ConfigurationResponse()),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });
            client.Setup(x => x.RequestConfigurationAsync(
                    It.IsAny<ConfigurationRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(fakeCall);

            var result = await _target.ConfigureClient(_message, CancellationToken.None);
            Assert.IsNotNull(result);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public async Task NoClientConnectedTest()
        {
            var client = new Mock<ClientApi.ClientApiClient>(MockBehavior.Default);
            _endpointProvider.Setup(x => x.IsConnected).Returns(false);
            _endpointProvider.Setup(x => x.Client).Returns(client.Object);
            _ = await _target.ConfigureClient(_message, CancellationToken.None);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public async Task NullClientTest()
        {
            _endpointProvider.Setup(x => x.IsConnected).Returns(true);
            _endpointProvider.Setup(x => x.Client).Returns((ClientApi.ClientApiClient)null);
            _ = await _target.ConfigureClient(_message, CancellationToken.None);
        }
    }
}