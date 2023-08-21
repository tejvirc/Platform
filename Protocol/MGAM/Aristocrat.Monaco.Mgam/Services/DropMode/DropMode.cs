namespace Aristocrat.Monaco.Mgam.Services.DropMode
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Application.Contracts.Identification;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Attributes;
    using Cabinet.Contracts;
    using Common;
    using Common.Events;
    using CreditValidators;
    using Gaming.Contracts;
    using Gaming.Contracts.InfoBar;
    using Handlers;
    using Hardware.Contracts.Audio;
    using Kernel;
    using Localization.Properties;
    using PlayerTracking;

    /// <summary>
    ///     Manages messages, commands and notification for drop mode.
    /// </summary>
    public class DropMode : IService, IDropMode, IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly IAttributeManager _attributes;
        private readonly IAudio _audio;
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly IMeterManager _meterManager;
        private readonly IEmployeeLogin _employeeLogin;
        private readonly IPlayerTracking _playerTracking;
        private readonly ICashOut _cashOutHandler;
        private readonly IGamePlayState _gamePlay;
        private readonly IPropertiesManager _properties;
        private readonly ILogger<DropMode> _logger;

        private readonly Guid _disableGuid = new Guid("72AA5B9F-BCA2-4383-96E1-2786DCFB7E68");

        private bool _systemWasMuted;
        private bool _disposed;

        /// <inheritdoc />
        public string Name => typeof(DropMode).FullName;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IDropMode) };

        /// <inheritdoc/>
        public bool Active { get; private set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DropMode"/> class.
        /// </summary>
        /// <param name="eventBus">Instance of <see cref="IEventBus"/>.</param>
        /// <param name="attributes">Instance of <see cref="IAttributeManager"/>.</param>
        /// <param name="audio">Instance of <see cref="IAudio"/>.</param>
        /// <param name="systemDisableManager">Instance of <see cref="ISystemDisableManager"/>.</param>
        /// <param name="meterManager">Instance of <see cref="IMeterManager"/>.</param>
        /// <param name="employeeLogin">Instance of <see cref="IEmployeeLogin"/>.</param>
        /// <param name="playerTracking">Instance of <see cref="IPlayerTracking"/>.</param>
        /// <param name="cashOutHandler">Instance of <see cref="ICashOut"/>.</param>
        /// <param name="gamePlay">Instance of <see cref="IGamePlayState"/>.</param>
        /// <param name="properties">Instance of <see cref="IPropertiesManager"/>.</param>
        /// <param name="logger">Instance of <see cref="ILogger"/>.</param>
        public DropMode(
            IEventBus eventBus,
            IAttributeManager attributes,
            IAudio audio,
            ISystemDisableManager systemDisableManager,
            IMeterManager meterManager,
            IEmployeeLogin employeeLogin,
            IPlayerTracking playerTracking,
            ICashOut cashOutHandler,
            IGamePlayState gamePlay,
            IPropertiesManager properties,
            ILogger<DropMode> logger)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));
            _audio = audio ?? throw new ArgumentNullException(nameof(audio));
            _systemDisableManager = systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
            _employeeLogin = employeeLogin ?? throw new ArgumentNullException(nameof(employeeLogin));
            _playerTracking = playerTracking ?? throw new ArgumentNullException(nameof(playerTracking));
            _cashOutHandler = cashOutHandler ?? throw new ArgumentNullException(nameof(cashOutHandler));
            _gamePlay = gamePlay ?? throw new ArgumentNullException(nameof(gamePlay));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            SubscribeToEvents();
        }

        /// <summary>
        ///     Destroy.
        /// </summary>
        ~DropMode()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public void Initialize()
        {
        }

        /// <inheritdoc />
        public void ClearMeters()
        {
            _logger.LogInfo("Processing ClearMeters");

            _meterManager.ClearAllPeriodMeters();
        }

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
                Active = false;

                _eventBus?.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<AttributeChangedEvent>(this, _ => DropModeChanged(), e => e.AttributeName == AttributeNames.DropMode);
        }

        private void SetActive(bool active)
        {
            if (Active == active || _eventBus == null)
            {
                return;
            }

            Active = active;
            _logger.LogDebug($"SetActive({Active})");

            if (Active)
            {
                _systemWasMuted = _audio.GetSystemMuted();

                if (_gamePlay.InGameRound)
                {
                    _properties.SetProperty(MgamConstants.ForceCashoutAfterGameRoundKey, true);
                    _properties.SetProperty(MgamConstants.EndPlayerSessionAfterGameRoundKey, true);
                    _properties.SetProperty(MgamConstants.EnterDropModeAfterGameRoundKey, true);
                }
                else
                {
                    _playerTracking.EndPlayerSession();
                    _logger.LogInfo($"Ending player session for player with {_cashOutHandler.Credits} credits");
                    _cashOutHandler.CashOut();

                    // Drop Mode works like a virtual employee login.
                    _employeeLogin.Login(ResourceKeys.DropMode);

                    _audio.SetSystemMuted(true);
                }

                _systemDisableManager.Disable(_disableGuid, SystemDisablePriority.Normal,
                    () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DropMode));
            }
            else
            {
                _systemDisableManager.Enable(_disableGuid);

                _audio.SetSystemMuted(_systemWasMuted);

                _employeeLogin.Logout(ResourceKeys.DropMode);

                var clearInfoBarEvent = new InfoBarClearMessageEvent(
                    SignHandler.InfoBarMessageHandle,
                    DisplayRole.VBD,
                    InfoBarRegion.Center);

                _eventBus.Publish(clearInfoBarEvent);
            }
        }

        private void DropModeChanged()
        {
            _logger.LogDebug($"Processing attribute-change for {AttributeNames.DropMode}");

            var active = _attributes.Get(AttributeNames.DropMode, false);

            SetActive(active);
        }
    }
}
