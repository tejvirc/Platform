namespace Aristocrat.Monaco.Hardware.DFU
{
    using System;
    using Contracts;
    using Contracts.SharedDevice;
    using Kernel;

    /// <summary>A dfu factory.</summary>
    /// <seealso cref="IDfuFactory" />
    public class DfuFactory : IDfuFactory
    {
        private readonly IEventBus _eventBus;

        /// <summary>
        ///     Creates an instance of <see cref="DfuFactory"/>
        /// </summary>
        /// <param name="eventBus">An instance of <see cref="IEventBus"/></param>
        public DfuFactory(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        /// <inheritdoc />
        public IDfuAdapter CreateAdapter(IDfuDevice device)
        {
            return new DfuAdapter(device, _eventBus);
        }
    }
}