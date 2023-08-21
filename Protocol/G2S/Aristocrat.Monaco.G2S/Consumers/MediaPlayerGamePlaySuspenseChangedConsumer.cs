namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Application.Contracts.Media;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;

    /// <summary>
    ///     Handles the <see cref="MediaPlayerGamePlaySuspenseChangedEvent" />, which sends a media event to the G2S host.
    /// </summary>
    public class MediaPlayerGamePlaySuspenseChangedConsumer : Consumes<MediaPlayerGamePlaySuspenseChangedEvent>
    {
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MediaPlayerGamePlaySuspenseChangedConsumer" /> class.
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="eventLift">The G2S event lift.</param>
        public MediaPlayerGamePlaySuspenseChangedConsumer(IG2SEgm egm, IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        /// <inheritdoc />
        public override void Consume(MediaPlayerGamePlaySuspenseChangedEvent theEvent)
        {
            var device = _egm.GetDevice<ICabinetDevice>();
            if (device == null)
            {
                return;
            }

            _eventLift.Report(device, theEvent.On ? EventCode.G2S_MDE115 : EventCode.G2S_MDE116, theEvent);
        }
    }
}