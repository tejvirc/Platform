namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Contracts;
    using Kernel;
    using Runtime;
    using Runtime.Client;

    public class GameInitializationCompletedConsumer : Consumes<GameInitializationCompletedEvent>
    {
        private readonly IPropertiesManager _properties;
        private readonly IRuntime _runtime;

        public GameInitializationCompletedConsumer(IPropertiesManager properties, IRuntime runtime)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        public override void Consume(GameInitializationCompletedEvent theEvent)
        {
            if (_properties.GetValue(GamingConstants.AutocompleteSet, false) &&
                _properties.GetValue(GamingConstants.AutocompleteExpired, false) &&
                _properties.GetValue(GamingConstants.AutocompleteGameRoundEnabled, true))
            {
                _runtime.UpdateFlag(RuntimeCondition.AutoCompleteGameRound, true);
            }
        }
    }
}