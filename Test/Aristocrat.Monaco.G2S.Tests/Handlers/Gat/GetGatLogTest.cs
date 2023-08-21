namespace Aristocrat.Monaco.G2S.Tests.Handlers.Gat
{
    using System;
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;
    using Application.Contracts.Authentication;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Common.GAT.Storage;
    using G2S.Handlers.Gat;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Storage;
    using Moq;

    [TestClass]
    public class GetGatLogTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetGatLog(null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructNullDBContextExpectException()
        {
            var egm = new Mock<IG2SEgm>();

            var handler = new GetGatLog(egm.Object, null, null);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructNullRepositoryExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var context = new Mock<IMonacoContextFactory>();
            var handler = new GetGatLog(egm.Object, context.Object, null);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var context = new Mock<IMonacoContextFactory>();
            var repo = new Mock<IGatVerificationRequestRepository>();

            var handler = new GetGatLog(egm.Object, context.Object, repo.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var context = new Mock<IMonacoContextFactory>();
            var repo = new Mock<IGatVerificationRequestRepository>();

            var handler = new GetGatLog(egm.Object, context.Object, repo.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var context = new Mock<IMonacoContextFactory>();
            var repo = new Mock<IGatVerificationRequestRepository>();
            var handler = new GetGatLog(HandlerUtilities.CreateMockEgm<IGatDevice>(), context.Object, repo.Object);

            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            const int invalidHostId = 99;

            var device = new Mock<IGatDevice>();
            device.SetupGet(d => d.Owner).Returns(invalidHostId);

            var context = new Mock<IMonacoContextFactory>();
            var repo = new Mock<IGatVerificationRequestRepository>();
            var handler = new GetGatLog(HandlerUtilities.CreateMockEgm(device), context.Object, repo.Object);

            await VerificationTests.VerifyChecksForOwnerAndGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithGuestAccessExpectNoError()
        {
            const int otherHostId = 99;

            var device = new Mock<IGatDevice>();

            device.SetupGet(d => d.Owner).Returns(otherHostId);
            device.SetupGet(d => d.Guests).Returns(new List<int> { TestConstants.HostId });

            var context = new Mock<IMonacoContextFactory>();
            var repo = new Mock<IGatVerificationRequestRepository>();
            var handler = new GetGatLog(HandlerUtilities.CreateMockEgm(device), context.Object, repo.Object);

            await VerificationTests.VerifyAllowsGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var device = new Mock<IGatDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var context = new Mock<IMonacoContextFactory>();
            var repo = new Mock<IGatVerificationRequestRepository>();
            var handler = new GetGatLog(HandlerUtilities.CreateMockEgm(device), context.Object, repo.Object);

            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenHandleExpectResponse()
        {
            const long lastSequence = 999;

            var egm = new Mock<IG2SEgm>();
            var device = new Mock<IGatDevice>();
            var context = new Mock<IMonacoContextFactory>();
            var repo = new Mock<IGatVerificationRequestRepository>();

            device.SetupGet(comms => comms.DeviceClass).Returns(typeof(gat).Name);
            egm.Setup(e => e.GetDevice<IGatDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);
            egm.Setup(e => e.Devices).Returns(new List<IDevice> { device.Object });

            var command = ClassCommandUtilities.CreateClassCommand<gat, getGatLog>(
                TestConstants.HostId,
                TestConstants.EgmId);
            command.Command.lastSequence = 0;
            command.Command.totalEntries = 0;

            var entity = new GatVerificationRequest
            {
                VerificationId = lastSequence + 1,
                Date = DateTime.UtcNow,
                DeviceId = 1,
                EmployeeId = "user",
                TransactionId = 2,
                Id = 7,
                FunctionType = FunctionType.DoVerification,
                ComponentVerifications = new List<GatComponentVerification>
                {
                    new GatComponentVerification(1)
                    {
                        AlgorithmType = AlgorithmType.Crc16,
                        ComponentId = "ComponentId-1",
                        EndOffset = 1,
                        GatExec = string.Empty,
                        Seed = new byte[0],
                        StartOffset = 1,
                        Result = ConvertExtensions.FromPackedHexString("F00D"),
                        State = ComponentVerificationState.Complete
                    }
                }
            };

            var logs = new List<GatVerificationRequest> { entity }.AsQueryable();

            repo.Setup(s => s.GetAll(It.IsAny<DbContext>())).Returns(logs);

            var handler = new GetGatLog(egm.Object, context.Object, repo.Object);
            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<gat, gatLogList>;
            Assert.IsNotNull(response);

            var log = response.Command.gatLog.FirstOrDefault();
            Assert.IsNotNull(log);

            Assert.AreEqual(log.verificationId, entity.VerificationId);
            Assert.AreEqual(log.deviceId, entity.DeviceId);
            Assert.AreEqual(log.transactionId, entity.TransactionId);
            Assert.AreEqual(log.functionType, t_functionTypes.G2S_doVerification);
            Assert.AreEqual(log.employeeId, entity.EmployeeId);
            Assert.AreEqual(log.gatDateTime, entity.Date);
            Assert.AreEqual(log.componentLog.Length, 1);
            Assert.IsTrue(log.componentLog.First().componentId == "ComponentId-1");
        }
    }
}
