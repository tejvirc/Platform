namespace Aristocrat.Monaco.Bingo.Tests.Services.Configuration
{
    using System;
    using Bingo.Services.Configuration;
    using Common;
    using Common.Exceptions;
    using Common.Storage.Model;
    using Gaming.Contracts;
    using Google.Protobuf.Collections;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using ServerApiGateway;

    [TestClass]
    public class ComplianceConfigurationTests
    {
        private readonly BingoServerSettingsModel _model = new();
        private readonly Mock<IPropertiesManager> _propertiesManager = new();
        private readonly Mock<ISystemDisableManager> _disableManager = new();

        private ComplianceConfiguration _target;

        [TestInitialize]
        public void Initialize()
        {
            _target = new ComplianceConfiguration(_propertiesManager.Object, _disableManager.Object);
        }

        [DataRow(true, false, DisplayName = "PropertiesManager null")]
        [DataRow(false, true, DisplayName = "DisableManager null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest(
            bool propertiesManagerNull,
            bool disableManagerNull)
        {
            _target = new ComplianceConfiguration(
                propertiesManagerNull ? null : _propertiesManager.Object,
                disableManagerNull ? null : _disableManager.Object);
        }

        [TestMethod]
        public void ConfigureTest()
        {
            const string expectedPlayerMayHideBingoCard = BingoConstants.ServerSettingOn;
            const bool expectedDispBingoCard = true;
            const bool expectedHideWhenInactive = true;
            const BingoType expectedBingoType = BingoType.CaliforniaCharity;
            const GameEndWinStrategy expectedStrategy = GameEndWinStrategy.BonusCredits;
            const ContinuousPlayMode expectedContinuousPlayMode = ContinuousPlayMode.PlayButtonOnePress;
            const PlayMode expectedPlayMode = PlayMode.Continuous;

            var messageConfigurationAttribute = new RepeatedField<ConfigurationResponse.Types.ClientAttribute>
            {
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = expectedPlayerMayHideBingoCard,
                    Name = ComplianceConfigurationConstants.PlayerMayHideBingoCard
                },
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = expectedDispBingoCard.ToString(), Name = ComplianceConfigurationConstants.DispBingoCard
                },
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = expectedHideWhenInactive.ToString(),
                    Name = ComplianceConfigurationConstants.HideBingoCardWhenInactive
                },
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = expectedBingoType.ToString("D"), Name = ComplianceConfigurationConstants.BingoType
                },
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = expectedStrategy.ToString("D"), Name = ComplianceConfigurationConstants.GameEndingPrize
                },
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = ComplianceConfigurationConstants.BallCallServiceLC2003,
                    Name = ComplianceConfigurationConstants.BallCallService
                },
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = expectedContinuousPlayMode.ToString("D"),
                    Name = ComplianceConfigurationConstants.ReadySetGoMode
                },
            };

            _propertiesManager
                .Setup(m => m.SetProperty(BingoConstants.PlayerMayHideBingoCard, expectedPlayerMayHideBingoCard))
                .Verifiable();
            _propertiesManager.Setup(m => m.SetProperty(BingoConstants.DisplayBingoCardGlobal, expectedDispBingoCard.ToString()))
                .Verifiable();
            _propertiesManager.Setup(m => m.SetProperty(GamingConstants.ContinuousPlayMode, expectedPlayMode))
                .Verifiable();

            _target.Configure(messageConfigurationAttribute, _model);

            _propertiesManager.Verify();

            Assert.AreEqual(expectedPlayerMayHideBingoCard, _model.PlayerMayHideBingoCard);
            Assert.AreEqual(expectedDispBingoCard, _model.DisplayBingoCard);
            Assert.AreEqual(expectedHideWhenInactive, _model.HideBingoCardWhenInactive);
            Assert.AreEqual(expectedBingoType, _model.BingoType);
            Assert.AreEqual(expectedStrategy, _model.GameEndingPrize);
            Assert.AreEqual(ComplianceConfigurationConstants.BallCallServiceLC2003, _model.BallCallService);
            Assert.AreEqual(expectedContinuousPlayMode, _model.ReadySetGo);
        }

        [TestMethod]
        public void ConfigureDisableTest()
        {
            const string expectedPlayerMayHideBingoCard = BingoConstants.ServerSettingOn;

            var messageConfigurationAttribute = new RepeatedField<ConfigurationResponse.Types.ClientAttribute>
            {
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = expectedPlayerMayHideBingoCard,
                    Name = ComplianceConfigurationConstants.PlayerMayHideBingoCard
                },
            };

            _propertiesManager
                .Setup(m => m.SetProperty(BingoConstants.PlayerMayHideBingoCard, expectedPlayerMayHideBingoCard))
                .Verifiable();

            _disableManager.Setup(m => m.Disable(
                BingoConstants.MissingSettingsDisableKey,
                SystemDisablePriority.Immediate,
                It.IsAny<Func<string>>(),
                true,
                It.IsAny<Func<string>>(),
                null));

            _target.Configure(messageConfigurationAttribute, _model);

            _propertiesManager.Verify();
            _disableManager.Verify();
        }

        [DataRow("Invalid", ComplianceConfigurationConstants.PlayerMayHideBingoCard, DisplayName = "Invalid Setting PlayerMayHideBingoCard is not Type bool")]
        [DataRow("", ComplianceConfigurationConstants.BallCallService, DisplayName = "Invalid Setting BallCallService Empty")]
        [DataRow("", ComplianceConfigurationConstants.BingoType, DisplayName = "Invalid Setting BingoType Empty")]
        [DataRow("Test", ComplianceConfigurationConstants.BingoType, DisplayName = "Invalid Setting BingoType Unknown")]
        [DataRow(BingoType.Unknown, ComplianceConfigurationConstants.BingoType, DisplayName = "Invalid Setting BingoType Unknown")]
        [DataRow(BingoType.MaxBingoType, ComplianceConfigurationConstants.BingoType, DisplayName = "Invalid Setting BingoType Unknown")]
        [DataRow("", ComplianceConfigurationConstants.GameEndingPrize, DisplayName = "Invalid Setting GameEndingPrize Empty")]
        [DataRow("Test", ComplianceConfigurationConstants.GameEndingPrize, DisplayName = "Invalid Setting GameEndingPrize Unknown")]
        [DataRow(GameEndWinStrategy.Unknown, ComplianceConfigurationConstants.GameEndingPrize, DisplayName = "Invalid Setting GameEndingPrize Unknown")]
        [DataRow(GameEndWinStrategy.BonusPattern, ComplianceConfigurationConstants.GameEndingPrize, DisplayName = "Invalid Setting GameEndingPrize Unsupported")]
        [DataRow(GameEndWinStrategy.CreditsFromTable, ComplianceConfigurationConstants.GameEndingPrize, DisplayName = "Invalid Setting GameEndingPrize Unsupported")]
        [DataRow(GameEndWinStrategy.FreeCard, ComplianceConfigurationConstants.GameEndingPrize, DisplayName = "Invalid Setting GameEndingPrize Unsupported")]
        [DataRow(GameEndWinStrategy.OneCentPerPlayer, ComplianceConfigurationConstants.GameEndingPrize, DisplayName = "Invalid Setting GameEndingPrize Unsupported")]
        [DataRow(ContinuousPlayMode.PlayButtonThreePress, ComplianceConfigurationConstants.ReadySetGoMode, DisplayName = "Invalid Setting ReadySetGoMode Unsupported")]
        [DataRow(ContinuousPlayMode.PlayButtonActiveParticipation, ComplianceConfigurationConstants.ReadySetGoMode, DisplayName = "Invalid Setting ReadySetGoMode Unsupported")]
        [DataRow(ContinuousPlayMode.PlayButtonTwoTouch, ComplianceConfigurationConstants.ReadySetGoMode, DisplayName = "Invalid Setting ReadySetGoMode Unsupported")]
        [DataRow(ContinuousPlayMode.PlayButtonTwoTouchRepeat, ComplianceConfigurationConstants.ReadySetGoMode, DisplayName = "Invalid Setting ReadySetGoMode Unsupported")]
        [DataRow("", ComplianceConfigurationConstants.WaitForPlayersLength, DisplayName = "Invalid Setting WaitForPlayersLength Empty")]
        [DataRow("-1", ComplianceConfigurationConstants.WaitForPlayersLength, DisplayName = "Invalid Setting WaitForPlayersLength < 0")]
        [DataRow("Invalid", ComplianceConfigurationConstants.DispBingoCard, DisplayName = "Invalid Setting DispBingoCard Unknown")]
        [DataRow("Invalid", ComplianceConfigurationConstants.HideBingoCardWhenInactive, DisplayName = "Invalid Setting HideBingoCardWhenInactive Unknown")]
        [DataTestMethod]
        [ExpectedException(typeof(ConfigurationException))]
        public void InvalidSettingEmptyStringTest(object value, string name)
        {
            var messageConfigurationAttribute = new RepeatedField<ConfigurationResponse.Types.ClientAttribute>
            {
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = value?.ToString(), Name = name
                }
            };

            // should throw exception
            _target.Configure(messageConfigurationAttribute, _model);
        }

        [DataRow(BingoType.CaliforniaCharity, ComplianceConfigurationConstants.BingoType, "2", "BingoType", DisplayName = "Invalid Setting BingoType Can Only Be Set Once")]
        [DataRow("TestBallCallService", ComplianceConfigurationConstants.BallCallService, ComplianceConfigurationConstants.BallCallServiceLC2003, "BallCallService", DisplayName = "Invalid Setting BallCallService Can Only Be Set Once")]
        [TestMethod]
        [ExpectedException(typeof(ConfigurationException))]
        public void SettingChangedTest(object value, string name, object newValue, string propertyName)
        {
            _model.GetType().GetProperty(propertyName).SetValue(_model, value);

            var messageConfigurationAttribute = new RepeatedField<ConfigurationResponse.Types.ClientAttribute>
            {
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = newValue?.ToString(), Name = name
                }
            };

            // should throw exception
            _target.Configure(messageConfigurationAttribute, _model);
        }
    }
}