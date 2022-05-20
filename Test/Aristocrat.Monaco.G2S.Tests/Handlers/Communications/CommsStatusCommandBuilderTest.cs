namespace Aristocrat.Monaco.G2S.Tests.Handlers.Communications
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers.Communications;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CommsStatusCommandBuilderTest
    {
        [TestMethod]
        public void WhenConstructExpectSuccess()
        {
            var builder = new CommsStatusCommandBuilder();

            Assert.IsNotNull(builder);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task WhenBuildWithNullDeviceExpectException()
        {
            var builder = new CommsStatusCommandBuilder();

            await builder.Build(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task WhenBuildWithNullCabinetProfileExpectException()
        {
            var builder = new CommsStatusCommandBuilder();

            var device = new Mock<ICommunicationsDevice>();

            await builder.Build(device.Object, null);
        }

        [TestMethod]
        public async Task WhenBuildExpectSuccess()
        {
            var builder = new CommsStatusCommandBuilder();

            var device = new Mock<ICommunicationsDevice>();

            device.SetupGet(d => d.ConfigurationId).Returns(123);
            device.SetupGet(d => d.HostEnabled).Returns(true);
            device.SetupGet(d => d.Enabled).Returns(true);
            device.SetupGet(d => d.OutboundOverflow).Returns(true);
            device.SetupGet(d => d.InboundOverflow).Returns(true);
            device.SetupGet(d => d.State).Returns(t_commsStates.G2S_opening);
            device.SetupGet(d => d.TransportState).Returns(t_transportStates.G2S_transportUp);

            var command = new commsStatus();

            await builder.Build(device.Object, command);

            Assert.AreEqual(command.configurationId, device.Object.ConfigurationId);
            Assert.AreEqual(command.hostEnabled, device.Object.HostEnabled);
            Assert.AreEqual(command.egmEnabled, device.Object.Enabled);
            Assert.AreEqual(command.outboundOverflow, device.Object.OutboundOverflow);
            Assert.AreEqual(command.inboundOverflow, device.Object.InboundOverflow);
            Assert.IsTrue(command.g2sProtocol);
            Assert.AreEqual(command.commsState, device.Object.State);
            Assert.AreEqual(command.transportState, device.Object.TransportState.ToString());
        }
    }
}