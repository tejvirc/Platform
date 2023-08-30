namespace Aristocrat.Monaco.G2S.Tests.Handlers.Meters
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Aristocrat.Monaco.G2S.Services;
    using G2S.Handlers.Meters;
    using G2S.Meters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetMeterInfoTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetMeterInfo(null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullMeterSubscriptionManagerExpectException()
        {
            var egm = new Mock<IG2SEgm>();

            var handler = new GetMeterInfo(egm.Object, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullProgressiveServiceExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var meterSubscriptionManager = new Mock<IMetersSubscriptionManager>();


            var handler = new GetMeterInfo(egm.Object, meterSubscriptionManager.Object, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var meterSubscriptionManager = new Mock<IMetersSubscriptionManager>();
            var progressiveLevelManager = new Mock<IProgressiveLevelManager>();

            var handler = new GetMeterInfo(egm.Object, meterSubscriptionManager.Object, progressiveLevelManager.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var meterSubscriptionManager = new Mock<IMetersSubscriptionManager>();
            var progressiveLevelManager = new Mock<IProgressiveLevelManager>();

            var handler = new GetMeterInfo(egm.Object, meterSubscriptionManager.Object, progressiveLevelManager.Object);
            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var meterSubscriptionManager = new Mock<IMetersSubscriptionManager>();
            var progressiveLevelManager = new Mock<IProgressiveLevelManager>();
            var queue = new Mock<ICommandQueue>();
            var device = new Mock<IMetersDevice>();
            queue.SetupGet(q => q.TimeToLiveBehavior).Returns(TimeToLiveBehavior.Strict);
            device.SetupGet(evt => evt.Queue).Returns(queue.Object);
            egm.Setup(e => e.GetDevice<IMetersDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);
            var handler = new GetMeterInfo(egm.Object, meterSubscriptionManager.Object, progressiveLevelManager.Object);
            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var egm = new Mock<IG2SEgm>();
            var meterSubscriptionManager = new Mock<IMetersSubscriptionManager>();
            var progressiveLevelManager = new Mock<IProgressiveLevelManager>();
            var device = new Mock<IMetersDevice>();
            var queue = new Mock<ICommandQueue>();

            queue.SetupGet(q => q.TimeToLiveBehavior).Returns(TimeToLiveBehavior.Strict);
            device.SetupGet(comms => comms.Queue).Returns(queue.Object);
            device.SetupGet(evt => evt.Owner).Returns(TestConstants.HostId);
            egm.Setup(e => e.GetDevice<IMetersDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);

            var handler = new GetMeterInfo(egm.Object, meterSubscriptionManager.Object, progressiveLevelManager.Object);
            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenSessionTypeIsG2SNotificationExpectNoResult()
        {
            var egm = new Mock<IG2SEgm>();
            var meterSubManager = new Mock<IMetersSubscriptionManager>();
            var progressiveLevelManager = new Mock<IProgressiveLevelManager>();

            var handler = new GetMeterInfo(egm.Object, meterSubManager.Object, progressiveLevelManager.Object);

            var command = CreateCommand();

            command.Class.sessionType = t_sessionTypes.G2S_notification;

            await handler.Handle(command);

            Assert.AreEqual(0, command.Responses.Count());

            meterSubManager.Verify(x => x.GetMeters(It.IsAny<getMeterInfo>(), It.IsAny<meterInfo>()), Times.Never);
        }

        [TestMethod]
        public async Task WhenHandleCommandExpectResponse()
        {
            var egm = new Mock<IG2SEgm>();
            var meterSubManager = new Mock<IMetersSubscriptionManager>();
            var progressiveLevelManager = new Mock<IProgressiveLevelManager>();
            meterSubManager.Setup(
                    x => x.GetMeters(It.Is<getMeterInfo>(i => i != null), It.Is<meterInfo>(i => i != null)))
                .Returns(ErrorCode.G2S_none);
            var handler = new GetMeterInfo(egm.Object, meterSubManager.Object, progressiveLevelManager.Object);

            var command = CreateCommand();
            command.Command.meterInfoType = "G2S_onDemand";

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<meters, meterInfo>;

            Assert.IsNotNull(response);
            Assert.AreEqual(response.Command.meterInfoType, command.Command.meterInfoType);
            Assert.IsTrue(response.Command.meterDateTime != default(DateTime));

            meterSubManager.Verify(x => x.GetMeters(It.IsAny<getMeterInfo>(), It.IsAny<meterInfo>()), Times.Once);
        }

        private ClassCommand<meters, getMeterInfo> CreateCommand()
        {
            var command = ClassCommandUtilities.CreateClassCommand<meters, getMeterInfo>(
                TestConstants.HostId,
                TestConstants.EgmId);

            return command;
        }
    }
}