namespace Aristocrat.Monaco.G2S.Tests.Options
{
    using System;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Data.Model;
    using G2S.Options;
    using Gaming.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ExtendedGamePlayDeviceOptionsServiceTest
    {
        private const int ConfigurationId = 123;

        private const string ThemeId = "G2S_theme_Id";

        private const string PaytableId = "G2S_paytable_Id";

        private const int MaxWagerCredits = 5;

        private const bool ProgressiveAllowed = true;

        private const bool SecondaryAllowed = true;

        private const bool CentralAllowed = true;

        private const int DeviceId = 1;

        private readonly Mock<IDeviceObserver> _deviceObserverMock = new Mock<IDeviceObserver>();

        private readonly Mock<IGameProvider> _gameProviderMock = new Mock<IGameProvider>();

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullGameProviderExpectException()
        {
            var handler = new GamePlayDeviceOptions(null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenMatchesExpectSuccess()
        {
            var service = new GamePlayDeviceOptions(_gameProviderMock.Object);

            Assert.IsTrue(service.Matches(DeviceClass.GamePlay));
            Assert.IsFalse(service.Matches(DeviceClass.Gat));
        }

        [TestMethod]
        public void WhenSetCorrectDeviceOptionConfigValuesExpectSuccess()
        {
            ConfigureGameProfile();

            var service = new GamePlayDeviceOptions(_gameProviderMock.Object);

            service.ApplyProperties(CreateDevice(), CreateCorrectDeviceOptionConfigValues());

            _gameProviderMock.Verify(
                x =>
                    x.Configure(
                        DeviceId,
                        It.Is<GameOptionConfigValues>(
                            g =>
                                g.ThemeId == ThemeId && g.PaytableId == PaytableId && g.MaximumWagerCredits.HasValue
                                && g.MaximumWagerCredits == MaxWagerCredits && g.ProgressiveAllowed.HasValue
                                && g.ProgressiveAllowed == ProgressiveAllowed && g.SecondaryAllowed.HasValue
                                && g.SecondaryAllowed == SecondaryAllowed && g.CentralAllowed.HasValue
                                && g.CentralAllowed == CentralAllowed)),
                Times.Once);
        }

        [TestMethod]
        public void WhenSetDeviceOptionConfigValuesWithDataNotExistExpectSuccess()
        {
            ConfigureGameProfile();

            var service = new GamePlayDeviceOptions(_gameProviderMock.Object);

            service.ApplyProperties(CreateDevice(), new DeviceOptionConfigValues(ConfigurationId));

            VerifyGameProviderWithIncorrectParameters();
        }

        [TestMethod]
        public void WhenSetDeviceOptionConfigValuesWithIncorrectDataExpectSuccess()
        {
            ConfigureGameProfile();

            var service = new GamePlayDeviceOptions(_gameProviderMock.Object);

            service.ApplyProperties(CreateDevice(), CreateIncorrectDeviceOptionConfigValues());

            VerifyGameProviderWithIncorrectParameters();
        }

        private GamePlayDevice CreateDevice()
        {
            return new GamePlayDevice(DeviceId, _deviceObserverMock.Object);
        }

        private void VerifyGameProviderWithIncorrectParameters()
        {
            _gameProviderMock.Verify(
                x =>
                    x.Configure(
                        DeviceId,
                        It.Is<GameOptionConfigValues>(
                            g =>
                                string.IsNullOrEmpty(g.ThemeId) && string.IsNullOrEmpty(g.PaytableId)
                                                                && !g.MaximumWagerCredits.HasValue &&
                                                                !g.ProgressiveAllowed.HasValue
                                                                && !g.SecondaryAllowed.HasValue &&
                                                                !g.CentralAllowed.HasValue)),
                Times.Once);
        }

        private void ConfigureGameProfile()
        {
            var gameProfile = new Mock<IGameDetail>();
            gameProfile.SetupGet(x => x.Id).Returns(DeviceId);

            _gameProviderMock.Setup(x => x.GetGame(DeviceId)).Returns(gameProfile.Object);
        }

        private DeviceOptionConfigValues CreateCorrectDeviceOptionConfigValues()
        {
            var deviceOptionConfigValues = new DeviceOptionConfigValues(ConfigurationId);

            deviceOptionConfigValues.AddOption("G2S_themeId", ThemeId);
            deviceOptionConfigValues.AddOption("G2S_paytableId", PaytableId);
            deviceOptionConfigValues.AddOption("G2S_maxWagerCredits", MaxWagerCredits.ToString());
            deviceOptionConfigValues.AddOption("G2S_progAllowed", ProgressiveAllowed.ToString());
            deviceOptionConfigValues.AddOption("G2S_secondaryAllowed", SecondaryAllowed.ToString());
            deviceOptionConfigValues.AddOption("G2S_centralAllowed", CentralAllowed.ToString());

            return deviceOptionConfigValues;
        }

        private DeviceOptionConfigValues CreateIncorrectDeviceOptionConfigValues()
        {
            var deviceOptionConfigValues = new DeviceOptionConfigValues(ConfigurationId);

            deviceOptionConfigValues.AddOption("G2S_themeId", "aaa");
            deviceOptionConfigValues.AddOption("G2S_paytableId", "bbb");
            deviceOptionConfigValues.AddOption("G2S_maxWagerCredits", "-5");

            return deviceOptionConfigValues;
        }
    }
}