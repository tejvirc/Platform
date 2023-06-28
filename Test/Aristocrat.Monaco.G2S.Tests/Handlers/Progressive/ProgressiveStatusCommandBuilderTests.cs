﻿namespace Aristocrat.Monaco.G2S.Tests.Handlers.Progressive
{
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Aristocrat.Monaco.G2S.Services.Progressive;
    using Aristocrat.Monaco.Test.Common;
    using G2S.Handlers.Progressive;
    using Gaming.Contracts.Progressives;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ProgressiveStatusCommandBuilderTests
    {
        [TestMethod]
        public async Task WhenBuildExpectSuccess()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IProgressiveService>(MockBehavior.Default);
            var progressiveProvider = new Mock<IProgressiveLevelProvider>();
            var protocolLinkedProgressiveAdapter = new Mock<IProtocolLinkedProgressiveAdapter>();
            var builder = new ProgressiveStatusCommandBuilder(progressiveProvider.Object, protocolLinkedProgressiveAdapter.Object);
            var device = new Mock<IProgressiveDevice>();
            var status = new progressiveStatus();

            progressiveProvider.Setup(m => m.GetProgressiveLevels())
                .Returns(Enumerable.Empty<ProgressiveLevel>().ToList());

            device.SetupAllProperties();

            await builder.Build(device.Object, status);

            Assert.AreEqual(status.configurationId, device.Object.ConfigurationId);
            Assert.AreEqual(status.egmEnabled, device.Object.Enabled);
            Assert.AreEqual(status.hostEnabled, device.Object.HostEnabled);
            Assert.AreEqual(status.configComplete, device.Object.ConfigComplete);
            Assert.AreEqual(status.configDateTime, device.Object.ConfigDateTime);
            Assert.IsFalse(status.configDateTimeSpecified);
            Assert.IsNotNull(status.levelStatus);
        }
    }
}