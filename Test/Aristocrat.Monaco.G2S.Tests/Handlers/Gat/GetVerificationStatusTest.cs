namespace Aristocrat.Monaco.G2S.Tests.Handlers.Gat
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Common.GAT.CommandHandlers;
    using Common.GAT.Models;
    using Common.GAT.Storage;
    using G2S.Handlers.Gat;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetVerificationStatusTest
    {
        private const long TransactionId = 999;
        private const int VerificationId = 7;

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetVerificationStatus(null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructNullGatServiceExpectException()
        {
            var egm = new Mock<IG2SEgm>();

            var handler = new GetVerificationStatus(egm.Object, null);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var gatService = new Mock<IGatService>();
            var handler = new GetVerificationStatus(egm.Object, gatService.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var gatService = new Mock<IGatService>();
            var handler = new GetVerificationStatus(egm.Object, gatService.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var gatService = new Mock<IGatService>();
            var handler = new GetVerificationStatus(HandlerUtilities.CreateMockEgm<IGatDevice>(), gatService.Object);

            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            const int invalidHostId = 99;

            var device = new Mock<IGatDevice>();
            device.SetupGet(d => d.Owner).Returns(invalidHostId);

            var gatService = new Mock<IGatService>();
            var handler = new GetVerificationStatus(HandlerUtilities.CreateMockEgm(device), gatService.Object);

            await VerificationTests.VerifyChecksForOwnerAndGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithGuestAccessExpectNoError()
        {
            const int otherHostId = 99;

            var device = new Mock<IGatDevice>();

            device.SetupGet(d => d.Owner).Returns(otherHostId);
            device.SetupGet(d => d.Guests).Returns(new List<int> { TestConstants.HostId });

            var gatService = new Mock<IGatService>();
            gatService.Setup(g => g.HasVerificationId(VerificationId)).Returns(true);
            gatService.Setup(g => g.HasTransactionId(TransactionId)).Returns(true);
            var handler = new GetVerificationStatus(HandlerUtilities.CreateMockEgm(device), gatService.Object);

            var command = CreateCommand(TransactionId, VerificationId);

            await VerificationTests.VerifyAllowsGuests(handler, command);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var device = new Mock<IGatDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var gatService = new Mock<IGatService>();
            gatService.Setup(g => g.HasVerificationId(VerificationId)).Returns(true);
            gatService.Setup(g => g.HasTransactionId(TransactionId)).Returns(true);
            var handler = new GetVerificationStatus(HandlerUtilities.CreateMockEgm(device), gatService.Object);

            var command = CreateCommand(TransactionId, VerificationId);

            await VerificationTests.VerifyCanSucceed(handler, command);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoVerificationdExpectError()
        {
            var gatService = new Mock<IGatService>();
            gatService.Setup(g => g.HasVerificationId(VerificationId)).Returns(false);

            var device = new Mock<IGatDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var handler = new GetVerificationStatus(HandlerUtilities.CreateMockEgm(device), gatService.Object);
            var command = CreateCommand(TransactionId, VerificationId);

            var result = await handler.Verify(command);

            Assert.AreEqual(result.Code, ErrorCode.G2S_GAX005);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoTransactionExpectError()
        {
            var gatService = new Mock<IGatService>();
            gatService.Setup(g => g.HasVerificationId(VerificationId)).Returns(true);
            gatService.Setup(g => g.HasTransactionId(VerificationId)).Returns(false);

            var device = new Mock<IGatDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var handler = new GetVerificationStatus(HandlerUtilities.CreateMockEgm(device), gatService.Object);
            var command = CreateCommand(TransactionId, VerificationId);

            var result = await handler.Verify(command);

            Assert.AreEqual(result.Code, ErrorCode.G2S_GAX012);
        }

        [TestMethod]
        public async Task WhenComponentVerificationResultsExistHandleExpectResponse()
        {
            var egm = new Mock<IG2SEgm>();
            var device = new Mock<IGatDevice>();

            egm.Setup(e => e.GetDevice<IGatDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);

            var command = CreateCommand(TransactionId, VerificationId);

            var gatService = new Mock<IGatService>();

            var verificationStatusResult = CreateVerificationStatusResult();

            gatService.Setup(
                    s =>
                        s.GetVerificationStatus(
                            It.Is<GetVerificationStatusByTransactionArgs>(
                                a => a.VerificationId == VerificationId && a.TransactionId == TransactionId)))
                .Returns(verificationStatusResult);

            var handler = new GetVerificationStatus(egm.Object, gatService.Object);
            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<gat, verificationStatus>;
            Assert.IsNotNull(response);

            Assert.AreEqual(
                response.Command.verificationId,
                verificationStatusResult.VerificationStatus.VerificationId);
            Assert.AreEqual(response.Command.transactionId, verificationStatusResult.VerificationStatus.TransactionId);
            Assert.AreEqual(response.Command.componentStatus.Length, 1);
            Assert.IsTrue(response.Command.componentStatus.First().componentId == "componentId-1");
        }

        [TestMethod]
        public async Task WhenComponentVerificationResultsNotExistHandleExpectResponse()
        {
            var egm = new Mock<IG2SEgm>();
            var device = new Mock<IGatDevice>();

            egm.Setup(e => e.GetDevice<IGatDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);

            var command = CreateCommand(TransactionId, VerificationId);

            var gatService = new Mock<IGatService>();

            var verificationStatusResult = CreateVerificationStatusResult(false);

            gatService.Setup(
                    s =>
                        s.GetVerificationStatus(
                            It.Is<GetVerificationStatusByTransactionArgs>(
                                a => a.VerificationId == VerificationId && a.TransactionId == TransactionId)))
                .Returns(verificationStatusResult);

            var handler = new GetVerificationStatus(egm.Object, gatService.Object);
            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<gat, verificationStatus>;
            Assert.IsNotNull(response);

            Assert.AreEqual(
                response.Command.verificationId,
                verificationStatusResult.VerificationStatus.VerificationId);
            Assert.AreEqual(response.Command.transactionId, verificationStatusResult.VerificationStatus.TransactionId);
            Assert.AreEqual(response.Command.componentStatus.Length, 0);
        }

        private VerificationStatusResult CreateVerificationStatusResult(bool createComponentVerificationResults = true)
        {
            List<ComponentVerificationResult> componentVerificationResults = null;

            if (createComponentVerificationResults)
            {
                componentVerificationResults = new List<ComponentVerificationResult>
                {
                    new ComponentVerificationResult(
                        "componentId-1",
                        ComponentVerificationState
                            .Complete,
                        string.Empty,
                        string.Empty)
                };
            }

            var status = new VerificationStatus(VerificationId, TransactionId, new List<ComponentStatus>());
            VerificationStatusResult entity = new VerificationStatusResult(
                true,
                componentVerificationResults,
                status);

            return entity;
        }

        private ClassCommand<gat, getVerificationStatus> CreateCommand(long transactionId, int verificationId)
        {
            var command = ClassCommandUtilities.CreateClassCommand<gat, getVerificationStatus>(
                TestConstants.HostId,
                TestConstants.EgmId);
            command.Command.transactionId = transactionId;
            command.Command.verificationId = verificationId;

            return command;
        }
    }
}