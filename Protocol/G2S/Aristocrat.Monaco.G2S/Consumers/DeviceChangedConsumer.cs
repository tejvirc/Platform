namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client;
    using Common.Events;

    /// <summary>
    ///     Handles the <see cref="DeviceChangedEvent" /> event.
    /// </summary>
    public class DeviceChangedConsumer : Consumes<DeviceChangedEvent>
    {
        private readonly IEngine _engine;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DeviceChangedConsumer" /> class.
        /// </summary>
        /// <param name="engine">An <see cref="IEngine" /> instance.</param>
        public DeviceChangedConsumer(IEngine engine)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        }

        /// <inheritdoc />
        public override void Consume(DeviceChangedEvent theEvent)
        {
            // This is being run async to avoid any contention with events that are emitted during a comms restart
            Task.Run(() => _engine.Restart(new StartupContext { DeviceChanged = true }));
        }
    }
}