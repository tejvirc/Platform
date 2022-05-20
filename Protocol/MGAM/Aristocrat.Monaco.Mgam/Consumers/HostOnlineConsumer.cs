namespace Aristocrat.Monaco.Mgam.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using Common.Events;
    using Hardware.Contracts.Audio;
    using Hardware.Contracts.TowerLight;
    using Kernel;

    /// <summary>
    ///     Handles the <see cref="HostOnlineEvent" /> event.
    /// </summary>
    public class HostOnlineConsumer : Consumes<HostOnlineEvent>
    {
        private readonly ISystemDisableManager _disableManager;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IAudio _audioService;
        private readonly ITowerLight _towerLightService;

        /// <summary>
        ///     Initializes a new instance of the HostOfflineConsumer class.
        /// </summary>
        public HostOnlineConsumer(
            ISystemDisableManager disableManager,
            IPropertiesManager propertiesManager,
            IAudio audioService,
            ITowerLight towerLightService)
        {
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
            _towerLightService = towerLightService ?? throw new ArgumentNullException(nameof(towerLightService));
        }

        /// <inheritdoc />
        public override Task Consume(HostOnlineEvent @event, CancellationToken cancellationToken)
        {
            _disableManager.Enable(MgamConstants.HostOfflineGuid);
            _propertiesManager.SetProperty(MgamConstants.PlayAlarmAfterGameRoundKey, false);
            _audioService.Stop();
            _towerLightService.SetFlashState(LightTier.Tier1, FlashState.Off, TimeSpan.MaxValue);

            return Task.CompletedTask;
        }
    }
}
