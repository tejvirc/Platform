namespace Aristocrat.Monaco.G2S.Tests.Handlers.OptionConfig
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers.OptionConfig;
    using G2S.Services;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class OptionConfigStatusCommandBuilderTest
    {
        private readonly Mock<IDisableConditionSaga> _configurationModeSagaMock = new Mock<IDisableConditionSaga>();
        private readonly Mock<IG2SEgm> _egmMock = new Mock<IG2SEgm>();

        [TestMethod]
        public void WhenConstructWithNullsExpectException()
        {
            ConstructorTest.TestConstructorNullChecks<OptionConfigStatusCommandBuilder>();
        }

        [TestMethod]
        public void WhenConstructWithAllParametersExpectSuccess()
        {
            var builder = new OptionConfigStatusCommandBuilder(_egmMock.Object, _configurationModeSagaMock.Object);

            Assert.IsNotNull(builder);
        }

        [TestMethod]
        public async Task WhenBuildExpectSuccess()
        {
            const int configurationId = 1;
            var configDateTime = DateTime.Now;

            var optionConfigDevice = new Mock<IOptionConfigDevice>();
            optionConfigDevice.SetupGet(x => x.ConfigurationId).Returns(configurationId);
            optionConfigDevice.SetupGet(x => x.ConfigDateTime).Returns(configDateTime);

            var cabinetDevice = new Mock<ICabinetDevice>();
            _egmMock.Setup(x => x.GetDevice<ICabinetDevice>()).Returns(cabinetDevice.Object);
            cabinetDevice.SetupGet(x => x.Device).Returns(optionConfigDevice.Object);

            _configurationModeSagaMock.Setup(m => m.Enabled(It.IsAny<IDevice>())).Returns(true);

            var builder = new OptionConfigStatusCommandBuilder(_egmMock.Object, _configurationModeSagaMock.Object);

            var command = new optionConfigModeStatus();

            Assert.AreEqual(command.configurationId, 0);
            Assert.IsFalse(command.enabled);
            Assert.AreEqual(command.configDateTime, default(DateTime));
            Assert.IsTrue(command.configComplete);
            Assert.IsFalse(command.egmLocked);

            await builder.Build(optionConfigDevice.Object, command);

            Assert.AreEqual(command.configurationId, configurationId);
            Assert.IsTrue(command.enabled);
            Assert.AreEqual(command.configDateTime, configDateTime);
            Assert.IsFalse(command.configComplete);
            Assert.IsTrue(command.egmLocked);
        }
    }
}