namespace Aristocrat.Monaco.G2S.Tests.Handlers.Progressive
{
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Aristocrat.Monaco.G2S.Services;
    using Aristocrat.Monaco.Test.Common;
    using G2S.Handlers.Progressive;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;
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
            var protocolLinkedProgressiveAdapter = new Mock<IProtocolLinkedProgressiveAdapter>();
            var builder = new ProgressiveStatusCommandBuilder(protocolLinkedProgressiveAdapter.Object);
            var device = new Mock<IProgressiveDevice>();
            var status = new progressiveStatus();

            protocolLinkedProgressiveAdapter.Setup(m => m.ViewLinkedProgressiveLevels())
                .Returns(Enumerable.Empty<LinkedProgressiveLevel>().ToList());

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