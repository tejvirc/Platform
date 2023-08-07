namespace Aristocrat.Monaco.Bingo.Tests.Services.Configuration
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Bingo.Services;
    using Bingo.Services.Configuration;
    using Common;
    using Common.Exceptions;
    using Common.Storage.Model;
    using Google.Protobuf.Collections;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using ServerApiGateway;

    [TestClass]
    public class MachineAndGameConfigurationTests
    {
        private const string GamesConfigurationString = @"[
  {
    GameTitleId: ""272"",
    ThemeSkinId: ""294"",
    PaytableId: ""12414"",
    Denomination: ""2"",
    QuickStopMode: ""false"",
    EvaluationTypePaytable: ""APP"",
    Bets: [
      18, 36, 54, 72, 90, 108, 126, 144, 162, 180, 198, 216, 234, 252, 270, 288,
      306, 324, 342, 360,
    ],
  },
  {
    GameTitleId: ""272"",
    ThemeSkinId: ""294"",
    PaytableId: ""46082"",
    Denomination: ""1"",
    QuickStopMode: ""false"",
    EvaluationTypePaytable: ""APP"",
    Bets: [
      1, 2, 3, 4, 5, 6, 7, 8, 9, 18, 27, 36, 45, 54, 63, 72, 81, 90, 99, 108,
      117, 126, 135, 144, 153, 162, 171, 180,
    ],
    HelpUrl: ""http://localhost/gamehelp/61/46082/""
  },
]";

        private const string GamesConfigurationChangedString = @"[
  {
    GameTitleId: ""272"",
    ThemeSkinId: ""294"",
    PaytableId: ""12414"",
    Denomination: ""2"",
    QuickStopMode: ""false"",
    EvaluationTypePaytable: ""APP"",
    Bets: [
      18, 36, 54, 72, 90, 108, 126, 144, 162, 180, 198, 216, 234, 252, 270, 288,
      306, 324, 342, 360,
    ],
    HelpUrl: ""http://localhost/gamehelp/61/46082/""
  }
]";

        private MachineAndGameConfiguration _target;
        private readonly BingoServerSettingsModel _model = new();
        private readonly Mock<IPropertiesManager> _propertiesManager = new(MockBehavior.Default);
        private readonly Mock<ISystemDisableManager> _disableManager = new(MockBehavior.Default);
        private readonly Mock<IBingoPaytableInstaller> _bingoGameConfiguration = new(MockBehavior.Default);
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Default);

        [TestInitialize]
        public void Initialize()
        {
            _target = CreateTarget();
        }

        [DataRow(true, false, false, false)]
        [DataRow(false, true, false, false)]
        [DataRow(false, false, true, false)]
        [DataRow(false, false, false, true)]
        [DataTestMethod]
        public void NullConstructorArgumentsTest(
            bool nullProperties,
            bool nullDisable,
            bool nullBingoGameConfiguration,
            bool nullEventBus)
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => _ = CreateTarget(nullProperties, nullDisable, nullBingoGameConfiguration, nullEventBus));
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
            const string expectedBingoCardPlacement = MachineAndGameConfigurationConstants.EgmSetting;
            const string initialBingoCardPlacement = MachineAndGameConfigurationConstants.EgmSetting;
            const bool expectedDispBingoCard = true;
            const string initialDispBingoCard = BingoConstants.ServerSettingOn;
            const bool expectedHideWhenInactive = true;
            const string initialHideWhenInactive = BingoConstants.ServerSettingOn;

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
                    Value = locationBankValue, Name = MachineAndGameConfigurationConstants.LocationBank
                },
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = locationPositionValue, Name = MachineAndGameConfigurationConstants.LocationPosition
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
                    Value = GamesConfigurationString, Name = MachineAndGameConfigurationConstants.GamesConfigured
                }
            };

            _propertiesManager.Setup(m => m.SetProperty(ApplicationConstants.Zone, locationZoneId)).Verifiable();
            _propertiesManager.Setup(m => m.SetProperty(ApplicationConstants.MachineId, expectedMachineSerial)).Verifiable();
            _propertiesManager.Setup(m => m.SetProperty(ApplicationConstants.Location, locationId)).Verifiable();
            _propertiesManager.Setup(m => m.SetProperty(ApplicationConstants.Bank, locationBankValue)).Verifiable();
            _propertiesManager.Setup(m => m.SetProperty(ApplicationConstants.Position, locationPositionValue)).Verifiable();
            _propertiesManager.Setup(m => m.SetProperty(BingoConstants.BingoCardPlacement, initialBingoCardPlacement)).Verifiable();
            _propertiesManager.Setup(m => m.SetProperty(BingoConstants.DisplayBingoCardEgm, initialDispBingoCard)).Verifiable();
            _bingoGameConfiguration.Setup(x => x.IsConfigurationValid(It.IsAny<IEnumerable<ServerGameConfiguration>>()))
                .Returns(true);

            _target.Configure(messageConfigurationAttribute, _model);

            _propertiesManager.Verify();

            Assert.AreEqual(locationZoneId, _model.ZoneId);
            Assert.AreEqual(GamesConfigurationString, _model.ServerGameConfiguration);
            Assert.AreEqual(locationBankValue, _model.BankId);
            Assert.AreEqual(locationPositionValue, _model.Position);
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

        [DataRow("0", MachineAndGameConfigurationConstants.MachineSerial, DisplayName = "Invalid Setting MachineSerial < 1")]
        [DataRow("", MachineAndGameConfigurationConstants.MachineSerial, DisplayName = "Invalid Setting MachineSerial Empty")]
        [DataRow("-1", MachineAndGameConfigurationConstants.LocationId, DisplayName = "Invalid Setting LocationId < 0")]
        [DataRow("-1", MachineAndGameConfigurationConstants.MachineTypeId, DisplayName = "Invalid Setting MachineTypeId < 0")]
        [DataRow("-1", MachineAndGameConfigurationConstants.CreditsManager, DisplayName = "Invalid Setting CreditsManager < 0")]
        [DataRow("Any Screen", MachineAndGameConfigurationConstants.BingoCardPlacement, DisplayName = "Invalid Setting Unknown BingoCardPlacement")]
        [DataRow("No Bingo Card", MachineAndGameConfigurationConstants.DispBingoCard, DisplayName = "Invalid Setting Unknown DispBingoCard")]
        [DataRow("No Bingo Card", MachineAndGameConfigurationConstants.HideBingoCardWhenInactive, DisplayName = "Invalid Setting Unknown HideBingoCardWhenInactive")]
        [DataRow("", MachineAndGameConfigurationConstants.GamesConfigured, DisplayName = "Invalid Setting GamesConfigured Empty")]
        [DataTestMethod]
        public void InvalidSettingTest(object value, string name)
        {
            var messageConfigurationAttribute = new RepeatedField<ConfigurationResponse.Types.ClientAttribute>
            {
                new ConfigurationResponse.Types.ClientAttribute { Value = value?.ToString(), Name = name}
            };

            Assert.ThrowsException<ConfigurationException>(() => _target.Configure(messageConfigurationAttribute, _model));
        }

        [DataRow(GamesConfigurationString, MachineAndGameConfigurationConstants.GamesConfigured, GamesConfigurationChangedString, nameof(BingoServerSettingsModel.ServerGameConfiguration), DisplayName = "Invalid Setting Games can only be set once")]
        [DataTestMethod]
        public void SettingChangedTest(object value, string name, object newValue, string propertyName)
        {
            _model.GetType().GetProperty(propertyName)?.SetValue(_model, value);

            var messageConfigurationAttribute = new RepeatedField<ConfigurationResponse.Types.ClientAttribute>
            {
                new ConfigurationResponse.Types.ClientAttribute { Value = newValue?.ToString(), Name = name}
            };

            Assert.ThrowsException<ConfigurationException>(() => _target.Configure(messageConfigurationAttribute, _model));
        }

        private MachineAndGameConfiguration CreateTarget(
            bool nullProperties = false,
            bool nullDisable = false,
            bool nullBingoGameConfiguration = false,
            bool nullEventBus = false)
        {
            return new MachineAndGameConfiguration(
                nullProperties ? null : _propertiesManager.Object,
                nullDisable ? null : _disableManager.Object,
                nullBingoGameConfiguration ? null : _bingoGameConfiguration.Object,
                nullEventBus ? null : _eventBus.Object);
        }
    }
}
