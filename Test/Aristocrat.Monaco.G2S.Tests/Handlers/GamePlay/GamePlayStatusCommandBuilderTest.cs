namespace Aristocrat.Monaco.G2S.Tests.Handlers.GamePlay
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers.GamePlay;
    using Gaming.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GamePlayStatusCommandBuilderTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var commandBuilder = new GamePlayStatusCommandBuilder(null, null);

            Assert.IsNull(commandBuilder);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullGameProviderExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var commandBuilder = new GamePlayStatusCommandBuilder(egm.Object, null);

            Assert.IsNull(commandBuilder);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var gameProfile = new Mock<IGameProvider>();
            var commandBuilder = new GamePlayStatusCommandBuilder(egm.Object, gameProfile.Object);

            Assert.IsNotNull(commandBuilder);
        }

        [TestMethod]
        public async Task WhenBuildCommandExpectSuccess()
        {
            const int deviceId = 1;
            const int configurationId = 1;
            const string themeId = "G2S_themeId";
            const string paytableId = "G2S_paytableId";
            var configDateTime = DateTime.Now;

            var cabinet = new Mock<ICabinetDevice>();
            var egm = new Mock<IG2SEgm>();
            egm.Setup(m => m.GetDevice<ICabinetDevice>()).Returns(cabinet.Object);

            var gameProfile = new Mock<IGameDetail>();
            gameProfile.SetupGet(x => x.Status).Returns(GameStatus.DisabledBySystem | GameStatus.DisabledByBackend);
            gameProfile.SetupGet(x => x.ThemeId).Returns(themeId);
            gameProfile.SetupGet(x => x.PaytableId).Returns(paytableId);

            var gameProvider = new Mock<IGameProvider>();
            gameProvider.Setup(x => x.GetGame(deviceId)).Returns(gameProfile.Object);

            var builder = new GamePlayStatusCommandBuilder(egm.Object, gameProvider.Object);

            var command = new gamePlayStatus();
            Assert.AreEqual(command.configurationId, 0);
            Assert.IsTrue(command.hostEnabled);
            Assert.IsTrue(command.egmEnabled);
            Assert.IsNull(command.themeId);
            Assert.IsNull(command.paytableId);
            Assert.IsFalse(command.generalTilt);
            Assert.AreEqual(command.configDateTime, default(DateTime));
            Assert.IsTrue(command.configComplete);
            Assert.IsFalse(command.egmLocked);

            var device = new Mock<IGamePlayDevice>();
            device.SetupGet(x => x.Id).Returns(deviceId);
            device.SetupGet(x => x.ConfigurationId).Returns(configurationId);
            device.SetupGet(x => x.ConfigDateTime).Returns(configDateTime);
            device.SetupGet(x => x.ConfigComplete).Returns(false);

            await builder.Build(device.Object, command);
            Assert.AreEqual(command.configurationId, configurationId);
            Assert.IsFalse(command.hostEnabled);
            Assert.IsFalse(command.egmEnabled);
            Assert.AreEqual(command.themeId, themeId);
            Assert.AreEqual(command.paytableId, paytableId);
            Assert.IsFalse(command.generalTilt);
            Assert.IsFalse(command.configComplete);
            Assert.IsFalse(command.egmLocked);
        }
    }
}