namespace Aristocrat.Monaco.Gaming.UI.Tests.PlayerInfoDisplay
{
    using Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using UI.PlayerInfoDisplay;

    [TestClass]
    public class PlayerInfoDisplayFeatureProviderTests
    {
        private Mock<IPropertiesManager> _propertiesManager;

        [TestInitialize]
        public void Setup()
        {
            _propertiesManager = new Mock<IPropertiesManager>();
            _propertiesManager.Setup(x => x.GetProperty(GamingConstants.PlayerInformationDisplay.Enabled, true)).Returns(false);
            _propertiesManager.Setup(x => x.GetProperty(GamingConstants.PlayerInformationDisplay.PlayerInformationScreenEnabled, true)).Returns(false);
            _propertiesManager.Setup(x => x.GetProperty(GamingConstants.PlayerInformationDisplay.GameRulesScreenEnabled, true)).Returns(false);
        }

        [TestCleanup]
        public void TearDown()
        {
        }

        [TestMethod]
        [DataRow(true, true)]
        [DataRow(false, false)]
        public void GivenPlayerInfoDisplayWhenValueThenSetToIsPlayerInfoDisplaySupported(bool flag, bool expected)
        {
            _propertiesManager.Setup(x => x.GetProperty(GamingConstants.PlayerInformationDisplay.Enabled, true)).Returns(flag).Verifiable();

            var underTest = new PlayerInfoDisplayFeatureProvider(_propertiesManager.Object);
            var result = underTest.IsPlayerInfoDisplaySupported;

            Assert.AreEqual(expected, result);
            _propertiesManager.Verify();
        }

        [TestMethod]
        [DataRow(true, true)]
        [DataRow(false, false)]
        public void GivenGameInfoWhenValueThenSetToIsGameInfoSupported(bool flag, bool expected)
        {
            _propertiesManager.Setup(x => x.GetProperty(GamingConstants.PlayerInformationDisplay.PlayerInformationScreenEnabled, true)).Returns(flag).Verifiable();

            var underTest = new PlayerInfoDisplayFeatureProvider(_propertiesManager.Object);
            var result = underTest.IsGameInfoSupported;

            Assert.AreEqual(expected, result);
            _propertiesManager.Verify();
        }

        [TestMethod]
        [DataRow(true, true)]
        [DataRow(false, false)]
        public void GivenGameRulesWhenValueThenSetToIsGameRulesSupported(bool flag, bool expected)
        {
            _propertiesManager.Setup(x => x.GetProperty(GamingConstants.PlayerInformationDisplay.GameRulesScreenEnabled, true)).Returns(flag).Verifiable();

            var underTest = new PlayerInfoDisplayFeatureProvider(_propertiesManager.Object);
            var result = underTest.IsGameRulesSupported;

            Assert.AreEqual(expected, result);
            _propertiesManager.Verify();
        }
    }
}