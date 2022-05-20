namespace Aristocrat.Monaco.G2S.Tests.Handlers.CoinAcceptor
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers.CoinAcceptor;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CoinAcceptorStatusCommandBuilderTest
    {
        [TestMethod]
        public async Task WhenBuildCommandWithValidParamsExpectSuccess()
        {
            var builder = new CoinAcceptorStatusCommandBuilder();
            var command = new coinAcceptorStatus();
            var device = new Mock<ICoinAcceptor>();
            device.SetupGet(m => m.ConfigurationId).Returns(1);
            device.SetupGet(m => m.HostEnabled).Returns(true);
            device.SetupGet(m => m.Enabled).Returns(true);
            device.SetupGet(m => m.ConfigComplete).Returns(true);
            device.SetupGet(m => m.ConfigDateTime).Returns(DateTime.MaxValue);

            await builder.Build(device.Object, command);

            Assert.AreEqual(1, command.configurationId);
            Assert.AreEqual(true, command.hostEnabled);
            Assert.AreEqual(true, command.egmEnabled);
            Assert.AreEqual(true, command.configComplete);
            Assert.AreEqual(DateTime.MaxValue, command.configDateTime);

            Assert.AreEqual(false, command.disconnected);
            Assert.AreEqual(false, command.firmwareFault);
            Assert.AreEqual(false, command.mechanicalFault);
            Assert.AreEqual(false, command.opticalFault);
            Assert.AreEqual(false, command.nvMemoryFault);
            Assert.AreEqual(false, command.illegalActivity);
            Assert.AreEqual(false, command.doorOpen);
            Assert.AreEqual(false, command.jammed);
            Assert.AreEqual(false, command.lockoutMalfunction);
            Assert.AreEqual(false, command.acceptorFault);
            Assert.AreEqual(false, command.diverterFault);
        }
    }
}