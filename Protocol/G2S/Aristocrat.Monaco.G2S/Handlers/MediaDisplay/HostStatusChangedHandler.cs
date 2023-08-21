namespace Aristocrat.Monaco.G2S.Handlers.MediaDisplay
{
    using System;
    using Application.Contracts.Media;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;

    public class HostStatusChangedHandler : IStatusChangedHandler<IMediaDisplay>
    {
        private readonly ICommandBuilder<IMediaDisplay, mediaDisplayStatus> _commandBuilder;
        private readonly IEventLift _eventLift;
        private readonly IMediaProvider _mediaProvider;

        public HostStatusChangedHandler(
            IMediaProvider mediaProvider,
            ICommandBuilder<IMediaDisplay, mediaDisplayStatus> commandBuilder,
            IEventLift eventLift)
        {
            _mediaProvider = mediaProvider ?? throw new ArgumentNullException(nameof(mediaProvider));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        public void Handle(IMediaDisplay device)
        {
            // The disabled event is handled in MediaDisplayEnabledConsumer
            if (device.HostEnabled)
            {
                _mediaProvider.Enable(device.Id, MediaPlayerStatus.DisabledByBackend);

                var status = new mediaDisplayStatus();
                _commandBuilder.Build(device, status);
                _eventLift.Report(device, EventCode.IGT_MDE004, device.DeviceList(status));
            }
            else
            {
                _mediaProvider.Disable(device.Id, MediaPlayerStatus.DisabledByBackend);
            }
        }
    }
}