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
    using Handlers.MediaDisplay;

    /// <summary>
    ///     Handles the <see cref="ReleaseContentMediaPlayerEvent" />, which sends a media content result to the G2S host.
    /// </summary>
    public class MediaPlayerReleaseContentConsumer : Consumes<ReleaseContentMediaPlayerEvent>
    {
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly ICommandBuilder<IMediaDisplay, mediaDisplayStatus> _status;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MediaPlayerReleaseContentConsumer" /> class.
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="status">a command builder for the <see cref="mediaDisplayStatus" /> event status</param>
        /// <param name="eventLift">The G2S event lift.</param>
        public MediaPlayerReleaseContentConsumer(
            IG2SEgm egm,
            ICommandBuilder<IMediaDisplay, mediaDisplayStatus> status,
            IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _status = status ?? throw new ArgumentNullException(nameof(status));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        /// <inheritdoc />
        public override async void Consume(ReleaseContentMediaPlayerEvent theEvent)
        {
            var device = _egm.GetDevice<IMediaDisplay>(theEvent.MediaPlayer.Id);
            if (device == null || theEvent.Media == null)
            {
                return;
            }

            var status = new mediaDisplayStatus();
            await _status.Build(device, status);

            var log = theEvent.Media.ToContentLog(device);

            _eventLift.Report(
                device,
                MediaContentError.None == theEvent.Media.ExceptionCode ? EventCode.IGT_MDE104 : EventCode.IGT_MDE105,
                device.DeviceList(status),
                theEvent.Media.TransactionId,
                device.TransactionList(log),
                null);
        }
    }
}
