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
    using Gaming.Contracts.Progressives;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetProgressiveLogStatusTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetProgressiveLogStatus(null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructNullProgressiveLogExpectException()
        {
            var egm = new Mock<IG2SEgm>();

            var handler = new GetProgressiveLogStatus(egm.Object, null);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var log = new Mock<ITransactionHistory>();

            var handler = new GetProgressiveLogStatus(egm.Object, log.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var log = new Mock<ITransactionHistory>();
            var handler = new GetProgressiveLogStatus(egm.Object, log.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var log = new Mock<ITransactionHistory>();
            var handler = new GetProgressiveLogStatus(
                HandlerUtilities.CreateMockEgm<IProgressiveDevice>(),
                log.Object);

            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            const int invalidHostId = 99;

            var device = new Mock<IProgressiveDevice>();
            device.SetupGet(d => d.Owner).Returns(invalidHostId);

            var log = new Mock<ITransactionHistory>();
            var handler = new GetProgressiveLogStatus(
                HandlerUtilities.CreateMockEgm(device),
                log.Object);

            await VerificationTests.VerifyChecksForOwnerAndGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithGuestAccessExpectNoError()
        {
            const int otherHostId = 99;

            var device = new Mock<IProgressiveDevice>();

            device.SetupGet(d => d.Owner).Returns(otherHostId);
            device.SetupGet(d => d.Guests).Returns(new List<int> { TestConstants.HostId });

            var log = new Mock<ITransactionHistory>();
            var handler = new GetProgressiveLogStatus(
                HandlerUtilities.CreateMockEgm(device),
                log.Object);

            await VerificationTests.VerifyAllowsGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var device = new Mock<IProgressiveDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var log = new Mock<ITransactionHistory>();
            var handler = new GetProgressiveLogStatus(
                HandlerUtilities.CreateMockEgm(device),
                log.Object);

            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenHandleExpectResponse()
        {
            var egm = new Mock<IG2SEgm>();
            var device = new Mock<IProgressiveDevice>();

            egm.Setup(e => e.GetDevice<IProgressiveDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);

            var command = ClassCommandUtilities.CreateClassCommand<progressive, getProgressiveLogStatus>(
                TestConstants.HostId,
                TestConstants.EgmId);

            var log = new Mock<ITransactionHistory>();
            log.Setup(m => m.RecallTransactions<JackpotTransaction>()).Returns(new List<JackpotTransaction>());

            var handler = new GetProgressiveLogStatus(egm.Object, log.Object);
            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<progressive, progressiveLogStatus>;

            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.Command.totalEntries);
            Assert.AreEqual(0, response.Command.lastSequence);
        }
    }
}