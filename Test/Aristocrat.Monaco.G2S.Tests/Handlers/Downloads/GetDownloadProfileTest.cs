namespace Aristocrat.Monaco.G2S.Tests.Handlers.Downloads
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers.Downloads;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetDownloadProfileTest
    {
        private readonly Mock<IG2SEgm> _egmMock = new Mock<IG2SEgm>();

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetDownloadProfile(null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var handler = new GetDownloadProfile(_egmMock.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var handler = new GetDownloadProfile(_egmMock.Object);
            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var egm = DownloadsUtilities.CreateMockEgm();

            var handler = new GetDownloadProfile(egm);
            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var device = new Mock<IDownloadDevice>();
            var queue = new Mock<ICommandQueue>();

            queue.SetupGet(q => q.TimeToLiveBehavior).Returns(TimeToLiveBehavior.Strict);
            device.SetupGet(comms => comms.Queue).Returns(queue.Object);
            device.SetupGet(evt => evt.Owner).Returns(TestConstants.HostId);
            _egmMock.Setup(e => e.GetDevice<IDownloadDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);

            var handler = new GetDownloadProfile(_egmMock.Object);
            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenSessionTypeIsG2SNotificationExpectNoResult()
        {
            var handler = new GetDownloadProfile(_egmMock.Object);

            var command = CreateCommand();

            command.Class.sessionType = t_sessionTypes.G2S_notification;

            await handler.Handle(command);

            Assert.AreEqual(0, command.Responses.Count());
        }

        [TestMethod]
        public async Task WhenDeviceNotFoundExpectNoResult()
        {
            var handler = new GetDownloadProfile(_egmMock.Object);

            var command = CreateCommand();

            await handler.Handle(command);

            Assert.AreEqual(0, command.Responses.Count());
        }

        [TestMethod]
        public async Task WhenHandleExpectResult()
        {
            const int deviceId = 1;
            const int authenticationWaitRetries = 5;
            const int authenticationWaitTimeOut = 65123;
            var configDateTime = DateTime.Now;
            const int configurationId = 1;
            const int minPackageListEntries = 15;
            const int minPackageLogEntries = 16;
            const int minScriptListEntries = 17;
            const int minScriptLogEntries = 18;
            const int noMessageTimer = 19;
            const int transferProgressFrequency = 20;
            const int timeToLive = 21;
            var noResponseTimer = new TimeSpan(1, 2, 3);

            var device = new Mock<IDownloadDevice>();
            device.SetupGet(x => x.Id).Returns(deviceId);
            device.SetupGet(x => x.UseDefaultConfig).Returns(true);
            device.SetupGet(x => x.AbortTransferSupported).Returns(true);
            device.SetupGet(x => x.AuthenticationWaitRetries).Returns(authenticationWaitRetries);
            device.SetupGet(x => x.AuthenticationWaitTimeOut).Returns(authenticationWaitTimeOut);
            device.SetupGet(x => x.ConfigComplete).Returns(true);
            device.SetupGet(x => x.ConfigDateTime).Returns(configDateTime);
            device.SetupGet(x => x.ConfigurationId).Returns(configurationId);
            device.SetupGet(x => x.DownloadEnabled).Returns(true);
            device.SetupGet(x => x.MinPackageListEntries).Returns(minPackageListEntries);
            device.SetupGet(x => x.MinPackageLogEntries).Returns(minPackageLogEntries);
            device.SetupGet(x => x.MinScriptListEntries).Returns(minScriptListEntries);
            device.SetupGet(x => x.MinScriptLogEntries).Returns(minScriptLogEntries);
            device.SetupGet(x => x.NoMessageTimer).Returns(noMessageTimer);
            device.SetupGet(x => x.PauseSupported).Returns(true);
            device.SetupGet(x => x.ProtocolListSupport).Returns(true);
            device.SetupGet(x => x.RequiredForPlay).Returns(true);
            device.SetupGet(x => x.RestartStatus).Returns(true);
            device.SetupGet(x => x.ScriptingEnabled).Returns(true);
            device.SetupGet(x => x.TransferProgressFrequency).Returns(transferProgressFrequency);
            device.SetupGet(x => x.TimeToLive).Returns(timeToLive);
            device.As<INoResponseTimer>().SetupGet(x => x.NoResponseTimer).Returns(noResponseTimer);

            _egmMock.Setup(x => x.GetDevice<IDownloadDevice>(deviceId)).Returns(device.Object);

            var handler = new GetDownloadProfile(_egmMock.Object);

            var command = CreateCommand();

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<download, downloadProfile>;

            Assert.IsNotNull(response);
            Assert.IsTrue(response.Command.useDefaultConfig);
            Assert.IsTrue(response.Command.abortTransferSupported);
            Assert.AreEqual(response.Command.authWaitRetries, authenticationWaitRetries);
            Assert.AreEqual(response.Command.authWaitTimeOut, authenticationWaitTimeOut);
            Assert.IsTrue(response.Command.configComplete);
            Assert.AreEqual(response.Command.configDateTime, configDateTime);
            Assert.IsTrue(response.Command.configDateTimeSpecified);
            Assert.AreEqual(response.Command.configurationId, configurationId);
            Assert.IsTrue(response.Command.downloadEnabled);
            Assert.AreEqual(response.Command.minPackageListEntries, minPackageListEntries);
            Assert.AreEqual(response.Command.minPackageLogEntries, minPackageLogEntries);
            Assert.AreEqual(response.Command.minScriptListEntries, minScriptListEntries);
            Assert.AreEqual(response.Command.minScriptLogEntries, minScriptLogEntries);
            Assert.AreEqual(response.Command.noMessageTimer, noMessageTimer);
            Assert.IsTrue(response.Command.pauseSupported);
            Assert.IsTrue(response.Command.requiredForPlay);
            Assert.IsTrue(response.Command.restartStatus);
            Assert.IsTrue(response.Command.scriptingEnabled);
            Assert.AreEqual(response.Command.transferProgressFreq, transferProgressFrequency);
            Assert.AreEqual(response.Command.noResponseTimer, (int)noResponseTimer.TotalMilliseconds);
            Assert.AreEqual(response.Command.timeToLive, timeToLive);
        }

        private ClassCommand<download, getDownloadProfile> CreateCommand()
        {
            var command = ClassCommandUtilities.CreateClassCommand<download, getDownloadProfile>(
                TestConstants.HostId,
                TestConstants.EgmId);

            return command;
        }
    }
}