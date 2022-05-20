namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Contracts;
    using Kernel;
    using Runtime;
    using Runtime.Client;

    public class DisableCountdownTimerExpiredConsumer : Consumes<DisableCountdownTimerExpiredEvent>
    {
        private readonly IPropertiesManager _properties;
        private readonly IRuntime _runtime;

        public DisableCountdownTimerExpiredConsumer(IRuntime runtime, IPropertiesManager properties)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public override void Consume(DisableCountdownTimerExpiredEvent theEvent)
        {
            _properties.SetProperty(GamingConstants.AutocompleteExpired, true);

            _runtime.UpdateFlag(RuntimeCondition.AutoCompleteGameRound, true);
        }
    }
}