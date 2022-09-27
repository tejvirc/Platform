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
    public class ReportTransactionServiceTest
    {
        private const int TransactionId = 123;
        private const string TestBarcode = "TestBarcode";
        private readonly Mock<IClientEndpointProvider<ClientApi.ClientApiClient>> _endpointProvider =
            new Mock<IClientEndpointProvider<ClientApi.ClientApiClient>>(MockBehavior.Default);

        private readonly ReportTransactionMessage _message = new ReportTransactionMessage(
            "123",
            DateTime.UtcNow,
            100,
            123,
            1234,
            TransactionId,
            12345,
            1000,
            1,
            TestBarcode);
        private ReportTransactionService _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new ReportTransactionService(_endpointProvider.Object);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullConstructorArgumentTest()
        {
            _target = new ReportTransactionService(null);
        }

        [TestMethod]
        public async Task ReportEventTest()
        {
            var client = new Mock<ClientApi.ClientApiClient>(MockBehavior.Default);
            _endpointProvider.Setup(x => x.IsConnected).Returns(true);
            _endpointProvider.Setup(x => x.Client).Returns(client.Object);
            var fakeCall = TestCalls.AsyncUnaryCall(
                Task.FromResult(new ReportTransactionAck { TransactionId = TransactionId, Succeeded = true }),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });
            client.Setup(x => x.ReportTransactionAsync(
                    It.IsAny<TransactionReport>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(fakeCall);

            var result = await _target.ReportTransaction(_message, CancellationToken.None);
            Assert.AreEqual(TransactionId, result.TransactionId);
            Assert.IsTrue(result.Succeeded);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public async Task NoClientConnectedTest()
        {
            var client = new Mock<ClientApi.ClientApiClient>(MockBehavior.Default);
            _endpointProvider.Setup(x => x.IsConnected).Returns(false);
            _endpointProvider.Setup(x => x.Client).Returns(client.Object);
            _ = await _target.ReportTransaction(_message, CancellationToken.None);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public async Task NullClientTest()
        {
            _endpointProvider.Setup(x => x.IsConnected).Returns(true);
            _endpointProvider.Setup(x => x.Client).Returns((ClientApi.ClientApiClient)null);
            _ = await _target.ReportTransaction(_message, CancellationToken.None);
        }
    }
}