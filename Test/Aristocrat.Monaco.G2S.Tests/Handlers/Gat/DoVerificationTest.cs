namespace Aristocrat.Monaco.G2S.Tests.Handlers.Gat
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Application.Contracts.Authentication;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Common.GAT.CommandHandlers;
    using Common.GAT.Models;
    using Common.GAT.Storage;
    using G2S.Handlers.Gat;
    using Kernel.Contracts.Components;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class DoVerificationTest
    {
        public const string Algorithm = @"G2S_SHA1";

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new DoVerification(null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructNullGatServiceExpectException()
        {
            var egm = new Mock<IG2SEgm>();

            var handler = new DoVerification(egm.Object, null, null);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructNullEventLiftExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var gatService = new Mock<IGatService>();
            var handler = new DoVerification(egm.Object, gatService.Object, null);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var gatService = new Mock<IGatService>();
            var eventLift = new Mock<IEventLift>();
            var handler = new DoVerification(egm.Object, gatService.Object, eventLift.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var gatService = new Mock<IGatService>();
            var eventLift = new Mock<IEventLift>();
            var handler = new DoVerification(egm.Object, gatService.Object, eventLift.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var gatService = new Mock<IGatService>();
            var eventLift = new Mock<IEventLift>();
            var handler = new DoVerification(
                HandlerUtilities.CreateMockEgm<IGatDevice>(),
                gatService.Object,
                eventLift.Object);

            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            const int invalidHostId = 99;

            var device = new Mock<IGatDevice>();
            device.SetupGet(d => d.Owner).Returns(invalidHostId);
            var eventLift = new Mock<IEventLift>();
            var gatService = new Mock<IGatService>();
            var handler = new DoVerification(
                HandlerUtilities.CreateMockEgm(device),
                gatService.Object,
                eventLift.Object);

            await VerificationTests.VerifyChecksForOwner(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithGuestAccessExpectError()
        {
            const int otherHostId = 99;

            var device = new Mock<IGatDevice>();
            var eventLift = new Mock<IEventLift>();
            device.SetupGet(d => d.Owner).Returns(otherHostId);
            device.SetupGet(d => d.Guests).Returns(new List<int> { TestConstants.HostId });

            var gatService = new Mock<IGatService>();
            var handler = new DoVerification(
                HandlerUtilities.CreateMockEgm(device),
                gatService.Object,
                eventLift.Object);

            await VerificationTests.VerifyDeniesGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithBogusAlgorithmExpectError()
        {
            var device = new Mock<IGatDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);
            var eventLift = new Mock<IEventLift>();
            var gatService = new Mock<IGatService>();
            var component = new Component { Type = ComponentType.None };
            gatService.Setup(a => a.GetComponent(It.IsAny<string>())).Returns(component);
            var handler = new DoVerification(
                HandlerUtilities.CreateMockEgm(device),
                gatService.Object,
                eventLift.Object);

            var command = ClassCommandUtilities.CreateClassCommand<gat, doVerification>(
                TestConstants.HostId,
                TestConstants.EgmId);
            command.Command.verifyComponent = new[] { new verifyComponent { algorithmType = "G2S_Bogus" } };

            var result = await handler.Verify(command);

            Assert.AreEqual(ErrorCode.G2S_GAX007, result.Code);
        }

        [TestMethod]
        public async Task WhenVerifyWithRequiredButMissingSeedExpectError()
        {
            var device = new Mock<IGatDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);
            var eventLift = new Mock<IEventLift>();
            var gatService = new Mock<IGatService>();
            var algorithms = new List<IAlgorithm>();
            algorithms.Add(
                new Algorithm
                {
                    SupportsOffsets = true,
                    Type = AlgorithmType.Crc16,
                    SupportsSeed = true,
                    SupportsSalt = true
                });
            gatService.Setup(d => d.GetSupportedAlgorithms(It.IsAny<ComponentType>())).Returns(algorithms);
            gatService.Setup(m => m.GetComponent(It.IsAny<string>())).Returns(new Component { Size = long.MaxValue });
            var handler = new DoVerification(
                HandlerUtilities.CreateMockEgm(device),
                gatService.Object,
                eventLift.Object);

            var command = ClassCommandUtilities.CreateClassCommand<gat, doVerification>(
                TestConstants.HostId,
                TestConstants.EgmId);
            command.Command.verifyComponent = new[] { new verifyComponent { algorithmType = "G2S_Crc16" } };

            var result = await handler.Verify(command);

            Assert.AreEqual(ErrorCode.G2S_GAX013, result.Code);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var device = new Mock<IGatDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);
            var eventLift = new Mock<IEventLift>();
            var gatService = new Mock<IGatService>();
            gatService.Setup(g => g.GetSupportedAlgorithms(It.IsAny<ComponentType>()))
                .Returns(new List<IAlgorithm> { new Algorithm { Type = AlgorithmType.Sha1 } });
            gatService.Setup(m => m.GetComponent(It.IsAny<string>())).Returns(new Component { Size = long.MaxValue });
            var handler = new DoVerification(
                HandlerUtilities.CreateMockEgm(device),
                gatService.Object,
                eventLift.Object);

            var command = ClassCommandUtilities.CreateClassCommand<gat, doVerification>(
                TestConstants.HostId,
                TestConstants.EgmId);
            command.Command.verifyComponent = new[] { new verifyComponent { algorithmType = "G2S_Sha1" } };
            command.Command.verificationId = 1;
            await VerificationTests.VerifyCanSucceed(handler, command);
        }

        [TestMethod]
        public async Task WhenHasVerificationOnHandleExpectResponse()
        {
            var command = ClassCommandUtilities.CreateClassCommand<gat, doVerification>(
                TestConstants.HostId,
                TestConstants.EgmId);
            command.Command.verificationId = 1;
            var eventLift = new Mock<IEventLift>();
            var gatService = new Mock<IGatService>();
            gatService.Setup(x => x.HasVerificationId(1)).Returns(true);
            gatService.Setup(x => x.GetVerificationRequestById(1))
                .Returns(new GatVerificationRequest { TransactionId = 1, VerificationId = 1 });
            gatService.Setup(
                    x =>
                        x.GetVerificationStatus(
                            It.Is<GetVerificationStatusByTransactionArgs>(
                                arg => arg.TransactionId == 1 && arg.VerificationId == 1)))
                .Returns(
                    new VerificationStatusResult(
                        true,
                        new List<ComponentVerificationResult>
                        {
                            new ComponentVerificationResult(
                                "componentId-1",
                                ComponentVerificationState.Complete,
                                string.Empty,
                                string.Empty)
                        },
                        new VerificationStatus(
                            1,
                            1,
                            new List<ComponentStatus>
                            {
                                new ComponentStatus("componentId-1", ComponentVerificationState.Complete)
                            })));

            var egm = new Mock<IG2SEgm>();

            var handler = new DoVerification(egm.Object, gatService.Object, eventLift.Object);
            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<gat, verificationStatus>;

            Assert.IsNotNull(response);
            Assert.AreEqual(response.Command.transactionId, 1);
            Assert.AreEqual(response.Command.verificationId, 1);
            Assert.AreEqual(response.Command.componentStatus.Length, 1);
            Assert.IsTrue(response.Command.componentStatus.First().componentId == "componentId-1");
            Assert.IsTrue(response.Command.componentStatus.First().verifyState == t_verifyStates.G2S_complete);
        }

        [TestMethod]
        public async Task WhenNotHasVerificationOnHandleExpectResponse()
        {
            var command = ClassCommandUtilities.CreateClassCommand<gat, doVerification>(
                TestConstants.HostId,
                TestConstants.EgmId);
            command.Command.verificationId = 1;
            command.Command.verifyComponent = new[]
            {
                new verifyComponent
                {
                    componentId = "componentId-1",
                    algorithmType = Algorithm,
                    salt = new byte[0],
                    startOffset = 0,
                    endOffset = -1
                }
            };

            var gatService = new Mock<IGatService>();
            gatService.Setup(x => x.HasVerificationId(1)).Returns(false);
            gatService.Setup(x => x.DoVerification(It.Is<DoVerificationArgs>(args => args.VerificationId == 1)))
                .Returns(
                    new VerificationStatus(
                        1,
                        1,
                        new List<ComponentStatus>
                        {
                            new ComponentStatus("componentId-1", ComponentVerificationState.Complete)
                        }));
            gatService.Setup(x => x.GetLogForTransactionId(1)).Returns(
                new GatVerificationRequest(1)
                {
                    Id = 1,
                    DeviceId = 1,
                    TransactionId = 1,
                    FunctionType = FunctionType.DoVerification,
                    VerificationId = 1,
                    EmployeeId = "EmployeeId-1",
                    Date = DateTime.UtcNow,
                    ComponentVerifications = new List<GatComponentVerification>
                    {
                        new GatComponentVerification(1)
                        {
                            AlgorithmType = AlgorithmType.Crc16,
                            ComponentId = "ComponentId-1",
                            EndOffset = 0,
                            GatExec = string.Empty,
                            State = ComponentVerificationState.Complete
                        }
                    }
                });

            var egm = new Mock<IG2SEgm>();
            var device = new Mock<IGatDevice>();
            device.SetupGet(x => x.DeviceClass).Returns("gat");
            egm.Setup(e => e.GetDevice<IGatDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);
            var eventLift = new Mock<IEventLift>();
            var handler = new DoVerification(egm.Object, gatService.Object, eventLift.Object);
            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<gat, verificationStatus>;

            Assert.IsNotNull(response);

            Assert.AreEqual(response.Command.transactionId, 1);
            Assert.AreEqual(response.Command.verificationId, 1);
            Assert.AreEqual(response.Command.componentStatus.Length, 1);
            Assert.IsTrue(response.Command.componentStatus.First().componentId == "componentId-1");
            Assert.IsTrue(response.Command.componentStatus.First().verifyState == t_verifyStates.G2S_complete);

            gatService.Verify(
                x => x.DoVerification(It.Is<DoVerificationArgs>(args => args.VerificationId == 1)),
                Times.Once);
        }
    }
}
