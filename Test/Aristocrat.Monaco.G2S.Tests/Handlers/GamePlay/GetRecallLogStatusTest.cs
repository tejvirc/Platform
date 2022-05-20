namespace Aristocrat.Monaco.G2S.Tests.Handlers.GamePlay
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers.GamePlay;
    using Gaming.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetRecallLogStatusTest
    {
        private readonly Mock<IG2SEgm> _egm = new Mock<IG2SEgm>();
        private readonly Mock<IGameHistory> _history = new Mock<IGameHistory>();

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetRecallLogStatus(null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullGameHistoryExpectException()
        {
            var handler = new GetRecallLogStatus(_egm.Object, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParameterExpectSuccess()
        {
            var handler = new GetRecallLogStatus(_egm.Object, _history.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var handler = new GetRecallLogStatus(_egm.Object, _history.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            const int invalidHostId = 99;

            var device = new Mock<IGamePlayDevice>();
            device.SetupGet(d => d.Owner).Returns(invalidHostId);

            var handler = new GetRecallLogStatus(HandlerUtilities.CreateMockEgm(device), _history.Object);

            await VerificationTests.VerifyChecksForOwnerAndGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithGuestAccessExpectNoError()
        {
            const int otherHostId = 99;

            var device = new Mock<IGamePlayDevice>();

            device.SetupGet(d => d.Owner).Returns(otherHostId);
            device.SetupGet(d => d.Guests).Returns(new List<int> { TestConstants.HostId });

            var handler = new GetRecallLogStatus(HandlerUtilities.CreateMockEgm(device), _history.Object);

            await VerificationTests.VerifyAllowsGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var device = new Mock<IGamePlayDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var handler = new GetRecallLogStatus(HandlerUtilities.CreateMockEgm(device), _history.Object);

            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenHandleExpectResponse()
        {
            const long sequence = 7;
            const int total = 5;

            _history.SetupGet(m => m.LogSequence).Returns(sequence);
            _history.SetupGet(m => m.TotalEntries).Returns(total);

            var handler = new GetRecallLogStatus(_egm.Object, _history.Object);
            var command = ClassCommandUtilities.CreateClassCommand<gamePlay, getRecallLogStatus>(
                TestConstants.HostId,
                TestConstants.EgmId);

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<gamePlay, recallLogStatus>;
            Assert.IsNotNull(response);
            Assert.AreEqual(sequence, response.Command.lastSequence);
            Assert.AreEqual(total, response.Command.totalEntries);
        }
    }
}