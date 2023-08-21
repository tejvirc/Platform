namespace Aristocrat.Monaco.G2S.Tests.Handlers.NoteAcceptor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers;
    using G2S.Handlers.NoteAcceptor;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetNoteAcceptorStatusTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetNoteAcceptorStatus(null, null);
            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullDeviceRegistryExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var handler = new GetNoteAcceptorStatus(egm.Object, null);
            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var command = new Mock<ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus>>();
            var handler = new GetNoteAcceptorStatus(egm.Object, command.Object);
            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var command = new Mock<ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus>>();
            var handler = new GetNoteAcceptorStatus(egm.Object, command.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenSessionTypeIsG2SResponseExpectNoResult()
        {
            var device = new Mock<INoteAcceptorDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);
            device.SetupGet(d => d.Id).Returns(TestConstants.HostId);
            var egm = HandlerUtilities.CreateMockEgm(device);
            var commandBuilder = new Mock<ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus>>();

            var handler = new GetNoteAcceptorStatus(egm, commandBuilder.Object);

            var command =
                ClassCommandUtilities.CreateClassCommand<noteAcceptor, getNoteAcceptorStatus>(
                    TestConstants.HostId,
                    TestConstants.EgmId);

            command.Class.sessionType = t_sessionTypes.G2S_response;

            await handler.Handle(command);

            Assert.AreEqual(0, command.Responses.Count());
        }

        [TestMethod]
        public async Task WhenSessionTypeIsG2SNotificationExpectNoResult()
        {
            var device = new Mock<INoteAcceptorDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);
            device.SetupGet(d => d.Id).Returns(TestConstants.HostId);
            var egm = HandlerUtilities.CreateMockEgm(device);
            var commandBuilder = new Mock<ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus>>();

            var handler = new GetNoteAcceptorStatus(egm, commandBuilder.Object);

            var command =
                ClassCommandUtilities.CreateClassCommand<noteAcceptor, getNoteAcceptorStatus>(
                    TestConstants.HostId,
                    TestConstants.EgmId);

            command.Class.sessionType = t_sessionTypes.G2S_notification;

            await handler.Handle(command);

            Assert.AreEqual(0, command.Responses.Count());
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var command = new Mock<ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus>>();
            var handler = new GetNoteAcceptorStatus(
                HandlerUtilities.CreateMockEgm<INoteAcceptorDevice>(),
                command.Object);

            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            const int invalidHostId = 99;

            var device = new Mock<INoteAcceptorDevice>();
            device.SetupGet(d => d.Owner).Returns(invalidHostId);

            var command = new Mock<ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus>>();
            var handler = new GetNoteAcceptorStatus(HandlerUtilities.CreateMockEgm(device), command.Object);

            await VerificationTests.VerifyChecksForOwnerAndGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithGuestAccessExpectNoError()
        {
            const int otherHostId = 99;

            var device = new Mock<INoteAcceptorDevice>();
            var egm = HandlerUtilities.CreateMockEgm(device);

            device.SetupGet(d => d.Owner).Returns(otherHostId);
            device.SetupGet(d => d.Guests).Returns(new List<int> { TestConstants.HostId });

            var command = new Mock<ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus>>();
            var handler = new GetNoteAcceptorStatus(egm, command.Object);

            await VerificationTests.VerifyAllowsGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var device = new Mock<INoteAcceptorDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var command = new Mock<ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus>>();
            var handler = new GetNoteAcceptorStatus(HandlerUtilities.CreateMockEgm(device), command.Object);

            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenHandleCommandExpectResponse()
        {
            var egm = HandlerUtilities.CreateMockEgm<INoteAcceptorDevice>();
            var commandBuilder = new Mock<ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus>>();

            var handler = new GetNoteAcceptorStatus(egm, commandBuilder.Object);

            var command = ClassCommandUtilities.CreateClassCommand<noteAcceptor, getNoteAcceptorStatus>(
                TestConstants.HostId,
                TestConstants.EgmId);

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<noteAcceptor, noteAcceptorStatus>;

            Assert.IsNotNull(response);

            commandBuilder.Verify(v => v.Build(It.IsAny<INoteAcceptorDevice>(), response.Command));
        }
    }
}