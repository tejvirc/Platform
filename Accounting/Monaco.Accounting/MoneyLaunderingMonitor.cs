namespace Aristocrat.Monaco.Accounting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts.Extensions;
    using Application.Util;
    using Contracts;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Aristocrat.Monaco.Hardware.Contracts.Audio;
    using Kernel;
    using Localization.Properties;
    using Aristocrat.Monaco.Hardware.Contracts.Button;
    using log4net;
    using Aristocrat.Monaco.Accounting.Contracts.Wat;
    using System.Runtime.CompilerServices;
    using Kernel.Contracts.MessageDisplay;

    public class MoneyLaunderingMonitor : IMoneyLaunderingMonitor, IService, IDisposable
    {
        private readonly IPropertiesManager _propertiesManager;
        private readonly IAudio _audio;
        private readonly IEventBus _eventBus;
        private readonly ISystemDisableManager _disableManager;
        private readonly IBank _bank;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private bool _disposed;
        private long _excessiveMeterValue;
        private string _soundFilePath;
        private static readonly object Lock = new object();

        public MoneyLaunderingMonitor()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<ISystemDisableManager>(),
                ServiceManager.GetInstance().GetService<IAudio>(),
                ServiceManager.GetInstance().GetService<IBank>())
        {
        }

        public MoneyLaunderingMonitor(
                IEventBus eventBus,
                IPropertiesManager propertiesManager,
                ISystemDisableManager disableManager,
                IAudio audio,
                IBank bank
                )
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _audio = audio ?? throw new ArgumentNullException(nameof(audio));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
        }

        ~MoneyLaunderingMonitor() => Dispose(false);

        /// <summary>
        /// GUID used for disabling/re-enabling the machine
        /// </summary>
        public static readonly Guid ExcessiveThresholdDisableKey = new Guid("{F1EC417B-3E13-4B36-9944-2DE051641DDF}");

        /// <inheritdoc />
        public string Name => typeof(MoneyLaunderingMonitor).FullName;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IMoneyLaunderingMonitor) };

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
                _eventBus.UnsubscribeAll(this);
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            _disposed = true;
        }

        /// <inheritdoc />
        public void Initialize()
        {
            Logger.Debug($"{nameof(MoneyLaunderingMonitor)} service initialized.");

            _excessiveMeterValue = (long)_propertiesManager.GetProperty(AccountingConstants.ExcessiveMeterValue, 0L);

            LoadAudio();

            RestoreMachineStatus();

            if (IsServiceEnabled())
                EnableService();

            _eventBus.Subscribe<PropertyChangedEvent>(this, PropertyChangedEventHandler, e => e.PropertyName == AccountingConstants.IncrementThresholdIsChecked);
        }

        /// <inheritdoc />
        public bool IsThresholdReached()
        {
            if (!IsServiceEnabled())
                return false;

            bool thresholdReached;

            var threshold = (long)_propertiesManager.GetProperty(AccountingConstants.IncrementThreshold, AccountingConstants.DefaultIncrementThreshold);

            lock (Lock)
            {
                Logger.Debug($"{nameof(IsThresholdReached)} was called. threshold={threshold.MillicentsToDollars()} meter value={ExcessiveMeterValue.MillicentsToDollars()}");

                thresholdReached = ExcessiveMeterValue >= threshold;
            }

            return thresholdReached;
        }

        /// <inheritdoc />
        public void NotifyGameStarted()
        {
            ResetMeter();
        }

        /// <summary>
        /// accumulated value for the meter
        /// </summary>
        public long ExcessiveMeterValue
        {
            get => _excessiveMeterValue;

            private set
            {
                _excessiveMeterValue = value;
                _propertiesManager.SetProperty(AccountingConstants.ExcessiveMeterValue, value);
            }
        }

        private void PropertyChangedEventHandler(PropertyChangedEvent e)
        {
            if (IsServiceEnabled())
                EnableService();
            else
                DisableService();
        }

        private void DisableService()
        {
            _eventBus.Unsubscribe<CurrencyInCompletedEvent>(this);
            _eventBus.Unsubscribe<WatOnCompleteEvent>(this);
            _eventBus.Unsubscribe<WatTransferCompletedEvent>(this);
            _eventBus.Unsubscribe<TransferOutCompletedEvent>(this);
            _eventBus.Unsubscribe<TransferOutStartedEvent>(this);

            ResetMeter();

            Logger.Debug("Service disabled.");
        }

        private void EnableService()
        {
            _eventBus.Subscribe<CurrencyInCompletedEvent>(this, CurrencyInCompletedEventHandler);
            _eventBus.Subscribe<WatOnCompleteEvent>(this, WatOnCompletedEventHandler);
            _eventBus.Subscribe<WatTransferCompletedEvent>(this, WatOffCompletedEventHandler);
            _eventBus.Subscribe<TransferOutCompletedEvent>(this, TransferOutCompletedEventHandler);
            _eventBus.Subscribe<TransferOutStartedEvent>(this, TransferOutStartedEventHandler);

            Logger.Debug("Service enabled.");
        }

        private void RestoreMachineStatus()
        {
            var restoreToDisabled = (bool)_propertiesManager.GetProperty(AccountingConstants.DisabledDueToExcessiveMeter, false);

            if (restoreToDisabled)
                DisableMachine();
        }

        private bool IsServiceEnabled()
        {
            return (bool)_propertiesManager.GetProperty(AccountingConstants.IncrementThresholdIsChecked, false);
        }

        private void LoadAudio()
        {
            _soundFilePath = (string)_propertiesManager?.GetProperty(AccountingConstants.ExcessiveMeterSound, string.Empty);

            _audio.LoadSound(_soundFilePath);
        }

        private void DisableMachine()
        {
            if (IsMachineDisabled())
                return;

            _disableManager.Disable(
                ExcessiveThresholdDisableKey,
                SystemDisablePriority.Immediate,
                ResourceKeys.ExcessiveThresholdDisableMessage,
                CultureProviderType.Player,
                true,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ExcessiveThresholdDisableHelpMessage));

            StoreMachineDisabledStatus(true);

            _audio.PlaySound(_propertiesManager, _soundFilePath);

            _eventBus.Subscribe<DownEvent>(this, e => EnableMachine(), e => e.LogicalId == (int)ButtonLogicalId.Button30);

            Logger.Debug("Machine disabled.");
        }

        private void StoreMachineDisabledStatus(bool disabled)
        {
            _propertiesManager.SetProperty(AccountingConstants.DisabledDueToExcessiveMeter, disabled);
        }

        private bool IsMachineDisabled()
        {
            return _disableManager.CurrentImmediateDisableKeys.Contains(ExcessiveThresholdDisableKey);
        }

        private void EnableMachine()
        {
            if (!IsMachineDisabled())
                return;

            _disableManager.Enable(ExcessiveThresholdDisableKey);

            StoreMachineDisabledStatus(false);

            _eventBus.Unsubscribe<DownEvent>(this);

            Logger.Debug("Machine enabled.");
        }

        private void TransferOutCompletedEventHandler(TransferOutCompletedEvent e)
        {
            ResetMeter();
        }

        private void TransferOutStartedEventHandler(TransferOutStartedEvent e)
        {
            Logger.Debug($"{nameof(TransferOutStartedEventHandler)} was called.");

            if (IsThresholdReached())
            {
                Logger.Debug($"{nameof(TransferOutStartedEventHandler)} was called and {nameof(IsThresholdReached)} was true. Going to Call DisableMachine().");

                DisableMachine();
            }
        }

        private void CurrencyInCompletedEventHandler(CurrencyInCompletedEvent e)
        {
            IncrementMeter(e.Amount);
        }

        private void WatOnCompletedEventHandler(WatOnCompleteEvent e)
        {
            // If successful, Transaction.Status would be Committed
            // TODO: This is assuming that when not successful, Status can only be Rejected. Might need verification
            // TODO: could be: if (e.Transaction.Status == WatStatus.Committed || e.Transaction.Status == WatStatus.Complete)
            if (e.Transaction.Status == WatStatus.Rejected)
            {
                Logger.Debug($"{nameof(WatOnCompletedEventHandler)} returned without processing. Transaction Status= {e.Transaction.Status}.");
                return;
            }

            IncrementMeter(e.Transaction.TransactionAmount);
        }

        private void WatOffCompletedEventHandler(WatTransferCompletedEvent e)
        {
            // If successful, Transaction.Status would be Complete
            // TODO: This is assuming that when not successful, Status can only be Rejected. Might need verification
            // TODO: could be: if (e.Transaction.Status == WatStatus.Committed || e.Transaction.Status == WatStatus.Complete)
            if (e.Transaction.Status == WatStatus.Rejected)
            {
                Logger.Debug($"{nameof(WatOffCompletedEventHandler)} returned without processing. Transaction Status= {e.Transaction.Status}.");
                return;
            }

            // In case of full WatOff if the balance becomes non-zero at this point (say due to cash-in) meter wouldn't get reset
            var balance = _bank.QueryBalance();

            if (balance == 0) // i.e. If it's Full WatOff
                ResetMeter();
        }

        private void IncrementMeter(long amount, [CallerMemberName] string caller = "")
        {
            lock (Lock)
            {
                ExcessiveMeterValue += amount;

                Logger.Debug($"Excessive Meter Value incremented by {amount.MillicentsToDollars()} to {ExcessiveMeterValue.MillicentsToDollars()} in {caller}.");
            }
        }

        private void ResetMeter([CallerMemberName] string caller = "")
        {
            lock (Lock)
            {
                Logger.Debug($"Excessive Meter Value was reset from {ExcessiveMeterValue.MillicentsToDollars()} to zero in {caller}.");

                ExcessiveMeterValue = 0;
            }
        }
    }
}
