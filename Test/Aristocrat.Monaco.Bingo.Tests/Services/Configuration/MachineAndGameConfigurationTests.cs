namespace Aristocrat.Monaco.Bingo.Tests.Services.Configuration
{
    using System;
    using System.IO;
    using System.Reflection;
    using Application.Contracts;
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
    using Test.Common;

    [TestClass]
    public class MachineAndGameConfigurationTests
    {
        private MachineAndGameConfiguration _target;
        private readonly BingoServerSettingsModel _model = new();
        private Mock<IPropertiesManager> _propertiesManager = new();
        private readonly Mock<ISystemDisableManager> _disableManager = new();

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);
            _target = new MachineAndGameConfiguration(_propertiesManager.Object, _disableManager.Object);
        }

        [DataRow(true, false, DisplayName = "PropertiesManager null")]
        [DataRow(false, true, DisplayName = "DisableManager null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest(
            bool propertiesManagerNull,
            bool disableManagerNull)
        {
            _target = new MachineAndGameConfiguration(
                propertiesManagerNull ? null : _propertiesManager.Object,
                disableManagerNull ? null : _disableManager.Object);
        }

        [TestMethod]
        public void ConfigureTest()
        {
            const string locationZoneId = "5";
            const uint expectedMachineSerial = 123u;
            const string initialMachineSerial = "123";
            const string locationId = "1";
            const string locationBankValue = "15";
            const string locationPositionValue = "7";
            const string initialQuickStopMode = "0";
            const bool expectedQuickStopMode = false;
            const string gameTitleId = "1";
            const string bonusGameValue = "0";
            const string initialPaytableEvalType = "1";
            const PaytableEvaluation expectedPaytableEvalType = PaytableEvaluation.HPP;
            const string themeSkinValue = "0";
            const string paytableId = "1";
            const string expectedBingoCardPlacement = MachineAndGameConfigurationConstants.EgmSetting;
            const string initialBingoCardPlacement = MachineAndGameConfigurationConstants.EgmSetting;
            const bool expectedDispBingoCard = true;
            const string initialDispBingoCard = BingoConstants.ServerSettingOn;
            const bool expectedHideWhenInactive = true;
            const string initialHideWhenInactive = BingoConstants.ServerSettingOn;
            var initialBingoHelpUri = "file:///" + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).Replace('\\', '/') + "/www/bingo";

            var messageConfigurationAttribute = new RepeatedField<ConfigurationResponse.Types.ClientAttribute>
            {
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = initialMachineSerial, Name = MachineAndGameConfigurationConstants.MachineSerial
                },
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = locationZoneId, Name = MachineAndGameConfigurationConstants.LocationZoneId
                },
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = locationId, Name = MachineAndGameConfigurationConstants.LocationId
                },
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = gameTitleId, Name = MachineAndGameConfigurationConstants.GameTitleId
                },
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = bonusGameValue, Name = MachineAndGameConfigurationConstants.BonusGame
                },
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = initialPaytableEvalType,
                    Name = MachineAndGameConfigurationConstants.EvaluationTypePaytable
                },
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = themeSkinValue, Name = MachineAndGameConfigurationConstants.ThemeSkin
                },
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = paytableId, Name = MachineAndGameConfigurationConstants.PaytableId
                },
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = locationBankValue, Name = MachineAndGameConfigurationConstants.LocationBank
                },
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = locationPositionValue, Name = MachineAndGameConfigurationConstants.LocationPosition
                },
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = initialQuickStopMode, Name = MachineAndGameConfigurationConstants.QuickStopMode
                },
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = initialBingoCardPlacement,
                    Name = MachineAndGameConfigurationConstants.BingoCardPlacement
                },
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = initialDispBingoCard, Name = MachineAndGameConfigurationConstants.DispBingoCard
                },
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = initialHideWhenInactive, Name = MachineAndGameConfigurationConstants.HideBingoCardWhenInactive
                },
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = initialBingoHelpUri, Name = MachineAndGameConfigurationConstants.BingoHelpUri
                }
            };

            _propertiesManager.Setup(m => m.SetProperty(ApplicationConstants.Zone, locationZoneId)).Verifiable();
            _propertiesManager.Setup(m => m.SetProperty(ApplicationConstants.MachineId, expectedMachineSerial)).Verifiable();
            _propertiesManager.Setup(m => m.SetProperty(ApplicationConstants.Location, locationId)).Verifiable();
            _propertiesManager.Setup(m => m.SetProperty(ApplicationConstants.Bank, locationBankValue)).Verifiable();
            _propertiesManager.Setup(m => m.SetProperty(ApplicationConstants.Position, locationPositionValue)).Verifiable();
            _propertiesManager.Setup(m => m.SetProperty(BingoConstants.BingoCardPlacement, initialBingoCardPlacement)).Verifiable();
            _propertiesManager.Setup(m => m.SetProperty(BingoConstants.DisplayBingoCardEgm, initialDispBingoCard)).Verifiable();
            _propertiesManager.Setup(m => m.SetProperty(BingoConstants.BingoHelpUri, initialBingoHelpUri)).Verifiable();
            _propertiesManager.Setup(m => m.SetProperty(GamingConstants.ReelStopEnabled, expectedQuickStopMode)).Verifiable();

            _target.Configure(messageConfigurationAttribute, _model);

            _propertiesManager.Verify();

            Assert.AreEqual(locationZoneId, _model.ZoneId);
            Assert.AreEqual(paytableId, _model.PaytableIds);
            Assert.AreEqual(themeSkinValue, _model.ThemeSkins);
            Assert.AreEqual(locationBankValue, _model.BankId);
            Assert.AreEqual(locationPositionValue, _model.Position);
            Assert.AreEqual(expectedQuickStopMode, _model.QuickStopMode);
            Assert.AreEqual(bonusGameValue, _model.BonusGames);
            Assert.AreEqual(gameTitleId, _model.GameTitles);
            Assert.AreEqual(expectedPaytableEvalType, _model.EvaluationTypePaytable);
            Assert.AreEqual(expectedBingoCardPlacement, _model.BingoCardPlacement);
            Assert.AreEqual(expectedDispBingoCard, _model.DisplayBingoCard);
            Assert.AreEqual(expectedHideWhenInactive, _model.HideBingoCardWhenInactive);
        }

        [TestMethod]
        public void ConfigureDisableTest()
        {
            const string locationZoneId = "5";

            var messageConfigurationAttribute = new RepeatedField<ConfigurationResponse.Types.ClientAttribute>
            {
                new ConfigurationResponse.Types.ClientAttribute { Value = locationZoneId, Name = MachineAndGameConfigurationConstants.LocationZoneId}
            };

            _propertiesManager.Setup(m => m.SetProperty(ApplicationConstants.Zone, locationZoneId)).Verifiable();

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

        [DataRow("-1", MachineAndGameConfigurationConstants.GameTitleId, DisplayName = "Invalid Setting GameTitleId < 0")]
        [DataRow("", MachineAndGameConfigurationConstants.GameTitleId, DisplayName = "Invalid Setting GameTitleId Empty")]
        [DataRow("0", MachineAndGameConfigurationConstants.MachineSerial, DisplayName = "Invalid Setting MachineSerial < 1")]
        [DataRow("", MachineAndGameConfigurationConstants.MachineSerial, DisplayName = "Invalid Setting MachineSerial Empty")]
        [DataRow("-1", MachineAndGameConfigurationConstants.LocationId, DisplayName = "Invalid Setting LocationId < 0")]
        [DataRow("-1", MachineAndGameConfigurationConstants.MachineTypeId, DisplayName = "Invalid Setting MachineTypeId < 0")]
        [DataRow("-1", MachineAndGameConfigurationConstants.CreditsManager, DisplayName = "Invalid Setting CreditsManager < 0")]
        [DataRow("0", MachineAndGameConfigurationConstants.NumGamesConfigured, DisplayName = "Invalid Setting NumGamesConfigured < 1")]
        [DataRow("-1", MachineAndGameConfigurationConstants.ThemeSkin, DisplayName = "Invalid Setting ThemeSkin < 0")]
        [DataRow("-1", MachineAndGameConfigurationConstants.PaytableId, DisplayName = "Invalid Setting PaytableId < 0")]
        [DataRow("-1", MachineAndGameConfigurationConstants.DenominationId, DisplayName = "Invalid Setting DenominationId < 0")]
        [DataRow("JunkData", MachineAndGameConfigurationConstants.QuickStopMode, DisplayName = "Invalid Setting QuickStopMode is not Type bool")]
        [DataRow("Any Screen", MachineAndGameConfigurationConstants.BingoCardPlacement, DisplayName = "Invalid Setting Unknown BingoCardPlacement")]
        [DataRow("No Bingo Card", MachineAndGameConfigurationConstants.DispBingoCard, DisplayName = "Invalid Setting Unknown DispBingoCard")]
        [DataRow("No Bingo Card", MachineAndGameConfigurationConstants.HideBingoCardWhenInactive, DisplayName = "Invalid Setting Unknown HideBingoCardWhenInactive")]
        [DataRow(PaytableEvaluation.Unknown, MachineAndGameConfigurationConstants.EvaluationTypePaytable, DisplayName = "Invalid Setting Unknown PaytableEvaluation")]
        [DataRow(PaytableEvaluation.MaxPaytableMethod, MachineAndGameConfigurationConstants.EvaluationTypePaytable, DisplayName = "Invalid Setting Unknown PaytableEvaluation")]
        [DataRow("", MachineAndGameConfigurationConstants.BingoHelpUri, DisplayName = "Invalid Setting BingoHelpUri Empty")]
        [DataTestMethod]
        [ExpectedException(typeof(ConfigurationException))]
        public void InvalidSettingTest(object value, string name)
        {
            var messageConfigurationAttribute = new RepeatedField<ConfigurationResponse.Types.ClientAttribute>
            {
                new ConfigurationResponse.Types.ClientAttribute { Value = value?.ToString(), Name = name}
            };

            // should throw exception
            _target.Configure(messageConfigurationAttribute, _model);
        }

        [DataRow("1", MachineAndGameConfigurationConstants.GameTitleId, "2", "GameTitles", DisplayName = "Invalid Setting GameTitleId Can Only Be Set Once")]
        [DataRow("0", MachineAndGameConfigurationConstants.BonusGame, "1", "BonusGames", DisplayName = "Invalid Setting BonusGame Can Only Be Set Once")]
        [DataRow("1", MachineAndGameConfigurationConstants.ThemeSkin, "2", "ThemeSkins", DisplayName = "Invalid Setting ThemeSkin Can Only Be Set Once")]
        [DataRow(false, MachineAndGameConfigurationConstants.QuickStopMode, "1", "QuickStopMode", DisplayName = "Invalid Setting QuickStopMode Can Only Be Set Once")]
        [DataRow("1", MachineAndGameConfigurationConstants.PaytableId, "2", "PaytableIds", DisplayName = "Invalid Setting PaytableId Can Only Be Set Once")]
        [DataRow(PaytableEvaluation.APP, MachineAndGameConfigurationConstants.EvaluationTypePaytable, "1", "EvaluationTypePaytable", DisplayName = "Invalid Setting EvaluationTypePaytable Can Only Be Set Once")]
        [DataTestMethod]
        [ExpectedException(typeof(ConfigurationException))]
        public void SettingChangedTest(object value, string name, object newValue, string propertyName)
        {
            _model.GetType().GetProperty(propertyName).SetValue(_model, value);

            var messageConfigurationAttribute = new RepeatedField<ConfigurationResponse.Types.ClientAttribute>
            {
                new ConfigurationResponse.Types.ClientAttribute { Value = newValue?.ToString(), Name = name}
            };

            // should throw exception
            _target.Configure(messageConfigurationAttribute, _model);
        }
    }
}
