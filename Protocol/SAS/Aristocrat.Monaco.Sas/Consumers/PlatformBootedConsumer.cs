namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Kernel;

    /// <summary>
    ///     The platform booted consumer
    /// </summary>
    public class PlatformBootedConsumer : Consumes<PlatformBootedEvent>
    {
        private readonly ISystemEventHandler _eventHandler;

        /// <summary>
        ///     Creates an instance of <see cref="PlatformBootedConsumer"/>
        /// </summary>
        /// <param name="eventHandler">The system event handler</param>
        public PlatformBootedConsumer(ISystemEventHandler eventHandler)
        {
            _eventHandler = eventHandler ?? throw new ArgumentNullException(nameof(eventHandler));
        }

        /// <inheritdoc />
        public override void Consume(PlatformBootedEvent theEvent)
        {
            _eventHandler.OnPlatformBooted(theEvent.CriticalMemoryCleared);
        }
    }
}
