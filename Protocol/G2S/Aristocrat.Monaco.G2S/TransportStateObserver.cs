namespace Aristocrat.Monaco.G2S
{
    using System;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Common.Events;
    using Kernel;

    /// <summary>
    ///     Observes transport state changes.
    /// </summary>
    public class TransportStateObserver : ITransportStateObserver
    {
        private readonly IEventBus _eventBus;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransportStateObserver" /> class.
        /// </summary>
        /// <param name="eventBus">Event bus.</param>
        public TransportStateObserver(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        /// <inheritdoc />
        public void Notify(ICommunicationsDevice device, t_transportStates state)
        {
            switch (state)
            {
                case t_transportStates.G2S_hostUnreachable:
                    _eventBus.Publish(new HostUnreachableEvent(device.Id));
                    break;
                case t_transportStates.G2S_transportUp:
                    _eventBus.Publish(new TransportUpEvent(device.Id));
                    break;
                case t_transportStates.G2S_transportDown:
                    _eventBus.Publish(new TransportDownEvent(device.Id));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }
}