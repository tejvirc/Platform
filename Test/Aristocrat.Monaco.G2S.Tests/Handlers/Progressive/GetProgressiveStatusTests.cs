namespace Aristocrat.Monaco.G2S.Tests.Handlers.Progressive
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers;
    using G2S.Handlers.Progressive;
    using Gaming.Contracts.Progressives;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetProgressiveStatusTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var commandBuilderMock = new Mock<ICommandBuilder<IProgressiveDevice, progressiveStatus>>();

            var handler = new GetProgressiveStatus(null, commandBuilderMock.Object, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullCommandBuilderExpectException()
        {
            var egmMock = new Mock<IG2SEgm>();
            
            var handler = new GetProgressiveStatus(egmMock.Object, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithOwnerAndGuestExpectSuccess()
        {
            var egm = HandlerUtilities.CreateMockEgm<IProgressiveDevice>();
            var commandBuilderMock = new Mock<ICommandBuilder<IProgressiveDevice, progressiveStatus>>();
            var progressiveProvider = new Mock<IProgressiveLevelProvider>();
            var handler = new GetProgressiveStatus(egm, commandBuilderMock.Object, progressiveProvider.Object);

            await VerificationTests.VerifyChecksForOwnerAndGuests(handler);
        }

        [TestMethod]
        public async Task WhenHandleWithValidParamsExpectSuccess()
        {
            var cabinetDeviceMock = new Mock<IProgressiveDevice>();
            var egm = HandlerUtilities.CreateMockEgm(cabinetDeviceMock);
            var commandBuilderMock = new Mock<ICommandBuilder<IProgressiveDevice, progressiveStatus>>();
            var progressiveProvider = new Mock<IProgressiveLevelProvider>();
            var handler = new GetProgressiveStatus(egm, commandBuilderMock.Object, progressiveProvider.Object);

            var command = ClassCommandUtilities.CreateClassCommand<progressive, getProgressiveStatus>(
                TestConstants.HostId,
                TestConstants.EgmId);

            await handler.Handle(command);

            commandBuilderMock.Verify(m => m.Build(cabinetDeviceMock.Object, It.IsAny<progressiveStatus>()));
        }
    }
}