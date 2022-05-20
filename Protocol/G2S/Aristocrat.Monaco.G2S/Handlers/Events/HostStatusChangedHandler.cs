namespace Aristocrat.Monaco.G2S.Handlers.Events
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;

    public class HostStatusChangedHandler : IStatusChangedHandler<IEventHandlerDevice>
    {
        private readonly ICommandBuilder<IEventHandlerDevice, eventHandlerStatus> _command;
        private readonly IEventLift _eventLift;

        public HostStatusChangedHandler(
            ICommandBuilder<IEventHandlerDevice, eventHandlerStatus> command,
            IEventLift eventLift)
        {
            _command = command ?? throw new ArgumentNullException(nameof(command));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        public void Handle(IEventHandlerDevice device)
        {
            var status = new eventHandlerStatus();
            _command.Build(device, status);
            _eventLift.Report(
                device,
                device.HostEnabled ? EventCode.G2S_EHE004 : EventCode.G2S_EHE003,
                device.DeviceList(status));
        }
    }
}