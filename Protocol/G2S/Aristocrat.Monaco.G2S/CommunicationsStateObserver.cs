namespace Aristocrat.Monaco.G2S
{
    using System;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Common.Events;
    using Kernel;

    /// <summary>
    ///     Observes communications state changes.
    /// </summary>
    public class CommunicationsStateObserver : ICommunicationsStateObserver
    {
        private readonly IEventBus _eventBus;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommunicationsStateObserver" /> class.
        /// </summary>
        /// <param name="eventBus">An <see cref="IEventBus" /> instance.</param>
        public CommunicationsStateObserver(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        /// <inheritdoc />
        public void Notify(ICommunicationsDevice device, t_commsStates state)
        {
            _eventBus.Publish(new CommunicationsStateChangedEvent(device.Id, state == t_commsStates.G2S_onLine));
        }
    }
}