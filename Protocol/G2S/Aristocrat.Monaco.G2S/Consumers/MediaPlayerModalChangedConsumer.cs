namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Application.Contracts.Media;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;

    /// <summary>
    ///     Handles the <see cref="MediaPlayerModalChangedEvent" />, which sends a media event to the G2S host.
    /// </summary>
    public class MediaPlayerModalChangedConsumer : Consumes<MediaPlayerModalChangedEvent>
    {
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MediaPlayerModalChangedConsumer" /> class.
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="eventLift">The G2S event lift.</param>
        public MediaPlayerModalChangedConsumer(
            IG2SEgm egm,
            IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        /// <inheritdoc />
        public override void Consume(MediaPlayerModalChangedEvent theEvent)
        {
            var device = _egm.GetDevice<ICabinetDevice>();
            if (device == null)
            {
                return;
            }

            _eventLift.Report(device, theEvent.On ? EventCode.G2S_MDE113 : EventCode.G2S_MDE114, theEvent);
        }
    }
}