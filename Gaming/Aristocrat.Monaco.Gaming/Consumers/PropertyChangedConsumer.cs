namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Hardware.Contracts.Audio;
    using Kernel;
    using Kernel.Contracts;
    using Runtime;

    public class PropertyChangedConsumer : Kernel.Consumes<PropertyChangedEvent>
    {
        private readonly IAudio _audio;
        private readonly IRuntime _runtime;

        public PropertyChangedConsumer(IRuntime runtimeService, IAudio audio, IEventBus eventBus)
            : base(eventBus, null, e => e.PropertyName == PropertyKey.DefaultVolumeLevel)
        {
            _runtime = runtimeService ?? throw new ArgumentNullException(nameof(runtimeService));
            _audio = audio ?? throw new ArgumentNullException(nameof(audio));
        }

        public override void Consume(PropertyChangedEvent theEvent)
        {
            if (!_runtime.Connected)
            {
                return;
            }

            _runtime.UpdateVolume(_audio.GetDefaultVolume());
        }
    }
}