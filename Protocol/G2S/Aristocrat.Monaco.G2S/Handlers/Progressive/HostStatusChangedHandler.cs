namespace Aristocrat.Monaco.G2S.Handlers.Progressive
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     A host status changed handler for progressive device.
    /// </summary>
    /// <seealso
    ///     cref="T:Aristocrat.Monaco.G2S.Handlers.IStatusChangedHandler{Aristocrat.G2S.Client.Devices.IProgressiveDevice}" />
    public class HostStatusChangedHandler : IStatusChangedHandler<IProgressiveDevice>
    {
        private readonly ICommandBuilder<IProgressiveDevice, progressiveStatus> _commandBuilder;
        private readonly IEventLift _eventLift;

        /// <summary>
        ///     Initializes a new instance of the Aristocrat.Monaco.G2S.Handlers.Progressive.HostStatusChangedHandler class.
        /// </summary>
        /// <param name="commandBuilder">The command builder.</param>
        /// <param name="eventLift">The event lift.</param>
        public HostStatusChangedHandler(
            ICommandBuilder<IProgressiveDevice, progressiveStatus> commandBuilder,
            IEventLift eventLift)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        /// <summary>
        ///     Handles the given device.
        /// </summary>
        /// <param name="device">The device.</param>
        public void Handle(IProgressiveDevice device)
        {
            var status = new progressiveStatus();

            _commandBuilder.Build(device, status).Wait();

            _eventLift.Report(
                device,
                device.HostEnabled ? EventCode.G2S_PGE004 : EventCode.G2S_PGE003,
                device.DeviceList(status));
        }
    }
}