namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Contracts;
    using Contracts.Meters;
    using Hardware.Contracts.Persistence;
    using Kernel;

    public class PrimaryGameFailedConsumer : Consumes<PrimaryGameFailedEvent>
    {
        private readonly IPropertiesManager _properties;
        private readonly IPersistentStorageManager _persistentStorage;
        private readonly IGameMeterManager _meters;

        public PrimaryGameFailedConsumer(
            IPropertiesManager properties,
            IPersistentStorageManager persistentStorage,
            IGameMeterManager meters)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _persistentStorage = persistentStorage ?? throw new ArgumentNullException(nameof(persistentStorage));
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
        }

        public override void Consume(PrimaryGameFailedEvent theEvent)
        {
            var gameId = _properties.GetValue(GamingConstants.SelectedGameId, 0);

            using (var scope = _persistentStorage.ScopedTransaction())
            {
                _meters.GetMeter(gameId, GamingMeters.FailedCount).Increment(1);

                scope.Complete();
            }
        }
    }
}
