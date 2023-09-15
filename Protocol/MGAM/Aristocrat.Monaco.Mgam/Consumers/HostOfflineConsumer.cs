namespace Aristocrat.Monaco.Mgam.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Common;
    using Common.Events;
    using Gaming.Contracts;
    using Hardware.Contracts;
    using Hardware.Contracts.Audio;
    using Hardware.Contracts.TowerLight;
    using Kernel;
    using Localization.Properties;

    /// <summary>
    ///     Handles the <see cref="HostOfflineEvent" /> event.
    /// </summary>
    public class HostOfflineConsumer : Consumes<HostOfflineEvent>
    {
        private readonly ISystemDisableManager _disableManager;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IGamePlayState _gamePlayStateService;
        private readonly IAudio _audioService;
        private readonly ITowerLight _towerLightService;
        private readonly ITime _timeService;


        /// <summary>
        ///     Initializes a new instance of the HostOfflineConsumer class.
        /// </summary>
        public HostOfflineConsumer(
            ISystemDisableManager disableManager,
            IPropertiesManager propertiesManager,
            IGamePlayState gamePlayStateService,
            IAudio audioService,
            ITowerLight towerLightService,
            ITime timeService)
        {
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _gamePlayStateService = gamePlayStateService ?? throw new ArgumentNullException(nameof(gamePlayStateService));
            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
            _towerLightService = towerLightService ?? throw new ArgumentNullException(nameof(towerLightService));
            _timeService = timeService ?? throw new ArgumentNullException(nameof(timeService));
        }

        /// <inheritdoc />
        public override Task Consume(HostOfflineEvent @event, CancellationToken cancellationToken)
        {
            _disableManager.Disable(
                MgamConstants.HostOfflineGuid,
                SystemDisablePriority.Normal,
                () => $"{Resources.HostDisconnected} {_timeService.GetLocationTime(@event.Timestamp)}");

            if (_gamePlayStateService.Idle)
            {
                var alertVolume = _propertiesManager.GetValue(HardwareConstants.AlertVolumeKey, MgamConstants.DefaultAlertVolume);
                _audioService.Play(SoundName.HostOffline, MgamConstants.DefaultAlertLoopCount, alertVolume);

                _towerLightService.SetFlashState(LightTier.Tier1, FlashState.FastFlash, TimeSpan.MaxValue);
            }
            else
            {
                _propertiesManager.SetProperty(MgamConstants.PlayAlarmAfterGameRoundKey, true);
            }

            return Task.CompletedTask;
        }
    }
}
