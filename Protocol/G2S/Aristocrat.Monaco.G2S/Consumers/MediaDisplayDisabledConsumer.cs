namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Application.Contracts.Media;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Handlers;

    public class MediaPlayerDisabledConsumer : Consumes<MediaPlayerDisabledEvent>
    {
        private readonly ICommandBuilder<IMediaDisplay, mediaDisplayStatus> _command;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        public MediaPlayerDisabledConsumer(
            IG2SEgm egm,
            ICommandBuilder<IMediaDisplay, mediaDisplayStatus> command,
            IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _command = command ?? throw new ArgumentNullException(nameof(command));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        public override void Consume(MediaPlayerDisabledEvent theEvent)
        {
            if (theEvent.Status != MediaPlayerStatus.DisabledByBackend)
            {
                return;
            }

            var device = _egm.GetDevice<IMediaDisplay>(theEvent.MediaPlayer.Id);

            var status = new mediaDisplayStatus();
            _command.Build(device, status);
            _eventLift.Report(device, EventCode.IGT_MDE003, device.DeviceList(status));
        }
    }
}
