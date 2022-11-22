namespace Aristocrat.Monaco.Gaming.Consumers
{
    
    using System;

    using Contracts;
    using Runtime;
    using Runtime.Client;

    public class PlayerLanguageChangedConsumer : Consumes<PlayerLanguageChangedEvent>
    {
        private readonly IRuntime _runtime;

        /// <inheritdoc />
        public PlayerLanguageChangedConsumer(IRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        public override void Consume(PlayerLanguageChangedEvent theEvent)
        {
            _runtime.UpdateState(RuntimeState.LanguageUpdate);
        }
    }
}
