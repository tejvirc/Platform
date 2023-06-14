namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Reflection;
    using Application.Contracts.Localization;
    using Application.Util;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Application.Contracts;
    using Contracts;
    using Hardware.Contracts.Audio;
    using Hardware.Contracts.Button;
    using Kernel;
    using Localization.Properties;
    using log4net;

    /// <summary>
    ///     This class monitor banknote in and coin in meter after game end
    ///     if there is excessive meter increment, raise lockup
    /// </summary>
    public class ExcessiveMeterIncrementMonitor : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IMeterManager _meterManager;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IEventBus _eventBus;
        private readonly ISystemDisableManager _disableManager;
        private readonly IAudio _audioService;
        private bool _disposed;
        private long _banknoteLimit;
        private long _coinLimit;
        private string _excessiveMeterIncrementErrorSoundFilePath;

        public ExcessiveMeterIncrementMonitor(
            IMeterManager meterManager,
            IPropertiesManager propertiesManager,
            IEventBus eventBus,
            ISystemDisableManager disableManager,
            IAudio audio)
        {
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _audioService = audio ?? throw new ArgumentNullException(nameof(audio));

            var excessiveMeterIncrementTestEnabled = _propertiesManager.GetValue(GamingConstants.ExcessiveMeterIncrementTestEnabled, false);
            if (excessiveMeterIncrementTestEnabled)
            {
                Initialize();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Initialize()
        {
            Logger.Info("Initializing excessive meter increment monitor...");
            _banknoteLimit = _propertiesManager.GetValue(
                GamingConstants.ExcessiveMeterIncrementTestBanknoteLimit,
                GamingConstants.ExcessiveMeterIncrementTestDefaultBanknoteLimit);
            _coinLimit = _propertiesManager.GetValue(
                GamingConstants.ExcessiveMeterIncrementTestCoinLimit,
                GamingConstants.ExcessiveMeterIncrementTestDefaultCoinLimit);
            _eventBus.Subscribe<GameEndedEvent>(this, CheckExcessiveMeterIncrement);
            _eventBus.Subscribe<DownEvent>(this, Enable, evt => evt.LogicalId == (int)ButtonLogicalId.Button30);
            LoadSounds();
            var locked = _propertiesManager.GetValue(ApplicationConstants.ExcessiveMeterIncrementLockedKey, false);
            if (locked)
            {
                _disableManager.Disable(
                    ApplicationConstants.ExcessiveMeterIncrementErrorGuid,
                    SystemDisablePriority.Immediate,
                    () => Localizer.ForLockup().GetString(ResourceKeys.ExcessiveMeterIncrementError));
            }
        }

        private void CheckExcessiveMeterIncrement(GameEndedEvent evt)
        {
            var currentTotalBanknotesIn = _meterManager.GetMeter(AccountingMeters.CurrencyInAmount).Lifetime;
            var currentTotalCoinIn = _meterManager.GetMeter(AccountingMeters.TrueCoinIn).Lifetime;
            var previousGameEndTotalBanknotesIn = _propertiesManager.GetValue(ApplicationConstants.PreviousGameEndTotalBanknotesInKey, 0L);
            var previousGameEndTotalCoinIn = _propertiesManager.GetValue(ApplicationConstants.PreviousGameEndTotalCoinInKey, 0L);

            if (currentTotalBanknotesIn - previousGameEndTotalBanknotesIn >= _banknoteLimit
                || currentTotalCoinIn - previousGameEndTotalCoinIn >= _coinLimit)
            {
                PlayErrorSound();
                _disableManager.Disable(
                    ApplicationConstants.ExcessiveMeterIncrementErrorGuid,
                    SystemDisablePriority.Immediate,
                    () => Localizer.ForLockup().GetString(ResourceKeys.ExcessiveMeterIncrementError));
                _eventBus.Publish(new ExcessiveMeterIncrementEvent());
                _propertiesManager.SetProperty(ApplicationConstants.ExcessiveMeterIncrementLockedKey, true);
            }

            _propertiesManager.SetProperty(ApplicationConstants.PreviousGameEndTotalBanknotesInKey, currentTotalBanknotesIn);
            _propertiesManager.SetProperty(ApplicationConstants.PreviousGameEndTotalCoinInKey, currentTotalCoinIn);
        }

        private void Enable(DownEvent evt)
        {
            _disableManager.Enable(ApplicationConstants.ExcessiveMeterIncrementErrorGuid);
            _propertiesManager.SetProperty(ApplicationConstants.ExcessiveMeterIncrementLockedKey, false);
        }

        /// <summary>
        /// Load sound if configured
        /// </summary>
        private void LoadSounds()
        {
            _excessiveMeterIncrementErrorSoundFilePath = _propertiesManager?.GetValue(
                GamingConstants.ExcessiveMeterIncrementTestSoundFilePath,
                string.Empty);
            _audioService.LoadSound(_excessiveMeterIncrementErrorSoundFilePath);
        }

        /// <summary>
        /// Plays the sound defined in the Application Config for ExcessiveMeterIncrement.
        /// </summary>
        private void PlayErrorSound()
        {
            _audioService.PlaySound(_propertiesManager, _excessiveMeterIncrementErrorSoundFilePath);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }
    }
}