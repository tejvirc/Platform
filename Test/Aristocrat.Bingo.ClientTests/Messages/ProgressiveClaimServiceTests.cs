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
    public class ProgressiveClaimServiceTests
    {
        private readonly Mock<IClientEndpointProvider<ProgressiveApi.ProgressiveApiClient>> _clientEnpointProvider =
            new Mock<IClientEndpointProvider<ProgressiveApi.ProgressiveApiClient>>(MockBehavior.Default);
        private readonly Mock<IProgressiveLevelInfoProvider> _progressiveLevelInfoProvider = new Mock<IProgressiveLevelInfoProvider>(MockBehavior.Default);
        private ProgressiveClaimService _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = CreateTarget();
        }

        [DataRow(true, false )]
        [DataRow(false, true)]
        [ExpectedException(typeof(ArgumentNullException))]
        [DataTestMethod]
        public void NullConstructorArgumentsTest(bool nullLevelInfoProvider, bool nullEnpoint)
        {
            _target = CreateTarget(nullEnpoint, nullLevelInfoProvider);
        }

        [TestMethod]
        public async Task ProgressiveClaimTest()
        {
            int gameTitleId = 5;
            var machineSerial = "123";
            var progressiveLevelId = 1L;
            var winAmount = 101L;
            var awardId = 51;

            var client = new Mock<ProgressiveApi.ProgressiveApiClient>(MockBehavior.Default);
            _clientEnpointProvider.Setup(x => x.IsConnected).Returns(true).Verifiable();
            _clientEnpointProvider.Setup(x => x.Client).Returns(client.Object).Verifiable();

            var mappedProgressiveId = 10001L;
            _progressiveLevelInfoProvider.Setup(x => x.GetServerProgressiveLevelId(It.IsAny<int>())).Returns(mappedProgressiveId);
            var request = new ClaimProgressiveWinRequest
            {
                MachineSerial = machineSerial,
                ProgressiveLevelId = progressiveLevelId,
                ProgressiveWinAmount = winAmount
            };

            var claimResponse = CreateProgressiveClaimResponse(progressiveLevelId, winAmount, awardId);

            var response = TestCalls.AsyncUnaryCall(
                Task.FromResult(claimResponse),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });

            client.Setup(x => x.ClaimProgressiveWinAsync(
                It.IsAny<ClaimProgressiveWinRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
                .Returns(response);

            var handledResponse = new ProgressiveInformationResponse(ResponseCode.Ok);

            var message = new ProgressiveRegistrationMessage(machineSerial, gameTitleId);
            var result = await _target.ClaimProgressive(new ProgressiveClaimRequestMessage(machineSerial, progressiveLevelId, winAmount), CancellationToken.None);

            _clientEnpointProvider.Verify();

            Assert.IsTrue(result.ResponseCode == ResponseCode.Ok);
            Assert.AreEqual(result.ProgressiveLevelId, progressiveLevelId);
            Assert.AreEqual(result.ProgressiveWinAmount, winAmount);
            Assert.AreEqual(result.ProgressiveAwardId, awardId);
        }

        private ProgressiveClaimService CreateTarget(bool nullLevelInfoProvider = false, bool nullEnpoint = false)
        {
            return new ProgressiveClaimService(
                nullEnpoint ? null : _clientEnpointProvider.Object,
                nullLevelInfoProvider ? null : _progressiveLevelInfoProvider.Object);
        }

        private ProgressiveWinAck CreateProgressiveClaimResponse(long levelId, long winAmount, int awardId)
        {
            var claim = new ProgressiveWinAck
            {
                ProgressiveLevelId = levelId,
                ProgressiveWinAmount = winAmount,
                ProgressiveAwardId = awardId
            };

            return claim;
        }
    }
}