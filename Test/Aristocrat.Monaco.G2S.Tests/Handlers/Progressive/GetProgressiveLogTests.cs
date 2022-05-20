namespace Aristocrat.Monaco.G2S.Tests.Handlers.Progressive
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers.Progressive;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetProgressiveLogTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetProgressiveLog(null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructNullProgressiveLogExpectException()
        {
            var egm = new Mock<IG2SEgm>();

            var handler = new GetProgressiveLog(egm.Object, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithGameProviderNullParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var log = new Mock<ITransactionHistory>();
            var handler = new GetProgressiveLog(egm.Object, log.Object, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var log = new Mock<ITransactionHistory>();
            var gameProvider = new Mock<IGameProvider>();
            var handler = new GetProgressiveLog(egm.Object, log.Object, gameProvider.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();

            var log = new Mock<ITransactionHistory>();
            var gameProvider = new Mock<IGameProvider>();

            var handler = new GetProgressiveLog(egm.Object, log.Object, gameProvider.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var log = new Mock<ITransactionHistory>();
            var gameProvider = new Mock<IGameProvider>();
            var handler = new GetProgressiveLog(
                HandlerUtilities.CreateMockEgm<IProgressiveDevice>(),
                log.Object,
                gameProvider.Object);

            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            const int invalidHostId = 99;

            var device = new Mock<IProgressiveDevice>();
            var gameProvider = new Mock<IGameProvider>();
            device.SetupGet(d => d.Owner).Returns(invalidHostId);

            var log = new Mock<ITransactionHistory>();
            var handler = new GetProgressiveLog(
                HandlerUtilities.CreateMockEgm(device),
                log.Object,
                gameProvider.Object);

            await VerificationTests.VerifyChecksForOwnerAndGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithGuestAccessExpectNoError()
        {
            const int otherHostId = 99;

            var device = new Mock<IProgressiveDevice>();
            var gameProvider = new Mock<IGameProvider>();
            device.SetupGet(d => d.Owner).Returns(otherHostId);
            device.SetupGet(d => d.Guests).Returns(new List<int> { TestConstants.HostId });

            var log = new Mock<ITransactionHistory>();
            var handler = new GetProgressiveLog(
                HandlerUtilities.CreateMockEgm(device),
                log.Object,
                gameProvider.Object);

            await VerificationTests.VerifyAllowsGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var device = new Mock<IProgressiveDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);
            var gameProvider = new Mock<IGameProvider>();
            var log = new Mock<ITransactionHistory>();
            var handler = new GetProgressiveLog(
                HandlerUtilities.CreateMockEgm(device),
                log.Object,
                gameProvider.Object);

            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        [Ignore]
        public async Task WhenHandleExpectResponse()
        {
            var egm = new Mock<IG2SEgm>();
            var device = new Mock<IProgressiveDevice>();
            var log = new Mock<ITransactionHistory>();

            var gameProvider = new Mock<IGameProvider>();

            device.SetupGet(comms => comms.DeviceClass).Returns(typeof(gat).Name);
            egm.Setup(e => e.GetDevice<IProgressiveDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);
            egm.Setup(e => e.Devices).Returns(
                new List<IDevice>
                {
                    device.Object
                });

            var command = ClassCommandUtilities.CreateClassCommand<progressive, getProgressiveLog>(
                TestConstants.HostId,
                TestConstants.EgmId);
            command.Command.lastSequence = 0;
            command.Command.totalEntries = 1;
            var j = new JackpotTransaction(
                        1, //deviceId
                        DateTime.Now, //DateTime
                        1, //progressiveId
                        1, //levelId
                        2, //gameId
                        0L, //denomId
                        0, //winLevelIndex
                        20, //amount
                        "G2s_abc", // valueText
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

            log.Setup(js => js.RecallTransactions<JackpotTransaction>())
                .Returns(new List<JackpotTransaction> { j });

            var handler = new GetProgressiveLog(egm.Object, log.Object, gameProvider.Object);
            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<progressive, progressiveLogList>;
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Command.progressiveLog);
        }
    }
}