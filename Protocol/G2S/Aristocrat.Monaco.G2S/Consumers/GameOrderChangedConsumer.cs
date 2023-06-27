namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Gaming.Contracts;

    /// <summary>
    ///     Handles the <see cref="GameIconOrderChangedEvent" /> event.
    /// </summary>
    public class GameOrderChangedConsumer : Consumes<GameIconOrderChangedEvent>
    {
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameOrderChangedConsumer" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="eventLift">An <see cref="IEventLift" /> instance.</param>
        public GameOrderChangedConsumer(
            IG2SEgm egm,
            IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        /// <inheritdoc />
        public override void Consume(GameIconOrderChangedEvent theEvent)
        {
            if (!theEvent.OperatorChanged)
            {
                return;
            }

            var choosers = _egm.GetDevices<IChooserDevice>();

            foreach (var chooser in choosers)
            {
                _eventLift.Report(
                    chooser,
                    EventCode.G2S_CHE006,
                    theEvent);
            }
        }
    }
}
