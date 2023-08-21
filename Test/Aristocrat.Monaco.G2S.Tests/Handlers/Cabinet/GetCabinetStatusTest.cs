namespace Aristocrat.Monaco.G2S.Tests.Handlers.Cabinet
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers;
    using G2S.Handlers.Cabinet;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetCabinetStatusTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var commandBuilderMock = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();

            var handler = new GetCabinetStatus(null, commandBuilderMock.Object);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullCommandBuilderExpectException()
        {
            var egmMock = new Mock<IG2SEgm>();

            var handler = new GetCabinetStatus(egmMock.Object, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithOwnerAndGuestExpectSuccess()
        {
            var egm = HandlerUtilities.CreateMockEgm<ICabinetDevice>();
            var commandBuilderMock = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();

            var handler = new GetCabinetStatus(egm, commandBuilderMock.Object);

            await VerificationTests.VerifyChecksForOwnerAndGuests(handler);
        }

        [TestMethod]
        public async Task WhenHandleWithValidParamsExpectSuccess()
        {
            var cabinetDeviceMock = new Mock<ICabinetDevice>();
            var egm = HandlerUtilities.CreateMockEgm(cabinetDeviceMock);
            var commandBuilderMock = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();

            var handler = new GetCabinetStatus(egm, commandBuilderMock.Object);

            var command = ClassCommandUtilities.CreateClassCommand<cabinet, getCabinetStatus>(
                TestConstants.HostId,
                TestConstants.EgmId);

            await handler.Handle(command);

            commandBuilderMock.Verify(m => m.Build(cabinetDeviceMock.Object, It.IsAny<cabinetStatus>()));
        }
    }
}