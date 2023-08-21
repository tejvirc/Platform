namespace Aristocrat.Monaco.G2S.Tests.Handlers.Progressive
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers.Progressive;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class SetProgressiveValueTests
    {
        private Mock<IG2SEgm> _egm;

        [TestInitialize]
        public void Initialize()
        {
            _egm = new Mock<IG2SEgm>();
        }

        // TODO: Uncomment this test when g2s is fixed
        //[TestMethod]
        //public void WhenConstructWithEgmExpectException()
        //{
        //    var progressiveDataService = new Mock<IProgressiveService>();
        //    var progressiveProvider = new Mock<IProgressiveProvider>();
        //    var jackpotProvider = new Mock<G2SJackpotProvider>();
        //    var handler = new SetProgressiveValue(
        //        _egm.Object,
        //        progressiveProvider.Object,
        //        jackpotProvider.Object);

        //    Assert.IsNotNull(handler);
        //}

        // TODO: Uncomment this test when g2s is fixed
        //[TestMethod]
        //public async Task WhenVerifyOwnerExpectSuccess()
        //{
        //    var egm = HandlerUtilities.CreateMockEgm<IProgressiveDevice>();
        //    var handler = CreateHandler(egm);

        //    await VerificationTests.VerifyChecksForOwner(handler);
        //}

        // TODO: Uncomment this test when g2s is fixed
        //[TestMethod]
        //public async Task WhenHandleCommandWithProgressiveValueExpectSuccess()
        //{
        //    var deviceMock = new Mock<IProgressiveDevice>();
        //    deviceMock.SetupGet(m => m.DeviceClass).Returns(DeviceClass.G2S_progressive);
        //    var egm = HandlerUtilities.CreateMockEgm(deviceMock);
        //    var handler = CreateHandler(egm);

        //    var command = CreateCommand();
        //    var mockSetLevelValue = new Mock<setLevelValue>();
        //    command.Command.setLevelValue = new setLevelValue[1];
        //    command.Command.setLevelValue[0] = mockSetLevelValue.Object;

        //    await handler.Handle(command);
        //    var response = command.Responses.FirstOrDefault() as ClassCommand<progressive, progressiveValueAck>;

        //    Assert.IsNotNull(response);
        //}

        // TODO: Uncomment this test when g2s is fixed
        //[TestMethod]
        //public async Task WhenHandleCommandWithNullSetLevelValueExpectSuccess()
        //{
        //    var deviceMock = new Mock<IProgressiveDevice>();
        //    deviceMock.SetupGet(m => m.DeviceClass).Returns(DeviceClass.G2S_progressive);
        //    deviceMock.SetupGet(m => m.RequiredForPlay).Returns(true);
        //    var egm = HandlerUtilities.CreateMockEgm(deviceMock);
        //    var handler = CreateHandler(egm);

        //    var command = CreateCommand();
        //    command.Command.setLevelValue = null;
        //    await handler.Handle(command);

        //    var response = command.Responses.FirstOrDefault() as ClassCommand<progressive, progressiveValueAck>;

        //    Assert.IsNotNull(response);
        //}

        private SetProgressiveValue CreateHandler(IG2SEgm egm = null)
        {
            var handler = new SetProgressiveValue();

            return handler;
        }

        private ClassCommand<progressive, setProgressiveValue> CreateCommand()
        {
            var command = ClassCommandUtilities.CreateClassCommand<progressive, setProgressiveValue>(
                TestConstants.HostId,
                TestConstants.EgmId);

            return command;
        }
    }
}