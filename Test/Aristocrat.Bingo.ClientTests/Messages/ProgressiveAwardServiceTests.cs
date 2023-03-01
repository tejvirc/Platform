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
    public class ProgressiveAwardServiceTests
    {
        private readonly Mock<IClientEndpointProvider<ProgressiveApi.ProgressiveApiClient>> _clientEnpointProvider =
            new Mock<IClientEndpointProvider<ProgressiveApi.ProgressiveApiClient>>(MockBehavior.Default);
        private readonly Mock<IProgressiveLevelInfoProvider> _progressiveLevelInfoProvider = new Mock<IProgressiveLevelInfoProvider>(MockBehavior.Default);
        private ProgressiveAwardService _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = CreateTarget();
        }

        [DataRow(true, false, DisplayName = "Null IProgressiveLevelInfoProvider")]
        [DataRow(false, true, DisplayName = "Null IClientEndpointProvider")]
        [ExpectedException(typeof(ArgumentNullException))]
        [DataTestMethod]
        public void NullConstructorArgumentsTest(bool nullLevelInfoProvider, bool nullEnpoint)
        {
            _target = CreateTarget(nullEnpoint, nullLevelInfoProvider);
        }

        [TestMethod]
        public async Task ProgressiveClaimTest()
        {
            var machineSerial = "123";
            var levelId = 1;
            var winAmount = 101L;
            var awardId = 51;
            var pending = false;

            var client = new Mock<ProgressiveApi.ProgressiveApiClient>(MockBehavior.Default);
            _clientEnpointProvider.Setup(x => x.IsConnected).Returns(true).Verifiable();
            _clientEnpointProvider.Setup(x => x.Client).Returns(client.Object).Verifiable();

            _progressiveLevelInfoProvider.Setup(x => x.GetServerProgressiveLevelId(It.IsAny<int>())).Returns(10001L);

            var awardPaidAck = CreateProgressiveAwardPaidAck(true);

            var response = TestCalls.AsyncUnaryCall(
                Task.FromResult(awardPaidAck),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });

            client.Setup(x => x.AcknowledgeProgressiveWinAsync(
                It.IsAny<ProgressiveAwardPaid>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
                .Returns(response);

            var message = new ProgressiveAwardRequestMessage(machineSerial, awardId, levelId, winAmount, pending);
            var result = await _target.AwardProgressive(message, CancellationToken.None);

            _clientEnpointProvider.Verify();

            Assert.AreEqual(result.ResponseCode, ResponseCode.Ok);
        }

        [TestMethod]
        public async Task ProgressiveClaimFailedTest()
        {
            var machineSerial = "123";
            var levelId = 1;
            var winAmount = 101L;
            var awardId = 51;
            var pending = false;

            var client = new Mock<ProgressiveApi.ProgressiveApiClient>(MockBehavior.Default);
            _clientEnpointProvider.Setup(x => x.IsConnected).Returns(true).Verifiable();
            _clientEnpointProvider.Setup(x => x.Client).Returns(client.Object).Verifiable();

            _progressiveLevelInfoProvider.Setup(x => x.GetServerProgressiveLevelId(It.IsAny<int>())).Returns(10001L);

            var awardPaidAck = CreateProgressiveAwardPaidAck(false);

            var response = TestCalls.AsyncUnaryCall(
                Task.FromResult(awardPaidAck),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });

            client.Setup(x => x.AcknowledgeProgressiveWinAsync(
                It.IsAny<ProgressiveAwardPaid>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
                .Returns(response);

            var message = new ProgressiveAwardRequestMessage(machineSerial, awardId, levelId, winAmount, pending);
            var result = await _target.AwardProgressive(message, CancellationToken.None);

            _clientEnpointProvider.Verify();

            Assert.AreEqual(result.ResponseCode, ResponseCode.Rejected);
        }

        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public async Task ProgressiveClaimInvalidLevelIdTest()
        {
            var machineSerial = "123";
            var levelId = 1;
            var winAmount = 101L;
            var awardId = 51;
            var pending = false;

            _progressiveLevelInfoProvider.Setup(x => x.GetServerProgressiveLevelId(It.IsAny<int>())).Returns(-1L);

            var message = new ProgressiveAwardRequestMessage(machineSerial, awardId, levelId, winAmount, pending);
            var result = await _target.AwardProgressive(message, CancellationToken.None);
        }

        private ProgressiveAwardService CreateTarget(bool nullLevelInfoProvider = false, bool nullEnpoint = false)
        {
            return new ProgressiveAwardService(
                nullEnpoint ? null : _clientEnpointProvider.Object,
                nullLevelInfoProvider ? null : _progressiveLevelInfoProvider.Object);
        }

        private ProgressiveAwardPaidAck CreateProgressiveAwardPaidAck(bool success)
        {
            var award = new ProgressiveAwardPaidAck
            {
                Success = success,
            };

            return award;
        }
    }
}