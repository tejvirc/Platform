namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Hardware.Contracts.Battery;

    /// <summary>
    ///     Handles the <see cref="BatteryLowEvent" /> event.
    /// </summary>
    public class BatteryLowConsumer : Consumes<BatteryLowEvent>
    {
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        public BatteryLowConsumer(IG2SEgm egm, IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        public override void Consume(BatteryLowEvent batteryLowEvent)
        {
            var cabinet = _egm.GetDevice<ICabinetDevice>();
            if (cabinet == null)
            {
                return;
            }

            _eventLift.Report(cabinet, EventCode.G2S_CBE323, batteryLowEvent);
        }
    }
}