namespace Aristocrat.Monaco.Asp.Client.Consumers
{
    using System;
    using Contracts;
    using Hardware.Contracts.Door;

    /// <summary>
    ///     Handles the <see cref="DoorOpenMeteredEvent" /> event.
    /// </summary>
    public class DoorOpenMeteredEventConsumer : Consumes<DoorOpenMeteredEvent>
    {
        private readonly IDoorsDataSource _doorsDataSource;

        public DoorOpenMeteredEventConsumer(IDoorsDataSource doorsDataSource)
        {
            _doorsDataSource = doorsDataSource ?? throw new ArgumentNullException(nameof(doorsDataSource));
        }
        public override void Consume(DoorOpenMeteredEvent theEvent)
        {
            _doorsDataSource.OnDoorOpenMeterChanged(theEvent);
        }
    }
}