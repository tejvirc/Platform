namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Application.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;

    /// <summary>
    ///     Handles the TimeUpdatedEvent, which emits a G2S event (if necessary)
    /// </summary>
    public class TimeUpdatedConsumer : Consumes<TimeUpdatedEvent>
    {
        private const int TimeChangeThreshold = 5; // It's in seconds

        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeUpdatedConsumer" /> class.
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="eventLift">A G2S event lift.</param>
        public TimeUpdatedConsumer(IG2SEgm egm, IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        /// <inheritdoc />
        public override void Consume(TimeUpdatedEvent theEvent)
        {
            if (Math.Abs(theEvent.TimeUpdate.TotalSeconds) <= TimeChangeThreshold)
            {
                return;
            }

            var device = _egm.GetDevice<ICabinetDevice>();
            if (device == null)
            {
                return;
            }

            _eventLift.Report(device, EventCode.G2S_CBE315);
        }
    }
}