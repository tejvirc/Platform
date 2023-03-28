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
    public class StatusReportingServiceTest
    {
        private readonly Mock<IClientEndpointProvider<ClientApi.ClientApiClient>> _endpointProvider = new(MockBehavior.Default);
        private StatusReportingService _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new StatusReportingService(_endpointProvider.Object);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullConstructorArgumentTest()
        {
            _target = new StatusReportingService(null);
        }

        [TestMethod]
        public async Task ReportStatusTest()
        {
            const long cashInMeter = 10000;
            const long cashoutMeter = 20000;
            const long cashPlayedMeter = 30000;
            const long cashWonMeter = 40000;
            const int egmStatus = 0;

            var message = new StatusMessage(
                "TestMachineId",
                cashPlayedMeter,
                cashWonMeter,
                cashInMeter,
                cashoutMeter,
                egmStatus);

            var client = new Mock<ClientApi.ClientApiClient>(MockBehavior.Default);
            _endpointProvider.Setup(x => x.IsConnected).Returns(true);
            _endpointProvider.Setup(x => x.Client).Returns(client.Object);
            var fakeCall = TestCalls.AsyncUnaryCall(
                Task.FromResult(new EmptyResponse()),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });
            client.Setup(x => x.ReportStatusAsync(
                    It.IsAny<StatusReport>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(fakeCall)
                .Verifiable();

            await _target.ReportStatus(message, CancellationToken.None);
            client.Verify();
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public async Task NoClientConnectedTest()
        {
            var client = new Mock<ClientApi.ClientApiClient>(MockBehavior.Default);
            _endpointProvider.Setup(x => x.IsConnected).Returns(false);
            _endpointProvider.Setup(x => x.Client).Returns(client.Object);
            await _target.ReportStatus(new StatusMessage("TestMachineId", 0, 0, 0, 0, 0), CancellationToken.None);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public async Task NullClientTest()
        {
            _endpointProvider.Setup(x => x.IsConnected).Returns(true);
            _endpointProvider.Setup(x => x.Client).Returns((ClientApi.ClientApiClient)null);
            await _target.ReportStatus(new StatusMessage("TestMachineId", 0, 0, 0, 0, 0), CancellationToken.None);
        }
    }
}