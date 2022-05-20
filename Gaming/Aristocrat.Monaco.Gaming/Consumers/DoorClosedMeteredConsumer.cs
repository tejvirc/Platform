namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Contracts;
    using Hardware.Contracts.Door;
    using Kernel;

    /// <summary>
    ///     Handles the DoorClosedMeteredEvent.
    /// </summary>
    public class DoorClosedMeteredConsumer : Consumes<DoorClosedMeteredEvent>
    {
        private readonly IMeterManager _meters;
        private readonly List<DoorLogicalId> _doorsThatResetGamesPlayedSinceDoorClosed
            = new List<DoorLogicalId>
                {
                    DoorLogicalId.Main,
                    DoorLogicalId.MainOptic,
                    DoorLogicalId.DropDoor,
                    DoorLogicalId.TopBox,
                    DoorLogicalId.UniversalInterfaceBox,                    
                    DoorLogicalId.TopBoxOptic
                };

        /// <summary>
        ///     Initializes a new instance of the <see cref="DoorClosedMeteredConsumer" /> class.
        /// </summary>
        /// <param name="meters">The meter manger</param>
        /// <param name="properties">The properties manger</param>
        public DoorClosedMeteredConsumer(IMeterManager meters, IPropertiesManager properties)
        {
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
            var propertiesManager = properties ?? throw new ArgumentNullException(nameof(properties));
            if ((bool)propertiesManager.GetProperty(GamingConstants.ResetGamesPlayedSinceDoorClosedBelly, true))
            {
                _doorsThatResetGamesPlayedSinceDoorClosed.Add(DoorLogicalId.Belly);
            }
        }

        /// <inheritdoc />
        public override void Consume(DoorClosedMeteredEvent theEvent)
        {
            if (_doorsThatResetGamesPlayedSinceDoorClosed.Contains((DoorLogicalId)theEvent.LogicalId))
            {
                var meter = _meters.GetMeter(GamingMeters.GamesPlayedSinceDoorClosed);

                meter.Increment(-meter.Lifetime);
            }
        }
    }
}
