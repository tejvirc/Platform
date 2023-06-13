namespace Aristocrat.Monaco.Application.Monitors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Hardware.Contracts.KeySwitch;
    using Kernel;
    using Monaco.Localization.Properties;

    /// <summary>
    ///     A Service that monitors and responds to key switch events. 
    /// </summary>
    public class KeySwitchMonitor : IService, IDisposable
    {
        private const string OperatorKeyName = "Operator";

        private readonly IEventBus _eventBus;
        private readonly ISystemDisableManager _disableManager;
        private readonly IKeySwitch _keySwitchService;

        private bool _disposed;
        private bool _isKeyInEgm;
        private bool _isLockedUp;

        /// <inheritdoc />
        public string Name { get; } = typeof(KeySwitchMonitor).Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { GetType() };

        public KeySwitchMonitor()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IKeySwitch>(),
                ServiceManager.GetInstance().GetService<ISystemDisableManager>())
        {
        }

        public KeySwitchMonitor(IEventBus eventBus, IKeySwitch keySwitchService, ISystemDisableManager disableManager)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _keySwitchService = keySwitchService ?? throw new ArgumentNullException(nameof(keySwitchService));
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
        }

        /// <inheritdoc />
        public void Initialize()
        {
            SubscribeToEvents();

            _isKeyInEgm = IsOperatorKeyInEgm();
            if (_isKeyInEgm)
            {
                TriggerKeySwitchLockup();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (disposing)
            {
                _eventBus?.UnsubscribeAll(this);
            }
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<OnEvent>(this, HandleEvent);
            _eventBus.Subscribe<OffEvent>(this, HandleEvent);
            _eventBus.Subscribe<OperatorMenuEnteredEvent>(this, HandleEvent);
            _eventBus.Subscribe<OperatorMenuExitedEvent>(this, HandleEvent);
            _eventBus.Subscribe<KeySwitchServiceInitializedEvent>(this, HandleEvent);
        }

        private void TriggerKeySwitchLockup()
        {
            if (!_isLockedUp && _isKeyInEgm)
            {
                _isLockedUp = true;
                _disableManager.Disable(ApplicationConstants.OperatorKeyNotRemovedDisableKey, SystemDisablePriority.Normal,
                    () => Localizer.DynamicCulture().GetString(ResourceKeys.KeyLeftInEgmLockupMessage));
            }
        }

        private void ClearKeySwitchLockup()
        {
            if (_isLockedUp && !_isKeyInEgm)
            {
                _disableManager.Enable(ApplicationConstants.OperatorKeyNotRemovedDisableKey);
                _isLockedUp = false;
            }
        }

        private bool IsOperatorKeyInEgm()
        {
            var keySwitches = _keySwitchService.LogicalKeySwitches.Values;

            LogicalKeySwitch operatorKeySwitch = keySwitches.FirstOrDefault(key =>
                key.State == KeySwitchState.Enabled &&
                key.Name.Equals(OperatorKeyName, StringComparison.Ordinal) &&
                key.Action == KeySwitchAction.On);

            return operatorKeySwitch != null;
        }

        private void HandleEvent(OnEvent args)
        {
            if (args.KeySwitchName.Equals(OperatorKeyName, StringComparison.Ordinal))
            {
                _isKeyInEgm = true;
                TriggerKeySwitchLockup();
            }
        }

        private void HandleEvent(OffEvent args)
        {
            if (args.KeySwitchName.Equals(OperatorKeyName, StringComparison.Ordinal))
            {
                _isKeyInEgm = false;
                ClearKeySwitchLockup();
            }
        }

        private void HandleEvent(OperatorMenuEnteredEvent args)
        {
            ClearKeySwitchLockup();
        }

        private void HandleEvent(OperatorMenuExitedEvent args)
        {
            TriggerKeySwitchLockup();
        }

        private void HandleEvent(KeySwitchServiceInitializedEvent args)
        {
            _isKeyInEgm = IsOperatorKeyInEgm();

            TriggerKeySwitchLockup();
        }
    }
}