namespace Aristocrat.Monaco.G2S.Tests.Handlers.Cabinet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers.Cabinet;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class SetDateTimeTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new SetDateTime(null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullTimeExpectException()
        {
            var egm = new Mock<IG2SEgm>();

            var handler = new SetDateTime(egm.Object, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var time = new Mock<ITime>();

            var handler = new SetDateTime(egm.Object, time.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var time = new Mock<ITime>();
            var handler = new SetDateTime(egm.Object, time.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var time = new Mock<ITime>();
            var handler = new SetDateTime(HandlerUtilities.CreateMockEgm<ICabinetDevice>(), time.Object);

            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            const int invalidHostId = 99;

            var device = new Mock<ICabinetDevice>();
            device.SetupGet(d => d.Owner).Returns(invalidHostId);

            var time = new Mock<ITime>();
            var handler = new SetDateTime(HandlerUtilities.CreateMockEgm(device), time.Object);

            await VerificationTests.VerifyChecksForOwner(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithGuestAccessExpectError()
        {
            const int otherHostId = 99;

            var device = new Mock<ICabinetDevice>();
            var egm = HandlerUtilities.CreateMockEgm(device);

            device.SetupGet(d => d.Owner).Returns(otherHostId);
            device.SetupGet(d => d.Guests).Returns(new List<int> { TestConstants.HostId });

            var time = new Mock<ITime>();
            var handler = new SetDateTime(egm, time.Object);

            await VerificationTests.VerifyDeniesGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var device = new Mock<ICabinetDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var time = new Mock<ITime>();
            var handler = new SetDateTime(HandlerUtilities.CreateMockEgm(device), time.Object);

            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenHandleCommandExpectUpdateTime()
        {
            var now = DateTime.UtcNow;

            var device = new Mock<ICabinetDevice>();

            var command = ClassCommandUtilities.CreateClassCommand<cabinet, setDateTime>(
                TestConstants.HostId,
                TestConstants.EgmId);
            command.Command.cabinetDateTime = now;

            var time = new Mock<ITime>();
            var handler = new SetDateTime(HandlerUtilities.CreateMockEgm(device), time.Object);

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<cabinet, cabinetDateTime>;

            Assert.IsNotNull(response);

            time.Verify(d => d.Update(It.Is<DateTimeOffset>(t => t.DateTime == now)));
        }
    }
}