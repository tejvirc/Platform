namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Contracts;
    using Kernel;
    using Runtime;
    using Runtime.Client;

    public class DisableCountdownTimerConsumer : Consumes<DisableCountdownTimerEvent>
    {
        private readonly IPropertiesManager _properties;
        private readonly IRuntime _runtime;

        public DisableCountdownTimerConsumer(IRuntime runtime, IPropertiesManager properties)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public override void Consume(DisableCountdownTimerEvent theEvent)
        {
            _properties.SetProperty(GamingConstants.AutocompleteSet, theEvent.Start);
            _properties.SetProperty(GamingConstants.AutocompleteExpired, false);

            if (!theEvent.Start)
            {
                _runtime.UpdateFlag(RuntimeCondition.AutoCompleteGameRound, false);
            }
        }
    }
}