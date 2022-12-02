namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts;
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

        public PropertyChangedConsumer(IRuntime runtimeService, IAudio audio, IEventBus eventBus, IGameCategoryService gameCategoryService, IPropertiesManager properties)
            : base(eventBus, null, e => e.PropertyName == PropertyKey.DefaultVolumeLevel || e.PropertyName == ApplicationConstants.PlayerVolumeScalarKey)
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

            _runtime.UpdateVolume(GetMaxVolume());
        }

        /// <inheritdoc />
        private float GetMaxVolume()
        {
            var volumeLevel = (VolumeLevel)_properties.GetProperty(Kernel.Contracts.PropertyKey.DefaultVolumeLevel, ApplicationConstants.DefaultVolumeLevel);
            var masterVolume = _audio.GetVolume(volumeLevel);

            var playerVolumeScalar = _audio.GetVolumeScalar((VolumeScalar)_properties.GetValue(ApplicationConstants.PlayerVolumeScalarKey, ApplicationConstants.PlayerVolumeScalar));

            var useGameTypeVolume = _properties.GetValue(ApplicationConstants.UseGameTypeVolumeKey, ApplicationConstants.UseGameTypeVolume);
            var gameTypeVolumeScalar = useGameTypeVolume ? _audio.GetVolumeScalar(_gameCategoryService.SelectedGameCategorySetting.VolumeScalar) : 1.0f;


            

            return masterVolume * gameTypeVolumeScalar * playerVolumeScalar;
        }

    }
}