namespace Aristocrat.Monaco.G2S.Tests.Handlers.Progressive
{
    using System;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers;
    using G2S.Handlers.Progressive;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class SetProgressiveAwardTests
    {
        private Mock<IG2SEgm> _egm;

        [TestInitialize]
        public void Initialize()
        {
            _egm = new Mock<IG2SEgm>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new SetProgressiveAward(null);
            Assert.IsNull(handler);
        }

        // TODO: Uncomment this test when g2s is fixed
        //[TestMethod]
        //public void WhenConstructWithEgmExpectException()
        //{
        //    var jackpotProvider = new Mock<G2SJackpotProvider>();

        //    var handler = new SetProgressiveAward(
        //        _egm.Object,
        //        jackpotProvider.Object);

        //    Assert.IsNotNull(handler);
        //}

        [TestMethod]
        public async Task WhenVerifyOwnerExpectSuccess()
        {
            var egm = HandlerUtilities.CreateMockEgm<IProgressiveDevice>();
            var handler = CreateHandler(egm);

            await VerificationTests.VerifyChecksForOwner(handler);
        }

        // TODO: Uncomment this test when g2s is fixed
        //[TestMethod]
        //public async Task WhenHandleCommandWithSetProgressiveAwardExpectSuccess()
        //{
        //    var deviceMock = new Mock<IProgressiveDevice>();
        //    deviceMock.SetupGet(m => m.DeviceClass).Returns(DeviceClass.G2S_progressive);
        //    deviceMock.SetupGet(m => m.RequiredForPlay).Returns(true);
        //    var egm = HandlerUtilities.CreateMockEgm(deviceMock);
        //    var handler = CreateHandler(egm);

        //    var command = CreateCommand();
        //    command.Command.levelId = 1;
        //    command.Command.progId = 1;
        //    command.Command.progressiveAwardId = 1;
        //    command.Command.progValueAmt = 2;
        //    await handler.Handle(command);

        //    var response = command.Responses.FirstOrDefault() as ClassCommand<progressive, progressiveAwardAck>;

        //    Assert.IsNotNull(response);
        //}

        private SetProgressiveAward CreateHandler(IG2SEgm egm = null)
        {
            var log = new Mock<ITransactionHistory>();
            var gameProvider = new Mock<IGameProvider>();
            var properties = new Mock<IPropertiesManager>();
            var progressiveHitCommandBuilder = new Mock<ICommandBuilder<IProgressiveDevice, progressiveHit>>();
            var progressiveCommitCommandBuilder = new Mock<ICommandBuilder<IProgressiveDevice, progressiveCommit>>();

            properties.Setup(p => p.GetProperty(GamingConstants.SelectedGameId, It.IsAny<object>()))
                .Returns(2);
            properties.Setup(p => p.GetProperty(GamingConstants.SelectedDenom, It.IsAny<object>()))
                .Returns(0);
            var j = new JackpotTransaction(
                        1, //deviceId
                        DateTime.Now, //DateTime
                        1, //progressiveId
                        1, //levelId
                        2, //gameId
                        0, //denomId
                        0, //winLevelIndex
                        20, //amount
                        "G2s_abc", //valueText
                        10, //valueSequence
                        10, //resetValue
                        (int)ProgressiveLevelType.Sap,
                        string.Empty,
                        PayMethod.Any,
                        25000,
                        100000)
            {
                TransactionId = 1,
                LogSequence = 0,
                PaidDateTime = DateTime.Now,
                State = (ProgressiveState)t_progStates.G2S_progCommit,
                WinText = "G2s_abc",
                PaidAmount = 5000
            };

            //transaction.Setup(js => js.RecallTransactions<JackpotTransaction>())
            //    .Returns(new List<JackpotTransaction> { j });

            var handler = new SetProgressiveAward(egm ?? _egm.Object);

            return handler;
        }

        private ClassCommand<progressive, setProgressiveAward> CreateCommand()
        {
            var command = ClassCommandUtilities.CreateClassCommand<progressive, setProgressiveAward>(
                TestConstants.HostId,
                TestConstants.EgmId);

            return command;
        }
    }
}