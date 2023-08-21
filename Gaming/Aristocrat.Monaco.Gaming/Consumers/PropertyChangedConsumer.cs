namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Application.Contracts;
    using Contracts;
    using Hardware.Contracts.Audio;
    using Kernel;
    using Kernel.Contracts;
    using Runtime;

    public class PropertyChangedConsumer : Kernel.Consumes<PropertyChangedEvent>
    {
        private readonly IAudio _audio;
        private readonly IRuntime _runtime;
        private readonly IGameCategoryService _gameCategoryService;
        private readonly IPropertiesManager _properties;

        public PropertyChangedConsumer(
            IRuntime runtimeService, IAudio audio, IEventBus eventBus, IGameCategoryService gameCategoryService, IPropertiesManager properties)
            : base(eventBus, null, e => e.PropertyName == PropertyKey.DefaultVolumeLevel
                                        || e.PropertyName == ApplicationConstants.PlayerVolumeScalarKey)
        {
            _runtime = runtimeService ?? throw new ArgumentNullException(nameof(runtimeService));
            _audio = audio ?? throw new ArgumentNullException(nameof(audio));
            _gameCategoryService = gameCategoryService ?? throw new ArgumentNullException(nameof(gameCategoryService));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public override void Consume(PropertyChangedEvent theEvent)
        {
            if (!_runtime.Connected)
            {
                return;
            }

            var volumeControlLocation = (VolumeControlLocation)_properties.GetValue(
                ApplicationConstants.VolumeControlLocationKey,
                ApplicationConstants.VolumeControlLocationDefault);
            var showVolumeControlInLobbyOnly = volumeControlLocation == VolumeControlLocation.Lobby;

            _runtime.UpdateVolume(_audio.GetMaxVolume(_properties, _gameCategoryService, showVolumeControlInLobbyOnly));
        }
    }
}