namespace Aristocrat.Monaco.Gaming.Tests
{
    using System;
    using Application.Contracts;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Hardware.Contracts.Audio;
    using Aristocrat.Monaco.Hardware.Contracts.Button;
    using Kernel.Contracts.MessageDisplay;
    using Localization.Properties;
    using Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class ExcessiveMeterIncrementMonitorTest
    {
        private Mock<IEventBus> _eventBus;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<ISystemDisableManager> _systemDisableManager;
        private Mock<IMeterManager> _meterManager;
        private Mock<IAudio> _audioService;
        private Action<GameEndedEvent> _gameEndHandler;
        private Action<DownEvent> _downEventHandler;
        private ExcessiveMeterIncrementMonitor _target;

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _meterManager = MoqServiceManager.CreateAndAddService<IMeterManager>(MockBehavior.Default);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);
            _systemDisableManager = MoqServiceManager.CreateAndAddService<ISystemDisableManager>(MockBehavior.Strict);
            _audioService = MoqServiceManager.CreateAndAddService<IAudio>(MockBehavior.Strict);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _eventBus.Setup(x => x.UnsubscribeAll(It.IsAny<object>())).Verifiable();
            MoqServiceManager.RemoveInstance();
            _target?.Dispose();
        }

        [DataRow(true, false, false, false)]
        [DataRow(false, true, false, false)]
        [DataRow(false, false, true, false)]
        [DataRow(false, false, false, true)]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenNullExpectException(
            bool nullMeter = false,
            bool nullProperties = false,
            bool nullEvent = false,
            bool nullSystemDisable = false,
            bool nullAudioService = false)
        {
            SetupService(nullMeter, nullProperties, nullEvent, nullSystemDisable, nullAudioService);
            Assert.IsNull(_target);
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            SetupService();
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void GameEnd_ExcessiveMeterIncrement_ExpectLockupCreated()
        {
            SetupService();
            SetupMeters(_meterManager, AccountingMeters.CurrencyInAmount, 1000000000, 0, 0);
            SetupMeters(_meterManager, AccountingMeters.TrueCoinIn, 0, 0, 0);
            _systemDisableManager.Setup(m => m.Disable(
                   ApplicationConstants.ExcessiveMeterIncrementErrorGuid,
                   SystemDisablePriority.Immediate,
                   ResourceKeys.ExcessiveMeterIncrementError,
                   CultureProviderType.Player));
            _gameEndHandler?.Invoke(new GameEndedEvent(1, 1, " ", new GameHistoryLog(1)));
            _systemDisableManager.Verify(
                m => m.Disable(
                    ApplicationConstants.ExcessiveMeterIncrementErrorGuid,
                    SystemDisablePriority.Immediate,
                    ResourceKeys.ExcessiveMeterIncrementError,
                    CultureProviderType.Player),
                Times.Once);
        }

        [TestMethod]
        public void GameEnd_NoExcessiveMeterIncrement_ExpectNoLockupCreated()
        {
            SetupService();
            SetupMeters(_meterManager, AccountingMeters.CurrencyInAmount, 0, 0, 0);
            SetupMeters(_meterManager, AccountingMeters.TrueCoinIn, 0, 0, 0);
            _systemDisableManager.Setup(m => m.Disable(
                   ApplicationConstants.ExcessiveMeterIncrementErrorGuid,
                   SystemDisablePriority.Immediate,
                   ResourceKeys.ExcessiveMeterIncrementError,
                    CultureProviderType.Player));
            _gameEndHandler?.Invoke(new GameEndedEvent(1, 1, " ", new GameHistoryLog(1)));
            _systemDisableManager.Verify(
                m => m.Disable(
                    ApplicationConstants.ExcessiveMeterIncrementErrorGuid,
                    SystemDisablePriority.Immediate,
                    ResourceKeys.ExcessiveMeterIncrementError,
                    CultureProviderType.Player),
                Times.Never);
        }

        [TestMethod]
        public void ExcessiveMeterLockupPresent_JackpotKeyPressed_ExpectLockupCleared()
        {
            SetupService();
            SetupMeters(_meterManager, AccountingMeters.CurrencyInAmount, 1000000000, 0, 0);
            SetupMeters(_meterManager, AccountingMeters.TrueCoinIn, 0, 0, 0);
            _systemDisableManager.Setup(m => m.Disable(
                   ApplicationConstants.ExcessiveMeterIncrementErrorGuid,
                   SystemDisablePriority.Immediate,
                   ResourceKeys.ExcessiveMeterIncrementError,
                    CultureProviderType.Player));
            _systemDisableManager.Setup(m => m.Enable(ApplicationConstants.ExcessiveMeterIncrementErrorGuid));
            _gameEndHandler?.Invoke(new GameEndedEvent(1, 1, " ", new GameHistoryLog(1)));
            _systemDisableManager.Verify(
                m => m.Disable(
                    ApplicationConstants.ExcessiveMeterIncrementErrorGuid,
                    SystemDisablePriority.Immediate,
                    ResourceKeys.ExcessiveMeterIncrementError,
                    CultureProviderType.Player),
                Times.Once);
            _downEventHandler?.Invoke(new DownEvent((int)ButtonLogicalId.Button30));
            _systemDisableManager.Verify(m => m.Enable(ApplicationConstants.ExcessiveMeterIncrementErrorGuid), Times.Once);
        }

        [TestMethod]
        public void ExcessiveMeterLockupNotEnabled_ExcessiveMeterIncrement_ExpectNoLockup()
        {
            SetupService(false,false,false,false,false,false,false);
            SetupMeters(_meterManager, AccountingMeters.CurrencyInAmount, 1000000000, 0, 0);
            SetupMeters(_meterManager, AccountingMeters.TrueCoinIn, 0, 0, 0);
            _systemDisableManager.Setup(m => m.Disable(
                   ApplicationConstants.ExcessiveMeterIncrementErrorGuid,
                   SystemDisablePriority.Immediate,
                   ResourceKeys.ExcessiveMeterIncrementError,
                    CultureProviderType.Player));
            _gameEndHandler?.Invoke(new GameEndedEvent(1, 1, " ", new GameHistoryLog(1)));
            _systemDisableManager.Verify(
                m => m.Disable(
                    ApplicationConstants.ExcessiveMeterIncrementErrorGuid,
                    SystemDisablePriority.Immediate,
                    ResourceKeys.ExcessiveMeterIncrementError,
                    CultureProviderType.Player),
                Times.Never);
        }

        [TestMethod]
        public void ExcessiveMeterLockupPresent_PowerCycle_ExpectExcessiveMeterLockup()
        {
            _systemDisableManager.Setup(m => m.Disable(
                   ApplicationConstants.ExcessiveMeterIncrementErrorGuid,
                   SystemDisablePriority.Immediate,
                   ResourceKeys.ExcessiveMeterIncrementError,
                    CultureProviderType.Player));
            SetupService(false, false, false, false, false, true, true);
            _systemDisableManager.Verify(
                m => m.Disable(
                    ApplicationConstants.ExcessiveMeterIncrementErrorGuid,
                    SystemDisablePriority.Immediate,
                    ResourceKeys.ExcessiveMeterIncrementError,
                    CultureProviderType.Player),
                Times.Once);
        }

        private void SetupService(
            bool nullMeter = false,
            bool nullProperties = false,
            bool nullEvent = false,
            bool nullSystemDisable = false,
            bool nullAudioService = false,
            bool enableLockup = true,
            bool locked = false)
        {
            _eventBus.Setup(
                    x => x.Subscribe(
                        It.IsAny<ExcessiveMeterIncrementMonitor>(),
                        It.IsAny<Action<GameEndedEvent>>()))
                .Callback<object, Action<GameEndedEvent>>
                ((y, x) => _gameEndHandler = x);
            _eventBus.Setup(
                    x => x.Subscribe(
                        It.IsAny<ExcessiveMeterIncrementMonitor>(),
                        It.IsAny<Action<DownEvent>>(),
                        It.IsAny<Predicate<DownEvent>>()))
                .Callback<object, Action<DownEvent>, Predicate<DownEvent>>
                ((y, x, z) => _downEventHandler = x);
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.ExcessiveMeterIncrementTestBanknoteLimit, It.IsAny<object>()))
                .Returns(1000000000L);
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.ExcessiveMeterIncrementTestCoinLimit, It.IsAny<object>()))
                .Returns(100000000L);
            _propertiesManager.Setup(p => p.GetProperty(ApplicationConstants.PreviousGameEndTotalBanknotesInKey, It.IsAny<object>()))
               .Returns(0L);
            _propertiesManager.Setup(p => p.GetProperty(ApplicationConstants.PreviousGameEndTotalCoinInKey, It.IsAny<object>()))
               .Returns(0L);
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.ExcessiveMeterIncrementTestEnabled, It.IsAny<object>()))
              .Returns(enableLockup);
            _propertiesManager.Setup(p => p.GetProperty(ApplicationConstants.ExcessiveMeterIncrementLockedKey, It.IsAny<object>()))
              .Returns(locked);
            _target = new ExcessiveMeterIncrementMonitor(
                nullMeter ? null : _meterManager.Object,
                nullProperties ? null : _propertiesManager.Object,
                nullEvent ? null : _eventBus.Object,
                nullSystemDisable ? null : _systemDisableManager.Object,
                nullAudioService ? null : _audioService.Object);
        }

        private static void SetupMeters(
            Mock<IMeterManager> manager,
            string meterName,
            long lifetimeValue,
            long periodValue,
            long sessionValue)
        {
            var met = new Mock<IMeter>(MockBehavior.Strict);
            met.Setup(m => m.Lifetime).Returns(lifetimeValue);
            met.Setup(m => m.Period).Returns(periodValue);
            met.Setup(m => m.Session).Returns(sessionValue);
            manager.Setup(m => m.GetMeter(meterName)).Returns(met.Object);
        }
    }
}