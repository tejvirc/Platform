namespace Aristocrat.Monaco.Bingo.UI.Tests.ViewModels.OperatorMenu
{
    using System;
    using System.Globalization;
    using System.Windows;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Aristocrat.Monaco.Application.Contracts;
    using Common.Storage.Model;
    using Hardware.Contracts;
    using Hardware.Contracts.Cabinet;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using UI.ViewModels.OperatorMenu;

    [TestClass]
    public class BingoServerSettingsViewModelTests
    {
        private Mock<IBingoDataFactory> _bingoDataFactory;
        private Mock<ILocalizerFactory> _localizerFactory;
        private Mock<IServerConfigurationProvider> _serverConfigProvider;
        private Mock<ICabinetDetectionService> _cabinetDetectionService;

        private BingoServerSettingsViewModel _target;
        private BingoServerSettingsModel _model;

        private const string DefaultBingoSetting = "Pending";
        private const long DefaultAmount = 123;
        private const string ServerValue = "Sample value from server";

        [TestInitialize]
        public void TestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Loose);
            _bingoDataFactory = MoqServiceManager.CreateAndAddService<IBingoDataFactory>(MockBehavior.Default);
            _localizerFactory = MoqServiceManager.CreateAndAddService<ILocalizerFactory>(MockBehavior.Default);
            _serverConfigProvider = new Mock<IServerConfigurationProvider>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IOperatorMenuAccess>(MockBehavior.Default);
            _cabinetDetectionService = MoqServiceManager.CreateAndAddService<ICabinetDetectionService>(MockBehavior.Default);
            var propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);

            // Get around lamp test requirements
            propertiesManager.Setup(x => x.GetProperty(HardwareConstants.SimulateVirtualButtonDeck, It.IsAny<object>()))
                .Returns("TRUE");
            propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.CabinetControlsDisplayElements, It.IsAny<bool>()))
                .Returns(false);
            _model = new BingoServerSettingsModel();

            _localizerFactory.Setup(m => m.For(It.IsAny<string>())).Returns<string>(
                _ =>
                {
                    var localizer = new Mock<ILocalizer>();
                    localizer.Setup(m => m.CurrentCulture).Returns(new CultureInfo("es-US"));
                    localizer.Setup(m => m.GetString(It.IsAny<string>())).Returns(DefaultBingoSetting);
                    return localizer.Object;
                });

            _bingoDataFactory.Setup(m => m.GetConfigurationProvider()).Returns(_serverConfigProvider.Object).Verifiable();
            _serverConfigProvider.Setup(m => m.GetServerConfiguration()).Returns(_model).Verifiable();

            if (Application.Current == null)
            {
                _ = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            }
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullIBingoDataFactory()
        {
            _target = new BingoServerSettingsViewModel(null);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void ConstructorNullIServerConfigurationProvider()
        {
            _serverConfigProvider = null;
            _bingoDataFactory.Setup(m => m.GetConfigurationProvider()).Returns((IServerConfigurationProvider)null).Verifiable();

            _target = new BingoServerSettingsViewModel(_bingoDataFactory.Object);
        }

        [TestMethod]
        public void OnLoadedTest()
        {
            PopulateServerSettings();

            _target = new BingoServerSettingsViewModel(_bingoDataFactory.Object);
            _target.LoadedCommand.Execute(null);

            _serverConfigProvider.Verify();
            _bingoDataFactory.Verify();

            // Target values should match model values.
            AssessAllProperties(false);
        }

        [TestMethod]
        public void OnLoadedNullModel()
        {
            _model = null;
            _serverConfigProvider.Setup(m => m.GetServerConfiguration()).Returns(_model).Verifiable();

            _target = new BingoServerSettingsViewModel(_bingoDataFactory.Object);
            _target.LoadedCommand.Execute(null);

            _serverConfigProvider.Verify();
            _bingoDataFactory.Verify();

            // Target values should be default values.
            AssessAllProperties(true);
        }

        [TestMethod]
        public void OnLoadedNullModelValues()
        {
            _target = new BingoServerSettingsViewModel(_bingoDataFactory.Object);
            _target.LoadedCommand.Execute(null);

            _serverConfigProvider.Verify();
            _bingoDataFactory.Verify();

            // Target values should be default since we did not populate the model with server values.
            AssessAllProperties(true);
        }

        private void PopulateServerSettings()
        {
            _model.VoucherInLimit = DefaultAmount;
            _model.BillAcceptanceLimit = DefaultAmount;
            _model.TicketReprint = true;
            _model.CaptureGameAnalytics = true;
            _model.AlarmConfiguration = true;
            _model.PlayerMayHideBingoCard = ServerValue;
            _model.GameEndingPrize = GameEndWinStrategy.BonusCredits;
            _model.ReadySetGo = ContinuousPlayMode.PlayButtonOnePressNoRepeat;
            _model.BingoType = BingoType.NIGC;
            _model.DisplayBingoCard = true;
            _model.BingoCardPlacement = ServerValue;
            _model.MaximumVoucherValue = DefaultAmount;
            _model.MinimumJackpotValue = DefaultAmount;
            _model.JackpotStrategy = JackpotStrategy.HandpayJackpotWin;
            _model.JackpotAmountDetermination = JackpotDetermination.InterimPattern;
            _model.PrintHandpayReceipt = true;
            _model.LegacyBonusAllowed = ServerValue;
            _model.AftBonusingEnabled = true;
            _model.CreditsStrategy = CreditsStrategy.Sas;
            _model.BankId = ServerValue;
            _model.ZoneId = ServerValue;
            _model.Position = ServerValue;
            _model.LapLevelIDs = ServerValue;
        }

        private void AssessAllProperties(bool expectDefaultValues)
        {
            if (expectDefaultValues)
            {
                Assert.AreEqual(DefaultBingoSetting, _target.VoucherInLimit);
                Assert.AreEqual(DefaultBingoSetting, _target.BillAcceptanceLimit);
                Assert.AreEqual(DefaultBingoSetting, _target.TicketReprint);
                Assert.AreEqual(DefaultBingoSetting, _target.CaptureGameAnalytics);
                Assert.AreEqual(DefaultBingoSetting, _target.AlarmConfiguration);
                Assert.AreEqual(DefaultBingoSetting, _target.PlayerMayHideBingoCard);
                Assert.AreEqual(GameEndWinStrategy.Unknown, _target.GameEndingPrize);
                Assert.AreEqual(ContinuousPlayMode.Unknown, _target.PlayButtonBehavior);
                Assert.AreEqual(BingoType.Unknown, _target.BingoType);
                Assert.AreEqual(DefaultBingoSetting, _target.DisplayBingoCard);
                Assert.AreEqual(DefaultBingoSetting, _target.BingoCardPlacement);
                Assert.AreEqual(DefaultBingoSetting, _target.MaximumVoucherValue);
                Assert.AreEqual(DefaultBingoSetting, _target.MinimumJackpotValue);
                Assert.AreEqual(JackpotStrategy.Unknown, _target.JackpotStrategy);
                Assert.AreEqual(JackpotDetermination.Unknown, _target.JackpotAmountDetermination);
                Assert.AreEqual(DefaultBingoSetting, _target.PrintHandpayReceipt);
                Assert.AreEqual(DefaultBingoSetting, _target.LegacyBonusAllowed);
                Assert.AreEqual(DefaultBingoSetting, _target.AftBonusingEnabled);
                Assert.AreEqual(CreditsStrategy.Unknown, _target.CreditsStrategy);
                Assert.AreEqual(DefaultBingoSetting, _target.BankId);
                Assert.AreEqual(DefaultBingoSetting, _target.ZoneId);
                Assert.AreEqual(DefaultBingoSetting, _target.Position);
                Assert.AreEqual(DefaultBingoSetting, _target.LapLevelIDs);
            }
            else
            {
                Assert.AreEqual(_model.VoucherInLimit?.CentsToDollars().FormattedCurrencyString(), _target.VoucherInLimit);
                Assert.AreEqual(_model.BillAcceptanceLimit?.CentsToDollars().FormattedCurrencyString(), _target.BillAcceptanceLimit);
                Assert.AreEqual(_model.TicketReprint?.ToString(), _target.TicketReprint);
                Assert.AreEqual(_model.CaptureGameAnalytics?.ToString(), _target.CaptureGameAnalytics);
                Assert.AreEqual(_model.AlarmConfiguration?.ToString(), _target.AlarmConfiguration);
                Assert.AreEqual(_model.PlayerMayHideBingoCard, _target.PlayerMayHideBingoCard);
                Assert.AreEqual(_model.GameEndingPrize, _target.GameEndingPrize);
                Assert.AreEqual(_model.ReadySetGo, _target.PlayButtonBehavior);
                Assert.AreEqual(_model.BingoType, _target.BingoType);
                Assert.AreEqual(_model.DisplayBingoCard?.ToString(), _target.DisplayBingoCard);
                Assert.AreEqual(_model.BingoCardPlacement, _target.BingoCardPlacement);
                Assert.AreEqual(_model.MaximumVoucherValue?.CentsToDollars().FormattedCurrencyString(), _target.MaximumVoucherValue);
                Assert.AreEqual(_model.MinimumJackpotValue?.CentsToDollars().FormattedCurrencyString(), _target.MinimumJackpotValue);
                Assert.AreEqual(_model.JackpotStrategy, _target.JackpotStrategy);
                Assert.AreEqual(_model.JackpotAmountDetermination, _target.JackpotAmountDetermination);
                Assert.AreEqual(_model.PrintHandpayReceipt?.ToString(), _target.PrintHandpayReceipt);
                Assert.AreEqual(_model.LegacyBonusAllowed, _target.LegacyBonusAllowed);
                Assert.AreEqual(_model.AftBonusingEnabled?.ToString(), _target.AftBonusingEnabled);
                Assert.AreEqual(_model.CreditsStrategy, _target.CreditsStrategy);
                Assert.AreEqual(_model.BankId, _target.BankId);
                Assert.AreEqual(_model.ZoneId, _target.ZoneId);
                Assert.AreEqual(_model.Position, _target.Position);
                Assert.AreEqual(_model.LapLevelIDs, _target.LapLevelIDs);
            }
        }
    }
}
