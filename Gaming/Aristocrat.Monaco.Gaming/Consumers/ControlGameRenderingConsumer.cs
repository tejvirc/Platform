namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Contracts;
    using Runtime;
    using Runtime.Client;

    public class ControlGameRenderingConsumer : Consumes<ControlGameRenderingEvent>
    {
        private readonly IRuntime _runtime;

        /// <inheritdoc />
        public ControlGameRenderingConsumer(IRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        public override void Consume(ControlGameRenderingEvent theEvent)
        {
            // Cause GDK to pause/continue rendering loop (and elapsed time, used by game's internal attract timer)
            _runtime.UpdateState(theEvent.Starting ? RuntimeState.Normal : RuntimeState.Pause);
        }
    }
}