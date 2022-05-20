namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Contracts;
    using Hardware.Contracts.Door;

    /// <summary>
    ///     Handles the DoorOpenMeteredEvent.
    /// </summary>
    public class DoorOpenMeteredConsumer : Consumes<DoorOpenMeteredEvent>
    {
        private readonly IMeterManager _meters;
        private readonly List<DoorLogicalId> _doorsThatResetGamesPlayedSinceDoorOpen
            = new ()
            {
                DoorLogicalId.Main,
                DoorLogicalId.MainOptic
            };

        /// <summary>
        ///     Initializes a new instance of the <see cref="DoorOpenMeteredConsumer" /> class.
        /// </summary>
        /// <param name="meters">The meter manger</param>
        public DoorOpenMeteredConsumer(IMeterManager meters)
        {
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
        }

        /// <inheritdoc />
        public override void Consume(DoorOpenMeteredEvent theEvent)
        {
            if (_doorsThatResetGamesPlayedSinceDoorOpen.Contains((DoorLogicalId)theEvent.LogicalId))
            {
                var meter = _meters.GetMeter(GamingMeters.GamesPlayedSinceDoorOpen);

                meter.Increment(-meter.Lifetime);
            }
        }
    }
}
