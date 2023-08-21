namespace Aristocrat.Monaco.Bingo.Tests.Services.Configuration
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Aristocrat.Monaco.Application.Contracts.Protocol;
    using Bingo.Services.Configuration;
    using Common;
    using Common.Exceptions;
    using Common.Storage.Model;
    using Google.Protobuf.Collections;
    using Hardware.Contracts;
    using Kernel;
    using Kernel.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Contracts.SASProperties;
    using ServerApiGateway;
    using Test.Common;

    [TestClass]
    public class SystemConfigurationTests
    {
        private readonly BingoServerSettingsModel _model = new();
        private SystemConfiguration _target;
        private Mock<IPropertiesManager> _propertiesManager;
        private readonly Mock<ISystemDisableManager> _disableManager = new();
        private readonly Mock<IMultiProtocolConfigurationProvider> _protocolConfigurationProvider = new();

        private static IEnumerable<object[]> SettingChangedData => new List<object[]>
        {
            new object[]
            {
                new BingoServerSettingsModel { MinimumJackpotValue = 0 },
                SystemConfigurationConstants.MinJackpotValue,
                "1"
            },
            new object[]
            {
                new BingoServerSettingsModel { MaximumVoucherValue = 10000 },
                SystemConfigurationConstants.VoucherThreshold,
                "20000"
            },
            new object[]
            {
                new BingoServerSettingsModel { JackpotStrategy = JackpotStrategy.LockJackpotWin },
                SystemConfigurationConstants.JackpotHandlingStrategy,
                JackpotStrategy.HandpayJackpotWin.ToString()
            },
            new object[]
            {
                new BingoServerSettingsModel { LegacyBonusAllowed = "none" },
                SystemConfigurationConstants.SasLegacyBonusing,
                "allowed"
            }
        };

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);
            _target = new SystemConfiguration(_propertiesManager.Object, _protocolConfigurationProvider.Object, _disableManager.Object);
        }

        [DataRow(true, false, false, DisplayName = "PropertiesManager null")]
        [DataRow(false, true, false, DisplayName = "ProtocolConfigurationProvider null")]
        [DataRow(false, false, true, DisplayName = "DisableManager null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest(
            bool propertiesManagerNull,
            bool protocolConfigurationProviderNull,
            bool disableManagerNull)
        {
            _target = new SystemConfiguration(
                propertiesManagerNull ? null : _propertiesManager.Object,
                protocolConfigurationProviderNull ? null : _protocolConfigurationProvider.Object,
                disableManagerNull ? null : _disableManager.Object);
        }

        [TestMethod]
        public void ConfigureMaxVoucherInTest()
        {
            const long expectedMaxVoucherIn = 300000L;
            const string initialVoucherMaxVoucherIn = "300000";
            const string maxVoucherIn = "300000000";
            var messageConfigurationAttribute = new RepeatedField<ConfigurationResponse.Types.ClientAttribute>
            {
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = initialVoucherMaxVoucherIn, Name = SystemConfigurationConstants.Gen8MaxVoucherIn
                }
            };

            _propertiesManager
                .Setup(m => m.SetProperty(AccountingConstants.VoucherInLimit, Convert.ToInt64(maxVoucherIn)))
                .Verifiable();

            _target.Configure(messageConfigurationAttribute, _model);

            _propertiesManager.Verify();
            Assert.AreEqual(expectedMaxVoucherIn, _model.VoucherInLimit);
        }

        [TestMethod]
        public void ConfigureHandpayReceiptTest()
        {
            const bool expectedHandpayReceipt = true;
            const string initialHandpayReceipt = "true";
            var messageConfigurationAttribute = new RepeatedField<ConfigurationResponse.Types.ClientAttribute>
            {
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = initialHandpayReceipt, Name = SystemConfigurationConstants.HandpayReceipt
                }
            };

            _propertiesManager
                .Setup(m => m.SetProperty(AccountingConstants.EnableReceipts, initialHandpayReceipt));

            _target.Configure(messageConfigurationAttribute, _model);

            _propertiesManager.Verify();
            Assert.AreEqual(expectedHandpayReceipt, _model.PrintHandpayReceipt);
        }

        [TestMethod]
        public void ConfigureTicketReprintLastTest()
        {
            const bool expectedTicketReprint = true;
            const string initialTicketReprint = "true";
            const string ticketReprintBehavior = "Last";
            var messageConfigurationAttribute = new RepeatedField<ConfigurationResponse.Types.ClientAttribute>
            {
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = initialTicketReprint, Name = SystemConfigurationConstants.TicketReprint
                }
            };

            _propertiesManager
                .Setup(m => m.SetProperty(AccountingConstants.ReprintLoggedVoucherBehavior, ticketReprintBehavior))
                .Verifiable();

            _target.Configure(messageConfigurationAttribute, _model);

            _propertiesManager.Verify();
            Assert.AreEqual(expectedTicketReprint, _model.TicketReprint);
        }

        [TestMethod]
        public void ConfigureTicketReprintNoneTest()
        {
            const bool expectedTicketReprint = false;
            const string initialTicketReprint = "false";
            const string ticketReprintBehavior = "None";
            var messageConfigurationAttribute = new RepeatedField<ConfigurationResponse.Types.ClientAttribute>
            {
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = initialTicketReprint, Name = SystemConfigurationConstants.TicketReprint
                }
            };

            _propertiesManager
                .Setup(m => m.SetProperty(AccountingConstants.ReprintLoggedVoucherBehavior, ticketReprintBehavior))
                .Verifiable();

            _target.Configure(messageConfigurationAttribute, _model);

            _propertiesManager.Verify();
            Assert.AreEqual(expectedTicketReprint, _model.TicketReprint);
        }

        [TestMethod]
        public void ConfigureGen8MaxCashInTest()
        {
            const long expectedGen8MaxCashIn = 100000L;
            const string initialGen8MaxCashIn = "100000";
            const string gen8MaxCashIn = "100000000";
            var messageConfigurationAttribute = new RepeatedField<ConfigurationResponse.Types.ClientAttribute>
            {
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = initialGen8MaxCashIn, Name = SystemConfigurationConstants.Gen8MaxCashIn
                }
            };

            _propertiesManager
                .Setup(m => m.SetProperty(PropertyKey.MaxCreditsIn, Convert.ToInt64(gen8MaxCashIn)))
                .Verifiable();

            _target.Configure(messageConfigurationAttribute, _model);

            _propertiesManager.Verify();
            Assert.AreEqual(expectedGen8MaxCashIn, _model.BillAcceptanceLimit);
        }

        [TestMethod]
        public void ConfigureVoucherThresholdTest()
        {
            const long expectedVoucherThreshold = 100000L;
            const string initialVoucherThreshold = "100000";
            const string voucherThreshold = "100000000";
            var messageConfigurationAttribute = new RepeatedField<ConfigurationResponse.Types.ClientAttribute>
            {
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = initialVoucherThreshold, Name = SystemConfigurationConstants.VoucherThreshold
                }
            };

            _propertiesManager
                .Setup(m => m.SetProperty(AccountingConstants.VoucherOutLimit, Convert.ToInt64(voucherThreshold)))
                .Verifiable();

            _target.Configure(messageConfigurationAttribute, _model);

            _propertiesManager.Verify();
            Assert.AreEqual(expectedVoucherThreshold, _model.MaximumVoucherValue);
        }

        [TestMethod]
        public void ConfigureMinJackpotValueTest()
        {
            const long expectedMinJackpotValue = 100000L;
            const string initialMinJackpotValue = "100000";
            const string minJackpotValue = "100000000";
            const long initialHandpayLimit = 10L;
            var messageConfigurationAttribute = new RepeatedField<ConfigurationResponse.Types.ClientAttribute>
            {
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = initialMinJackpotValue, Name = SystemConfigurationConstants.MinJackpotValue
                }
            };

            _propertiesManager
                .Setup(m => m.SetProperty(AccountingConstants.LargeWinLimit, Convert.ToInt64(minJackpotValue)))
                .Verifiable();
            _propertiesManager
                .Setup(m => m.GetProperty(AccountingConstants.HandpayLimit, long.MinValue))
                .Returns(initialHandpayLimit)
                .Verifiable();
            _propertiesManager
                .Setup(m => m.SetProperty(AccountingConstants.HandpayLimit, Convert.ToInt64(minJackpotValue)))
                .Verifiable();

            _target.Configure(messageConfigurationAttribute, _model);

            _propertiesManager.Verify();
            Assert.AreEqual(expectedMinJackpotValue, _model.MinimumJackpotValue);
        }

        [TestMethod]
        public void ConfigureAudibleAlarmSettingTest()
        {
            const bool expectedAudibleAlarmSetting = true;
            const string initialAudibleAlarmSetting = "true";
            var messageConfigurationAttribute = new RepeatedField<ConfigurationResponse.Types.ClientAttribute>
            {
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = initialAudibleAlarmSetting, Name = SystemConfigurationConstants.AudibleAlarmSetting
                }
            };

            _propertiesManager
                .Setup(m => m.SetProperty(HardwareConstants.DoorAlarmEnabledKey, initialAudibleAlarmSetting));

            _target.Configure(messageConfigurationAttribute, _model);

            _propertiesManager.Verify();
            Assert.AreEqual(expectedAudibleAlarmSetting, _model.AlarmConfiguration);
        }

        [TestMethod]
        public void ConfigureAftBonusingAllowedTrueTest()
        {
            const bool expectedAftBonusing = true;
            const string initialAftBonusing = "true";
            var messageConfigurationAttribute = new RepeatedField<ConfigurationResponse.Types.ClientAttribute>
            {
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = initialAftBonusing, Name = SystemConfigurationConstants.AftBonusing
                }
            };

            var sasFeatures = _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<object>()))
                .Returns(new SasFeatures { AftBonusAllowed = true });

            _propertiesManager.Setup(m => m.SetProperty(SasProperties.SasFeatureSettings, sasFeatures));

            _target.Configure(messageConfigurationAttribute, _model);

            _propertiesManager.Verify();
            Assert.AreEqual(expectedAftBonusing, _model.AftBonusingEnabled);
        }

        [TestMethod]
        public void ConfigureAftBonusingAllowedFalseTest()
        {
            const bool expectedAftBonusing = false;
            const string initialAftBonusing = "false";
            var messageConfigurationAttribute = new RepeatedField<ConfigurationResponse.Types.ClientAttribute>
            {
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = initialAftBonusing, Name = SystemConfigurationConstants.AftBonusing
                }
            };

            var sasFeatures = _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<object>()))
                .Returns(new SasFeatures { AftBonusAllowed = false });

            _propertiesManager.Setup(m => m.SetProperty(SasProperties.SasFeatureSettings, sasFeatures));

            _target.Configure(messageConfigurationAttribute, _model);

            _propertiesManager.Verify();
            Assert.AreEqual(expectedAftBonusing, _model.AftBonusingEnabled);
        }

        [TestMethod]
        public void ConfigureSasLegacyBonusingTrueTest()
        {
            const string expectedSasLegacyBonusing = "true";
            const string initialSasLegacyBonusing = "true";
            var messageConfigurationAttribute = new RepeatedField<ConfigurationResponse.Types.ClientAttribute>
            {
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = initialSasLegacyBonusing, Name = SystemConfigurationConstants.SasLegacyBonusing
                }
            };

            var sasFeatures = _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<object>()))
                .Returns(new SasFeatures { LegacyBonusAllowed = true });

            _propertiesManager.Setup(m => m.SetProperty(SasProperties.SasFeatureSettings, sasFeatures));

            _target.Configure(messageConfigurationAttribute, _model);

            _propertiesManager.Verify();
            Assert.AreEqual(expectedSasLegacyBonusing, _model.LegacyBonusAllowed);
        }

        [TestMethod]
        public void ConfigureSasLegacyBonusingFalseTest()
        {
            const string expectedSasLegacyBonusing = "false";
            const string initialSasLegacyBonusing = "false";
            var messageConfigurationAttribute = new RepeatedField<ConfigurationResponse.Types.ClientAttribute>
            {
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = initialSasLegacyBonusing, Name = SystemConfigurationConstants.SasLegacyBonusing
                }
            };

            var sasFeatures = _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<object>()))
                .Returns(new SasFeatures { LegacyBonusAllowed = false });

            _propertiesManager.Setup(m => m.SetProperty(SasProperties.SasFeatureSettings, sasFeatures));

            _target.Configure(messageConfigurationAttribute, _model);

            _propertiesManager.Verify();
            Assert.AreEqual(expectedSasLegacyBonusing, _model.LegacyBonusAllowed);
        }

        [TestMethod]
        public void ConfigureDisableTest()
        {
            const string initialAudibleAlarmSetting = "true";

            var messageConfigurationAttribute = new RepeatedField<ConfigurationResponse.Types.ClientAttribute>
            {
                new ConfigurationResponse.Types.ClientAttribute
                {
                    Value = initialAudibleAlarmSetting, Name = SystemConfigurationConstants.AudibleAlarmSetting
                }
            };

            _propertiesManager
                .Setup(m => m.SetProperty(HardwareConstants.DoorAlarmEnabledKey, initialAudibleAlarmSetting));

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

        [DataRow(SystemConfigurationConstants.RecordGamePlay, "On", true, DisplayName = "AdditionalConfigurationCaptureGameAnalytics RecordGamePlay On")]
        [DataRow(SystemConfigurationConstants.RecordGamePlay, "Off", false, DisplayName = "AdditionalConfigurationCaptureGameAnalytics  RecordGamePlay Off")]
        [DataRow(SystemConfigurationConstants.CaptureGameAnalytics, "On", true, DisplayName = "AdditionalConfigurationCaptureGameAnalytics CaptureGameAnalytics On")]
        [DataRow(SystemConfigurationConstants.CaptureGameAnalytics, "Off", false, DisplayName = "AdditionalConfigurationCaptureGameAnalytics CaptureGameAnalytics Off")]
        [DataTestMethod]
        public void AdditionalConfigurationCaptureGameAnalyticsTest(string name, object value, object expected)
        {
            var messageConfigurationAttribute = new RepeatedField<ConfigurationResponse.Types.ClientAttribute>
            {
                new ConfigurationResponse.Types.ClientAttribute { Value = value.ToString(), Name = name }
            };

            _target.Configure(messageConfigurationAttribute, _model);

            Assert.AreEqual(expected, _model.CaptureGameAnalytics);
        }

        [DataRow(SystemConfigurationConstants.JackpotHandlingStrategy, JackpotStrategy.LockJackpotWin, DisplayName = "JackpotHandlingStrategy LockJackpotWin")]
        [DataRow(SystemConfigurationConstants.JackpotHandlingStrategy, JackpotStrategy.HandpayJackpotWin, DisplayName = "JackpotHandlingStrategy HandpayJackpotWin")]
        [DataRow(SystemConfigurationConstants.JackpotHandlingStrategy, JackpotStrategy.LockJackpotWinNoHistory, DisplayName = "JackpotHandlingStrategy LockJackpotWinNoHistory")]
        [DataTestMethod]
        public void AdditionalConfigurationJackpotHandlingStrategyTest(string name, object value)
        {
            var messageConfigurationAttribute = new RepeatedField<ConfigurationResponse.Types.ClientAttribute>
            {
                new ConfigurationResponse.Types.ClientAttribute { Value = value.ToString(), Name = name }
            };

            _target.Configure(messageConfigurationAttribute, _model);

            Assert.AreEqual(value, _model.JackpotStrategy);
        }

        [DataRow(SystemConfigurationConstants.CreditsStrategy, CreditsStrategy.Sas, DisplayName = "AdditionalConfigurationCreditsManager Sas")]
        [DataTestMethod]
        public void AdditionalConfigurationCreditsManagerTest(string name, object value)
        {
            var messageConfigurationAttribute = new RepeatedField<ConfigurationResponse.Types.ClientAttribute>
            {
                new ConfigurationResponse.Types.ClientAttribute { Value = value.ToString(), Name = name }
            };

            _target.Configure(messageConfigurationAttribute, _model);

            Assert.AreEqual(value, _model.CreditsStrategy);
        }

        [DataRow(SystemConfigurationConstants.JackpotAmountDetermination, JackpotDetermination.InterimPattern, DisplayName = "AdditionalConfigurationJackpotAmountDetermination InterimPattern")]
        [DataRow(SystemConfigurationConstants.JackpotAmountDetermination, JackpotDetermination.TotalWins, DisplayName = "AdditionalConfigurationJackpotAmountDetermination TotalWins")]
        [DataTestMethod]
        public void AdditionalConfigurationJackpotAmountDeterminationTest(string name, object value)
        {
            var messageConfigurationAttribute = new RepeatedField<ConfigurationResponse.Types.ClientAttribute>
            {
                new ConfigurationResponse.Types.ClientAttribute { Value = value.ToString(), Name = name }
            };

            _target.Configure(messageConfigurationAttribute, _model);

            Assert.AreEqual(value, _model.JackpotAmountDetermination);
        }

        [DataRow(SystemConfigurationConstants.MinJackpotValue, "", DisplayName = "InvalidSetting MinJackpotValue Empty")]
        [DataRow(SystemConfigurationConstants.MinJackpotValue, "0", DisplayName = "InvalidSetting MinJackpotValue == 0")]
        [DataRow(SystemConfigurationConstants.MinJackpotValue, "-1", DisplayName = "InvalidSetting MinJackpotValue < 0")]
        [DataRow(SystemConfigurationConstants.MinHandpayValue, "", DisplayName = "InvalidSetting MinHandpayValue Empty")]
        [DataRow(SystemConfigurationConstants.MinHandpayValue, "0", DisplayName = "InvalidSetting MinHandpayValue == 0")]
        [DataRow(SystemConfigurationConstants.MinHandpayValue, "-1", DisplayName = "InvalidSetting MinHandpayValue < 0")]
        [DataRow(SystemConfigurationConstants.TransferLimit, "", DisplayName = "InvalidSetting TransferLimit Empty")]
        [DataRow(SystemConfigurationConstants.TransferLimit, "-1", DisplayName = "InvalidSetting TransferLimit < 0")]
        [DataRow(SystemConfigurationConstants.VoucherThreshold, "", DisplayName = "InvalidSetting VoucherThreshold Empty")]
        [DataRow(SystemConfigurationConstants.VoucherThreshold, "-1", DisplayName = "InvalidSetting VoucherThreshold < 0")]
        [DataRow(SystemConfigurationConstants.Gen8MaxVoucherIn, "", DisplayName = "InvalidSetting MaxVoucherIn Empty")]
        [DataRow(SystemConfigurationConstants.Gen8MaxVoucherIn, "Invalid Int", DisplayName = "InvalidSetting MaxVoucherIn Not an integer")]
        [DataRow(SystemConfigurationConstants.Gen8MaxVoucherIn, "-1", DisplayName = "InvalidSetting MaxVoucherIn < 0")]
        [DataRow(SystemConfigurationConstants.BadCountThreshold, "", DisplayName = "InvalidSetting BadCountThreshold Empty")]
        [DataRow(SystemConfigurationConstants.BadCountThreshold, "-1", DisplayName = "InvalidSetting BadCountThreshold < 0")]
        [DataRow(SystemConfigurationConstants.JackpotHandlingStrategy, "", DisplayName = "InvalidSetting JackpotHandlingStrategy Empty")]
        [DataRow(SystemConfigurationConstants.JackpotHandlingStrategy, "Test", DisplayName = "InvalidSetting JackpotHandlingStrategy Test")]
        [DataRow(SystemConfigurationConstants.JackpotHandlingStrategy, JackpotStrategy.Unknown, DisplayName = "InvalidSetting JackpotHandlingStrategy Unknown")]
        [DataRow(SystemConfigurationConstants.JackpotHandlingStrategy, JackpotStrategy.MaxJackpotStrategy, DisplayName = "InvalidSetting JackpotHandlingStrategy MaxJackpotStrategy")]
        [DataRow(SystemConfigurationConstants.JackpotAmountDetermination, "", DisplayName = "InvalidSetting JackpotAmountDetermination Empty")]
        [DataRow(SystemConfigurationConstants.JackpotAmountDetermination, "Test", DisplayName = "InvalidSetting JackpotAmountDetermination Test")]
        [DataRow(SystemConfigurationConstants.JackpotAmountDetermination, JackpotDetermination.Unknown, DisplayName = "InvalidSetting JackpotAmountDetermination Unknown")]
        [DataRow(SystemConfigurationConstants.JackpotAmountDetermination, JackpotDetermination.MaxJackpotDetermination, DisplayName = "InvalidSetting JackpotAmountDetermination MaxJackpotDetermination")]
        [DataRow(SystemConfigurationConstants.HandpayReceipt, "", DisplayName = "InvalidSetting HandpayReceipt Empty")]
        [DataRow(SystemConfigurationConstants.HandpayReceipt, "Invalid Bool", DisplayName = "InvalidSetting HandpayReceipt Invalid Bool")]
        [DataRow(SystemConfigurationConstants.AudibleAlarmSetting, "", DisplayName = "InvalidSetting AudibleAlarmSetting Empty")]
        [DataRow(SystemConfigurationConstants.AudibleAlarmSetting, "Invalid Bool", DisplayName = "InvalidSetting AudibleAlarmSetting Invalid Bool")]
        [DataRow(SystemConfigurationConstants.TicketReprint, "", DisplayName = "InvalidSetting TicketReprint Empty")]
        [DataRow(SystemConfigurationConstants.TicketReprint, "Invalid Bool", DisplayName = "InvalidSetting TicketReprint Invalid Bool")]
        [DataRow(SystemConfigurationConstants.AftBonusing, "", DisplayName = "InvalidSetting AftBonusing Empty")]
        [DataRow(SystemConfigurationConstants.AftBonusing, "Invalid Bool", DisplayName = "InvalidSetting AftBonusing Invalid Bool")]
        [DataRow(SystemConfigurationConstants.CreditsStrategy, "", DisplayName = "InvalidSetting CreditsManager Empty")]
        [DataRow(SystemConfigurationConstants.CreditsStrategy, "Test", DisplayName = "InvalidSetting CreditsManager Test")]
        [DataRow(SystemConfigurationConstants.CreditsStrategy, CreditsStrategy.Local, DisplayName = "InvalidSetting CreditsManager Local")]
        [DataRow(SystemConfigurationConstants.CreditsStrategy, CreditsStrategy.Tito, DisplayName = "InvalidSetting CreditsManager Tito")]
        [DataRow(SystemConfigurationConstants.CreditsStrategy, CreditsStrategy.Mgam, DisplayName = "InvalidSetting CreditsManager Mgam")]
        [DataRow(SystemConfigurationConstants.CreditsStrategy, CreditsStrategy.Pin, DisplayName = "InvalidSetting CreditsManager Pin")]
        [DataRow(SystemConfigurationConstants.CreditsStrategy, CreditsStrategy.Alesis, DisplayName = "InvalidSetting CreditsManager Alesis")]
        [DataRow(SystemConfigurationConstants.CreditsStrategy, CreditsStrategy.Caliente, DisplayName = "InvalidSetting CreditsManager Caliente")]
        [DataRow(SystemConfigurationConstants.CreditsStrategy, CreditsStrategy.Unknown, DisplayName = "InvalidSetting CreditsManager Unknown")]
        [DataRow(SystemConfigurationConstants.JackpotHandlingStrategy, JackpotStrategy.CreditJackpotWin, DisplayName = "InvalidSetting JackpotHandlingStrategy CreditJackpotWin")]
        [DataRow(SystemConfigurationConstants.JackpotHandlingStrategy, JackpotStrategy.Unknown, DisplayName = "InvalidSetting JackpotHandlingStrategy Unknown")]
        [DataRow(SystemConfigurationConstants.JackpotHandlingStrategy, JackpotStrategy.MaxJackpotStrategy, DisplayName = "InvalidSetting JackpotHandlingStrategy MaxJackpotStrategy")]
        [DataRow(SystemConfigurationConstants.JackpotAmountDetermination, JackpotDetermination.Unknown, DisplayName = "InvalidSetting JackpotAmountDetermination Unknown")]
        [DataRow(SystemConfigurationConstants.JackpotAmountDetermination, JackpotDetermination.MaxJackpotDetermination, DisplayName = "InvalidSetting JackpotAmountDetermination MaxJackpotDetermination")]
        [DataRow(SystemConfigurationConstants.SasAft, "", DisplayName = "InvalidSetting SasAft Empty")]
        [DataRow(SystemConfigurationConstants.CashOutButton, "", DisplayName = "InvalidSetting CashOutButton Empty")]
        [DataRow(SystemConfigurationConstants.TransferWin2Host, "", DisplayName = "InvalidSetting TransferWin2Host Empty")]
        [DataTestMethod]
        [ExpectedException(typeof(ConfigurationException))]
        public void InvalidSettingTest(string name, object value)
        {
            var messageConfigurationAttribute = new RepeatedField<ConfigurationResponse.Types.ClientAttribute>
            {
                new ConfigurationResponse.Types.ClientAttribute { Value = value.ToString(), Name = name }
            };

            // should throw exception
            _target.Configure(messageConfigurationAttribute, _model);
        }

        [DynamicData(nameof(SettingChangedData))]
        [DataTestMethod]
        [ExpectedException(typeof(ConfigurationException))]
        public void SettingChangedTest(BingoServerSettingsModel model, string name, string value)
        {
            var messageConfigurationAttribute = new RepeatedField<ConfigurationResponse.Types.ClientAttribute>
            {
                new ConfigurationResponse.Types.ClientAttribute { Value = value, Name = name }
            };

            // should throw exception
            _target.Configure(messageConfigurationAttribute, model);
        }
    }
}