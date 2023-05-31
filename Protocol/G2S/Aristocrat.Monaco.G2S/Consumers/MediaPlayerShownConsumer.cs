namespace Aristocrat.Monaco.G2S.Consumers
{
    using Application.Contracts.Media;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Handlers;
    using System;

    /// <summary>
    ///     Handles the <see cref="ShowMediaPlayerEvent"/>, which sends a media display hidden event to the G2S host.
    /// </summary>
    public class MediaPlayerShownConsumer : Consumes<ShowMediaPlayerEvent>
    {
        private readonly IG2SEgm _egm;
        private readonly ICommandBuilder<IMediaDisplay, mediaDisplayStatus> _status;
        private readonly IEventLift _eventLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MediaPlayerShownConsumer" /> class.
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="status">a command builder for the <see cref="mediaDisplayStatus"/> event status</param>
        /// <param name="eventLift">The G2S event lift.</param>
        public MediaPlayerShownConsumer(
            IG2SEgm egm,
            ICommandBuilder<IMediaDisplay, mediaDisplayStatus> status,
            IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _status = status ?? throw new ArgumentNullException(nameof(status));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        /// <inheritdoc />
        public override async void Consume(ShowMediaPlayerEvent theEvent)
        {
            var device = _egm.GetDevice<IMediaDisplay>(theEvent.MediaPlayer.Id);
            if (device == null)
            {
                return;
            }

            var status = new mediaDisplayStatus();
            await _status.Build(device, status);

            // Override visible state because we are queuing states and want to return the correct expected state
            status.deviceVisibleState = t_deviceVisibleStates.IGT_shown;

            _eventLift.Report(device, EventCode.IGT_MDE106, device.DeviceList(status), theEvent);
        }
    }
}
