namespace Aristocrat.Bingo.Client.Tests.Messages
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Client.Messages;
    using Google.Protobuf.WellKnownTypes;
    using Grpc.Core;
    using Grpc.Core.Testing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using ServerApiGateway;

    [TestClass]
    public class ActivityReportServiceTest
    {
        private readonly Mock<IClientEndpointProvider<ClientApi.ClientApiClient>> _endpointProvider = new(MockBehavior.Default);
        private ActivityReportService _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new ActivityReportService(_endpointProvider.Object);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullConstructorArgumentTest()
        {
            _target = new ActivityReportService(null);
        }

        [TestMethod]
        public async Task ReportStatusTest()
        {
            var expectedResponse = DateTime.UtcNow;
            var message = new ActivityReportMessage(expectedResponse, "TestMachineId");
            var client = new Mock<ClientApi.ClientApiClient>(MockBehavior.Default);
            _endpointProvider.Setup(x => x.IsConnected).Returns(true);
            _endpointProvider.Setup(x => x.Client).Returns(client.Object);
            var fakeCall = TestCalls.AsyncUnaryCall(
                Task.FromResult(new ActivityResponse { ActivityResponseTime = expectedResponse.ToTimestamp() }),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });
            client.Setup(x => x.ReportActivityAsync(
                    It.IsAny<ActivityRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(fakeCall)
                .Verifiable();

            var result = await _target.ReportActivity(message, CancellationToken.None);
            client.Verify();

            Assert.AreEqual(ResponseCode.Ok, result.ResponseCode);
            Assert.AreEqual(expectedResponse, result.ResponseTime);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public async Task NoClientConnectedTest()
        {
            var client = new Mock<ClientApi.ClientApiClient>(MockBehavior.Default);
            _endpointProvider.Setup(x => x.IsConnected).Returns(false);
            _endpointProvider.Setup(x => x.Client).Returns(client.Object);
            await _target.ReportActivity(new ActivityReportMessage(DateTime.UtcNow, "TestMachineId"), CancellationToken.None);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public async Task NullClientTest()
        {
            _endpointProvider.Setup(x => x.IsConnected).Returns(true);
            _endpointProvider.Setup(x => x.Client).Returns((ClientApi.ClientApiClient)null);
            await _target.ReportActivity(new ActivityReportMessage(DateTime.UtcNow, "TestMachineId"), CancellationToken.None);
        }
    }
}